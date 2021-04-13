using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// A class that represents wall object.
    /// </summary>
    public class Wall
    {

        [JsonProperty(PropertyName = "wall")]
        private int ID;

        [JsonProperty(PropertyName = "p1")]
        private Vector2D startpoint;

        public Vector2D Start
        {
            get { return startpoint; }
        }

        [JsonProperty(PropertyName = "p2")]
        private Vector2D endpoint;

        public Vector2D End
        {
            get { return endpoint; }
        }


    }

}
