using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// A class that represents the powerup. 
    /// </summary>
    public class Powerup
    {
        private static int nextID = 0;
        private static object mutexPowerupID = new object();

        [JsonProperty(PropertyName = "power")]
        private int ID;
        [JsonIgnore]
        public int puID
        {
            get { return ID; }
        }
      
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;
        [JsonIgnore]
        public Vector2D Location
        {
            get
            {
                return location;
            }
        }

        [JsonProperty(PropertyName = "died")]
        private bool died = false;
        [JsonIgnore]
        public bool Died
        {
            get { return died; }
            set { died = value; }
        }

        public Powerup(Vector2D loc)
        {
            location = loc;
            lock (mutexPowerupID)
            {
                ID = nextID;
                nextID++;
            }
        }
 
    }
}
