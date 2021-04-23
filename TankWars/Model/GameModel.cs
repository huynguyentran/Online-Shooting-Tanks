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
        //--------------------------------------------Instance Variables used by both Client and Server side Models--------------------------------------------

        /// A group of dictionaries for each different type of the game objects. 

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

        //--------------------------------------------Client-Side Instance Variables--------------------------------------------

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

        //--------------------------------------------Client-Side Methods --------------------------------------------

        /// <summary>
        /// A construcor that intialize when the information is sent by the server. 
        /// Assigns the map size of the tank war game.
        /// </summary>
        /// <param name="_size">The map size that sent by the server</param>
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

        //--------------------------------------------Server-Side Instance Variables--------------------------------------------

        /// <summary>
        /// The constants found in the xml file for this match.
        /// </summary>
        private GameConstants gameConstants;
        /// <summary>
        /// The maxiumum number of powerups allowed on the feild at any given time.
        /// </summary>
        private readonly uint maxNumberOfActivePowerups;
        /// <summary>
        /// The maximum amount of time it takes a powerup to respawn.
        /// </summary>
        private readonly uint maxPowerupRespawnFrames;
        /// <summary>
        /// The amount of time until THIS next powerup will resond.
        /// When this value reaches zero, a new powerup will respawn.
        /// </summary>
        private float timeUntilNextPowerupRespawn;
        /// <summary>
        /// The framerate of the game.
        /// </summary>
        private readonly float numOfFramePerSec;
        /// <summary>
        /// Whether the match is a hot potato match, or a boring match.
        /// </summary>
        private readonly bool hotPotatoGameMode;

        /// <summary>
        /// The age of the current potato (how long it has lived).
        /// When this value reaches potatoLifetime, the potato detonates.
        /// Only applicable in hot potato mode.
        /// </summary>
        private float potatoAge;

        /// <summary>
        /// How long the potato waits before detonating.
        /// Only applicable in hot potato mode.
        /// </summary>
        private float potatoLifetime;

        /// <summary>
        /// The tank that is currently "it" in a game of hot potato.
        /// Only applicable in hot potato mode.
        /// </summary>
        private Tank hotPotatoTank;

        /// <summary>
        /// The time between hot potato matches.
        /// This timer gives a breif window of pause between hot potato
        /// matches and the winner of the match an opportunity to brag.
        /// Only applicable in hot potato mode.
        /// </summary>
        private float timerUntilNextMatch = 5f;

        //--------------------------------------------Server-Side Methods--------------------------------------------
        
        /// <summary>
        /// A constructor for a server-side game model.
        /// </summary>
        /// <param name="_size">The size of the world.</param>
        /// <param name="_const">An object containing most of the important game constants read off an xml file.</param>
        public GameModel(int _size, GameConstants _const)
        {
            size = _size;
            tanks = new Dictionary<int, Tank>();
            projectiles = new Dictionary<int, Projectile>();
            powerups = new Dictionary<int, Powerup>();
            walls = new Dictionary<int, Wall>();
            gameConstants = _const;
            
            //Retreiving the walls of the map.
            foreach (Tuple<Vector2D, Vector2D> points in _const.WallList)
            {
                Wall wall = new Wall(points.Item1, points.Item2, walls.Count);
                walls.Add(wall.WallID, wall);
            }

            //Calculating 
            numOfFramePerSec = 1000 / (gameConstants.FrameRate);
            
            //Transferring constant values into instance variables.
            maxNumberOfActivePowerups = (uint)gameConstants.ActivePUs;
            maxPowerupRespawnFrames = gameConstants.PURespawn;
            
            //Transferring constant values specific to certain game objects to their classes.
            Tank.SetTankParam(gameConstants.TankHP, gameConstants.TankSize, gameConstants.TankSpeed);
            Wall.SetWallParam(gameConstants.WallSize);
            Projectile.SetProjParam(gameConstants.ProjSpeed);
            
            //Transferring the hot potato game mode settings to the instance variables.
            hotPotatoGameMode = gameConstants.GameMode;
            potatoLifetime = gameConstants.GameModeTimer;

            //Hot potato mode does not include any powerups.
            if (hotPotatoGameMode == true)
            {
                maxNumberOfActivePowerups = 0;
            }


        }

        /// <summary>
        /// Adds a tank to model at a random location with the given id and name.
        /// </summary>
        /// <param name="id">The ident of the tank.</param>
        /// <param name="tankName">The name of the tank.</param>
        public void AddTank(int id, string tankName)
        {
            Tank tank = new Tank(id, tankName, GetRandomTankRespawnLocation());
            tanks[tank.TankID] = tank;
        }

        /// <summary>
        /// Updates a tank's movement for a frame. This includes:
        /// -Moving the tank turret.
        /// -Moving the tank.
        /// -Detecting wall collisions.
        /// -Implementing wraparound.
        /// </summary>
        /// <param name="t">The tank to update.</param>
        /// <param name="cmd">The way the tank wants to move (commands given by a client).</param>
        /// <param name="deltaTime">The amount of time that has passed between this frame and the last.</param>
        private void UpdateTankMovement(Tank t, ControlCommands cmd, float deltaTime)
        {
            //Turn the turret direction in the direction of the client's mouse.
            t.UpdatingTurretDirection(cmd);

            //Retrieve the direction the client wants the tank to move in from cmd.
            Vector2D movementDirection;
            switch (cmd.Move)
            {
                case "up":
                    movementDirection = new Vector2D(0, -1);
                    break;
                case "down":
                    movementDirection = new Vector2D(0, 1);
                    break;
                case "left":
                    movementDirection = new Vector2D(-1, 0);
                    break;
                case "right":
                    movementDirection = new Vector2D(1, 0);
                    break;
                default:
                    movementDirection = new Vector2D(0, 0);
                    break;
            }

            //If the tank is moving (or trying to), show which direction it's moving in.
            if (movementDirection.Length() != 0)
            {
                t.Orientation = movementDirection;
            }


            /*
             * Calculate the framerate-independent speed of the tank 
             * (Tank.Speed is the distance the tank should move every frame).
             */
            double speed = (Tank.TankSpeed * numOfFramePerSec) * deltaTime;

            //The distance the tank will move this frame.
            movementDirection *= speed;

            //The expected location of the tank ager moving.
            Vector2D expectedLocation = t.Location + movementDirection;

            /*
             * All these if statements implement the wraparound of tank.
             * When the tank is out of bounds on one side of the world, it is teleported to the other side.
             * These calculations assume that the tank will never move more than the length of the map in a single frame.
             * -This breaks with huge lag spikes and really high tank speeds.
             */

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

            /*
             * Detect tank collisions with any walls.
             * If there is at least one collision, the tank doesn't move from its last position.
             * These calculations assume that the tank will never move more than the width of a wall in a single frame.
             * -This breaks with huge lag spikes and really high tank speeds.
             */
            foreach (Wall wall in walls.Values)
            {
                if (WallCollisionCheck((int)Tank.TankSize / 2, wall, expectedLocation))
                {
                    expectedLocation = t.Location;
                    break;
                }
            }

            //Move the tank to the correct position.
            t.Location = expectedLocation;
        }

        /// <summary>
        /// Update a tank for a single frames. This includes:
        /// -Moving the tank.
        /// -Collecting powerups.
        /// -Spawning projectiles / beams.
        /// -Respawning the tank.
        /// </summary>
        /// <param name="id">The id of the tank to update.</param>
        /// <param name="cmd">The way the tank wants to move (commands given by a client).</param>
        /// <param name="deltaTime">The amount of time that has passed between frames.</param>
        /// <returns>A beam if it was fired, null otherwise.</returns>
        public Beam UpdateTank(int id, ControlCommands cmd, float deltaTime)
        {
            //Get the tank we're talking about (to update).
            Tank t = tanks[id];

            //If the tank has died, there is no reason to worry about movement, powerups, or projectiles.
            if (t.Died == false && t.HitPoints > 0)
            {
                UpdateTankMovement(t, cmd, deltaTime);

                /*
                 * Detect collisions between this tank and powerups and increment the number
                 * of powerups this tank has.
                 */
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

                //If the tank still need to cool down between shots, we decrement its cool down timer.
                if (t.TankCoolDown > 0)
                {
                    t.TankCoolDown -= deltaTime;
                }

                //Normalize the direction the tank is pointing in to prevent bugs with ToAngle.
                cmd.directionOfTank.Normalize();

                //Processing fire requests.
                switch (cmd.Fire)
                {
                    case "main":
                        {
                            /*
                             * Normally a tank can fire whenever (as long as it's not on cool down),
                             * but in hot potato mode, a tank can only fire if they are the hot potato.
                             */
                            if (t.TankCoolDown <= 0 && (t.IsHotPotato || !hotPotatoGameMode))
                            {
                                //Set up the projectile fired by the tank.
                                Vector2D projetileDir = t.TurretDirection;
                                //Move it forward so that it comes out of the barrel instead of the center of the tank.
                                Projectile newProjectile = new Projectile(t.Location + projetileDir * (Tank.TankSize / 2), projetileDir, t.TankID);
                                projectiles[newProjectile.ProjID] = newProjectile;

                                //Since FramePerShot is in frames, we need to convert to seconds.
                                t.TankCoolDown = gameConstants.FramePerShot / numOfFramePerSec;
                            }
                            break;
                        }
                    case "alt":
                        {
                            /*
                             * Beams only exist in the normal gamemode.
                             */
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
            else // If the tank is dead, we need to worry about respawning.
            {
                /*
                 * If we're in hot potato game mode, we'll respawn once the match is done later in the code.
                 * See line 656.
                 */
                if (!hotPotatoGameMode)
                {
                    //Decrement the respawn timer if the tank is died recently.
                    if (t.RespawnCD > 0)
                    {
                        t.RespawnCD -= deltaTime;
                    }
                    else //Otherwise respawn the tank.
                    { 
                        t.HitPoints = (int)Tank.MaxHP;
                        t.Location = GetRandomTankRespawnLocation();
                    }
                }

            }
            return null;
        }

        /// <summary>
        /// Finds a spot to respawn a tank that doesn't collide with the walls.
        /// </summary>
        /// <returns>The new position of the tank.</returns>
        private Vector2D GetRandomTankRespawnLocation()
        {
            bool checkCollision;
            Vector2D loc;

            //Generate respawn locations until we find one that doesn't collide with the walls.
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
            
            return loc;
        }


        /// <summary>
        /// Updates all gameobjects in the world for a frame.
        /// </summary>
        /// <param name="tankCommands">The movement commands of the tanks this frame.</param>
        /// <param name="deltaTime">the amount of time that has passed between frames.</param>
        /// <returns>The beams created this frame.</returns>
        public IList<Beam> UpdatingWorld(IEnumerable<KeyValuePair<int, ControlCommands>> tankCommands, float deltaTime)
        {
            List<Beam> beams = new List<Beam>();

            //Update the tanks and collect any beams.
            foreach (KeyValuePair<int, ControlCommands> pair in tankCommands)
            {
                Beam b = UpdateTank(pair.Key, pair.Value, deltaTime);
                if (b != null)
                {
                    beams.Add(b);
                }
            }

            // Add powerups to the world if we can.
            if (timeUntilNextPowerupRespawn <= 0 && powerups.Count < maxNumberOfActivePowerups)
            {
                //Spawn Powerup

                bool checkCollision;
                Vector2D loc;
                Random rnd = new Random();

                //Generate random powerup spawn locations until we find one that doesn't collide with the walls or tanks.
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

                //Wait a random amount of time until we spawn the next powerup.
                timeUntilNextPowerupRespawn = rnd.Next(0, (int)maxPowerupRespawnFrames) / numOfFramePerSec;
            }
            //If we can't spawn a powerup yet, but there is a spot for a powerup, wait until our timer is over.
            else if (powerups.Count < maxNumberOfActivePowerups)
            {
                timeUntilNextPowerupRespawn -= deltaTime;
            }

            //Destroy tanks hit by beams.
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

            //Process collisions between projectiles and walls and tanks.
            foreach (Projectile proj in projectiles.Values)
            {
                //Move projectiles forward.
                proj.Location = proj.Location + proj.Orientation * Projectile.ProjSpeed * numOfFramePerSec * deltaTime;
                //Destory projectiles that hit walls or that go out of bounds.
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
                
                //Detect collisions between this projectile and any tanks.
                foreach (Tank t in tanks.Values)
                {
                    if (t.HitPoints > 0 && (proj.Location - t.Location).Length() <= (int)(Tank.TankSize / 2) && t.TankID != proj.PlayerID)
                    {
                        /*
                         * In normal mode...
                         * Decrement hp from tanks that are hit by projectiles (and destroy the tanks if their hp is 0).
                         */
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
                        /*
                         * In hot potato mode...
                         * Transfer the "hot potato" status if the tank who fired the projectile is
                         * still the "hot potato" (and alive).
                         */
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

            //Special hot potato mode constraints.
            if (hotPotatoGameMode)
            {
                //Count how many tanks are still alive, and set their scores to how long until the potato explodes.
                int numberOfAliveTanks = tanks.Count;
                foreach (Tank _tank in tanks.Values)
                {
                    if (_tank.HitPoints == 0)
                    {
                        numberOfAliveTanks--;
                    }
                    _tank.Score = (int)(potatoLifetime - potatoAge);
                }

                //If one or less tanks is still alive, get ready to start a new match.
                if (numberOfAliveTanks <= 1)
                {
                    //Once the between-match break has ended...
                    if (timerUntilNextMatch < 0)
                    {
                        //Respawn all the tanks that have died in the previous match.
                        foreach (Tank t in tanks.Values)
                        {
                            if (t.Died || t.HitPoints == 0)
                            {
                                t.HitPoints = (int)Tank.MaxHP;
                                t.Location = GetRandomTankRespawnLocation();
                                numberOfAliveTanks++;
                            }
                        }

                    }

                    //Clear the hot potato status.
                    if (!ReferenceEquals(hotPotatoTank, null))
                    {
                        potatoAge = 0;
                        hotPotatoTank.IsHotPotato = false;
                        hotPotatoTank = null;
                    }
                }

                //If there is a hot potato tank, age the potato.
                if (hotPotatoTank != null)
                    potatoAge += deltaTime;
                //Once the game has the minimun # of tanks to start a match, start one.
                else if (numberOfAliveTanks > 2) 
                {
                    ChooseHotPotato(numberOfAliveTanks);
                    timerUntilNextMatch = 5f;
                }
                //Decrement the between-match break timer.
                else
                {
                    timerUntilNextMatch -= deltaTime;
                }

                //If a hot potato has reached its lifetime...
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


        /// <summary>
        /// Checks if a circular object has collided with a wall.
        /// </summary>
        /// <param name="objLength">The radius of the object.</param>
        /// <param name="wall">The wall to check if the collision with.</param>
        /// <param name="expectedLocation">The location of the object.</param>
        /// <returns>Whether there is a collision.</returns>
        private bool WallCollisionCheck(int objLength, Wall wall, Vector2D expectedLocation)
        {
            int border = (int)(Wall.WallSize / 2) + objLength;

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

        /// <summary>
        /// Clean up the model after it has been updated.
        /// </summary>
        public void PostUpdateWorld()
        {
            //Update tank statuses and clear out tanks that have disconnected.
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

            //Remove projectiles that have died.
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
            
            //Remove powerups that have died.
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

        /// <summary>
        /// Switches the hot potato status from the current hot potato tank to a random tank.
        /// </summary>
        /// <param name="numberOfAliveTanks">The number of alive tanks left in the match(used
        ///  to randomly select a new tank).</param>
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

        /// <summary>
        /// Switches the hot potato status from the current hot potato tank to another.
        /// </summary>
        /// <param name="newHotPotato">The new hot potato tank.</param>
        private void SwitchHotPotato(Tank newHotPotato)
        {
            if (!ReferenceEquals(hotPotatoTank, null))
                hotPotatoTank.IsHotPotato = false;
            newHotPotato.IsHotPotato = true;
            hotPotatoTank = newHotPotato;
        }

    }


}
