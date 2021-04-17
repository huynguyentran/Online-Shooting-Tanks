using System;

namespace Constants
{
    public class GameConstants
    {
        private readonly int mapSize = 250;
        public int Size
        {
            get { return mapSize; }
        }

        private readonly int frameRate = 17;
        public int FrameRate 
        {
            get { return frameRate; }
        }

        private readonly int respawnRate = 5;
        public int RespawnRate
        {
            get { return respawnRate; }
        }
        public GameConstants()
        {

        }

        
    }
}
