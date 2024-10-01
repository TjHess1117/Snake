using NetworkUtil;
using System.Text.RegularExpressions;
namespace SnakeGame
{
    public class GameController
    {
        public event Action<World> updateView;
        public event Action<string> ErrorOccurred;
        private SocketState server;
        private World gameWorld;
        private string snakeName; // once we connect we need to send the name so we will store it here


        /// <summary>
        /// default constructor to create a game world and subscribe to events
        /// </summary>
        public GameController() 
        {
            gameWorld = new World(); // ask controller what the universe size is
            gameWorld.WorldUpdated += TellViewWorldIsUpdated;
        }
        /// <summary>
        /// informes the view the world is up to date and needs to draw all game objects
        /// </summary>
        /// <param name="gameObjects"></param>
        private void TellViewWorldIsUpdated(World gameWorld)
        {
            lock(gameWorld) // need to lock the world because it is shared among the classes
            {
                updateView(gameWorld);
            }
        }
        /// <summary>
        /// connects to the server on the correct port
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="snakeName"></param>
        public void Connect(string hostName,string snakeName)
        {
            this.snakeName = snakeName;
            Networking.ConnectToServer(OnNetworkAction,hostName,11000); // this is running on a different thread becaues it is an asyncronos call
        }
        /// <summary>
        /// sends the users name starting the handshake prosess 
        /// and starts to ask For data
        /// </summary>
        /// <param name="socketState"></param>
        private void OnNetworkAction(SocketState socketState)
        {
            //Upon connection, send a single '\n' terminated string representing
            //the player's name. The name should be no longer than 16 characters
            //(not including the newline).
            server = socketState;
            Networking.Send(server.TheSocket, snakeName + "\n"); // starts the asycrones send acrosse the socket sending the name to start the handshake
            server.OnNetworkAction = ReciveMessage;
            Networking.GetData(server);
        }
        /// <summary>
        /// recives information from the server 
        /// </summary>
        /// <param name="state"></param>
        private void ReciveMessage(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                ErrorOccurred(state.ErrorMessage);
                return;
            }
            ProcessMessages(state);

            // Continue the event loop
            // state.OnNetworkAction has not been changed, 
            // so this same method (ReceiveMessage) 
            // will be invoked when more data arrives
            Networking.GetData(state);
        }
        /// <summary>
        /// Process any buffered messages separated by '\n'
        /// Display them, then remove them from the buffer.
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state)
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


                // triggers the event and every project who is subscribed
                // will trigger the event with the message p this will likely
                // invoke a method in the modle andupdate the modle then the modle will
                // tell the view to redraw.
                lock (gameWorld)
                {
                    gameWorld.updateWorld(p);
                }

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }
        }

        

        //===============================Movment send oporations==========================
        // each of the oporaios just sends the appropriaate json string.

        /// <summary>
        /// this is the event handeler for when move up is pressed
        /// </summary>
        public void OnMoveUp()
        {
            if(gameWorld.dataTransfered == true)
                Networking.Send(server.TheSocket, "{\"moving\":\"up\"}\n");
        }
        /// <summary>
        /// this is the event handeler for when move right is pressed
        /// </summary>
        public void OnMoveRight()
        {
            if (gameWorld.dataTransfered == true)
                Networking.Send(server.TheSocket, "{\"moving\":\"right\"}\n");
        }
        /// <summary>
        /// this is the event handeler for when move down is pressed
        /// </summary>
        public void OnMoveDown()
        {
            if (gameWorld.dataTransfered == true)
                Networking.Send(server.TheSocket, "{\"moving\":\"down\"}\n");
        }
        /// <summary>
        /// this is the event handeler for when move Left is pressed
        /// </summary>
        public void OnMoveLeft()
        {
            if (gameWorld.dataTransfered == true)
                Networking.Send(server.TheSocket, "{\"moving\":\"left\"}\n");
        }
        /// <summary>
        /// this is the event handeler for when no movement is asked
        /// </summary>
        public void OnNoMovement()
        {
            if (gameWorld.dataTransfered == true)
                Networking.Send(server.TheSocket, "{\"moving\":\"none\"}\n");
        }

    }
}
