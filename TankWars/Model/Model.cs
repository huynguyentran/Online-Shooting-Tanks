using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace theMap
{
    public class Model
    {

        //private Dictionary<int, string> playerlist;


        private HashSet<int> IDs;

        private Dictionary<int, Tank> tanks;
        public Dictionary<int, Tank> Tanks { get { return tanks; } }

        private Dictionary<int, Projectile> projectiles;
        public Dictionary<int, Projectile> Projectiles { get { return projectiles; } }

        private Dictionary<int, Powerup> powerups;
        public Dictionary<int, Powerup> Powerups { get { return powerups; } }

        private Dictionary<int, Wall> walls;
        public Dictionary<int, Wall> Walls { get { return walls; } }

        private Dictionary<int, Beam> beams;
        public Dictionary<int, Beam> Beams { get { return beams; } }


        private int playerID;

        public int clientID
        {
            get
            {
                return playerID;
            }

            set
            {
                playerID = value;
            }
        }

        private int size;

        public Model(int _size)
        {
            size = _size;

            tanks = new Dictionary<int, Tank>();
            projectiles = new Dictionary<int, Projectile>();
            powerups = new Dictionary<int, Powerup>();
            walls = new Dictionary<int, Wall>();
            beams = new Dictionary<int, Beam>();
        }


        public void DeserializeGameObject(string input)
        {
            JObject gObj = JObject.Parse(input);

            /*
             * May be slow because it forces the Model to wait for the frame to finish redering to start loading new objects.
             * Consider replacing with individual lacks for each if statement.
             */
            lock (this)
            {
                if (gObj["tank"] != null)
                {
                    Tank t = Tank.Deserialize(input);
                    //Ensure this is the right way to get a intance variable from JSON.
                    tanks[gObj.Value<int>("tank")] = t;
                }
                else if (gObj["wall"] != null)
                {
                    Wall w = Wall.Deserialize(input);
                    //Ensure this is the right way to get a intance variable from JSON.
                    walls[gObj.Value<int>("wall")] = w;
                }
                else if (gObj["proj"] != null)
                {
                    Projectile pr = Projectile.Deserialize(input);
                    //Ensure this is the right way to get a intance variable from JSON.
                    projectiles[gObj.Value<int>("proj")] = pr;
                }
                else if (gObj["power"] != null)
                {
                    Powerup pu = Powerup.Deserialize(input);
                    //Ensure this is the right way to get a intance variable from JSON.
                    powerups[gObj.Value<int>("power")] = pu;
                }
                else if (gObj["beam"] != null)
                {
                    Beam b = Beam.Deserialize(input);
                    //Ensure this is the right way to get a intance variable from JSON.
                    beams[gObj.Value<int>("beam")] = b;
                }
                else
                    throw new ArgumentException("Unrecognized game object received: " + input);
            }
        }

        public void SetSize(int newSize)
        {
            size = newSize;
        }
    }


}
