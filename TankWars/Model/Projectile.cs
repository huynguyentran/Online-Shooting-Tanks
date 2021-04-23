using Newtonsoft.Json;
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

        private static int nextID = 0;
        private static object mutexProjID = new object();

        [JsonIgnore]
        public int ProjID
        {
            get { return ID; }
        }

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonIgnore]
        public Vector2D Location
        {
            get { return location; }
            set { location = value; }
        }

        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;
        [JsonIgnore]
        public Vector2D Orientation
        {
            get { return orientation; }

        }

        [JsonProperty(PropertyName = "died")]
        private bool died = false;
        [JsonIgnore]
        public bool Died
        {
            get { return died; }
            set { died = value; }
        }

        [JsonProperty(PropertyName = "owner")]
        private int playerID;
        [JsonIgnore]
        public int PlayerID
        {
            get { return playerID; }
        }

        private static uint projSpeed;
        public static uint ProjSpeed
        {
            get { return projSpeed; }
        }

        public Projectile(Vector2D _location, Vector2D _orientation, int _playerID)
        {
            location = _location;
            orientation = _orientation;
            playerID = _playerID;
            lock (mutexProjID)
            {
                ID = nextID;
                nextID++;
            }

        }
        public static void SetProjParam(uint speed)
        {
            projSpeed = speed;
        }

    }
}
