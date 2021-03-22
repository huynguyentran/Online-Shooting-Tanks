using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetworkUtil;

namespace NetworkTests
{
    [TestClass]
    public class NetworkTests
    {
        private const int port = 1101;

        [TestMethod]
        public void TestMethod1()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            System.Threading.Thread.Sleep(10000);
            listener.BeginAcceptSocket(CallbackHappened, null);

            listener.Stop();
        }

        private void CallbackHappened(IAsyncResult ar)
        {
            Console.WriteLine("A callback occured.");

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            Socket s = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            s.BeginConnect(ip, port, SocketConnectCallback, s);
        }

        private void SocketConnectCallback(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            s.EndAccept(ar);

        }

    }
}
