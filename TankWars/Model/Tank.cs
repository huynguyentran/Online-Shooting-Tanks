using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// A class that represents the tank object.
    /// </summary>
    public class Tank
    {
        [JsonProperty(PropertyName = "tank")]
        private int ID;
        public int TankID
        {
            get { return ID; }
        }

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

        public string Name
        {
            get
            {
                return name;
            }
        }

        [JsonProperty(PropertyName = "hp")]
        private int hitPoints = 0;
        public int HitPoints
        {
            get { return hitPoints; }
        }

        [JsonProperty(PropertyName = "score")]
        private int score = 0;

        public int Score
        {
            get
            {
                return score;
            }
        }

        [JsonProperty(PropertyName = "died")]
        private bool died = false;
        [JsonIgnore]
        public bool Died
        {
            get { return died; }
        }

        [JsonProperty(PropertyName = "dc")]
        private bool disconnected = false;

        [JsonProperty(PropertyName = "join")]
        private bool joined = false;

        /// <summary>
        /// Desrialize the tank object. 
        /// </summary>
        /// <param name="input">The string input by the server</param>
        /// <returns>The tank object</returns>
        public static Tank Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Tank>(input);
        }

        public Tank(long stateID, string playerName)
        {
            ID = (int)stateID;
            name = playerName;
        }

        public void UpdatingTank(ControlCommands cmd)
        {
            aiming = cmd.directionOfTank;
        }
    }

}
