using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace InputshareLib
{

    /// <summary>
    /// Manages a message only win32 window to receive&send window messages
    /// CreateWindow() must be called before calling any other method
    /// </summary>
    public class WinWindow
    {

        #region native
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr CreateWindowEx(int dwExStyle,
       //UInt16 regResult,
       [MarshalAs(UnmanagedType.LPStr)]
       string lpClassName,
       [MarshalAs(UnmanagedType.LPStr)]
       string lpWindowName,
       UInt32 dwStyle,
       int x,
       int y,
       int nWidth,
       int nHeight,
       IntPtr hWndParent,
       IntPtr hMenu,
       IntPtr hInstance,
       IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt16 RegisterClassEx(ref WNDCLASSEX classEx);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszClassName;
            public IntPtr hIconSm;

            //Use this function to make a new one with cbSize already filled in.
            //For example:
            //var WndClss = WNDCLASSEX.Build()
            public static WNDCLASSEX Build()
            {
                var nw = new WNDCLASSEX();
                nw.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
                return nw;
            }
        }

        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public UIntPtr wParam;
            public IntPtr lParam;
            public int time;
            public POINT pt;
        }
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
          uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern int GetMessage(out MSG message, IntPtr hwnd, uint min, uint max);

        [DllImport("user32.dll")]
        static extern void DispatchMessage(ref MSG message);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook,
            LLHookCallback lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook,
                    IntPtr lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        static extern IntPtr SetClipboardViewer(IntPtr newWin);

        [DllImport("user32.dll")]
        static extern bool ChangeClipboardChain(IntPtr hwndRemove, IntPtr hwndNext);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool AddClipboardFormatListener(IntPtr window);

        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProcA(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr intPtr, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool OpenClipboard(IntPtr newWnd);

        [DllImport("user32.dll")]
        static extern bool CloseClipboard();

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostThreadMessage(uint threadId, uint msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern void PostQuitMessage(int code);

        [DllImport("user32.dll")]
        static extern bool UnregisterClassA(string className, IntPtr hInstance);

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private const int WM_QUIT = 0x0012;
        private const int WM_CLOSE = 0x0010;

        private IntPtr KeyboardProcID = IntPtr.Zero;
        private IntPtr MouseProcID = IntPtr.Zero;

        private readonly static IntPtr HWND_MESSAGE = new IntPtr(-3);

        #endregion

        public delegate IntPtr LLHookCallback(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// The handle to the window, null if window does not exist
        /// </summary>
        public IntPtr WindowHandle { get; private set; }

        private delegate IntPtr WndProcCallback(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
        private WndProcCallback callbackDelegate;
        private WinEventDelegate wEventCallback;

        private WNDCLASSEX wndClass;
        private Thread procThread;
        private uint procThreadId;
        private bool exitThread = false;

        private LLHookCallback mouseCallback;
        private LLHookCallback keyboardCallback;
        private IntPtr hWinEventHook;

        public event EventHandler ClipboardContentChanged;
        public event EventHandler DesktopSwitched;

        /// <summary>
        /// Is the keyboard hook active
        /// </summary>
        public bool KeyboardHooked { get; private set; }

        /// <summary>
        /// Is the mouse hook active
        /// </summary>
        public bool MouseHooked { get; private set; }

        /// <summary>
        /// Is the window monitoring for clipboard changes
        /// </summary>
        public bool MonitoringClipboard { get; private set; }

        /// <summary>
        /// Is the window monitoring for desktop changes
        /// </summary>
        public bool MonitoringDesktopChanged { get; private set; }

        private ManualResetEventSlim windowCreatedEvent;

        /// <summary>
        /// Creates a message only window
        /// </summary>
        /// <param name="createThread">If true, the window will run on a dedicated thread</param>
        /// <param name="mouseCallback">Callback to receive mouse messages from the low level mouse hook (WH_MOUSE_LL). set to null to disable mouse hook</param>
        /// <param name="kbCallback">Callback to receive keyboard messages from the low level keyboard hook (WH_KEYBOARD_LL). set to null to disable keyboard hook</param>
        /// <param name="monitorDesktops">If true, the DesktopSwitched event will fire whenever the window receives EVENT_SYSTEM_DESKTOPSWITCH from the wineventhook</param>
        /// <param name="monitorClipboard">If true, ClipboardContentChanged will be fired when clipboard change message is received by the ClipboardFormatListener</param>
        public void CreateWindow(bool createThread, LLHookCallback mouseCallback, LLHookCallback kbCallback, bool monitorDesktops, bool monitorClipboard)
        {
            if (procThread != null)
            {
                ISLogger.Write("Warning: Can't create message window: thread already exists");
                return;
            }

            windowCreatedEvent = new ManualResetEventSlim(false);
            exitThread = false;

            if (createThread)
            {
                procThread = new Thread(() => WndThread(mouseCallback, kbCallback, monitorDesktops, monitorClipboard));
                procThread.SetApartmentState(ApartmentState.STA);
                procThread.IsBackground = false;
                procThread.Name = "WindowOnlyMessageThread";
                procThread.Start();
            }
            else
            {
                WndThread(mouseCallback, kbCallback, monitorDesktops, monitorClipboard);
            }

            //We want to wait until the window has actually been created before leaving this method to ensure calls are not made before window
            //is created properly. 2000ms timeout
            windowCreatedEvent.Wait(2000);
        }

        public void StartMonitoringClipboard()
        {
            if(!(WindowHandle != IntPtr.Zero &&  !MonitoringClipboard)){
                ISLogger.Write("Warning: Failed to start monitoring clipboard: Window does not exist OR already monitoring clipboard");
                return;
            }

            if (!AddClipboardFormatListener(WindowHandle))
            {
                ISLogger.Write("Warning: Failed to add clipboard listener: Win32 code {0}", Marshal.GetLastWin32Error().ToString("X"));
                return;
            }

            ISLogger.Write("WinWindow->Monitoring for clipboard changes");
        }


        public void StartMonitoringDesktopSwitches()
        {
            if (!(WindowHandle != IntPtr.Zero && !MonitoringDesktopChanged))
            {
                ISLogger.Write("Warning: cannot monitor desktop switches: window is not created OR already monitoring");
                return;
            }
            wEventCallback = WinEventCallback;
            hWinEventHook = SetWinEventHook(0x0020, 0x0020, IntPtr.Zero, wEventCallback, 0, 0, 0);
            MonitoringDesktopChanged = true;
            ISLogger.Write("WinWindow->Monitoring for desktop switches");
        }
    
        private IntPtr CreateMessageOnlyWindow()
        {
            wndClass = new WNDCLASSEX();
            wndClass.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
            callbackDelegate = WndProc;
            wndClass.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(callbackDelegate);
            wndClass.lpszClassName = "isclass";
            wndClass.cbWndExtra = 0;
            wndClass.hIcon = IntPtr.Zero;
            wndClass.hCursor = IntPtr.Zero;
            wndClass.hIconSm = IntPtr.Zero;
            wndClass.hbrBackground = IntPtr.Zero;
            wndClass.hInstance = Process.GetCurrentProcess().Handle;
            wndClass.lpszMenuName = null;
            ushort ret = RegisterClassEx(ref wndClass);
            
            if(ret == 0)
            {
                ISLogger.Write("Failed to create window class: win32 error " + Marshal.GetLastWin32Error());
                return IntPtr.Zero;
            }

            IntPtr window = CreateWindowEx(0, wndClass.lpszClassName, "ismsg", 0, 0, 0, 0, 0, HWND_MESSAGE, IntPtr.Zero, Process.GetCurrentProcess().Handle, IntPtr.Zero);

            if(window == IntPtr.Zero)
            {
                ISLogger.Write("Failed to create message only window - " + Marshal.GetLastWin32Error());
                return IntPtr.Zero;
            }

            return window;
        }

        public void CloseWindow()
        {
            exitThread = true;
            if(procThread != null)
            {
                SendMessage(WindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            UnhookWindowsHookEx(MouseProcID);
            UnhookWindowsHookEx(KeyboardProcID);
            UnregisterClassA("isclass", Process.GetCurrentProcess().Handle);
            WindowHandle = IntPtr.Zero;

        }
        private void WinEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            DesktopSwitched?.Invoke(this, null);
        }

        private void WndThread(LLHookCallback mouseCallback, LLHookCallback kbCallback, bool monitorDesktops, bool monitorClipboard)
        {
            procThreadId = GetCurrentThreadId();
            WindowHandle = CreateMessageOnlyWindow();
            windowCreatedEvent.Set();

            if (mouseCallback != null)
            {
                MouseProcID = SetMouseHook(mouseCallback);
            }
                

            if (kbCallback != null)
                KeyboardProcID = SetKeyboardHook(kbCallback);

            if (monitorDesktops)
                StartMonitoringDesktopSwitches();

            if (monitorClipboard)
                StartMonitoringClipboard();

            if(WindowHandle == IntPtr.Zero)
            {
                ISLogger.Write($"Failed to create window: Win32 error {Marshal.GetLastWin32Error().ToString("X")}");
                return;
            }


            while (!exitThread)
            {
                if (GetMessage(out MSG message, WindowHandle, 0, 0) != 0)
                {
                    DispatchMessage(ref message);
                }
                else
                {
                    ISLogger.Write("Recevied WM_QUIT");
                    break;
                }
            }

            ISLogger.Write("Window message loop exited");
        }

        private IntPtr WndProc(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam)
        {
            switch (message)
            {
                case WM_CLIPBOARDUPDATE:
                    ISLogger.Write("WinWindow->Clipboard content changed");
                    ClipboardContentChanged?.Invoke(this, null);
                    break;
                case WM_CLOSE:
                    PostQuitMessage(0);
                    break;
            }

            return DefWindowProcA(hwnd, message, wParam, lParam);
        }

        private IntPtr SetKeyboardHook(LLHookCallback callback)
        {
            keyboardCallback = callback;
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, keyboardCallback, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        private IntPtr SetMouseHook(LLHookCallback callback)
        {
            mouseCallback = callback;
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_MOUSE_LL, mouseCallback, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }
    }
}
