using System;

namespace InputshareLib
{
    public class InvalidOperatingSystemException : Exception
    {
        public InvalidOperatingSystemException(string message) : base(message)
        {

        }
    }
}
