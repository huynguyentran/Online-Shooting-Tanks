using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TankWars;
using Constants;

namespace Model
{
    /// <summary>
    /// The model that holds all the information of the game objects. 
    /// The model will aslo Deserialize the information send by the server.
    /// </summary>

    /// <summary>
    ///  The model of the 2D Tank Wars game. 
    /// </summary>
    /// <author>Huy Nguyen</author>
    /// <author>William Erignac</author>
    /// <version>04/09/2021</version>
    public class GameModel
    {
        /// <summary>
        /// A HashSet of player IDs. The player IDs should be unique.
        /// </summary>
        private HashSet<int> IDs;

        /// <summary>
        /// A group of dictionaries for each different type of the game objects. 
        /// </summary>

        //A dictionary for tanks and their ids.
        private Dictionary<int, Tank> tanks;
        public Dictionary<int, Tank> Tanks { get { return tanks; } }

        ///A dictionary for projectiles and their ids.
        private Dictionary<int, Projectile> projectiles;
        public Dictionary<int, Projectile> Projectiles { get { return projectiles; } }

        //A dictionary for powerups and thier ids.
        private Dictionary<int, Powerup> powerups;
        public Dictionary<int, Powerup> Powerups { get { return powerups; } }

        //A dictionary for walls and their ids.
        private Dictionary<int, Wall> walls;
        public Dictionary<int, Wall> Walls { get { return walls; } }

        private GameConstants gameConstants;

        private readonly uint maxNumberOfActivePowerups;
        private readonly uint maxPowerupRespawnFrames;
        private float timeUntilNextPowerupRespawn;
        private readonly float numOfFramePerSec;

        private readonly bool hotPotatoGameMode;

        /// <summary>
        /// The age of the current potato (how long it has lived).
        /// </summary>
        private float potatoAge;

        /// <summary>
        /// How long the potato waits before detonating.
        /// </summary>
        private float potatoLifetime;

        private Tank hotPotatoTank;

        private float timerTilNextMatch = 5f;

        //The intial client player ID, -1 is an invalid number.
        private int playerID = -1;
        public int clientID
        {
            get
            {
                return playerID;
            }

            set
            {
                playerID = value;
            }
        }

        public void AddTank(int stateID, string clientName)
        {
            bool checkCollision;
            Vector2D loc;
            do
            {
                Random rnd = new Random();
                int VecX = rnd.Next(-(MapSize / 2), MapSize / 2);
                int VecY = rnd.Next(-(MapSize / 2), MapSize / 2);
                loc = new Vector2D(VecX, VecY);
                checkCollision = false;
                foreach (Wall w in walls.Values)
                {
                    if (WallCollisionCheck(30, w, loc))
                    {
                        checkCollision = true;
                    }
                }

            }
            while (checkCollision);

            Tank tank = new Tank(stateID, clientName, loc);
            tanks[tank.TankID] = tank;
        }

        //The map size 
        private int size;
        public int MapSize
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        /// <summary>
        /// A construcor that intialize when the information is sent by the server. 
        /// Assigns the map size of the tank war game.
        /// </summary>
        /// <param name="_size">The map size that sent by the server</param>
        public GameModel(int _size, GameConstants _const)
        {
            size = _size;
            tanks = new Dictionary<int, Tank>();
            projectiles = new Dictionary<int, Projectile>();
            powerups = new Dictionary<int, Powerup>();
            walls = new Dictionary<int, Wall>();
            gameConstants = _const;
            foreach (Tuple<Vector2D, Vector2D> points in _const.WallList)
            {

                Wall wall = new Wall(points.Item1, points.Item2, walls.Count);
                walls.Add(wall.WallID, wall);
            }

            numOfFramePerSec = 1000 / (gameConstants.FrameRate);
            maxNumberOfActivePowerups = (uint)gameConstants.ActivePUs;
            maxPowerupRespawnFrames = gameConstants.PURespawn;
            Tank.SetTankParam(gameConstants.TankHP, gameConstants.TankSize, gameConstants.TankSpeed);
            Wall.SetWallParam(gameConstants.WallSize);
            Projectile.SetProjParam(gameConstants.ProjSpeed);
            hotPotatoGameMode = gameConstants.GameMode;
            potatoLifetime = gameConstants.GameModeTimer;
            if (hotPotatoGameMode == true)
            {
                maxNumberOfActivePowerups = 0;
            }


        }

        public GameModel(int _size)
        {


            size = _size;
            tanks = new Dictionary<int, Tank>();
            projectiles = new Dictionary<int, Projectile>();
            powerups = new Dictionary<int, Powerup>();
            walls = new Dictionary<int, Wall>();


        }

        public void AddingToDictionary(object obj, int id)
        {
            lock (this)
            {
                if (obj is Tank t)
                {
                    //If the tank is not died.
                    if (!t.Died && t.HitPoints > 0)
                        //Register the tank to the list and keep drawing the tank.
                        tanks[id] = t;
                    else
                   //We temporary remove the tank from the list so that the tank disappears on the screen.
                   if (tanks.ContainsKey(id))
                        tanks.Remove(id);
                }
                else if (obj is Wall w)
                {
                    walls[id] = w;
                }
                else if (obj is Projectile pr)
                {
                    //If the projectile is not died.
                    if (!pr.Died)
                        //Register the projectile to the list and keep drawing the projectile
                        projectiles[id] = pr;
                    else
                        //Remove the projectile from the list.
                        projectiles.Remove(id);
                }
                else if (obj is Powerup pu)
                {
                    if (!pu.Died)
                        //Register the powerup to the list and keep drawing the powerup.
                        powerups[id] = pu;
                    else
                        //Remove the powerup from the list.
                        powerups.Remove(id);
                }
            }


        }

        private void UpdateTankMovement(Tank t, ControlCommands cmd, float deltaTime)
        {
            t.UpdatingTurretDirection(cmd);
            Vector2D movementDirection;
            switch (cmd.Move)
            {
                case "up":
                    {
                        movementDirection = new Vector2D(0, -1);

                        break;
                    }
                case "down":
                    {
                        movementDirection = new Vector2D(0, 1);
                        break;
                    }
                case "left":
                    {
                        movementDirection = new Vector2D(-1, 0);
                        break;
                    }
                case "right":
                    {
                        movementDirection = new Vector2D(1, 0);
                        break;
                    }
                default:
                    {
                        movementDirection = new Vector2D(0, 0);
                        break;
                    }
            }

            if (movementDirection.Length() != 0)
            {
                t.Orientation = movementDirection;
            }


            //We need to look at the Constants object (and possibly the time passed).
            double speed = (Tank.TankSpeed * numOfFramePerSec) * deltaTime; //Velocity * Time passed between frames
            movementDirection *= speed;

            Vector2D expectedLocation = t.Location + movementDirection;

            if (expectedLocation.GetX() < -(MapSize / 2))
            {
                expectedLocation = new Vector2D((MapSize / 2) + (expectedLocation.GetX() + MapSize / 2), expectedLocation.GetY());
            }
            if (expectedLocation.GetX() > MapSize / 2)
            {
                expectedLocation = new Vector2D(-(MapSize / 2) + (expectedLocation.GetX() - MapSize / 2), expectedLocation.GetY());
            }

            if (expectedLocation.GetY() > MapSize / 2)
            {
                expectedLocation = new Vector2D(expectedLocation.GetX(), -(MapSize / 2) + (expectedLocation.GetY() - MapSize / 2));
            }

            if (expectedLocation.GetY() < -(MapSize / 2))
            {
                expectedLocation = new Vector2D(expectedLocation.GetX(), (MapSize / 2) + (expectedLocation.GetY() + MapSize / 2));
            }

            foreach (Wall wall in walls.Values)
            {
                if (WallCollisionCheck((int)Tank.TankSize / 2, wall, expectedLocation))
                {
                    expectedLocation = t.Location;
                    break;
                }
            }



            t.Location = expectedLocation;
        }


        public Beam UpdateTank(int id, ControlCommands cmd, float deltaTime)
        {

            Tank t = tanks[id];
            if (t.Died == false && t.HitPoints > 0)
            {

                UpdateTankMovement(t, cmd, deltaTime);

                //Collision  tanks and powerups 
                foreach (Powerup powerup in Powerups.Values)
                {
                    if (!powerup.Died)
                    {
                        bool collision = (powerup.Location - t.Location).Length() <= (Tank.TankSize / 2);

                        if (collision)
                        {
                            t.Powers++;
                            powerup.Died = true;

                        }
                    }

                }

                if (t.TankCoolDown > 0)
                {
                    t.TankCoolDown -= deltaTime;
                }

                cmd.directionOfTank.Normalize();

                switch (cmd.Fire)
                {
                    case "main":
                        {
                            if (t.TankCoolDown <= 0 && (t.IsHotPotato || !hotPotatoGameMode))
                            {
                                Vector2D projetileDir = t.TurretDirection;
                                Projectile newProjectile = new Projectile(t.Location + projetileDir * (Tank.TankSize / 2), projetileDir, t.TankID);
                                projectiles[newProjectile.ProjID] = newProjectile;
                                t.TankCoolDown = gameConstants.FramePerShot / numOfFramePerSec; // Constant fire rate
                            }

                            break;
                        }
                    case "alt":
                        {
                            if (t.Powers > 0 && !hotPotatoGameMode)
                            {

                                Vector2D beamDir = t.TurretDirection;
                                Beam b = new Beam(t.Location + beamDir * (Tank.TankSize / 2), beamDir, t.TankID);
                                t.Powers--;
                                return b;
                            }
                            break;
                        }
                    default:
                        break;
                }
                return null;
            }
            else
            {
                if (!hotPotatoGameMode)
                {
                    if (t.RespawnCD > 0)
                    {
                        t.RespawnCD -= deltaTime;
                    }
                    else
                    {
                        RespawningTank(t);

                    }
                }

            }
            return null;
        }

        private void RespawningTank(Tank t)
        {
            t.HitPoints = (int)Tank.MaxHP;
            bool checkCollision;
            Vector2D loc;
            do
            {
                Random rnd = new Random();
                int VecX = rnd.Next(-(MapSize / 2), MapSize / 2);
                int VecY = rnd.Next(-(MapSize / 2), MapSize / 2);
                loc = new Vector2D(VecX, VecY);
                checkCollision = false;
                foreach (Wall w in walls.Values)
                {
                    if (WallCollisionCheck((int)Tank.TankSize / 2, w, loc))
                    {
                        checkCollision = true;
                    }
                }

            }
            while (checkCollision);
            t.Location = loc;
        }



        public IList<Beam> UpdatingWorld(IEnumerable<KeyValuePair<int, ControlCommands>> clientsInfo, float deltaTime)
        {
            List<Beam> beams = new List<Beam>();

            foreach (KeyValuePair<int, ControlCommands> pair in clientsInfo)
            {
                Beam b = UpdateTank(pair.Key, pair.Value, deltaTime);
                if (b != null)
                {
                    beams.Add(b);
                }
            }
            // Collisions when adding in the powerup. Checking for the collsion between the powerup and tank and wall has been checked once in updateTank method  
            // powerups

            if (timeUntilNextPowerupRespawn <= 0 && powerups.Count < maxNumberOfActivePowerups)
            {
                //Spawn Powerup
                bool checkCollision;
                Vector2D loc;
                Random rnd = new Random();
                do
                {
                    int VecX = rnd.Next(-(MapSize / 2), MapSize / 2);
                    int VecY = rnd.Next(-(MapSize / 2), MapSize / 2);
                    loc = new Vector2D(VecX, VecY);

                    checkCollision = false;
                    foreach (Wall w in walls.Values)
                    {
                        checkCollision = checkCollision || WallCollisionCheck(15, w, loc);
                    }

                    foreach (Tank t in tanks.Values)
                    {
                        checkCollision = checkCollision || ((loc - t.Location).Length() <= ((int)(Tank.TankSize / 2) + 15));
                    }
                }
                while (checkCollision);

                Powerup pu = new Powerup(loc);
                powerups.Add(pu.puID, pu);

                timeUntilNextPowerupRespawn = rnd.Next(0, (int)maxPowerupRespawnFrames) / numOfFramePerSec;
            }
            else if (powerups.Count < maxNumberOfActivePowerups)
            {
                timeUntilNextPowerupRespawn -= deltaTime;
            }

            foreach (Beam b in beams)
            {
                foreach (Tank t in tanks.Values)
                {
                    if (Intersects(b.origin, b.Direction, t.Location, (int)(Tank.TankSize / 2)) && b.OwnerID != t.TankID)
                    {
                        t.Died = true;
                        t.HitPoints = 0;

                        if (tanks.ContainsKey(b.OwnerID))
                        {
                            tanks[b.OwnerID].Score++;
                        }
                    }
                }

            }

            foreach (Projectile proj in projectiles.Values)
            {
                proj.Location = proj.Location + proj.Orientation * Projectile.ProjSpeed * numOfFramePerSec * deltaTime;

                foreach (Wall wall in walls.Values)
                {
                    if (WallCollisionCheck(15, wall, proj.Location))
                    {
                        proj.Died = true;
                    }

                    if (Math.Abs(proj.Location.GetX()) >= MapSize / 2 || Math.Abs(proj.Location.GetY()) >= MapSize / 2)
                    {
                        proj.Died = true;
                    }
                }

                foreach (Tank t in tanks.Values)
                {
                    if (t.HitPoints > 0 && (proj.Location - t.Location).Length() <= (int)(Tank.TankSize / 2) && t.TankID != proj.PlayerID)
                    {
                        if (!hotPotatoGameMode)
                        {
                            t.HitPoints--;
                            proj.Died = true;
                            if (t.HitPoints == 0)
                            {
                                t.Died = true;
                                if (tanks.ContainsKey(proj.PlayerID))
                                {
                                    tanks[proj.PlayerID].Score++;
                                }
                            }
                        }
                        else
                        {
                            if (tanks.ContainsKey(proj.PlayerID) && tanks[proj.PlayerID].IsHotPotato)
                            {
                                SwitchHotPotato(t);
                            }

                            proj.Died = true;
                            break;
                        }

                    }
                }
            }

            if (hotPotatoGameMode)
            {
                int numberOfAliveTanks = tanks.Count;
                foreach (Tank _tank in tanks.Values)
                {
                    if (_tank.HitPoints == 0)
                    {
                        numberOfAliveTanks--;
                    }
                    _tank.Score = (int)(potatoLifetime - potatoAge);
                }

                if (numberOfAliveTanks <= 1)
                {
                    if (timerTilNextMatch < 0)
                    {
                        foreach (Tank t in tanks.Values)
                        {
                            if (t.Died || t.HitPoints == 0)
                            {
                                RespawningTank(t);
                                numberOfAliveTanks++;
                            }
                        }

                    }


                    if (!ReferenceEquals(hotPotatoTank, null))
                    {
                        potatoAge = 0;
                        hotPotatoTank.IsHotPotato = false;
                        hotPotatoTank = null;
                    }


                }


                if (hotPotatoTank != null)
                    potatoAge += deltaTime;
                else if (numberOfAliveTanks > 2) //Should only be called at the start of the a match.
                {
                    ChooseHotPotato(numberOfAliveTanks);
                    timerTilNextMatch = 5f;
                }
                else
                {
                    timerTilNextMatch -= deltaTime;
                }

                if (potatoAge >= potatoLifetime)
                {
                    //Explode the hot potato.
                    hotPotatoTank.HitPoints = 0;
                    hotPotatoTank.Died = true;
                    //Choose another potato.
                    ChooseHotPotato(numberOfAliveTanks);
                }
            }

            return beams;
        }



        private bool WallCollisionCheck(int objLength, Wall wall, Vector2D expectedLocation)
        {
            ///
            int border = (int)(Wall.WallSize / 2) + objLength; //Constant

            Vector2D lower;
            Vector2D higher;
            if (wall.Start.GetY() > wall.End.GetY())
            {
                lower = wall.End;
                higher = wall.Start;
            }
            else
            {
                lower = wall.Start;
                higher = wall.End;
            }

            Vector2D leftMost;
            Vector2D rightMost;
            if (wall.Start.GetX() > wall.End.GetX())
            {
                leftMost = wall.End;
                rightMost = wall.Start;
            }
            else
            {
                leftMost = wall.Start;
                rightMost = wall.End;
            }

            bool collision = (expectedLocation.GetX() < (rightMost.GetX() + border) && expectedLocation.GetX() > (leftMost.GetX() - border))
                && (expectedLocation.GetY() < (higher.GetY() + border) && expectedLocation.GetY() > (lower.GetY() - border));

            return collision;

        }

        public void postUpdateWorld()
        {
            foreach (Tank t in tanks.Values)
            {
                if (t.Joined)
                {
                    t.Joined = false;
                }
                if (t.Died)
                {
                    t.Died = false;
                    t.RespawnCD = gameConstants.RespawnRate / (1000 / gameConstants.FrameRate);
                }
                if (t.DC)
                {

                    tanks.Remove(t.TankID);
                }

            }
            HashSet<Projectile> projToRemove = new HashSet<Projectile>();
            foreach (Projectile proj in projectiles.Values)
            {
                if (proj.Died)
                {
                    projToRemove.Add(proj);
                }
            }
            foreach (Projectile proj in projToRemove)
            {
                projectiles.Remove(proj.ProjID);
            }
            HashSet<Powerup> powerupsToRemove = new HashSet<Powerup>();
            foreach (Powerup powerup in powerups.Values)
            {
                if (powerup.Died)
                {
                    powerupsToRemove.Add(powerup);
                }

            }
            foreach (Powerup powerup in powerupsToRemove)
            {
                powerups.Remove(powerup.puID);
            }
        }


        /// <summary>
        /// Determines if a ray interescts a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        private void ChooseHotPotato(int numberOfAliveTanks)
        {
            int selectedTank = new Random().Next(numberOfAliveTanks);

            foreach (Tank t in tanks.Values)
            {
                if (t.HitPoints != 0 && selectedTank-- == 0)
                {
                    SwitchHotPotato(t);
                    potatoAge = 0;
                    break;
                }
            }
        }

        private void SwitchHotPotato(Tank newHotPotato)
        {
            if (!ReferenceEquals(hotPotatoTank, null))
                hotPotatoTank.IsHotPotato = false;
            newHotPotato.IsHotPotato = true;
            hotPotatoTank = newHotPotato;
        }

    }


}
