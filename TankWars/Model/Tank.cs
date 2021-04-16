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
        [JsonIgnore]
        public int TankID
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
            set
            { location = value; }
        }

        [JsonProperty(PropertyName = "bdir")]
        private Vector2D orientation;
        [JsonIgnore]
        public Vector2D Orientation
        {
            get
            {
                return orientation;
            }
        }

        [JsonProperty(PropertyName = "tdir")]
        private Vector2D aiming = new Vector2D(0, -1);
        [JsonIgnore]
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
        private string name = "default";
       
        [JsonIgnore]
        public string Name
        {
            get
            {
                return name;
            }
        }

        [JsonProperty(PropertyName = "hp")]
        private int hitPoints = 3;

        [JsonIgnore]
        public int HitPoints
        {
            get { return hitPoints; }
        }

        [JsonProperty(PropertyName = "score")]
        private int score = 0;


        [JsonIgnore]
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
            set { died = false; }
        }

        [JsonProperty(PropertyName = "dc")]
        private bool disconnected = false;

        [JsonProperty(PropertyName = "join")]
        private bool joined =true;
        [JsonIgnore]
        public bool Joined
        {
            get { return joined; }
            set { joined = value; }
        }

        [JsonIgnore]
        private int powerups = 0;
        [JsonIgnore]
        public int Powers
        {
            get { return powerups; }
            set { powerups = value; }
        }

        public Tank(long stateID, string playerName)
        {
            ID = (int)stateID;
            name = playerName;
            location = new Vector2D(0, 0);
            orientation = new Vector2D(1, 0);
        }

        public void UpdatingTank(ControlCommands cmd)
        {
            aiming = cmd.directionOfTank;
        }
    }

}
