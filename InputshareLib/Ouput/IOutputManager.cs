using InputshareLib.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.Ouput
{
    public interface IOutputManager
    {
        void Send(ISInputData input);
        void ReleaseAllKeys();
    }
}
