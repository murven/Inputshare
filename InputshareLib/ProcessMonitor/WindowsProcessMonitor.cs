using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace InputshareLib.ProcessMonitor
{
    public class WindowsProcessMonitor : IProcessMonitor
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr wnd, out RECT rect);
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int metric);
        [DllImport("User32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


        public bool Monitoring { get; private set; }
        public bool FullscreenApplicationRunning { get; private set; }
        public Process CurrentFullscreenProcess { get; private set; }

        public event EventHandler<Process> ProcessEnteredFullscreen;
        public event EventHandler<Process> ProcessExitedFullscreen;


        private Timer monitorTimer;

        public void StartMonitoring()
        {
            if (Monitoring)
                return;


            monitorTimer = new Timer(monitorTimerCallback, null, 0, 1000);
            Monitoring = true;
        }

        private void monitorTimerCallback(object sync)
        {
            IntPtr window = GetForegroundWindow();

            if (window == null)
            {
                ISLogger.Write("GetForegroundWindow() returned null");
                return;
            }

            int w = GetSystemMetrics(0); //width of primay monitor
            int h = GetSystemMetrics(1); //height of primary monitor
            GetWindowRect(window, out RECT rect);

            int pw = Math.Abs(rect.left - rect.right);
            int ph = Math.Abs(rect.top - rect.bottom);

            //To detect a fullscreen application, we check to see if size of the foreground window is equal to the primary monitor
            //This only works for fullscreen applications that are displayed on primary monitor

            if (pw == w && ph == h)
            {
                if (!FullscreenApplicationRunning)
                {
                    CurrentFullscreenProcess = GetForegroundWindowProcess(window);
                    ProcessEnteredFullscreen?.Invoke(this, CurrentFullscreenProcess);
                    FullscreenApplicationRunning = true;
                }
            }
            else
            {
                if (FullscreenApplicationRunning)
                {
                    ProcessExitedFullscreen?.Invoke(this, CurrentFullscreenProcess);
                    CurrentFullscreenProcess = null;
                    FullscreenApplicationRunning = false;
                }
            }

        }

        private Process GetForegroundWindowProcess(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint pId);
            return Process.GetProcessById((int)pId);
        }

        public void StopMonitoring()
        {
            if (Monitoring)
            {
                monitorTimer?.Dispose();
                Monitoring = false;
            }
        }

        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}
