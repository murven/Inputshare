using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.Net.Messages
{
    public class ClientLoginMessage : INetworkMessage
    {
        public MessageType Type { get; }
        public string ClientName { get; }
        public Guid ClientGuid { get; }

        public ClientLoginMessage(string clientName, Guid clientGuid)
        {
            if (clientName.Length > 64)
                throw new ArgumentException("Name too long");

            Type = MessageType.ClientLoginInfo;
            ClientName = clientName;
            ClientGuid = clientGuid;
        }
        
        /// <summary>
        /// 0 type
        /// 1-17 GUID
        /// 18 - name length (byte)
        /// 19-X = name
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte strLen = (byte)Encoding.ASCII.GetByteCount(ClientName);
            byte[] data = new byte[23 + strLen];
            Buffer.BlockCopy(BitConverter.GetBytes(data.Length-4), 0, data, 0, 4);
            data[4] = (byte)Type;
            Buffer.BlockCopy(ClientGuid.ToByteArray(), 0, data, 5, 16);
            data[22] = strLen;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(ClientName), 0, data, 23, strLen);
            return data;
        }

        public static ClientLoginMessage FromBytes(byte[] data)
        {
            if (data.Length == 0)
                throw new ArgumentException("Data was length 0");
            try
            {
                byte[] idData = new byte[16];
                Buffer.BlockCopy(data, 5, idData, 0, 16);
                Guid id = new Guid(idData);
                byte strLen = data[22];
                byte[] nameData = new byte[strLen];
                Buffer.BlockCopy(data, 23, nameData, 0, strLen);
                string name = Encoding.ASCII.GetString(nameData);
                return new ClientLoginMessage(name, id);
            }
            catch (Exception ex)
            {
                throw new MessageUnreadableException(ex.Message);
            }
        }
    }
}
