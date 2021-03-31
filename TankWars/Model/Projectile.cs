using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    class Projectile
    {
        public static Projectile Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Projectile>(input);
        }
    }
}
