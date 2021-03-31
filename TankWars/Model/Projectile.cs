using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace theMap
{
    class Projectile
    {
        [JsonProperty(PropertyName = "proj")]
        private int ID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "died")]
        private bool died = false;

        [JsonProperty(PropertyName = "owner")]
        private int playerID;

        public static Projectile Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Projectile>(input);
        }
    }
}
