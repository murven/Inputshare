using InputshareLib;
using InputshareLib.CursorMonitor;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace InputshareSP
{
    /// <summary>
    /// This class monitors the position of the cursor on the screen to detect when the cursor is at the edge of the screen
    /// This is used instead of InputshareLib/CursorMonitor as it can switch to the current input desktop (required to use GetCursorPos())
    /// </summary>
    class DesktopCursorMonitor 
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int metric);

        public bool Running { get; private set; }

        public event EventHandler<BoundEdge> EdgeHit;

        private System.Threading.Timer monitorTimer;
        private int monitorTimerInterval = 50;
        private VirtualScreenBounds screenBounds;

        /// <summary>
        /// Timer used to perodically update the screen bounds incase any display settings are changed.
        /// </summary>
        private Timer changeMonitorTimer;

        /// <summary>
        /// Set this to true to switch the timer callback thread to the current input desktop (TODO - timer callback threading?)
        /// </summary>
        public bool SwitchToInputDesktop { get; set; }

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
            Thread.CurrentThread.Name = "CursorMonitorThread";

            if (SwitchToInputDesktop)
            {
                SwitchToInputDesktop = false;
                WinDesktop.SwitchThreadToInputDesktop();
            }

            if(!GetCursorPos(out POINT pos))
            {
                if(!WinDesktop.GetThreadDesktop().InputDesktop)
                {
                    WinDesktop.SwitchThreadToInputDesktop();
                }
                
                return;
            }

            if (pos.Y == screenBounds.Top)
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
