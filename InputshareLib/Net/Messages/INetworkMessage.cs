using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.Net.Messages
{
    public interface INetworkMessage
    {
        MessageType Type { get; }
        byte[] ToBytes();
    }
}
