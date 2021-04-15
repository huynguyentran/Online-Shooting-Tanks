using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// A class represents a Beam object. 
    /// </summary>
    public class Beam
    {

        [JsonProperty(PropertyName = "beam")]
        private int ID;
        [JsonIgnore]
        private static object mutexBeamID;
        [JsonIgnore]
        private static int nextID = 0;
        [JsonIgnore]
        public int BeamID
        {
            get { return ID; }
            set { ID = value; }
        }

        [JsonProperty(PropertyName = "org")]
        private Vector2D shootLocaiton;
        [JsonIgnore]
        public Vector2D origin
        {
            get { return shootLocaiton; }
        }

        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction;
        [JsonIgnore]
        public Vector2D Direction
        {
            get { return direction; }
        }
        [JsonProperty(PropertyName = "owner")]
        private int playerID;

        public Beam(Vector2D _origin, Vector2D _direction, int _playerID)
        {
            shootLocaiton  = _origin;
            direction = _direction;
            playerID = _playerID;

            lock (mutexBeamID)
            {
                ID = nextID;
                nextID++;
            }
          
        }
    }
}
