using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.Server
{
    class ClientManager
    {
        public ConnectedClient[] AllClients { get => clientList.ToArray(); }

        private List<ConnectedClient> clientList;
        private int maxClients;
        public ClientManager(int clientLimit)
        {
            maxClients = clientLimit;
            clientList = new List<ConnectedClient>();
        }

        public void AddClient(ConnectedClient client)
        {
            if (GetClientFromName(client.ClientName) != null)
                throw new DuplicateNameException();
            if (GetClientFromGuid(client.ClientGuid) != null)
                throw new DuplicateGuidException();
            if (clientList.Count >= maxClients)
                throw new MaxClientsReachedException();

            clientList.Add(client);
        }

        public void RemoveClient(ConnectedClient client)
        {
            if (!clientList.Remove(client))
            {
                throw new ClientNotFoundException();
            }
        }

        public ConnectedClient GetClientFromName(string name)
        {
            foreach (var client in AllClients)
            {
                if (client.ClientName.ToLower() == name.ToLower())
                {
                    return client;
                }
            }
            return null;
        }

        public ConnectedClient GetClientFromGuid(Guid id)
        {
            foreach (var client in AllClients)
            {
                if (client.ClientGuid == id)
                {
                    return client;
                }
            }
            return null;
        }

        public class DuplicateNameException : Exception
        {
            public DuplicateNameException(string message = "Client name in use") : base(message)
            {

            }
        }

        public class DuplicateGuidException : Exception
        {
            public DuplicateGuidException(string message = "Client Guid in use") : base(message)
            {

            }
        }

        public class MaxClientsReachedException : Exception
        {
            public MaxClientsReachedException(string message = "Max clients reached") : base(message)
            {

            }
        }

        public class ClientNotFoundException : Exception
        {
            public ClientNotFoundException(string message = "Client could not be found") : base(message)
            {

            }
        }
    }
}
