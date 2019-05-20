using InputshareLib;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace InputshareSP
{
    /// <summary>
    /// Runs a dedicated thread on the window stations default desktop.
    /// This is mainly used to Copy/Paste to the clipboard from a window running
    /// on the WinLogon desktop
    /// </summary>
    public class DefaultDesktopThread
    {
        private Thread dDeskThread;
        private BlockingCollection<Action> invokeQueue;
        private CancellationTokenSource cancelToken;

        public DefaultDesktopThread()
        {
            invokeQueue = new BlockingCollection<Action>();
            cancelToken = new CancellationTokenSource();
            dDeskThread = new Thread(ThreadInit);
            //dDeskThread.SetApartmentState(ApartmentState.STA);
            dDeskThread.Name = "DefaultDesktop";
            dDeskThread.Priority = ThreadPriority.Highest;
            dDeskThread.Start();
        }

        public void InvokeAction(Action action)
        {
            invokeQueue.Add(action);
        }

        private void ThreadInit()
        {
            WinDesktop.SwitchToDefaultDesktop();

            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    Action invoke = invokeQueue.Take(cancelToken.Token);
                    invoke();
                }
                catch (Exception ex)
                {
                    ISLogger.Write("An error occured while invoking method on desktop thread: {0}", ex.Message);
                }

            }
            ISLogger.Write("Default desktop thread exited");
        }
    }
}
