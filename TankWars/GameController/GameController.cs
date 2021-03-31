using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json.Linq;
using theMap;

namespace Controller
{

    public class GameController
    {
        private event Action<string> errorEvent;

        private Model theWorld;

        public Model world
        {
            get { return theWorld; }
        }
        //Bad practice to use -1 to signify an invalid value.
        private int playerID = -1;
        private int mapSize = -1;

        //Do we want event to be public
        public delegate void DataEvent();
        public event DataEvent updateView;

        public void AddErrorHandler(Action<string> onError)
        {
            errorEvent += onError;
        }

        public GameController()
        {
            theWorld = new Model(0);
        }

        public void ConnectToServer(string address, string playerName)
        {

            void InititateHandshake(SocketState state)
            {
                if (state.ErrorOccurred)
                {
                    errorEvent.Invoke(state.ErrorMessage);
                    return;
                }

                Networking.Send(state.TheSocket, playerName + "\n");
                state.OnNetworkAction = RetrievePlayerIDandMapSize;
                Networking.GetData(state);
            }

            Networking.ConnectToServer(InititateHandshake, address, 11000);
        }

        private void RetrievePlayerIDandMapSize(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                errorEvent.Invoke(state.ErrorMessage);
                return;
            }

            ParsePlayerIDandMapSize(state);

            if (playerID != -1 && mapSize != -1)
            {
                //Receive Walls and Stuff
                //Almost everytime resets on network action. 
                state.OnNetworkAction = RetrieveGameObjects;
            }

            Networking.GetData(state);
        }

        private void ParsePlayerIDandMapSize(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                //Get rid of extra newline character.
                string trimmedPart = p.Substring(0, p.Length - 1);

                //Switch to assigning values to model.
                if (playerID == -1)
                    playerID = Int32.Parse(trimmedPart);
                else if (mapSize == -1)
                {
                    mapSize = Int32.Parse(trimmedPart);
                    theWorld.SetSize(mapSize);
                }
                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);

                if (playerID != -1 && mapSize != -1)
                    break;
            }
        }

        private void RetrieveGameObjects(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                errorEvent.Invoke(state.ErrorMessage);
                return;
            }
            //Deserialization.

            ParseGameObjects(state);

            //Parse Walls
            //If we've received all the walls, start taking frames.

            Networking.GetData(state);
        }

        private void ParseGameObjects(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                //Get rid of extra newline character.
                string trimmedPart = p.Substring(0, p.Length - 1);

                theWorld.DeserializeGameObject(trimmedPart);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            updateView();
        }







        ///// <summary>
        ///// Example of handling movement request
        ///// </summary>
        //public void HandleMoveRequest(/* pass info about which command here */)
        //{
        //    movingPressed = true;
        //}

        ///// <summary>
        ///// Example of canceling a movement request
        ///// </summary>
        //public void CancelMoveRequest(/* pass info about which command here */)
        //{
        //    movingPressed = false;
        //}

        ///// <summary>
        ///// Example of handling mouse request
        ///// </summary>
        //public void HandleMouseRequest(/* pass info about which button here */)
        //{
        //    mousePressed = true;
        //}

        ///// <summary>
        ///// Example of canceling mouse request
        ///// </summary>
        //public void CancelMouseRequest(/* pass info about which button here */)
        //{
        //    mousePressed = false;
        //}


    }
}
