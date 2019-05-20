using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.Net.Messages
{
    public class SingleCommandMessage : INetworkMessage
    {
        public MessageType Type { get; }

        public SingleCommandMessage(MessageType type)
        {
            Type = type;
        }

        public byte[] ToBytes()
        {
            byte[] data = new byte[5];
            Buffer.BlockCopy(BitConverter.GetBytes(data.Length-4), 0, data, 0, 4);
            data[4] = (byte)Type;
            return data;
        }

        public static SingleCommandMessage FromBytes(byte[] data)
        {
            try
            {
                if (data.Length != 5)
                    throw new ArgumentException("data was invalid length");

                return new SingleCommandMessage((MessageType)data[4]);
            }catch(Exception ex)
            {
                throw new MessageUnreadableException(ex.Message);
            }
           
        }
    }
}
