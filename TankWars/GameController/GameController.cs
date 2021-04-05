using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json.Linq;
using TankWars;
using theMap;

namespace Controller
{

    public class GameController
    {
        private event Action<string> errorEvent;

        private Model theWorld;


        private ControlCommands cmd;

        public Model world
        {
            get { return theWorld; }
        }

        //Do we want event to be public
        public delegate void DataEvent();
        public event DataEvent updateView;

        public event Action<object> deathEvent;

        public void AddErrorHandler(Action<string> onError)
        {
            errorEvent += onError;
        }

        public GameController()
        {
            theWorld = new Model(0);
           
            cmd = new ControlCommands();
            cmd.Move = "none";
            cmd.Fire = "none";
            Vector2D s = new Vector2D(0, 1);
            cmd.directionOfTank = s;

            //movementMemory = new LinkedList<string>();
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

            if (theWorld.mapSize !=0 && theWorld.clientID != 0)
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
                if (theWorld.clientID == -1)
                {
                    theWorld.clientID = Int32.Parse(trimmedPart);
                }

                else if (theWorld.mapSize == 0)
                {
                    theWorld.mapSize = Int32.Parse(trimmedPart);
                }
                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);

                if (theWorld.clientID != -1 && theWorld.mapSize != 0)
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

            string serializedCommand = ControlCommands.Serialize(cmd);

            Networking.Send(state.TheSocket, serializedCommand + "\n");
            if (cmd.Fire.Equals("alt"))
                cmd.Fire = "none";

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

                Tuple<bool, object> died = theWorld.DeserializeGameObject(trimmedPart);

                if (died.Item1)
                {
                    deathEvent(died.Item2);
                }

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            updateView();
        }





        public enum MovementDirection { UP, DOWN, LEFT, RIGHT };

        /// <summary>
        /// Example of handling movement request
        /// </summary>
        public void HandleMoveRequest(MovementDirection m)
        {
            cmd.Move = MovementDirectionToString(m);
           
            //movementMemory.AddLast(stringDir);
        }


        //private LinkedList<string> movementMemory;

        private string MovementDirectionToString(MovementDirection m)
        {
            switch (m)
            {
                case MovementDirection.UP:
                    return "up";
                case MovementDirection.DOWN:
                    return "down";
                case MovementDirection.LEFT:
                    return "left";
                case MovementDirection.RIGHT:
                    return "right";
                default:
                    return "none";
            }
        }


        /// <summary>
        /// Example of canceling a movement request
        /// </summary>
        public void CancelMoveRequest(MovementDirection m)
        {
            //movementMemory.Remove(MovementDirectionToString(m));

            if (cmd.Move.Equals(MovementDirectionToString(m)))//movementMemory.Count == 0)
            {
                cmd.Move = "none";
            }
            /*
            else
            {
                cmd.Move = movementMemory.Last.Value;
            }
            */
        }


        public enum MouseClickRequest { main, alt };

        public void HandleMouseRequest(MouseClickRequest m)
        {
     
            cmd.Fire = MouseClickRequestToString(m);
        }

        public string MouseClickRequestToString(MouseClickRequest m)
        {
            switch (m)
            {
                case MouseClickRequest.main:
                    return "main";
                case MouseClickRequest.alt:
                    return "alt";
                default:
                    return "none";
            }
        }

        public void MouseCancelRequest(MouseClickRequest m)
        {
            if (cmd.Fire.Equals(MouseClickRequestToString(m)))
            {
                cmd.Fire = "none";
            }
        }


        public void MouseMovementRequest(int x, int y)

        {
            Vector2D tdir = new Vector2D(x, y);
            tdir.Normalize();
            cmd.directionOfTank = tdir;
        }

        //public void HandleMousePosition()
        //{

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
