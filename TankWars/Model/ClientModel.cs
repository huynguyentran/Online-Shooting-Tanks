using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// The model that holds all the information of the game objects. 
    /// The model will aslo Deserialize the information send by the server.
    /// </summary>
    public class ClientModel
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
        public ClientModel(int _size)
        {
            size = _size;
            tanks = new Dictionary<int, Tank>();
            projectiles = new Dictionary<int, Projectile>();
            powerups = new Dictionary<int, Powerup>();
            walls = new Dictionary<int, Wall>();
        }

        /// <summary>
        /// A method that desrialize the game objects.
        /// </summary>
        /// <param name="input">The information string sent by the server</param>
        /// <returns>The tuple of the game object, this tuple will indicate if the game objects has died or not.</returns>
        public Tuple<bool, object> DeserializeGameObject(string input)
        {
            JObject gObj = JObject.Parse(input);
            lock (this)
            {
                //If the game object is a tank.
                if (gObj["tank"] != null)
                {
                    //Deserialize the string that sent by the server. 
                    Tank t = Tank.Deserialize(input);
                    //Save the unique id of each tank into the model.
                    int id = gObj.Value<int>("tank");
                    //If the tank is not died.
                    if (!t.Died && t.HitPoints > 0)
                        //Register the tank to the list and keep drawing the tank.
                        tanks[id] = t;
                    else
                    //We temporary remove the tank from the list so that the tank disappears on the screen.
                        if (tanks.ContainsKey(id))
                            tanks.Remove(id);
                    //Return the tuple object.
                    return new Tuple<bool, object>(t.Died, t);
                }
                //If the game object is a wall.
                else if (gObj["wall"] != null)
                {
                    //Deserialize the string that sent by the server. 
                    Wall w = Wall.Deserialize(input);
                    //Save the information of the wall into the model.
                    walls[gObj.Value<int>("wall")] = w;
                }
                //If the game object is a projectile.
                else if (gObj["proj"] != null)
                {
                    //Deserialize the string that sent by the server. 
                    Projectile pr = Projectile.Deserialize(input);
                    // Save the unique id of each porojectile into the model.
                    int id = gObj.Value<int>("proj");
                    //If the projectile is not died.
                    if (!pr.Died)
                        //Register the projectile to the list and keep drawing the projectile
                        projectiles[id] = pr;
                    else
                        //Remove the projectile from the list.
                        projectiles.Remove(id);
                    //Return the tuple object.
                    return new Tuple<bool, object>(pr.Died, pr);
                }
                //If the game object is a powerup.
                else if (gObj["power"] != null)
                {
                    Powerup pu = Powerup.Deserialize(input);
                    int id = gObj.Value<int>("power");
                    //If the powerup is not died.
                    if (!pu.Died)
                        //Register the powerup to the list and keep drawing the powerup.
                        powerups[id] = pu;
                    else
                        //Remove the powerup from the list.
                        powerups.Remove(id);
                    //Return the tuple object.
                    return new Tuple<bool, object>(pu.Died, pu);
                }
                //If the game object is a beam.
                else if (gObj["beam"] != null)
                {
                    Beam b = Beam.Deserialize(input);
                    //Return the tuple object. Since the beam is only sent on one frame, and animation is handle by the view, we do not need
                    //A list to contain it.
                    return new Tuple<bool, object>(true, b);

                }
                //Throw an excpetion if the server sent invalid information.
                else
                    throw new ArgumentException("Unrecognized game object received: " + input);
            }

            return new Tuple<bool, object>(false, null);
        }


    }


}
