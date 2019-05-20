namespace InputshareLib.Net.Messages
{
    public enum MessageType : byte
    {
        INVALID = 0,
        ServerOK = 1,
        ClientDuplicateName = 2,
        ClientDuplicateGuid = 3,
        ClientLimitReached = 4,
        ClientLoginInfo = 5,
        Heartbeat = 6,
        Input = 7,
        ClientBoundsTop = 8,
        ClientBoundsLeft = 9,
        ClientBoundsRight = 10,
        ClientBoundsBottom = 11,
        ClientInFocus = 12,
        ClientOutOfFocus = 13,
        SetClipboardText = 14,
        ClientEnableReportEdge = 15,
        ClientDisableReportEdge = 16
    }
}
