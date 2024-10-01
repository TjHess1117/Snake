using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SnakeGame;

namespace SnakeGame
{   

    public class Powerup
    {
        public int power {  get; set; }
        public Vector2D loc { get; set; }
        public bool died { get; set; }
        /// <summary>
        /// Snake game object powerup. This object will spawn at randomly and increase the snake's body
        /// </summary>
        /// <param name="power"></param>  an int representing the powerup's unique ID.
        /// <param name="loc"></param>  a Vector2D representing the location of the powerup.
        /// <param name="died"></param>  a bool indicating if the powerup "died" (was collected by a player) on this frame.
        /// The server will send the dead powerups only once.
        public Powerup(int power, Vector2D loc, bool died) 
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
        }
    }
}