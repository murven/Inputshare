using InputshareLib;
using InputshareLib.Input;
using InputshareLib.Ouput;
using System.Collections.Concurrent;
using System.Threading;

namespace InputshareSP
{
    /// <summary>
    /// This class is used to make sure that all sendinput() calls are made to the correct desktop
    /// by switching desktop whenever a desktop switch is detected.
    /// 
    /// When switchthreaddesktop is set to true, the thread will switch to the input desktop before the next input is processed
    /// </summary>
    class spOutputThread
    {
        private Thread outputThread;
        public BlockingCollection<ISInputData> outputQueue { get; private set; }
        private CancellationTokenSource cancelToken;

        private WindowsOutputManager outManager;
        public bool SwitchThreadDesktop { get; set; }

        public spOutputThread()
        {
        }
            
        public void CreateThread()
        {
            if(outputThread != null)
            {
                ISLogger.Write("Warning: Cannot create output thread: another thread is already running");
                return;
            }

            outputThread = new Thread(ThreadLoop);
            outputThread.Name = "OutputThread";
            outputQueue = new BlockingCollection<ISInputData>();
            cancelToken = new CancellationTokenSource();
            outManager = new WindowsOutputManager();

            outputThread.Start();
        }

        private void ThreadLoop()
        {
            while (!cancelToken.IsCancellationRequested)
            {
                ISInputData input = outputQueue.Take();

                if (SwitchThreadDesktop)
                {
                    WinDesktop.SwitchThreadToInputDesktop();
                    SwitchThreadDesktop = false;
                }

                outManager.Send(input);
            }

            ISLogger.Write("Exited output thread loop");
        }
    }
}
