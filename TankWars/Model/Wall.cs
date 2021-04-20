using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// A class that represents wall object.
    /// </summary>
    /// 


    public class Wall
    {

        [JsonProperty(PropertyName = "wall")]
        private int ID;
        [JsonIgnore]
        public int WallID
        {
            get { return ID; }
        }

        [JsonProperty(PropertyName = "p1")]
        private Vector2D startpoint;
        [JsonIgnore]
        public Vector2D Start
        {
            get { return startpoint; }
        
        }

        [JsonProperty(PropertyName = "p2")]
        private Vector2D endpoint;

        [JsonIgnore]
        public Vector2D End
        {
            get { return endpoint; }
     
        }

        [JsonIgnore]
        private static uint wallSize;
        [JsonIgnore]
        public static uint WallSize
        {
            get { return wallSize; }
        }

        public Wall(Vector2D start,Vector2D end, int _id)
        {
            ID = _id;
            startpoint = start;
            endpoint = end;
        }


        public static void SetWallParam(uint _wallSize)
        {
            wallSize = _wallSize;
        }
    }

}
