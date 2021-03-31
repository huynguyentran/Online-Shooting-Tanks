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

        private Dictionary<int, Wall> wall;

        private int size;

        public Model(int _size)
        {
            size = _size;
        }


        public object Deserialize(string input)
        {
            JObject gObj = JObject.Parse(input);
            if (gObj["tank"] != null)
                Tank.Deserialize(input);
            else if (gObj["wall"] != null)
                Wall.Deserialize(trimmedPart);
            else if (gObj["proj"] != null)
                Projectile.Deserialize(trimmedPart);
            else if (gObj["power"] != null)
                Powerup.Deserialize(trimmedPart);
            else if (gObj["beam"] != null)
                Beam.Deserialize(trimmedPart);
            else
                throw new ArgumentException("Unrecognized game object received: " + trimmedPart);

        }
    }


}
