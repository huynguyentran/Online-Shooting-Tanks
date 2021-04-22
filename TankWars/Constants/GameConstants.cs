using System;
using System.Collections.Generic;
using System.Xml;
using TankWars;



namespace Constants
{
    public class GameConstants
    {
        private readonly uint mapSize;
        public int Size
        {
            get { return (int)mapSize; }
        }

        private readonly uint frameRate;
        public int FrameRate
        {
            get { return (int)frameRate; }
        }

        private readonly uint respawnRate;
        public int RespawnRate
        {
            get { return (int)respawnRate; }
        }

        private readonly uint framePerShot;
        public int FramePerShot
        {
            get { return (int)framePerShot; }
        }

        private readonly uint shotCD;

        private readonly List<Tuple<Vector2D, Vector2D>> wallList;

        public List<Tuple<Vector2D, Vector2D>> WallList
        {
            get { return wallList; }
        }

        private readonly uint activePUs;
        public int ActivePUs
        {
            get { return (int)activePUs; }
        }

        private readonly uint puRespawn;

        public uint PURespawn
        {
            get { return puRespawn; }
        }

        private readonly uint tankHP;
        public uint TankHP
        {
            get { return tankHP; }
        }

        private readonly uint tankSize;
        public uint TankSize
        {
            get { return tankSize; }
        }

        private readonly uint tankSpeed;
        public uint TankSpeed
        {
            get { return tankSpeed; }
        }

        private readonly uint wallSize;
        public uint WallSize
        {
            get { return wallSize; }
        }
        private readonly uint projSpeed;
        public uint ProjSpeed
        {
            get { return projSpeed; }
        }

        private readonly bool gameMode;

        public bool GameMode
        {
            get { return gameMode; }
        }

        private readonly uint gameModeTimer;

        public uint GameModeTimer
        {
            get { return gameModeTimer; }
        }


        public GameConstants(string filepath)
        {

            wallList = new List<Tuple<Vector2D, Vector2D>>();
            //try
            //{

            using (XmlReader reader = XmlReader.Create(filepath))
            {
                //Queue: <-x <-y  

                double x = 0;
                bool hasY = false;
                double y = 0;
                bool hasX = false;
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
                                mapSize = uint.Parse(reader.Value);
                                break;
                            case "MSPerFrame":
                                reader.Read();
                                frameRate = uint.Parse(reader.Value);
                                break;
                            case "FramesPerShot":
                                reader.Read();
                                framePerShot = uint.Parse(reader.Value);
                                break;
                            case "RespawnRate":
                                reader.Read();
                                respawnRate = uint.Parse(reader.Value);
                                break;
                            case "x":
                                hasX = true;
                                reader.Read();
                                x = int.Parse(reader.Value);
                                break;
                            case "y":
                                hasY = true;
                                reader.Read();
                                y = int.Parse(reader.Value);
                                break;
                            case "MaxNumberOfActivePowerups":
                                reader.Read();
                                activePUs = uint.Parse(reader.Value);
                                break;
                            case "FramePerPowerUpRespawn":
                                reader.Read();
                                puRespawn = uint.Parse(reader.Value);
                                break;
                            case "TankHitPoints":
                                reader.Read();
                                tankHP = uint.Parse(reader.Value);
                                break;
                            case "TankSize":
                                reader.Read();
                                tankSize = uint.Parse(reader.Value);
                                break;
                            case "TankSpeed":
                                reader.Read();
                                tankSpeed= uint.Parse(reader.Value);
                                break;
                            case "WallSize":
                                reader.Read();
                                wallSize = uint.Parse(reader.Value);
                                break;
                            case "ProjectileSpeed":
                                reader.Read();
                                projSpeed = uint.Parse(reader.Value);
                                break;
                            case "HotPotatoGameMode":
                                reader.Read();
                                gameMode = bool.Parse(reader.Value);
                                break;
                            case "TimeForHotPotato":
                                reader.Read();
                                gameModeTimer = uint.Parse(reader.Value);
                                break;
                        }
                    }

                    else
                    {
                        switch (reader.Name)
                        {
                            case "p1":
                                if (!(hasX && hasY))
                                    throw new ArgumentException();
                                p1 = new Vector2D(x, y);
                                hasX = false;
                                hasY = false;
                                break;
                            case "p2":
                                if (!(hasX && hasY))
                                    throw new ArgumentException();
                                p2 = new Vector2D(x, y);
                                hasX = false;
                                hasY = false;
                                break;
                            case "Wall":
                                if (p1 == null || p2 == null)
                                    throw new ArgumentException();
                                if (!(p1.GetX() == p2.GetX() || p1.GetY() == p2.GetY()))
                                    throw new ArgumentException("Missing Equivalence: Either both xs or ys of a wall must match.");
                                else
                                {
                                    wallList.Add(new Tuple<Vector2D, Vector2D>(p1, p2));
                                    p1 = null;
                                    p2 = null;
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
