using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace theMap
{
    class Powerup
    {
        public static Powerup Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Powerup>(input);
        }
    }
}
