using System;
using NetworkUtil;
using System.Collections;
using System.Collections.Generic;
using Model;
using Constants;
using Newtonsoft.Json;
using Controller;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Text;

namespace ServerController
{
    class SController
    {
        private Dictionary<int, Tuple<SocketState, ControlCommands>> clientInfo;

        private Dictionary<int, ControlCommands> clientCommands;

        private GameConstants consts;

        //Change ClientModel to just model or something that makes sense for both the client and the server.
        private GameModel serverModel;

        static void Main(string[] args)
        {
            //Construct Controller
            SController controller = new SController();

            bool serverActive = true;

            long lastTime = 0;
            Stopwatch waitFrame = new Stopwatch();
            waitFrame.Start();
            //The amount of time that passed between frames in seconds.
            float deltaTime = 0;

            while (serverActive)
            {
                while(waitFrame.ElapsedMilliseconds - lastTime <= controller.consts.FrameRate){ }
                deltaTime = ((float)(waitFrame.ElapsedMilliseconds - lastTime)) / 1000f;
                lastTime = waitFrame.ElapsedMilliseconds;

                controller.UpdateWorld(deltaTime);
            }
            //Update World loop.
        }

        private void UpdateWorld(float deltaTime)
        {
            string frameJsonComposite;
            lock (serverModel)
            {
                IList<Beam> beams;
                //Access the commands that we have. 
                lock (clientInfo)
                {
                    beams = serverModel.UpdatingWorld(clientCommands, deltaTime);

                }

                frameJsonComposite = "";

                frameJsonComposite += JsonSerializationComposite(serverModel.Tanks.Values);
                frameJsonComposite += JsonSerializationComposite(serverModel.Projectiles.Values);
                frameJsonComposite += JsonSerializationComposite(beams);
                frameJsonComposite += JsonSerializationComposite(serverModel.Powerups.Values);

                foreach(Tank t in serverModel.Tanks.Values)
                {
                    //if (t.Joined)
                    //    t.Joined = false;
                    if (t.Died)
                        t.Died = false;
                }
                HashSet<Projectile> projToRemove = new HashSet<Projectile>();
                foreach(Projectile proj in serverModel.Projectiles.Values)
                {
                    if (proj.Died)
                    {
                        projToRemove.Add(proj);
                    }
                }
                foreach(Projectile proj in projToRemove)
                {
                    serverModel.Projectiles.Remove(proj.ProjID);
                }
                HashSet<Powerup> powerupsToRemove = new HashSet<Powerup>();
                foreach (Powerup powerup in serverModel.Powerups.Values)
                {
                    if (powerup.Died)
                    {
                        powerupsToRemove.Add(powerup);
                    }

                }
                foreach(Powerup powerup in powerupsToRemove )
                {
                    serverModel.Powerups.Remove(powerup.puID);
                }
            }

            lock(clientInfo)
            {
                foreach(Tuple<SocketState, ControlCommands> statePair in clientInfo.Values)
                {
                    Networking.Send(statePair.Item1.TheSocket, frameJsonComposite);
                }
            }

        }

        private static string JsonSerializationComposite<T>(IEnumerable<T> gameObjects)
        {
            StringBuilder sb = new StringBuilder();

            foreach(T gameObject in gameObjects)
            {
                sb.Append(JsonConvert.SerializeObject(gameObject) + '\n');
            }

            return sb.ToString();
        }

        public SController()
        {
            clientInfo = new Dictionary<int, Tuple<SocketState, ControlCommands>>();
            consts = new GameConstants();

            clientCommands = new Dictionary<int, ControlCommands>();
            serverModel = new GameModel(consts.Size);
            
            Networking.StartServer(OnConnection, 11000);
            //Initialize Server w/ TCP Listener
            //Set up the callback for getting a connection.
        }

        


        private void OnConnection(SocketState state)
        {
            //Lock clientinfo
            //Add this socketstate to clientinfo

            if (state.ErrorOccurred)
            {
                throw new Exception("Something bad happened: " + state.ErrorMessage);
            }

            ////On connection.
            ///
            state.OnNetworkAction = InitialServerData;
            Networking.GetData(state);
        }

        private void InitialServerData(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                throw new Exception("Something bad happened: " + state.ErrorMessage);
            }


            string clientName = state.GetData();
            int newlineIndex = clientName.IndexOf('\n');

            if (newlineIndex == -1)
            {
                Networking.GetData(state);
            }
            else
            {
                clientName = clientName.Substring(0, newlineIndex);
                state.RemoveData(0, clientName.Length + 1);

             

                lock (serverModel)
                {
                    serverModel.AddTank((int)state.ID, clientName);
                }

                lock (clientInfo)
                {
                    clientInfo[(int)state.ID] = new Tuple<SocketState, ControlCommands>(state, new ControlCommands());
                    clientCommands[(int)state.ID] = new ControlCommands();
                }

                SendingFirstData(state);
            }
        }

        private void SendingFirstData(SocketState state)
        {
            Networking.Send(state.TheSocket, ""+(int)state.ID +"\n" + consts.Size + "\n");

            string walls = JsonSerializationComposite(serverModel.Walls.Values);

            if (walls.Length == 0)
                walls = "\n";

            Networking.Send(state.TheSocket,walls);

            state.OnNetworkAction = GetClientCommand;
            Networking.GetData(state);
        }

        private void GetClientCommand(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                throw new Exception("Something bad happened: " + state.ErrorMessage);
            }

            string[] commands = Regex.Split(state.GetData(), @"(?<=[\n])");

            lock (clientInfo)
            {
                // Loop until we have processed all messages.
                foreach (string command in commands)
                {
                    if (command.Length == 0)
                        continue;

                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (command[command.Length - 1] != '\n')
                        break;

                    //Get rid of extra newline character.
                    string trimmedCommand = command.Substring(0, command.Length - 1);


                    ControlCommands deserializedCommand = new ControlCommands();
                    if(Deserialize(command) != null)
                    {
                        deserializedCommand = Deserialize(command);
                    }

                    clientInfo[(int)state.ID] = new Tuple<SocketState, ControlCommands>(state, deserializedCommand);
                    clientCommands[(int)state.ID] = deserializedCommand;

                    // Then remove it from the SocketState's growable buffer
                    state.RemoveData(0, command.Length);
                }


               
            }

            //Extract commands from the state.s
            //Replace command in clientInfo.

            Networking.GetData(state);
        }


        /// <summary>
        /// Serialize the command then send to the server.
        /// </summary>
        /// <param name="command">ControlCommands object</param>
        /// <returns>A serialized Json command</returns>

        private static ControlCommands Deserialize(string input)
        {
            JObject gObj = JObject.Parse(input);
            if (gObj["moving"] != null && gObj["fire"] != null && gObj["tdir"] != null)
            {
                ControlCommands commands = JsonConvert.DeserializeObject<ControlCommands>(input);
                return commands;
            }

            return null;

        }


    }
}
