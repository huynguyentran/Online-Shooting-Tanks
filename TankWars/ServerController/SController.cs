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
    /// <summary>
    /// The server controller of the Tank War game.
    /// </summary>
    /// <author>Huy Nguyen</author>
    /// <author>William Erignac</author>
    /// <version>04/23/2021</version>
    class SController
    {
        //A dictionary contains the client infos. The keys are the client state ID
        private Dictionary<int, Tuple<SocketState, ControlCommands>> clientInfo;
        //A dictionary contains the client commands. The keys are the client state ID
        private Dictionary<int, ControlCommands> clientCommands;
        //The class contains the constants that would detemrines how the tank war work
        private GameConstants consts;
        //The model that contains every information for the tank war game.
        private GameModel serverModel;

        static void Main(string[] args)
        {
            //Construct Controller
            SController controller = new SController();

            //A boolean to make sure the server would be in a loop to accept new IDs.
            bool serverActive = true;

            //Variables to make sure the server update in certain frame of time.
            long lastTime = 0;
            Stopwatch waitFrame = new Stopwatch();
            waitFrame.Start();
            //The amount of time that passed between frames in seconds.
            float deltaTime = 0;

            //Update World loop.
            while (serverActive)
            {
                while (waitFrame.ElapsedMilliseconds - lastTime <= controller.consts.FrameRate) { }
                deltaTime = ((float)(waitFrame.ElapsedMilliseconds - lastTime)) / 1000f;
                lastTime = waitFrame.ElapsedMilliseconds;
                controller.UpdateWorld(deltaTime);
            }
        }

        /// <summary>
        /// A private method to update the world frame.
        /// </summary>
        /// <param name="deltaTime">The amount of time that passes between frame</param>
        private void UpdateWorld(float deltaTime)
        {
            string frameJsonComposite;
            lock (serverModel)
            {
                //Since beam is only appeared for one frame, we update the beam through the controller.
                IList<Beam> beams;
                lock (clientInfo)
                {
                    beams = serverModel.UpdatingWorld(clientCommands, deltaTime);
                }

                //Serialize all game objects to send to the clients.
                frameJsonComposite = "";
                frameJsonComposite += JsonSerializationComposite(serverModel.Tanks.Values);
                frameJsonComposite += JsonSerializationComposite(serverModel.Projectiles.Values);
                frameJsonComposite += JsonSerializationComposite(beams);
                frameJsonComposite += JsonSerializationComposite(serverModel.Powerups.Values);

                //Update the clients to catch up with the world frame.
                serverModel.postUpdateWorld();
            }

            //For each clients, send the serialized code back.
            lock (clientInfo)
            {
                foreach (Tuple<SocketState, ControlCommands> statePair in clientInfo.Values)
                {
                    Networking.Send(statePair.Item1.TheSocket, frameJsonComposite);
                }
            }

        }

        /// <summary>
        /// Serialized the game object 
        /// </summary>
        /// <typeparam name="T"> The type of game Objects such as Tank, Wall,...</typeparam>
        /// <param name="gameObjects"> Game objects</param>
        /// <returns> The string message.</returns>
        private static string JsonSerializationComposite<T>(IEnumerable<T> gameObjects)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T gameObject in gameObjects)
            {
                //If the hot potato mode is on, change the name of the hot potato.
                if (gameObject is Tank t && t.IsHotPotato)
                {
                    string realName = t.Name;
                    t.Name = "HOT POTATO - " + realName;
                    sb.Append(JsonConvert.SerializeObject(gameObject) + '\n');
                    t.Name = realName;
                }
                else
                {
                    sb.Append(JsonConvert.SerializeObject(gameObject) + '\n');
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// A constructor for the server controller.
        /// </summary>
        public SController()

        {
            //A bool to identify if there is something wrong with the settings.XML, if it is true, then we would not attemp to connect
            //The settings of the server has to follow a certain protocol: An XML file, with the name "settings" or else it would not work
            bool stopWorking = false;
            string root = AppDomain.CurrentDomain.BaseDirectory;
            clientInfo = new Dictionary<int, Tuple<SocketState, ControlCommands>>();
            try
            {
                consts = new GameConstants(root + @"..\..\..\..\Resources\settings.XML");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Read();
                stopWorking = true;
            }

            //We do not attempt to start the server if the settings XML file is wrong.
            if (!stopWorking)
            {
                clientCommands = new Dictionary<int, ControlCommands>();
                serverModel = new GameModel(consts.Size, consts);
                Networking.StartServer(OnConnection, 11000);
            }

            //Initialize Server w/ TCP Listener
            //Set up the callback for getting a connection.
        }




        private void OnConnection(SocketState state)
        {
            //Lock clientinfo
            //Add this socketstate to clientinfo

            if (!state.ErrorOccurred)
            {
                state.OnNetworkAction = InitialServerData;
                Networking.GetData(state);
            }

        }

        private void InitialServerData(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                state.TheSocket.Close();
            }
            else
            {
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


        }

        private void SendingFirstData(SocketState state)
        {
            Networking.Send(state.TheSocket, "" + (int)state.ID + "\n" + consts.Size + "\n");

            string walls = JsonSerializationComposite(serverModel.Walls.Values);

            if (walls.Length == 0)
                walls = "\n";

            Networking.Send(state.TheSocket, walls);

            state.OnNetworkAction = GetClientCommand;
            Networking.GetData(state);
        }

        private void GetClientCommand(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                lock (clientInfo)
                {
                    clientInfo.Remove((int)state.ID);
                    clientCommands.Remove((int)state.ID);



                }
                lock (serverModel)
                {
                    serverModel.Tanks[(int)state.ID].DC = true;
                    serverModel.Tanks[(int)state.ID].Died = true;
                    serverModel.Tanks[(int)state.ID].HitPoints = 0;
                }
                state.TheSocket.Close();

            }
            else
            {
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
                        if (Deserialize(command) != null)
                        {
                            deserializedCommand = Deserialize(command);
                        }

                        clientInfo[(int)state.ID] = new Tuple<SocketState, ControlCommands>(state, deserializedCommand);
                        clientCommands[(int)state.ID] = deserializedCommand;

                        // Then remove it from the SocketState's growable buffer
                        state.RemoveData(0, command.Length);
                    }

                }

                Networking.GetData(state);

            }
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
