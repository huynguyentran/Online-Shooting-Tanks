using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkUtil
{
    /// <summary>
    /// Author: William Erignac, Huy Nguyen
    /// Date: 03/24/2012
    /// CS 3500
    /// Description: A class that simplified running TCP network operation.
    /// </summary>
    public static class Networking
    {


        /////////////////////////////////////////////////////////////////////////////////////////
        // Server-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Starts a TcpListener on the specified port and starts an event-loop to accept new clients.
        /// The event-loop is started with BeginAcceptSocket and uses AcceptNewClient as the callback.
        /// AcceptNewClient will continue the event-loop.
        /// </summary>
        /// <param name="toCall">The method to call when a new connection is made</param>
        /// <param name="port">The the port to listen on</param>
        public static TcpListener StartServer(Action<SocketState> toCall, int port)
        {
            TcpListener listener = null;

            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                //Makes a tuple to hold the Action<SocketState> and the listener. 
                Tuple<Action<SocketState>, TcpListener> argument = new Tuple<Action<SocketState>, TcpListener>(toCall, listener);
                //Starts the event loop. 
                listener.BeginAcceptSocket(AcceptNewClient, argument);
            }
            catch (Exception)
            {

            }
            return listener;
        }

        /// <summary>
        /// To be used as the callback for accepting a new client that was initiated by StartServer, and 
        /// continues an event-loop to accept additional clients.
        ///
        /// Uses EndAcceptSocket to finalize the connection and create a new SocketState. The SocketState's
        /// OnNetworkAction should be set to the delegate that was passed to StartServer.
        /// Then invokes the OnNetworkAction delegate with the new SocketState so the user can take action. 
        /// 
        /// If anything goes wrong during the connection process (such as the server being stopped externally), 
        /// the OnNetworkAction delegate should be invoked with a new SocketState with its ErrorOccurred flag set to true 
        /// and an appropriate message placed in its ErrorMessage field. The event-loop should not continue if
        /// an error occurs.
        ///
        /// If an error does not occur, after invoking OnNetworkAction with the new SocketState, an event-loop to accept 
        /// new clients should be continued by calling BeginAcceptSocket again with this method as the callback.
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginAcceptSocket. It must contain a tuple with 
        /// 1) a delegate so the user can take action (a SocketState Action), and 2) the TcpListener</param>
        private static void AcceptNewClient(IAsyncResult ar)
        {
            //Passes in the argument to hold the toCall in our code and the listener. 
            Tuple<Action<SocketState>, TcpListener> argument = (Tuple<Action<SocketState>, TcpListener>)ar.AsyncState;
            Action<SocketState> toCall = argument.Item1;

            Socket newClient = null;

            try
            {
                newClient = argument.Item2.EndAcceptSocket(ar);
            }
            catch (Exception e)
            {
                ReportError(e.Message, new SocketState(toCall, null));
                return;
            }

            SocketState state = new SocketState(toCall, newClient);
            toCall(state);

            try
            {
                //Tries continuing the loop. 
                argument.Item2.BeginAcceptSocket(AcceptNewClient, argument);
            }
            catch (Exception e)
            {
                ReportError(e.Message, new SocketState(toCall, null));
            }
        }

        /// <summary>
        /// Stops the given TcpListener.
        /// </summary>
        public static void StopServer(TcpListener listener)
        {
            try
            {
                listener.Stop();
            }
            catch (Exception)
            {

            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Client-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of connecting to a server via BeginConnect, 
        /// and using ConnectedCallback as the method to finalize the connection once it's made.
        /// 
        /// If anything goes wrong during the connection process, toCall should be invoked 
        /// with a new SocketState with its ErrorOccurred flag set to true and an appropriate message 
        /// placed in its ErrorMessage field. Depending on when the error occurs, this should happen either
        /// in this method or in ConnectedCallback.
        ///
        /// This connection process should timeout and produce an error (as discussed above) 
        /// if a connection can't be established within 3 seconds of starting BeginConnect.
        /// 
        /// </summary>
        /// <param name="toCall">The action to take once the connection is open or an error occurs</param>
        /// <param name="hostName">The server to connect to</param>
        /// <param name="port">The port on which the server is listening</param>
        public static void ConnectToServer(Action<SocketState> toCall, string hostName, int port)
        {

            // Establish the remote endpoint for the socket.
            IPHostEntry ipHostInfo;
            IPAddress ipAddress = IPAddress.None;

            // Determine if the server address is a URL or an IP
            try
            {
                ipHostInfo = Dns.GetHostEntry(hostName);
                bool foundIPV4 = false;
                foreach (IPAddress addr in ipHostInfo.AddressList)
                    if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        foundIPV4 = true;
                        ipAddress = addr;
                        break;
                    }
                // Didn't find any IPV4 addresses
                if (!foundIPV4)
                {
                    ReportError("Can not find any IPv4 address/Invalid IPv4.", new SocketState(toCall, null));
                    return;
                }
            }
            catch (Exception)
            {
                // see if host name is a valid ipaddress
                try
                {
                    ipAddress = IPAddress.Parse(hostName);
                }
                catch (Exception)
                {
                    ReportError("Host name is not a valid ipaddress.", new SocketState(toCall, null));
                    return;
                }
            }

            // Create a TCP/IP socket.
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // This disables Nagle's algorithm (google if curious!)
            // Nagle's algorithm can cause problems for a latency-sensitive 
            // game like ours will be 
            socket.NoDelay = true;

            SocketState state = new SocketState(toCall, socket);

            try
            {
                /*
                 * Used AsyncWaitHandle from a Stack Overflow example:
                 * https://stackoverflow.com/questions/1062035/how-to-configure-socket-connect-timeout
                 * Also this stategy was suggested by a TA we visited.
                 */
                IAsyncResult result = socket.BeginConnect(ipAddress, port, ConnectedCallback, state);
                bool success = result.AsyncWaitHandle.WaitOne(3000, true);

                //If it is timeout in 3 seconds, we close the socket. 
                if (!success)
                {
                    try
                    {
                        socket.Close();
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                ReportError(e.Message, new SocketState(toCall, socket));
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a connection process that was initiated by ConnectToServer.
        ///
        /// Uses EndConnect to finalize the connection.
        /// 
        /// As stated in the ConnectToServer documentation, if an error occurs during the connection process,
        /// either this method or ConnectToServer should indicate the error appropriately.
        /// 
        /// If a connection is successfully established, invokes the toCall Action that was provided to ConnectToServer (above)
        /// with a new SocketState representing the new connection.
        /// 
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginConnect</param>
        private static void ConnectedCallback(IAsyncResult ar)
        {
            SocketState state = (SocketState)ar.AsyncState;

            try
            {
                //Finalizes the connection
                state.TheSocket.EndConnect(ar);
            }
            catch (Exception e)
            {
                ReportError(e.Message, state);
                return;
            }

            state.OnNetworkAction(state);
        }


        /////////////////////////////////////////////////////////////////////////////////////////
        // Server and Client Common Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of receiving data via BeginReceive, using ReceiveCallback 
        /// as the callback to finalize the receive and store data once it has arrived.
        /// The object passed to ReceiveCallback via the AsyncResult should be the SocketState.
        /// 
        /// If anything goes wrong during the receive process, the SocketState's ErrorOccurred flag should 
        /// be set to true, and an appropriate message placed in ErrorMessage, then the SocketState's
        /// OnNetworkAction should be invoked. Depending on when the error occurs, this should happen either
        /// in this method or in ReceiveCallback.
        /// </summary>
        /// <param name="state">The SocketState to begin receiving</param>
        public static void GetData(SocketState state)
        {
            try
            {
                //Begins the receive process
                state.TheSocket.BeginReceive(state.buffer, 0, SocketState.BufferSize, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception e)
            {
                ReportError(e.Message, state);
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a receive operation that was initiated by GetData.
        /// 
        /// Uses EndReceive to finalize the receive.
        ///
        /// As stated in the GetData documentation, if an error occurs during the receive process,
        /// either this method or GetData should indicate the error appropriately.
        /// 
        /// If data is successfully received:
        ///  (1) Read the characters as UTF8 and put them in the SocketState's unprocessed data buffer (its string builder).
        ///      This must be done in a thread-safe manner with respect to the SocketState methods that access or modify its 
        ///      string builder.
        ///  (2) Call the saved delegate (OnNetworkAction) allowing the user to deal with this data.
        /// </summary>
        /// <param name="ar"> 
        /// This contains the SocketState that is stored with the callback when the initial BeginReceive is called.
        /// </param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            SocketState state = (SocketState)ar.AsyncState;

            try
            {
                //Finalizes the receive process 
                int numBytes = state.TheSocket.EndReceive(ar);

                //If the server has closed, then throws. 
                if (numBytes == 0)
                {
                    ReportError("Server has closed.", state);
                    return;
                }

                //lock to make sure the data doesn't get corrupted
                lock (state.data)
                {
                    string message = Encoding.UTF8.GetString(state.buffer, 0, numBytes);
                    state.data.Append(message);
                }

                state.OnNetworkAction(state);
            }
            catch (Exception e)
            {
                ReportError(e.Message, state);
            }
        }

        /// <summary>
        /// Begin the asynchronous process of sending data via BeginSend, using SendCallback to finalize the send process.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool Send(Socket socket, string data)
        {
            if (socket.Connected)
            {
                try
                {
                    byte[] message = Encoding.UTF8.GetBytes(data);
                    socket.BeginSend(message, 0, message.Length, SocketFlags.None, SendCallback, socket);
                    return true;
                }
                catch (Exception)
                {
                    try
                    {
                        socket.Close();
                    }
                    catch (Exception)
                    {

                    }
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by Send.
        ///
        /// Uses EndSend to finalize the send.
        /// 
        /// This method must not throw, even if an error occurred during the Send operation.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndSend(ar);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Begin the asynchronous process of sending data via BeginSend, using SendAndCloseCallback to finalize the send process.
        /// This variant closes the socket in the callback once complete. This is useful for HTTP servers.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool SendAndClose(Socket socket, string data)
        {
            //Try catch
            if (socket.Connected)
            {
                try
                {
                    byte[] message = Encoding.UTF8.GetBytes(data);
                    socket.BeginSend(message, 0, message.Length, SocketFlags.None, SendAndCloseCallback, socket);
                    return true;
                }
                catch (Exception)
                {
                    try
                    {
                        socket.Close();
                    }
                    catch (Exception)
                    {

                    }
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by SendAndClose.
        ///
        /// Uses EndSend to finalize the send, then closes the socket.
        /// 
        /// This method must not throw, even if an error occurred during the Send operation.
        /// 
        /// This method ensures that the socket is closed before returning.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendAndCloseCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            try
            {
                socket.EndSend(ar);
                socket.Close();
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// A private helper method to set the error message, and use the socket state to report an error back to the user. 
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="state">The socket state</param>
        private static void ReportError(string message, SocketState state)
        {
            state.ErrorMessage = message;
            state.ErrorOccurred = true;
            state.OnNetworkAction(state);
        }

    }
}
