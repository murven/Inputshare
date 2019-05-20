using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace InputshareLib
{
    public static class WinDesktop
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit,
        uint dwDesiredAccess);
        private const int MAXIMUM_ALLOWED = 0x02000000;

        [DllImport("user32.dll")]
        static extern IntPtr OpenDesktop(string desktop, uint flags, bool inherit, uint desiredAccess);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        static extern bool CloseDesktop(IntPtr hDesk);

        [DllImport("user32.dll")]
        static extern IntPtr GetThreadDesktop(uint thread);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        static extern bool GetUserObjectInformationA(IntPtr hObj, int nIndex, [Out] byte[] pvInfo, int infoLen, out int lenNeeded);

        public struct DESKTOPOBJECT
        {
            public DESKTOPOBJECT(string name, bool input)
            {
                Name = name;
                InputDesktop = input;
            }

            public string Name { get; }
            public bool InputDesktop { get; }
        }

        public static DESKTOPOBJECT GetThreadDesktop()
        {
            IntPtr hDesk = GetThreadDesktop(GetCurrentThreadId());

            GetUserObjectInformationA(hDesk, 2, null, 0, out int neededLen);
            IntPtr lpName = Marshal.AllocHGlobal((neededLen/2)+1);
            byte[] data = new byte[neededLen];
            if (!GetUserObjectInformationA(hDesk, 2, data, neededLen, out _))
            {
                ISLogger.Write("Failed to get current desktop name");
                return new DESKTOPOBJECT();
            }

            string dName = Encoding.ASCII.GetString(data);
            Marshal.FreeHGlobal(lpName);

            byte[] uio = new byte[1];
            GetUserObjectInformationA(hDesk, 6, uio, 1, out _);
            bool UIO_IO = uio[0] != 0;

            return new DESKTOPOBJECT(dName, UIO_IO);
        }

        public static void SwitchThreadToInputDesktop()
        {
            IntPtr iDesk = OpenInputDesktop(0, false, MAXIMUM_ALLOWED);

            if (iDesk == IntPtr.Zero)
            {
                ISLogger.Write($"Failed to open input desktop on thread {Thread.CurrentThread.Name}");
                return;
            }

            if (!SetThreadDesktop(iDesk))
            {
                ISLogger.Write($"Failed to set thread {Thread.CurrentThread.Name} to input desktop: {Marshal.GetLastWin32Error().ToString("X")}");
                return;
            }
            CloseDesktop(iDesk);
            //ISLogger.Write($"Thread {Thread.CurrentThread.Name} switched to input desktop");
        }

        public static void SwitchToDefaultDesktop()
        {
            IntPtr iDesk = OpenDesktop("Default", 0, false, MAXIMUM_ALLOWED);

            if (iDesk == IntPtr.Zero)
            {
                ISLogger.Write($"Failed to open default desktop on thread {Thread.CurrentThread.Name}");
                return;
            }

            if (!SetThreadDesktop(iDesk))
            {
                ISLogger.Write($"Failed to set thread {Thread.CurrentThread.Name} to desktop 'default': {Marshal.GetLastWin32Error().ToString("X")}");
                return;
            }
            CloseDesktop(iDesk);
            //ISLogger.Write($"Thread {Thread.CurrentThread.Name} switched to default desktop");
        }
        public static void SwitchToWinlogonDesktop()
        {
            IntPtr iDesk = OpenDesktop("Winlogon", 0, false, MAXIMUM_ALLOWED);

            if (iDesk == IntPtr.Zero)
            {
                ISLogger.Write($"Failed to open default desktop on thread {Thread.CurrentThread.Name}");
                return;
            }

            if (!SetThreadDesktop(iDesk))
            {
                ISLogger.Write($"Failed to set thread {Thread.CurrentThread.Name} to desktop 'Winlogon': {Marshal.GetLastWin32Error().ToString("X")}");
                return;
            }
            CloseDesktop(iDesk);
            //ISLogger.Write($"Thread {Thread.CurrentThread.Name} switched to Winlogon desktop");
        }
    }
}
