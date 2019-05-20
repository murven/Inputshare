using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.Input
{
    public struct ISInputData
    {
        public ISInputData(ISInputCode code, short param1, short param2)
        {
            Code = code;
            Param1 = param1;
            Param2 = param2;
        }

        public ISInputCode Code { get; }
        public short Param1 { get; }
        public short Param2 { get; }
    }
}
