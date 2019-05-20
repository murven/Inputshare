using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace InputshareLib.CursorMonitor
{
    public class WindowsCursorMonitor : ICursorMonitor
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int metric);

        public bool Running { get; private set; }

        public event EventHandler<BoundEdge> EdgeHit;

        private System.Threading.Timer monitorTimer;
        private int monitorTimerInterval = 50;

        private VirtualScreenBounds screenBounds;

        private Timer changeMonitorTimer;

        public void SetUpdateInterval(int interval)
        {
            monitorTimerInterval = interval;

            if (Running)
            {
                monitorTimer.Dispose();
                monitorTimer = new System.Threading.Timer(TimerCallback, null, 0, monitorTimerInterval);
            }
        }

        public void Start()
        {
            if (Running)
                throw new InvalidOperationException("Cursormonitor already running");

            screenBounds = CreateBounds();
            monitorTimer = new System.Threading.Timer(TimerCallback, null, 0, monitorTimerInterval);
            changeMonitorTimer = new Timer(ChangeMonitorTimerCallback, null, 0, 1500);
            Running = true;
        }
        
        private void ChangeMonitorTimerCallback(object sync)
        {
            screenBounds = CreateBounds();
        }

        private VirtualScreenBounds CreateBounds()
        {
            int left = GetSystemMetrics(76);//left
            int top = GetSystemMetrics(77);//top
            int w = GetSystemMetrics(78); //width
            int h = GetSystemMetrics(79); //height

            return new VirtualScreenBounds(left, left + w - 1, top, top + h - 1);
        }

        private void TimerCallback(object sync)
        {
            GetCursorPos(out POINT pos);
            if(pos.Y == screenBounds.Top)
            {
                EdgeHit?.Invoke(this, BoundEdge.Top);
            }
            else if (pos.Y == screenBounds.Bottom)
            {
                EdgeHit?.Invoke(this, BoundEdge.Bottom);
            }
            else if (pos.X == screenBounds.Left)
            {
                EdgeHit?.Invoke(this, BoundEdge.Left);
            }
            else if (pos.X == screenBounds.Right)
            {
                EdgeHit?.Invoke(this, BoundEdge.Right);
            }
        }

        public void Stop()
        {
            if (!Running)
                throw new InvalidOperationException("Cursormonitor not running");

            monitorTimer.Dispose();
            Running = false;
        }

    }
}
