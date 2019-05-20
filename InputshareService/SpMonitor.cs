using InputshareLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text;

namespace InputshareService
{
    class SpMonitor
    {
        private ManagementEventWatcher processStartWatcher;
        private ManagementEventWatcher processStoppedWatcher;

        public event EventHandler SpStarted;
        public event EventHandler SpStopped;

        public string SpProcessName { get; set; } = "Inputsharesp";

        public bool Monitoring { get; private set; }
        public void StartMonitoring()
        {
            if (Monitoring)
                return;

            try
            {
                processStartWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
                processStartWatcher.EventArrived += ProcessStartWatcher_EventArrived;
                processStartWatcher.Start();
            }
            catch (Exception ex)
            {
                ISLogger.Write("Failed to start process start event watcher: " + ex.Message);
                return;
            }

            try
            {
                processStoppedWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
                processStoppedWatcher.EventArrived += ProcessStoppedWatcher_EventArrived;
                processStoppedWatcher.Start();
            }
            catch (Exception ex)
            {
                ISLogger.Write("Failed to start process start event watcher: " + ex.Message);
                return;
            }
            Monitoring = true;
        }
        
        public void StopMonitoring()
        {
            if (!Monitoring)
                return;
            Monitoring = false;

            processStartWatcher?.Dispose();
            processStoppedWatcher?.Dispose();
        }

        private void ProcessStoppedWatcher_EventArrived(object sender, EventArrivedEventArgs args)
        {
            if (!Monitoring)
                return;

            string pName = args.NewEvent.Properties["ProcessName"].Value.ToString();
            //ISLogger.Write($"{pName} stopped");
            if(pName.ToLower().Contains(SpProcessName.ToLower()))
            {
                SpStopped?.Invoke(this, null);
            }
        }

        private void ProcessStartWatcher_EventArrived(object sender, EventArrivedEventArgs args)
        {
            if (!Monitoring)
                return;

            string pName = args.NewEvent.Properties["ProcessName"].Value.ToString();
            //ISLogger.Write($"{pName} started");
            if (pName.ToLower().Contains(SpProcessName.ToLower()))
            {
                SpStarted?.Invoke(this, null);
            }
        }
    }
}
