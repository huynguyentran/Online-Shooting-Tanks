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
        public GameConstants()
        {

        }

        
    }
}
