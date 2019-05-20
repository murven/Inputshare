using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib
{
    public class InvalidOperatingSystemException : Exception
    {
        public InvalidOperatingSystemException(string message) : base(message)
        {

        }
    }
}
