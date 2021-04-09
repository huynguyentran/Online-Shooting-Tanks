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

        [JsonProperty(PropertyName = "org")]
        private Vector2D shootLocaiton;

        public Vector2D origin
        {
            get { return shootLocaiton; }
        }

        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction;

         public Vector2D Direction
        {
            get { return direction; }
        }
        [JsonProperty(PropertyName = "owner")]
        private int playerID;

        /// <summary>
        /// Desrialize the beam object. 
        /// </summary>
        /// <param name="input">The string input by the server</param>
        /// <returns>The beam object</returns>
        public static Beam Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Beam>(input);
        }
    }
}
