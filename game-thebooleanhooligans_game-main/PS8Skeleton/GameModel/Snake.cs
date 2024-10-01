using SnakeGame;
using System.Xml.Linq;
namespace SnakeGame
{

    public class Snake
    {
        public int snake { get; set; } // int representing the snakes ID
        public string name { get; set; } // string representing the players name
        public List<Vector2D> body { get; set; } // a List<Vector2D> representing the entire body of the snake.
        public Vector2D dir { get; set; }
        public int score { get; set; }
        public bool died { get; set; }
        public bool alive { get; set; }
        public bool dc {  get; set; }
        public bool join {  get; set; }
        public int deathtimer { get; set; }
        public int speed { get; set; }
        public int growthBuffer { get; set; } 
        public int movmentCommandBuffer {  get; set; }
        /// <summary>
        /// Constructor for snake that will include all of the json data that will be sent and recieved through the server.
        /// paramaters will provide important paramaters that determine score, name, direction, body and etc.
        /// </summary>
        /// <param name="snake"></param> unique snake ID
        /// <param name="name"></param> string representation of the snakes name
        /// <param name="body"></param> representation of the snake body as a whole
        /// <param name="score"></param> score of the player
        /// <param name="died"></param> boolean to determine if snake is dead
        /// <param name="alive"></param> boolean to determin if snake is alive
        /// <param name="dc"></param>   boolean indicating if the snake has disconnected from the server
        /// <param name="join"></param> boolean indicating if the snake has joined the server
        ///  <param name="dir"></param> 
        public Snake(int snake, string name, List<Vector2D> body,Vector2D dir, int score, bool died, bool alive, bool dc, bool join) // TODO: compleat the snake constructor
        {
            this.snake = snake;
            this.name = name;   
            this.body = body;
            this.score = score;
            this.died = died;
            this.alive = alive;
            this.dc = dc;
            this.dir = dir;
            this.join = join;
            speed = 5;
            deathtimer = 20;
            growthBuffer = 0;
        }

    }
}
