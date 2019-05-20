using System;

namespace InputshareLib.NamedIPC
{
    [Serializable]
    public class NIpcConnectMessage
    {
        public string Address { get; }
        public int Port { get; }
        public AddressMode AddressType { get; }
        public string ClientName { get; }
        public Guid ClientId { get; }

        public enum AddressMode
        {
            IP = 0,
            HostName = 1
        }

        public NIpcConnectMessage(string address, int port, AddressMode addressType, string clientName, Guid clientId)
        {
            Address = address;
            Port = port;
            AddressType = addressType;
            ClientName = clientName;
            ClientId = clientId;
        }
    }

    [Serializable]
    public class NIpcServiceStateMessage
    {
        public NIpcServiceStateMessage(bool connected, string address, int port, string userName, Guid id)
        {
            Connected = connected;
            Address = address;
            Port = port;
            UserName = userName;
            Id = id;
        }

        public bool Connected { get; }
        public string Address { get; }
        public int Port { get; }
        public string UserName { get; }
        public Guid Id { get; }
    }

    public enum NIpcBasicMessage
    {
        GetState = 1,
        AlreadyConnected = 2,
        AlreadyDisconnected = 3,
        InvalidGuid = 4,
        InvalidName = 5,
        InvalidAddress = 6,
        InvalidPort = 7,
        ConnectionFailed = 8,
        ConnectionError = 9,
        Disconnected = 10,
        Connected = 11,
        Disconnect = 12,
        AttemptingConnection = 13
    }
}
