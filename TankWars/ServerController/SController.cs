using System;
using NetworkUtil;
using System.Collections;
using System.Collections.Generic;
using Model;
using Constants;

using Controller;

namespace ServerController
{
    class SController
    {
        private Dictionary<long, Tuple<SocketState, ControlCommands>> clientInfo;

        private GameConstants consts;

        //Change ClientModel to just model or something that makes sense for both the client and the server.
        private GameModel serverModel;

        static void Main(string[] args)
        {
            //Construct Controller
            SController controller = new SController();



          
            //Update World loop.
        }

     
        public SController()
        {
            clientInfo = new Dictionary<long, Tuple<SocketState, ControlCommands>>();
            consts = new GameConstants();

            
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



            Networking.GetData(state);
        }

        private void InitialServerData(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                //
            }

            lock (serverModel)
            {
                Tank tank = new Tank(state.ID,state.GetData());
                state.RemoveData(0, state.GetData().Length);
            }
           

            lock (clientInfo)
            {
                
                clientInfo[state.ID] = new Tuple<SocketState, ControlCommands>(state, new ControlCommands());
                
            }
          
           // state.OnNetworkAction = //;

        }

        private void SendingFirstData(SocketState state)
        {
            if (state.ErrorOccurred)
            {

            }
            Networking.Send(state.TheSocket, ""+state.ID+"\n"+consts.Size+"\n");
            Networking.Send(state.TheSocket,serverModel.Serialization(serverModel.Walls));


            state.OnNetworkAction = SendingWall
            Networking.GetData();
        }

        private void SendingWall(SocketState state)
        {

        }


    }
}
