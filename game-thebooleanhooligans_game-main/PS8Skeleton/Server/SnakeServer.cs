using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32.SafeHandles;
using NetworkUtil;
using Server;
using SnakeGame;
using Timer = System.Timers.Timer;
class SnakeServer
{
    private Dictionary<long, SocketState> clients; // clients are sockets with the ID representing the sockets
                                                   
    private Dictionary<int, Vector2D> movmentCommands = new();  // stores vectors of commands sent
    private Stopwatch watch; // keeps track of fps
    private World serverWorld;
    private GameLogic logic = new();
    private Dictionary<long, Snake> snakeToRemove = new();
    private Dictionary<long, bool> namesSent = new();
    // settings holds msperframe, worldsize, respawnrate and walls
    private GameSettings gameSettings;
    /// <summary>
    /// Snake server constructor creates a new dictionary that holds clients
    /// </summary>
    public SnakeServer()
    {
        DataContractSerializer serializer = new(typeof(GameSettings));
        XmlReader reader = XmlReader.Create("settings.xml");
        
        GameSettings settings = (GameSettings) serializer.ReadObject(reader);

        this.gameSettings = settings;
        serverWorld = new(gameSettings.UniverseSize);
        clients = new();
        this.watch = new();
        for (int i = 0; i < 20; i++)
        {   
            Powerup power = logic.SpawnPowerup(i, gameSettings);
            while (!goodSpaceForPowerups(power))
            {
                power = logic.SpawnPowerup(i, gameSettings);
            }
            serverWorld.powerups.Add(i, power);
        }
    }
    /// <summary>
    /// Checks if there is sufficient space between all of the power ups
    /// </summary>
    /// <param name="powerup"></param>
    /// <returns></returns> boolean that checks if distance between powerups is good
    public bool goodSpaceForPowerups(Powerup powerup)
    {
        foreach(Powerup p in serverWorld.powerups.Values)
        {
            double distX = Math.Pow(powerup.loc.X - p.loc.X, 2);
            double distY = Math.Pow(powerup.loc.Y - p.loc.Y, 2);
            double dist = Math.Sqrt(distX + distY);

            if (dist < 25)
            {
                return false;
            }
        }
        return true;
    }
    public bool goodSpaceForSnakes(Snake snake)
    {
        foreach (Snake s in serverWorld.snakes.Values)
        {
            double distX1 = Math.Pow(snake.body[0].X - s.body[0].X, 2);
            double distY1 = Math.Pow(snake.body[0].Y - s.body[0].Y, 2);
            double dist1 = Math.Sqrt(distX1 + distY1);

            double distX2 = Math.Pow(snake.body[snake.body.Count - 1].X - s.body[snake.body.Count - 1].X, 2);
            double distY2 = Math.Pow(snake.body[snake.body.Count - 1].Y - s.body[snake.body.Count - 1].Y, 2);
            double dist2 = Math.Sqrt(distX2 + distY2);
            if (dist1 < 25 && dist2 < 25)
            {
                return false;
            } 
        }
        return true;
    }
    /// <summary>
    /// reponsible for starting the server updating the world and sending the world
    /// </summary>
    /// <param name="args"></param>
    private static void Main (string[] args)
    {
        SnakeServer snakeServer = new SnakeServer();
        Thread acceptClientsThread = new Thread(()=>snakeServer.StartServer());
        acceptClientsThread.Start();

        /// should be own mehtod / thread will improve later
        snakeServer.watch.Start();

        while (true)  //  this makes up the entirety of the game loop. 
        {
            // note: we start a stop watch that will run for the MSPerFrame and then restart update the world and continue the loop.
            while (snakeServer.watch.ElapsedMilliseconds < snakeServer.gameSettings.MSPerFrame) 
            {
                // do nothing this is a bussy loop that will run for msperframe
            }

            snakeServer.watch.Restart();
            snakeServer.UpdateWorld();
            snakeServer.sendWorld();
        }
    }
    /// <summary>
    /// sends the current state of the world to all clients
    /// </summary>
    private void sendWorld()
    {
        lock(serverWorld)
        {

            foreach (SocketState state in clients.Values)
            {
                foreach(Snake s in serverWorld.snakes.Values)
                {
                    string jsonSnake = JsonSerializer.Serialize(s);
                    Send(state.TheSocket, jsonSnake);
                }
                foreach(Powerup p in serverWorld.powerups.Values)
                {
                    string jsonPowerup = JsonSerializer.Serialize(p);
                    Send(state.TheSocket, jsonPowerup);
                }
            }
        }
    }
    /// <summary>
    /// Sends all walls to client, should only be called once during the handshake
    /// </summary>
    private void SendStartUpFrame(Socket socket)
    {
        lock(clients)
        {

            foreach(Wall wall in gameSettings.Walls)
            {
                string jsonWall = JsonSerializer.Serialize(wall);
                Send(socket, jsonWall);
            }
        }
    }
    /// <summary>
    /// sends the json string form of an object to the specified client
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="JsonObject"></param>
    private void Send(Socket socket, String JsonObject)
    {        
        Networking.Send(socket, JsonObject + "\n");
    }
    /// <summary>
    /// Updates the world on ever frame
    /// </summary>
    private void UpdateWorld()
    {
        lock (serverWorld)
        {
            logic.movmentUpdates(serverWorld, movmentCommands,gameSettings); // update the positoin of the snake 
            logic.ReplenishPowerups(serverWorld, gameSettings);
            foreach (SocketState s in clients.Values)
            {
                logic.SnakeWrapAround(serverWorld.snakes[(int)s.ID], gameSettings, serverWorld);
            }
            foreach (Snake snake in serverWorld.snakes.Values)
            {
                if (snake.died) { snake.died = false;}
                if(snake.deathtimer == 0)
                {
                    snake.deathtimer = gameSettings.RespawnRate; // this will get smaller everyframe untill you respwan
                    snake.alive = true;

                    Snake newSnake = logic.SpawnSnake(snake.snake, snake.name, gameSettings);
                    foreach (Snake s in serverWorld.snakes.Values)
                    {
                        while (!goodSpaceForSnakes(newSnake))
                        {
                            newSnake = logic.SpawnSnake(snake.snake, snake.name, gameSettings);
                        }
                    }
                    serverWorld.snakes[snake.snake] = newSnake;
                }
                if(snake.alive == false)
                {
                    snake.deathtimer--;
                }
                if (logic.WallCollision(snake,gameSettings) || logic.SnakeCollisions(serverWorld,snake) || logic.SelfCollisions(snake))
                {
                    snake.died = true;
                    snake.alive = false;
                    snake.speed = 0;
                    
                }
            }
            foreach (Snake s in snakeToRemove.Values) // cleanup the deactivated objects
            {
                s.alive = false;
                s.dc = true;
                s.died = true;
                s.alive = false;
                sendWorld();  // if a snake disconects we need to send that to other clients
                serverWorld.snakes.Remove(s.snake);
                snakeToRemove.Remove(s.snake);
            }     
        }
    }
    /// <summary>
    /// Start accepting Tcp sockets connections from clients
    /// </summary>
    public void StartServer()
    {
        // This begins an "event loop"
        Networking.StartServer(NewClientConnected, 11000);
    }

    /// <summary>
    /// Method to be invoked by the networking library
    /// when a new client connects (see line 41)
    /// </summary>
    /// <param name="state">The SocketState representing the new client</param>
    private void NewClientConnected(SocketState state)
    {
        if (state.ErrorOccurred)
            return;

        // change the state's network action to the 
        // receive handler so we can process data when something
        // happens on the network
        namesSent.Add(state.ID, false);
        state.OnNetworkAction = ReceiveMessage;

        Networking.GetData(state);
    }
    /// <summary>
    /// Method to be invoked by the networking library
    /// when a network action occurs
    /// </summary>
    /// <param name="state"></param>
    private void ReceiveMessage(SocketState state)
    {
        // Remove the client if they aren't still connected
        if (state.ErrorOccurred)
        {
            RemoveClient(state.ID);
            return;
        }

        ProcessMessage(state);
        // Continue the event loop that receives messages from this client
        Networking.GetData(state);
    }

    /// <summary>
    /// Given the data that has arrived so far, 
    /// potentially from multiple receive operations, 
    /// determine if we have enough to make a complete message,
    /// and process it (print it and broadcast it to other clients).
    /// </summary>
    /// <param name="sender">The SocketState that represents the client</param>
    private void ProcessMessage(SocketState state)
    {
        lock (serverWorld)
        {
            string totalData = state.GetData();
           
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.
            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                if (!namesSent[state.ID])
                {
                    Send(state.TheSocket, state.ID.ToString());  // send clients ID
                    Send(state.TheSocket, serverWorld.size.ToString()); // send World size
                    SendStartUpFrame(state.TheSocket); // send walls


                    Snake s = logic.SpawnSnake((int)state.ID, p.Substring(0, p.Length - 1), gameSettings); // create snake at valid location
                    serverWorld.snakes.Add((int)state.ID, s);
                    clients.Add(state.ID, state);         //compleate the handshake and add the snake and the client to the world
                    namesSent[state.ID] = true;

                }
                else
                {
                    Processmovment(p.Substring(0, p.Length - 1), state);
                    // process movement caommands and ensure that movement data is syntactacly correct
                }
                state.data.Remove(0, p.Length);
            }
        }
    }

    private void Processmovment(string movmentCommand, SocketState client)
    {
        if (movmentCommand == "{\"moving\":\"none\"}")
        {
            return;
        }
        else if (movmentCommand == "{\"moving\":\"left\"}")
        {
            if (serverWorld.snakes[(int)client.ID].dir.X == 1)
            {
                return;
                
            }
         
            Vector2D left = new Vector2D(-1,0);
            if (logic.TurnsTooFast(serverWorld,left,(int)client.ID))
            {
                return;
            }
            movmentCommands.Remove((int)client.ID);
            movmentCommands.Add((int)client.ID, left);
        }
        else if(movmentCommand == "{\"moving\":\"right\"}")
        {
            if (serverWorld.snakes[(int)client.ID].dir.X == -1)
            {
                return;
                
            }
            Vector2D right = new Vector2D(1, 0);
            if (logic.TurnsTooFast(serverWorld, right, (int)client.ID))
            {
                return;
            }
                movmentCommands.Remove((int)client.ID);
            movmentCommands.Add((int)client.ID, right);
        }
        else if (movmentCommand == "{\"moving\":\"up\"}")
        {
            if (serverWorld.snakes[(int)client.ID].dir.Y == 1)
            {
                return;
                
            }
            Vector2D up = new Vector2D(0, -1);
            if (logic.TurnsTooFast(serverWorld, up, (int)client.ID))
            {
                return;
            }
            movmentCommands.Remove((int)client.ID);
            movmentCommands.Add((int)client.ID, up);
        }
        else if(movmentCommand == "{\"moving\":\"down\"}")
        {
            if (serverWorld.snakes[(int)client.ID].dir.Y == -1)
            {
                return;
                
            }
            Vector2D down = new Vector2D(0, 1);
            if (logic.TurnsTooFast(serverWorld, down, (int)client.ID))
            {
                return;
            }
            movmentCommands.Remove((int)client.ID);
            movmentCommands.Add((int)client.ID, down);
        }
    }

    /// <summary>
    /// Removes a client from the clients dictionary and adds to remove dictionary
    /// for a cleaner cleanup process
    /// </summary>
    /// <param name="id">The ID of the client</param>
    private void RemoveClient(long id)
    {
        lock (serverWorld)
        {
            snakeToRemove.Add(id, serverWorld.snakes[(int)id]);
            clients.Remove(id);
        }
    }
}