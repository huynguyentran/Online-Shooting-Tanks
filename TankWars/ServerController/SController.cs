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


            while (serverActive)
            {
                while(waitFrame.ElapsedMilliseconds - lastTime >= controller.consts.FrameRate){ }
                lastTime = waitFrame.ElapsedMilliseconds;




                controller.UpdateWorld();

            }
            //Update World loop.
        }

        private void UpdateWorld()
        {
            //Access the commands that we have. 

            serverModel.UpdatingWorld(clientCommands);

        }

        public SController()
        {
            clientInfo = new Dictionary<int, Tuple<SocketState, ControlCommands>>();
            consts = new GameConstants();

            clientCommands = new Dictionary<int, ControlCommands>();

            
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
                return; 
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
                return;
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
                state.RemoveData(0, clientName.Length);

                Tank tank = new Tank(state.ID, clientName);

                lock (serverModel)
                {
                    serverModel.AddTank(tank);
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
            Networking.Send(state.TheSocket, ""+state.ID+"\n"+consts.Size+"\n");
            Networking.Send(state.TheSocket,serverModel.Serialization(serverModel.Walls));

            state.OnNetworkAction = GetClientCommand;
            Networking.GetData(state);
        }

        private void GetClientCommand(SocketState state)
        {
            if (state.ErrorOccurred)
            {

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
                ControlCommands commands = (ControlCommands)JsonConvert.DeserializeObject(input);
                return commands;
            }

            return null;

        }


    }
}
