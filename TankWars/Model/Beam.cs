﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace theMap
{
    class Beam
    {
        public static Beam Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<Beam>(input);
        }
    }
}
