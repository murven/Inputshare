using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace InputshareLib
{
    public static class ISLogger
    {
        private static string logFileName = "Inputshare.log";
        private static readonly object syncObject = new object();
        public static bool EnableConsole { get; set; }
        public static bool EnableLogFile { get; set; } = false;

        public static bool EnableDebugLog { get; set; } = false;

        private static Thread logThread;
        private static CancellationTokenSource logCancelSource;

        private static BlockingCollection<string> logQueue;

        public static event EventHandler<string>  MessageOut;

        static ISLogger()
        {

        }

        public static void Exit()
        {
            logCancelSource?.Cancel();
            logThread = null;
        }

        private static void CreateThread()
        {
            if (logThread != null)
                return;

            logCancelSource = new CancellationTokenSource();
            logThread = new Thread(ThreadLoop);
            logQueue = new BlockingCollection<string>();
            logThread.IsBackground = true;
            logThread.Start();
        }

        private static void ThreadLoop()
        {
            try
            {
                while (!logCancelSource.IsCancellationRequested)
                {
                    string str = logQueue.Take(logCancelSource.Token);

                    if (EnableDebugLog)
                        Debug.Write(str);

                    if (EnableConsole)
                        Console.Write(str);

                    try
                    {
                        File.AppendAllText(logFileName, str);
                    }
                    catch (Exception)
                    {

                    }

                    MessageOut?.Invoke(null, str);
                }
            }
            catch (OperationCanceledException)
            {

            }
            
        }

        public static void SetLogFileName(string name)
        {
            logFileName = name;
        }

        public static void Write(string message)
        {
            if (logThread == null)
                CreateThread();

            logQueue.Add(string.Format("{0}: {1}{2}",
                    DateTime.Now.ToShortTimeString(), message, Environment.NewLine));

        }

        public static void Write(string message, params object[] args)
        {
            try
            {
                Write(string.Format(message, args));
            }
            catch (Exception)
            {
                Write(message);
            }
            
        }
    }
}
