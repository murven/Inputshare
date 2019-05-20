using InputshareLib;
using System;

namespace InputshareSP
{
    class Program
    {
        static void Main(string[] args)
        {
            ISLogger.SetLogFileName(@".\logs\InputshareSP.log");
            ISLogger.EnableConsole = false;
            ISLogger.EnableDebugLog = true;
            ISLogger.EnableLogFile = true;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Console.CancelKeyPress += Console_CancelKeyPress;
            SP p = new SP();
            p.Run(args);
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("UNHANDLED EXCEPTION");
            Exception ex = e.ExceptionObject as Exception;

            while(ex.InnerException != null) { ex = ex.InnerException; }
            
            ISLogger.Write(ex.Message);
            ISLogger.Exit();
        }
    }
}
