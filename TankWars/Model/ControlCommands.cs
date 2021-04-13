using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TankWars;
using Newtonsoft.Json.Linq;

namespace Model
{
    /// <summary>
    /// Control Command class that send the JSon requests to the server.
    /// For the tank, there are 3 kind of request: The direction of the turret,
    /// the tank's movement, and the tank's fire type.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommands
    {
        public ControlCommands()
        {
            movement = "none";
            fireType = "none";
            direction = new Vector2D(0, 1);
        }

        /// <summary>
        /// Json of the tank's movement with getter and setter for easy modification
        /// </summary>
        [JsonProperty(PropertyName = "moving")]
        private string movement;
        [JsonIgnore]
        public string Move
        {
            get
            {
                return movement;
            }
            set
            {
                movement = value;
            }
        }

        /// <summary>
        /// Json of the tank's fire type with getter and setter for easy modification
        /// </summary>
        [JsonProperty(PropertyName = "fire")]
        private string fireType;
        [JsonIgnore]
        public string Fire
        {
            get
            {
                return fireType;
            }
            set
            {
                fireType = value;
            }
        }

        /// <summary>
        /// Json of the direction of the tank with getter and setter for easy modification
        /// </summary>
        [JsonProperty(PropertyName = "tdir")]
        private Vector2D direction;
        [JsonIgnore]
        public Vector2D directionOfTank
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
            }
        }

    

    }




}
