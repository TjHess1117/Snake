using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace SnakeGame
{
    public class World
    {
        public event Action<World> WorldUpdated;

        public Dictionary<int, Snake> snakes;
        public Dictionary<int,Powerup> powerups;
        public Dictionary<int, Wall> walls;
        public int size;
        public int playerID;
        public bool dataTransfered = false;
        public int count = 0;
        /// <summary>
        /// The world represents the state of the game and concists of game obects
        /// </summary>
        public World() 
        {
            snakes = new Dictionary<int, Snake>();
            powerups = new Dictionary<int, Powerup>();
            walls = new Dictionary<int, Wall>();
        }
        /// <summary>
        /// The world represents the state of the game and concists of game obects. a secondary constructor for the server.
        /// </summary>
        public World(int size)
        {
            snakes = new Dictionary<int, Snake>();
            powerups = new Dictionary<int, Powerup>();
            walls = new Dictionary<int, Wall>();
            this.size = size;
        }
        /// <summary>
        /// creates the game objects of the world based on the json discription of the world
        /// </summary>
        /// <param name="JsonGameObject"></param>
        public void updateWorld(string JsonGameObject)
        {
            lock (this) {
                if (int.TryParse(JsonGameObject, out int x)) // world size or snakes ID
                {
                    if(count == 0)
                    {
                        count++;
                        playerID = x;
                    }
                    else if(count == 1)
                    {
                        size = x;
                    }
                }
                else
                {
                    JsonDocument doc = JsonDocument.Parse(JsonGameObject);
                    // the json is now most deffinitly a snake a power up or a wall
                    if (doc.RootElement.TryGetProperty("wall", out _)) // if the root element is a snake
                    {
                        Wall wall = doc.Deserialize<Wall>();
                        walls.Add(wall.wall, wall);
                    }
                    if (doc.RootElement.TryGetProperty("snake", out _))// if the root element is a wall
                    {
                        // check the qualifieres for the snake to be removed or just updated
                        bool removed = false;
                        Snake snake = doc.Deserialize<Snake>();
                        if (snake.join)
                        {
                            snakes.Add(snake.snake, snake);
                        }
                        if (snake.dc)
                        {
                            snakes.Remove(snake.snake);
                            removed = true;
                        }
                        if (!removed)
                        {
                            snakes[snake.snake] = snake;
                        }
                        dataTransfered = true;
                    }
                    if (doc.RootElement.TryGetProperty("power", out _))// if the root element is a powerup
                    {
                        // check if the powerup need to be removed.
                        Powerup powerup = doc.Deserialize<Powerup>();
                        if (!powerups.ContainsKey(powerup.power) && powerup.died == false)
                        {
                            powerups.Add(powerup.power,powerup);
                        }
                        if (powerup.died)
                        {
                            powerups.Remove(powerup.power);
                        }
                    }
                }
                WorldUpdated(this); // triggers the event the controller is subscribed to that will update the view
            }
        }
    }
}
