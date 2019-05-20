namespace InputshareLib.Net.Messages
{
    public interface INetworkMessage
    {
        MessageType Type { get; }
        byte[] ToBytes();
    }
}
