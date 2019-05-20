using InputshareLib.Input.Hotkeys;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace InputshareLib.Server
{
    public class ConnectedClientInfo 
    {
        public ConnectedClientInfo(string clientName, Guid clientId, IPAddress clientAddress, Hotkey key, ConnectedClientInfo leftClient,
            ConnectedClientInfo rightClient, ConnectedClientInfo aboveClient, ConnectedClientInfo belowClient)
        {
            ClientName = clientName;
            ClientId = clientId;
            ClientAddress = clientAddress;
            Key = key;
            if (leftClient == null)
                LeftClient = None;
            else
                LeftClient = leftClient;

            if (rightClient == null)
                RightClient = None;
            else
                RightClient = rightClient;

            if (belowClient == null)
                BelowClient = None;
            else
                BelowClient = belowClient;

            if (aboveClient == null)
                AboveClient = None;
            else
                AboveClient = aboveClient;

        }

        public override string ToString()
        {
            return ClientName;
        }
        public ConnectedClientInfo()
        {
            ClientName = "None";
            ClientId = Guid.Empty;
            ClientAddress = IPAddress.Any;
            Key = new Hotkey(0, 0);
        }

        public static ConnectedClientInfo None = new ConnectedClientInfo();

        public string ClientName { get; }
        public Guid ClientId { get; }
        public IPAddress ClientAddress { get; }
        public Hotkey Key { get; }
        public ConnectedClientInfo LeftClient { get; }
        public ConnectedClientInfo RightClient { get; }
        public ConnectedClientInfo AboveClient { get; }
        public ConnectedClientInfo BelowClient { get; }
    }
}
