using System;

namespace InputshareLib.Net.Messages
{
    class MessageUnreadableException : Exception
    {
        public MessageUnreadableException(string message) : base(message)
        {

        }

        public MessageUnreadableException() : base()
        {

        }
    }
}
