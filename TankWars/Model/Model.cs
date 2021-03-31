using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Model
{
    public class Model
    {

        //private Dictionary<int, string> playerlist;


        private HashSet<int> IDs;

        private Dictionary<int, Tank> tanks;

        private Dictionary<int, Projectile> projectiles;

        private Dictionary<int, Powerup> powerups;

        private Dictionary<int, Wall> walls;

        private Dictionary<int, Beam> beams;

        private int size;

        public Model(int _size)
        {
            size = _size;
        }


        public void Deserialize(string input)
        {
            JObject gObj = JObject.Parse(input);

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


}
