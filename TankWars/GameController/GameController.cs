using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json.Linq;
using TankWars;
using Model;

namespace Controller
{

    /// <summary>
    ///  The controller of the 2D Tank Wars game. 
    /// </summary>
    /// <author>Huy Nguyen</author>
    /// <author>William Erignac</author>
    /// <version>04/09/2021</version>
    public class GameController
    {
        //An event to handle all the errors.
        private event Action<string> errorEvent;

        //The ClientModel/Model object that holds all information sent by the server. 
        private GameModel clientWorld;

        //A Control Commands that send the client's control to the server.
        private ControlCommands cmd;

        //A getter to return the ClientModel/Model object.
        public GameModel world
        {
            get { return clientWorld; }
        }

        //A event that notify the View that it should be updated. 
        public delegate void DataEvent();
        public event DataEvent updateView;

        //A event that notify an object has "died" in the Tank War game. 
        public event Action<object> deathEvent;

        private SocketState connectionState;

        /// <summary>
        /// Handling the error and sent event that notify the view that an error has occured.
        /// </summary>
        public void AddErrorHandler(Action<string> onError)
        {
            errorEvent += onError;
        }

        /// <summary>
        /// The constructor for the GameController.
        /// </summary>
        public GameController()
        {
            clientWorld = new GameModel(0);
            cmd = new ControlCommands();
            cmd.Move = "none";
            cmd.Fire = "none";
            Vector2D s = new Vector2D(0, 1);
            cmd.directionOfTank = s;
        }

        /// <summary>
        /// Initializing the Handshake between the player and the server.
        /// </summary>
        /// <param name="address">The IP address of the server.</param>
        /// <param name="playerName">The player name.</param>
        public void ConnectToServer(string address, string playerName)
        {

            void InititateHandshake(SocketState state)
            {
                //If an error occur, send an errorEvent and stop the handshake.
                if (state.ErrorOccurred)
                {
                    errorEvent.Invoke(state.ErrorMessage);
                    return;
                }

                //Send the input name to the server, and try to get data if the connection is success 
                connectionState = state;
                Networking.Send(state.TheSocket, playerName + "\n");
                state.OnNetworkAction = RetrievePlayerIDandMapSize;
                Networking.GetData(state);
            }
            //Connect to the server.
            Networking.ConnectToServer(InititateHandshake, address, 11000);
        }

        /// <summary>
        /// Retriving the initial player assigned ID and the Map size of the game.
        /// </summary>
        /// <param name="state">The Socket State</param>
        private void RetrievePlayerIDandMapSize(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                errorEvent.Invoke(state.ErrorMessage);
                return;
            }

            //Pasre in the information that was sent by the server. 
            ParsePlayerIDandMapSize(state);

            //If the map size and the client ID is valid.
            if (clientWorld.MapSize != 0 && clientWorld.clientID != -1)
            {
                //Receive game objects from the server.
                //Almost everytime resets on network action. 
                state.OnNetworkAction = RetrieveGameObjects;
            }

            //The event loops that continously retriving data from the server. 
            Networking.GetData(state);
        }

        /// <summary>
        /// Parse in the player id and the map size. 
        /// </summary>
        /// <param name="state">The Socket State</param>
        private void ParsePlayerIDandMapSize(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            // Loop until we have processed all messages.
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

                //Parse in the client ID if the client has not received it. 
                if (clientWorld.clientID == -1)
                {
                    clientWorld.clientID = Int32.Parse(trimmedPart);
                }

                //Parse in the map size if the client has not received it. 
                else if (clientWorld.MapSize == 0)
                {
                    clientWorld.MapSize = Int32.Parse(trimmedPart);
                }
                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);

                //If the player has recieved the information, we don't need to parse in twice.
                if (clientWorld.clientID != -1 && clientWorld.MapSize != 0)
                    break;
            }
        }

        /// <summary>
        /// Retrive the game objects that are sent by the server through JSon.
        /// </summary>
        /// <param name="state">The Socket State</param>
        private void RetrieveGameObjects(SocketState state)
        {
            //If an error occur, send an errorEvent
            if (state.ErrorOccurred)
            {
                errorEvent.Invoke(state.ErrorMessage);
                return;
            }
            //Deserialization.
            ParseGameObjects(state);

            //If we've received all the walls, start taking frames.
            Networking.GetData(state);
        }

        /// <summary>
        /// Parse the game objects that are sent by the server into the client through JSon.
        /// </summary>
        /// <param name="state">The Socket State</param>
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

                //A Tuple to register that if the object has died or not.
                Tuple<bool, object> died = clientWorld.DeserializeGameObject(trimmedPart);

                //If a game object has died, call the death Event.
                if (died.Item1)
                {
                    try
                    {
                        deathEvent(died.Item2);
                    }
                    catch
                    {
                    }
                }
                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            //Update the client view. 
            try
            {
                updateView();

            }
            catch
            {

            }
        }

        /// <summary>
        /// Send the information back to the server. This is call after the Invalidiate method in the View so the 
        /// GameController can send information on each frame.
        /// </summary>
        public void OnNewFrame()
        {
            string serializedCommand = ControlCommands.Serialize(cmd);
            Networking.Send(connectionState.TheSocket, serializedCommand + "\n");
            if (cmd.Fire.Equals("alt"))
                cmd.Fire = "none";
        }

        public enum MovementDirection { UP, DOWN, LEFT, RIGHT };

        /// <summary>
        /// Handle the movement request by the view when a key is pressed down.
        /// </summary>
        /// <param name="m">The movement direction</param>
        public void HandleMoveRequest(MovementDirection m)
        {
            string movementDirection = MovementDirectionToString(m);
            //If the player still holds the a movement key while pressing a new one, the controller will remember
            //and resume the orignal movement key after the new movement key has been released. 
            if (!movementDirection.Equals(cmd.Move))
            {
                lastPress = cmd.Move;
                cmd.Move = movementDirection;
            }
        }

        private string lastPress = "none";
        /// <summary>
        /// Returns the string movement request for the server
        /// </summary>
        /// <param name="m">Movement direction</param>
        /// <returns>The request for the server</returns>
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
        /// Handle the movement request by the view when a key is released.
        /// </summary>
        /// <param name="m">The movement direction</param>
        public void CancelMoveRequest(MovementDirection m)
        {
            string movementDirection = MovementDirectionToString(m);
            //If the player still holds the previous movement, the controller will resume that movement.
            if (cmd.Move.Equals(movementDirection))
            {
                cmd.Move = lastPress;
                lastPress = "none";
            }
            //Else stop the tank.
            else if (movementDirection.Equals(lastPress))
            {
                lastPress = "none";
            }
        }

        /// <summary>
        /// Clear all movement requests when the player click outside of the form.
        /// </summary>
        public void ClearAllMovement()
        {
            lastPress = "none";
            cmd.Move = lastPress;
            cmd.Fire = "none";
        }


        public enum MouseClickRequest { main, alt };

        /// <summary>
        /// Handle the fire request by the view when a mouse button is click.
        /// </summary>
        /// <param name="m">The mouse click request</param>
        public void HandleMouseRequest(MouseClickRequest m)
        {

            cmd.Fire = MouseClickRequestToString(m);
        }

        /// <summary>
        /// Returns the string mouse fire request for the server
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Handle the mouse fire request by the view when a mouse button is released.
        /// </summary>
        /// <param name="m">The mouse click request</param>
        public void MouseCancelRequest(MouseClickRequest m)
        {
            if (cmd.Fire.Equals(MouseClickRequestToString(m)))
            {
                cmd.Fire = "none";
            }
        }

        /// <summary>
        /// Normalize registering the location of the mouse for the direction of the projectiles and turrets.
        /// </summary>
        /// <param name="x">x coordinate of the mouse</param>
        /// <param name="y">y coordinate of the mouse</param>
        public void MouseMovementRequest(int x, int y)
        {
            Vector2D tdir = new Vector2D(x, y);
            tdir.Normalize();
            cmd.directionOfTank = tdir;
        }
    }
}
