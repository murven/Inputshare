using InputshareLib.Input;

namespace InputshareLib.Ouput
{
    public interface IOutputManager
    {
        void Send(ISInputData input);
        void ReleaseAllKeys();
    }
}
