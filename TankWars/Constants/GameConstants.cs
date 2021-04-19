using System;
using System.Collections.Generic;
using System.Xml;
using TankWars;



namespace Constants
{
    public class GameConstants
    {
        private readonly int mapSize;
        public int Size
        {
            get { return mapSize; }
        }

        private readonly int frameRate;
        public int FrameRate 
        {
            get { return frameRate; }
        }

        private readonly int respawnRate;
        public int RespawnRate
        {
            get { return respawnRate; }
        }

        private readonly int framePerShot;
        public int FramePerShot
        {
            get { return framePerShot; }
        }

        private readonly int shotCD;

        private readonly List<Tuple<Vector2D, Vector2D>> wallList ;

       public List<Tuple<Vector2D, Vector2D>> WallList
        {
            get { return wallList; }
        }

        private readonly int activePUs;
        public int ActivePUs
        {
            get { return activePUs; }
        }

        private readonly int puRespawn;

        public int PURespawn
        {
            get { return puRespawn; }
        }

        public GameConstants(string filepath)
        {

            wallList = new List<Tuple<Vector2D, Vector2D>>();
            //try
            //{

                using (XmlReader reader = XmlReader.Create(filepath))
                {
                    double x = 0;
                    double y = 0;
                    Vector2D p1 = null;
                    Vector2D p2 = null;
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "GameSettings":
                                    continue;
                                case "UniverseSize":
                                    reader.Read();
                                    mapSize = int.Parse(reader.Value);
                                    break;
                                case "MSPerFrame":
                                    reader.Read();
                                    frameRate = int.Parse(reader.Value);
                                    break;
                                case "FramesPerShot":
                                    reader.Read();
                                    framePerShot = int.Parse(reader.Value);
                                    break;
                                case "RespawnRate":
                                    reader.Read();
                                    respawnRate = int.Parse(reader.Value);
                                    break;
                                case "x":
                                    reader.Read();
                                    x = int.Parse(reader.Value);
                                    break;
                                case "y":
                                    reader.Read();
                                    y =int.Parse(reader.Value);
                                    break;
                                case "MaxNumberOfActivePowerups":
                                    reader.Read();
                                    activePUs = int.Parse(reader.Value);
                                    break;
                                case "FramePerPowerUpRespawn":
                                    reader.Read();
                                    puRespawn = int.Parse(reader.Value);
                                    break;
                            }
                        }

                        else
                        {
                            switch (reader.Name)
                            {
                                case "p1":
                                    p1 = new Vector2D(x, y);
                                    break;
                                case "p2":
                                    p2 = new Vector2D(x, y);
                                    break;
                                case "Wall":
                                    if (p1 == null || p2 == null)
                                        throw new ArgumentException();
                                    else
                                    {
                                        wallList.Add(new Tuple<Vector2D, Vector2D>(p1, p2));
                                    }
                                    break;
                            }
                        }
               
                    }
                }
            //}
            //catch
            //{

            //}
        }

        
    }
}
