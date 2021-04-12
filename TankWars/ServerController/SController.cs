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

        public SController()
        {
            //Initialize Server w/ TCP Listener
            //Set up the callback for getting a connection.
        }

        static void Main(string[] args)
        {
            //Construct Controller

            //Update World loop.
        }

        private void OnConnection(SocketState s)
        {
            //Lock clientinfo
            //Add this socketstate to clientinfo
        }


    }
}
