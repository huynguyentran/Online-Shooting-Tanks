﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// A class that represents the projectile.
    /// </summary>
    public class Projectile
    {
        [JsonProperty(PropertyName = "proj")]
        private int ID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonIgnore]
        public Vector2D Location
        {
            get { return location; }
        }

        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;
        [JsonIgnore]
        public Vector2D Orientation
        {
            get { return orientation; }
        }

        /// <summary>
        /// Desrialize the projectile object. 
        /// </summary>
        /// <param name="input">The string input by the server</param>
        /// <returns>The projectile object</returns>
        [JsonProperty(PropertyName = "died")]
        private bool died = false;
        [JsonIgnore]
        public bool Died
        {
            get { return died; }
        }

        [JsonProperty(PropertyName = "owner")]
        private int playerID;
        [JsonIgnore]
        public int PlayerID
        {
            get { return playerID; }
        }


    }
}
