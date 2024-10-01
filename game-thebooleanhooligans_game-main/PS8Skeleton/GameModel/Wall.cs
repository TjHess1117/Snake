using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SnakeGame;
namespace SnakeGame
{
    [DataContract(Name = "Wall", Namespace = "")]
    public class Wall
    {
       [DataMember (Name = "ID")]
        public int wall { get; set; }
        [DataMember]
        public Vector2D p1 { get; set; }
        [DataMember]
        public Vector2D p2 { get; set; }
        /// <summary>
        /// represents a wall using p1 and p2.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Wall(int wall, Vector2D p1, Vector2D p2)
        {
            this.wall = wall;
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
