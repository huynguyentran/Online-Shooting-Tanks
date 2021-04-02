using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace theMap
{
    public class Powerup
    {

        [JsonProperty(PropertyName = "power")]
        private int ID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "died")]
        private bool died = false;
        [JsonIgnore]
        public bool Died
        {
            get { return died; }
        }

        public static Powerup Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Powerup>(input);
        }
    }
}
