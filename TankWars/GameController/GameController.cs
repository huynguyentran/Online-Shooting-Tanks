using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json.Linq;

namespace Controller
{

    public class GameController
    {
        private event Action<string> errorEvent;

        //Bad practice to use -1 to signify an invalid value.
        private int playerID = -1;
        private int mapSize = -1;

        //Do we want event to be public
        public delegate void DataEvent();
        public event DataEvent updateView;

        public GameController(Action<string> onError)
        {
            errorEvent += onError;
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
                    mapSize = Int32.Parse(trimmedPart);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
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

        
                //if (gObj["tank"] != null)
                //    //Model.Tank.Deserialize(trimmedPart);
                //else if (gObj["wall"] != null)
                //    //Model.Wall.Deserialize(trimmedPart);
                //else if (gObj["proj"] != null)
                //    //Model.Projectiles.Deserialize(trimmedPart);
                //else if (gObj["power"] != null)
                //    //Model.PowerUps.Deserialize(trimmedPart);
                //else if (gObj["beam"] != null)
                //    //Model.Beam.Deserialize(trimmedPart);
                //else
                //    throw new ArgumentException("Unrecognized game object received: " + trimmedPart);

                Model.Deserialize(trimmedPart);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            updateView();
        }



    }
}
