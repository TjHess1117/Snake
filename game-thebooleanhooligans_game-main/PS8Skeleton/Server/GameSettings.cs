using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server
{
    [DataContract(Namespace = "")]
    internal class GameSettings
    {

        [DataMember]
        public int GrowthBuffer;

        [DataMember]
        public int Mode;

        [DataMember]
        public int MSPerFrame;

        [DataMember]
        public int UniverseSize;

        [DataMember]
        public int RespawnRate;

        [DataMember]
        public List<Wall> Walls;
    }
}
