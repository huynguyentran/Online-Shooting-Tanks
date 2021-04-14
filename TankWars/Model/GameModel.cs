﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TankWars;

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

        public void AddTank(Tank tank)
        {
            //Add tank to model dictionary.
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

        public string Serialization<T>(Dictionary<int, T> d)
        {
            String json = "";
            foreach (T values in d.Values)
            {
                json += JsonConvert.SerializeObject(values) + "\n";
            }
            return json;
        }



        public Beam UpdateTank(int id, ControlCommands cmd)
        {

            Tank t = tanks[id];
            t.UpdatingTank(cmd);

            Vector2D movementDirection;
            switch (cmd.Move)
            {
                case "up":
                    {
                        movementDirection = new Vector2D(0, 1);
                        break;
                    }
                case "down":
                    {
                        movementDirection = new Vector2D(0, -1);
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

            //We need to look at the Constants object (and possibly the time passed).
            double speed = 1.0; //Velocity * Time passed between frames
            movementDirection *= speed;

            Vector2D expectedLocation = t.Location + movementDirection;

            foreach(Wall wall in walls.Values)
            {
                int border = 25 + 30; //Constant

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

                if (collision)
                {
                    expectedLocation = t.Location;
                    break;
                }
            }

            t.Location = expectedLocation;

            //Collision  tanks and powerups 

            foreach (Powerup powerup in Powerups.Values)
            {
                if (!powerup.Died)
                {
                    bool collision = (powerup.Location - t.Location).Length() <= 30;

                    if (collision)
                    {
                        t.Powers++;
                        powerup.Died = true;

                    }
                }
   
            }

            cmd.directionOfTank.Normalize();
            switch (cmd.Fire)
            {
                case "main":
                    {
                      
                        Vector2D projetileDir = new Vector2D(cmd.directionOfTank.GetX(), cmd.directionOfTank.GetY());
                        Projectile newProjectile = new Projectile(t.Location, projetileDir, t.TankID);
                        projectiles[newProjectile.ProjID]= newProjectile;
                        break;
                    }
                case "alt":
                    {
                        if (t.Powers > 0)
                        {
                          
                            Vector2D beamDir= new Vector2D(cmd.directionOfTank.GetX(), cmd.directionOfTank.GetY());
                            Beam b = new Beam(t.Location,beamDir, t.TankID);
                            
                            //pass into update game object
                            // pass into the controller

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



        public void UpdatingWorld(IEnumerable<KeyValuePair<int, ControlCommands>> clientsInfo)
        {
            List<Beam> beams = new List<Beam>();
            foreach (KeyValuePair<int, ControlCommands> pair in clientsInfo)
            {
                Beam b = UpdateTank(pair.Key, pair.Value);
                if (b != null)
                {
                    beams.Add(b);
                }
            }
   
            // Collisions when adding in the powerup. Checking for the collsion between the powerup and tank and wall has been checked once in updateTank method  
            // powerups

            foreach (Beam b in beams)
            {
                // process the beam, send back to the controller. 
            }


            //projectiles


            //Collision;



        }



    }


}
