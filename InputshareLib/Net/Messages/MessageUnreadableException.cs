using System;
using System.Collections.Generic;
using System.Text;

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
