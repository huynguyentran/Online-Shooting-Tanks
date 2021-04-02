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


        public Tuple<bool, object> DeserializeGameObject(string input)
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
                    int id = gObj.Value<int>("tank");
                    if (!t.Died && t.HitPoints > 0)
                        tanks[id] = t;
                    else
                        if (tanks.ContainsKey(id))
                            tanks.Remove(id);
                    return new Tuple<bool, object>(t.Died, t);
                }
                else if (gObj["wall"] != null)
                {
                    Wall w = Wall.Deserialize(input);
                    walls[gObj.Value<int>("wall")] = w;
                }
                else if (gObj["proj"] != null)
                {
                    Projectile pr = Projectile.Deserialize(input);
                    int id = gObj.Value<int>("proj");
                    if (!pr.Died)
                        projectiles[id] = pr;
                    else
                        projectiles.Remove(id);
                    return new Tuple<bool, object>(pr.Died, pr);
                }
                else if (gObj["power"] != null)
                {
                    Powerup pu = Powerup.Deserialize(input);
                    int id = gObj.Value<int>("power");
                    if (!pu.Died)
                        powerups[id] = pu;
                    else
                        powerups.Remove(id);
                    return new Tuple<bool, object>(pu.Died, pu);
                }
                else if (gObj["beam"] != null)
                {
                    Beam b = Beam.Deserialize(input);
                    beams[gObj.Value<int>("beam")] = b;
                }
                else
                    throw new ArgumentException("Unrecognized game object received: " + input);
            }

            return new Tuple<bool, object>(false, null);
        }

        public void SetSize(int newSize)
        {
            size = newSize;
        }
    }


}
