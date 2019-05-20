using System;
using System.Net.Sockets;

namespace InputshareLib.Server
{
    class ClientConnectedArgs : EventArgs
    {
        public ClientConnectedArgs(Socket clientSocket, string clientName, Guid clientId)
        {
            ClientSocket = clientSocket;
            ClientName = clientName;
            ClientId = clientId;
        }

        public Socket ClientSocket { get; }
        public string ClientName { get; }
        public Guid ClientId { get; }
    }
}
