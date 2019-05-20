using System;

namespace InputshareLib.CursorMonitor
{
    public interface ICursorMonitor
    {
        event EventHandler<BoundEdge> EdgeHit;
        void SetUpdateInterval(int interval);
        void Start();
        void Stop();
        bool Running { get; }
    }
    public struct VirtualScreenBounds
    {
        public VirtualScreenBounds(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public int Left { get; }
        public int Right { get; }
        public int Top { get; }
        public int Bottom { get; }
    }
}
