﻿using System;

namespace Constants
{
    public class GameConstants
    {
        private readonly int mapSize = 2000;
        public int Size
        {
            get { return mapSize; }
        }

        private readonly int frameRate;
        public int FrameRate
        {
            get { return frameRate; }
        }
        public GameConstants()
        {

        }

        
    }
}
