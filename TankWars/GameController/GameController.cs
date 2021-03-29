using System;
using System.Text.RegularExpressions;
using NetworkUtil;
namespace Controller
{

    public class GameController
    {
        private event Action<string> errorEvent;



        public GameController(Action<string> onError)
        {
            errorEvent += onError;
        }

        public void connectToServer(string address, string playerName)
        {

            void toCall(SocketState state)
            {
                if (state.ErrorOccurred)
                {
                    ///
                }

                Networking.Send(state.TheSocket, playerName + "\n");
                state.OnNetworkAction = changeOnCall;


                Networking.GetData(state);



            }

            Networking.ConnectToServer(toCall, address, 11000);
        }

        private void changeOnCall(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                ///
            }

            ProcessMessages(state);

            //Receive Walls and Stuff
            //Almost everytime resets on network action. 
            state.OnNetworkAction = parseWall;
            Networking.GetData(state);
        }


        private void parseWall(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                ///
            }
            //put the wall into the map. 
            //Deserialization.


            state/
            Networking.GetData(state);

        }

        private void ProcessMessages(SocketState state)
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

            // Then remove it from the SocketState's growable buffer
            state.RemoveData(0, p.Length);
        }

        int ID = Int32.Parse(parts[0]);
        int mapSize = Int32.Parse(parts[1]);
        // prase into the model. 
    }
}
}
