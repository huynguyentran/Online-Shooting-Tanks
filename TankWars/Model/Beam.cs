using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace theMap
{
    class Beam
    {


        [JsonProperty(PropertyName = "beam")]
        private int ID;

        [JsonProperty(PropertyName = "org")]
        private Vector2D shootLocaiton;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction;

        [JsonProperty(PropertyName = "owner")]
        private int playerID;

        public static Beam Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Beam>(input);
        }
    }
}
