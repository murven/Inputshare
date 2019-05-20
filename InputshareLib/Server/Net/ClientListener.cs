using InputshareLib;
using InputshareLib.Net.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static InputshareLib.Settings;

namespace InputshareLib.Server.Net
{
    class ClientListener
    {
        public event EventHandler<ClientConnectedArgs> ClientConnected;

        public IPEndPoint LocalAddress { get; private set; }
        public bool Listening { get; private set; }

        private CancellationTokenSource cancelTokenSource;
        private TcpListener listener;

        private List<Socket> socketList;

        public void Start(int port)
        {
            if (Listening)
            {
                throw new InvalidOperationException("Clientlistener already running");
            }

            socketList = new List<Socket>();
            cancelTokenSource = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Any, port);
            listener.Server.NoDelay = ClientListenerSocketNoDelay;
            listener.Start(ClientListenerQueueSize);
            Listening = true;
            LocalAddress = (IPEndPoint)listener.LocalEndpoint;
            IPAddress[] o = Dns.GetHostAddresses(Dns.GetHostName());

            ISLogger.Write("Local addresses:");
            foreach(IPAddress i in o)
            {
                if(i.AddressFamily == AddressFamily.InterNetwork)
                {
                    ISLogger.Write("----" + i.ToString());
                }
            }

            ISLogger.Write("ClientListener started on port " + port);
            cancelTokenSource.Token.Register(listener.Stop);
            listener.BeginAcceptSocket(Listener_AcceptSocketCallback, null);
        }

        public void Stop()
        {
            cancelTokenSource.Cancel();
            listener.Stop();
            foreach (var soc in socketList)
                soc.Dispose();

            Listening = false;
        }

        private void Listener_AcceptSocketCallback(IAsyncResult ar)
        {
            Socket soc = null;
            try
            {
                soc = listener.EndAcceptSocket(ar);

                if(socketList.Count > ClientListenerMaxOpenSockets)
                {
                    ISLogger.Write("Declining connection from {0}: Max open client sockets reached", soc.RemoteEndPoint);
                    soc.Dispose();
                    return;
                }

            }catch(ObjectDisposedException)
            {
                //ISLogger.Write("AcceptSocketCallback attempted to access disposed TcpListener");
                return;
            }

            //ISLogger.Write("Accepting connection from {0}", soc.RemoteEndPoint);
            socketList.Add(soc);
            Timer socTimer = new Timer(SocketTimeoutTimerElapsed, soc, ClientListenerSocketTimeout, Timeout.Infinite);
            SocketStateObject syncObj = new SocketStateObject(new byte[ClientListenerSocketBufferSize], soc, socTimer, (IPEndPoint)soc.RemoteEndPoint);

            soc.BeginReceive(syncObj.Buff, 0, 4, 0, SocketReceiveCallback, syncObj);

            listener.BeginAcceptSocket(Listener_AcceptSocketCallback, null);
        }

        private void SocketReceiveCallback(IAsyncResult ar)
        {
            SocketStateObject state = (SocketStateObject)ar.AsyncState;

            try
            {
                Socket soc = state.Soc;
                int bytesIn = soc.EndReceive(ar);
                int pSize = BitConverter.ToInt32(state.Buff, 0);

                if(pSize == 0 | bytesIn == 0)
                {
                    ISLogger.Write("Client {0} lost connection ", soc.RemoteEndPoint);
                    socketList.Remove(soc);
                    soc.Dispose();
                    state.TimeoutTimer.Dispose();
                    return;
                }


                int dRem = pSize;
                int bPos = 4;
                do
                {
                    int bIn = state.Soc.Receive(state.Buff, bPos, dRem, 0);
                    bPos += bIn;
                    dRem = pSize - bPos + 4;
                } while (dRem > 0);

                state.TimeoutTimer.Dispose();

                MessageType cmd = (MessageType)state.Buff[4];

                if(cmd == MessageType.ClientLoginInfo)
                {
                    //ISLogger.Write("{0} sent login info", soc.RemoteEndPoint);
                    ClientLoginMessage msg = ClientLoginMessage.FromBytes(state.Buff);
                    socketList.Remove(soc);
                    ClientConnected?.Invoke(this, new ClientConnectedArgs(soc, msg.ClientName, msg.ClientGuid));
                }
                else{
                    ISLogger.Write("{0} sent invalid data", soc.RemoteEndPoint);
                }
            }
            catch (ObjectDisposedException) //This is fine, it just means that the socket was disposed by the timeout timer
            {
                //ISLogger.Write("ClientListener attempted to access disposed socket");
            }catch(SocketException)
            {
 
                //TODO
            }
        }

        private void SocketTimeoutTimerElapsed(object sync)
        {
            Socket clientSoc = (Socket)sync;

            if(clientSoc != null && clientSoc.Connected)
            {
                ISLogger.Write("Client {0} did not send login data in time", clientSoc.RemoteEndPoint);
                socketList.Remove(clientSoc);
                clientSoc.Dispose();
            }
        }

        private class SocketStateObject : IDisposable
        {
            public SocketStateObject(byte[] buff, Socket soc, Timer timeoutTimer, IPEndPoint address)
            {
                Buff = buff;
                Soc = soc;
                TimeoutTimer = timeoutTimer;
                Address = address;
            }

            public byte[] Buff { get; }
            public Socket Soc { get; }
            public Timer TimeoutTimer { get; }
            public IPEndPoint Address { get; }

            public void Dispose()
            {
                TimeoutTimer.Dispose();
            }
        }
    }
}
