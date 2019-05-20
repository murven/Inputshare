using System;
using InputshareLib;
using System.Windows.Forms;

namespace Inputshare
{
    class Program
    {

        [STAThread]
        public static void Main()
        {
            ISLogger.SetLogFileName(@".\logs\Inputshare.log");
            ISLogger.EnableConsole = false;
            ISLogger.EnableLogFile = true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
