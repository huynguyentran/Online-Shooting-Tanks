using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace theMap
{
    class Wall
    {

        [JsonProperty(PropertyName = "wall")]
        private int ID;

        [JsonProperty(PropertyName = "p1")]
        private Vector2D startpoint;

        [JsonProperty(PropertyName = "p2")]
        private Vector2D endpoint;

        public static Wall Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Wall>(input);
        }
    }

}
