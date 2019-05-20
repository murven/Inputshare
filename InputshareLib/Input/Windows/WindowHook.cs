using InputshareLib;
using Inputshare;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Inputshare.Input.Windows.WindowHookWin32;

namespace Inputshare.Input.Windows
{
    static class WindowHook
    {
        public static event Action<WinInput> InputReceived;

        public static bool Running { get; private set; } = false;

        /// <summary>
        /// Disables user input while still listening for hotkeys
        /// </summary>
        public static bool BlockInputs { get; set; } = false;

        private static IntPtr KeyboardProcPtr = IntPtr.Zero;
        private static IntPtr MouseProcPtr = IntPtr.Zero;


        public static void Start(HookCallback keyboardCallback, HookCallback mouseCallback)
        {
            OSHelper.Os os = OSHelper.GetOsVersion();
            if(os.System != OSHelper.Platform.Windows)
                throw new InvalidOperatingSystemException("Windowhook can only be run on windows machines!");

            if (Running)
                throw new InvalidCastException("Windowhook already running");

            ISLogger.Write("Starting Windowhook on " + os);

            KeyboardProcPtr = SetKeyboardHook(keyboardCallback);
            if (KeyboardProcPtr == IntPtr.Zero)
                throw new Win32Exception("An error occurred while installing keyboard hook. Win32 error code " + Marshal.GetLastWin32Error());
            else
                ISLogger.Write("Installed keyboard hook");

            MouseProcPtr = SetMouseHook(mouseCallback);
            if (MouseProcPtr == IntPtr.Zero)
                throw new Win32Exception("An error occurred while installing mouse hook. Win32 error code " + Marshal.GetLastWin32Error());
            else
                ISLogger.Write("Installed Mouse hook");

            Running = true;
        }

        public static void Stop()
        {
            if (!Running)
                throw new InvalidOperationException("Windowhook was not running");

            try
            {
                if (!UnhookWindowsHookEx(KeyboardProcPtr))
                    throw new Win32Exception("Failed to uninstall keyboard hook. Win32 error code " + Marshal.GetLastWin32Error());
                else
                    ISLogger.Write("Uninstalled keyboard hook");

                if (!UnhookWindowsHookEx(MouseProcPtr))
                    throw new Win32Exception("Failed to uninstall mouse hook. Win32 error code " + Marshal.GetLastWin32Error());
                else
                    ISLogger.Write("Uninstalled mouse hook");
            }
            finally
            {
                Running = false;  
            }
        }

        private static IntPtr SetKeyboardHook(HookCallback callback)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, callback, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        private static IntPtr SetMouseHook(HookCallback callback)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_MOUSE_LL, callback, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        public struct WinInput
        {
            public WinInput(IntPtr wp, IntPtr lp)
            {
                WParam = wp;
                LParam = lp;
            }
            public IntPtr WParam;
            public IntPtr LParam;
        }
    }
}
