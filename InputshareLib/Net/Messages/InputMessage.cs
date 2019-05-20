using InputshareLib.Input;
using System;

namespace InputshareLib.Net.Messages
{
    public class InputMessage : INetworkMessage
    {
        public MessageType Type { get; } = MessageType.Input;

        public ISInputData Input { get; }

        public InputMessage(ISInputData input)
        {
            Input = input;
        }

        public byte[] ToBytes()
        {
            byte[] data = new byte[10];
            Buffer.BlockCopy(BitConverter.GetBytes(data.Length-4), 0, data, 0, 4);
            data[4] = (byte)Type;
            data[5] = (byte)Input.Code;
            Buffer.BlockCopy(BitConverter.GetBytes(Input.Param1), 0, data, 6, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Input.Param2), 0, data, 8, 2);
            return data;
        }

        public static InputMessage FromBytes(byte[] data)
        {
            try
            {
                return new InputMessage(new ISInputData((ISInputCode)data[5],
                BitConverter.ToInt16(data, 6), BitConverter.ToInt16(data, 8)));
            }catch(Exception ex)
            {
                throw new MessageUnreadableException(ex.Message);
            }
            
        }
    }
}
