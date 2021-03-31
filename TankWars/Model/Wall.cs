using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace theMap
{
    class Wall
    {
        public static Wall Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Wall>(input);
        }
    }

}
