using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace theMap
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        [JsonProperty(PropertyName = "tank")]
        private int ID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;


        public Vector2D Location
        {
            get
            {
                return location;
            }
        }

        [JsonProperty(PropertyName = "bdir")]
        private Vector2D orientation;

        public Vector2D Orientation
        {
            get
            {
                return orientation;
            }
        }

        [JsonProperty(PropertyName = "tdir")]
        private Vector2D aiming = new Vector2D(0, -1);



        public Vector2D TurretDirection
        {
            get
            {
                return aiming;
            }

            set
            {
                aiming = value;
            }
        }

        [JsonProperty(PropertyName = "name")]
        private string name;

        //[JsonProperty(PropertyName = "hp")]
        // private int hitPoints = Constants.MaxHP;

        [JsonProperty(PropertyName = "score")]
        private int score = 0;

        [JsonProperty(PropertyName = "died")]
        private bool died = false;

        [JsonProperty(PropertyName = "dc")]
        private bool disconnected = false;

        [JsonProperty(PropertyName = "join")]
        private bool joined = false;

        public Tank()
        {

        }


        public static Tank Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Tank>(input);
        }
    }

}
