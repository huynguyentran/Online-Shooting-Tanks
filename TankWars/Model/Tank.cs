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
            set
            {
                orientation = value;
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
            set { name = value; }
        }

        [JsonProperty(PropertyName = "hp")]
        private int hitPoints;

        [JsonIgnore]
        public int HitPoints
        {
            get { return hitPoints; }
            set { hitPoints = value; }
        }
        [JsonIgnore]
        private static uint maxHitPoints;

        [JsonIgnore]
        private static uint tankSize;
        [JsonIgnore]
        public static uint TankSize
        {
            get { return tankSize; }
        }
        [JsonIgnore]
        public static uint MaxHP
        {
        get{ return maxHitPoints; }
        }

        [JsonIgnore]
        private static uint tankSpeed;
        [JsonIgnore]
        public static uint TankSpeed
        {
            get { return tankSpeed; }
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
            set { score = value; }
        }

        [JsonProperty(PropertyName = "died")]
        private bool died = false;
        [JsonIgnore]
        public bool Died
        {
            get { return died; }
            set { died = value; }
        }

        [JsonProperty(PropertyName = "dc")]
        private bool disconnected = false;
        [JsonIgnore]
        public bool DC
        {
            get { return disconnected; }
            set { disconnected = value; }
        }

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

        [JsonIgnore]
        private float tankCoolDown = 0f;
        [JsonIgnore]
        public float TankCoolDown
        {
            get { return tankCoolDown; }
            set { tankCoolDown = value; }
        }

        [JsonIgnore]
        private float respawnCD = 0f;
        [JsonIgnore]
        public float RespawnCD
        {
            get { return respawnCD;}
            set { respawnCD = value; }
        }

        [JsonIgnore]
        private bool isHotPotato;
        [JsonIgnore]
        public bool IsHotPotato
        {
            get { return isHotPotato; }
            set { isHotPotato = value; }
        }

    

        public Tank(int stateID, string playerName, Vector2D _location)
        {
            ID = stateID;
            name = playerName;
            location = _location;
            orientation = new Vector2D(1, 0);
            hitPoints = (int)maxHitPoints;

        }

        public void UpdatingTank(ControlCommands cmd)
        {
            if (cmd.directionOfTank.Length()>= 0.05)
            {
                aiming = cmd.directionOfTank;
            }
     
        }

        public static void SetTankParam(uint _hitpoints, uint _size, uint _speed)
        {
            maxHitPoints =  _hitpoints;
            tankSize = _size;
            tankSpeed = _speed;
        }
    }

}
