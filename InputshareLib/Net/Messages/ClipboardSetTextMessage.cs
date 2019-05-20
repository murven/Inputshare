using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.Net.Messages
{
    public class ClipboardSetTextMessage : INetworkMessage
    {
        public MessageType Type { get; }
        public string Text { get; }
        public int PartCount { get; }
        public int Part { get; }

        public ClipboardSetTextMessage(string text, int partCount, int part)
        {
            Type = MessageType.SetClipboardText;
            Text = text;
            PartCount = partCount;
            Part = part;
        }

        public byte[] ToBytes()
        {
            byte[] tData = Encoding.ASCII.GetBytes(Text);
            byte[] data = new byte[tData.Length + 11];
            Buffer.BlockCopy(BitConverter.GetBytes(data.Length-4), 0, data, 0, 4);
            data[4] = (byte)Type;
            data[5] = (byte)Part;
            data[6] = (byte)PartCount;
            Buffer.BlockCopy(BitConverter.GetBytes(tData.Length), 0, data, 7, 4);
            Buffer.BlockCopy(tData, 0, data, 11, tData.Length);
            return data;
        }

        public static ClipboardSetTextMessage FromBytes(byte[] data)
        {
            try
            {
                int tSize = BitConverter.ToInt32(data, 7);
                byte part = data[5];
                byte partCount = data[6];
                byte[] tData = new byte[tSize];
                Buffer.BlockCopy(data, 11, tData, 0, tSize);
                return new ClipboardSetTextMessage(Encoding.ASCII.GetString(tData), (int)part, (int)partCount);
            }
            catch(Exception ex)
            {
                throw new MessageUnreadableException(ex.Message);
            }
            
        }
    }
}
