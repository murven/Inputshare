using System;
using System.Diagnostics;

namespace InputshareLib.ProcessMonitor
{
    public interface IProcessMonitor
    {
        event EventHandler<Process> ProcessEnteredFullscreen;
        event EventHandler<Process> ProcessExitedFullscreen;
        Process CurrentFullscreenProcess { get; }
        bool FullscreenApplicationRunning { get; }
        bool Monitoring { get; }
        void StartMonitoring();
        void StopMonitoring();

    }
}
