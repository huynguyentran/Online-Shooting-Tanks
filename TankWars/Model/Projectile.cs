using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    public class Projectile
    {
        [JsonProperty(PropertyName = "proj")]
        private int ID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonIgnore]
        public Vector2D Location
        {
            get { return location; }
        }

        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;
        [JsonIgnore]
        public Vector2D Orientation
        {
            get { return orientation; }
        }


        [JsonProperty(PropertyName = "died")]
        private bool died = false;
        [JsonIgnore]
        public bool Died
        {
            get { return died; }
        }

        [JsonProperty(PropertyName = "owner")]
        private int playerID;
        [JsonIgnore]
        public int PlayerID
        {
            get { return playerID; }
        }

        public static Projectile Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Projectile>(input);
        }
    }
}
