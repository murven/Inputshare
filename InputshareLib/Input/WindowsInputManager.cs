using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using InputshareLib.Input.Hotkeys;
using static Inputshare.Input.Windows.WindowHookWin32;
using static InputshareLib.Settings;
using System.Collections.Concurrent;

namespace InputshareLib.Input
{
    public class WindowsInputManager : InputManager
    {

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public override event EventHandler<ISInputData> InputReceived;
        public override event EventHandler<FunctionHotkey> FunctionHotkeyPressed;
        public override event EventHandler<ClientHotkey> ClientHotkeyPressed;
        public override event EventHandler<string> ClipboardTextCopied;

        private WinWindow.LLHookCallback mouseCallback;
        private WinWindow.LLHookCallback keyboardCallback;

        private BlockingCollection<Win32Msg> inputQueue;
        private Thread isInputThread;
        private CancellationTokenSource isInputThreadTokenSource;

        private Timer mPosTimer;
        private POINT mPos;

        private WinWindow msgWindow;

        private const int INPUTSHARE_CLIPBOARDTEXTCOPY = 601;

        private class Win32Msg
        {
            public uint wParam;
            public object lParam;
            public string cbText;
        }

        public WindowsInputManager()
        {
        }

        public override void BlockUserInput(bool block)
        {
            if (block)
            {
                UserInputBlocked = true;
                GetCursorPos(out mPos);
            }
            else
            {
                UserInputBlocked = false;
            }
        }

        /// <summary>
        /// Thread message loop.
        /// To stop windows becoming unresponsive due to us not returning from the hook callback methods fast enough, we process
        /// inputs on a dedicated thread.
        /// </summary>
        private void ThreadLoop(CancellationToken token)
        {

            try
            {
                while (!token.IsCancellationRequested)
                {
                    Win32Msg msg = inputQueue.Take(token);

                    int cmd = (int)msg.wParam;

                    if (cmd == WM_KEYDOWN || cmd == WM_KEYUP || cmd == WM_SYSKEYDOWN || cmd == WM_SYSKEYUP)
                    {
                        IsThreadHandleKeyboard(msg.wParam, (KBDLLHOOKSTRUCT)msg.lParam);
                    }
                    else if (cmd >= 512 && cmd <= 524)
                    {
                        IsThreadHandleMouse(msg.wParam, (MSLLHOOKSTRUCT)msg.lParam);
                    }
                    else if (cmd == INPUTSHARE_CLIPBOARDTEXTCOPY)
                    {
                        ClipboardTextCopied?.Invoke(this, msg.cbText);
                    }
                    else
                    {
                        ISLogger.Write("Unhandled wparam code {0}", msg.wParam);
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }

            ISLogger.Write("Input thread message queue exited");
        }

        ISInputData kbData = new ISInputData();
        private void IsThreadHandleKeyboard(uint wParam, KBDLLHOOKSTRUCT keyboardStruct)
        {

            int code = (int)wParam;
            switch (code)
            {
                case WM_SYSKEYDOWN:
                case WM_KEYDOWN:
                    {
                        if (keyboardStruct.scanCode == 0)
                        {
                            ISLogger.Write("Cannot get scancode for virtual key {0}", keyboardStruct.vkCode);
                            return;
                        }


                        if (keyboardStruct.scanCode == (int)ScanCode.Control)
                        {
                            currentModifiers |= Hotkey.Modifiers.Ctrl;
                        }
                        else if (keyboardStruct.scanCode == (int)ScanCode.Alt)
                        {
                            currentModifiers |= Hotkey.Modifiers.Alt;
                        }
                        else if (keyboardStruct.scanCode == (int)ScanCode.LShift | keyboardStruct.scanCode == (int)ScanCode.RShift)
                        {
                            currentModifiers |= Hotkey.Modifiers.Shift;
                        }

                        Hotkey[] list = hotkeyList.ToArray();

                        for (int i = 0; i < list.Length; i++)
                        {
                            if ((keyboardStruct.scanCode == (short)list[i].HkScan) && (currentModifiers == list[i].Mods))
                            {
                                if (list[i] is ClientHotkey)
                                {
                                    ClientHotkey hk = list[i] as ClientHotkey;
                                    ClientHotkeyPressed?.Invoke(this, hk);
                                }
                                else if (list[i] is FunctionHotkey)
                                {
                                    FunctionHotkey hk = list[i] as FunctionHotkey;
                                    FunctionHotkeyPressed?.Invoke(this, hk);
                                }
                            }
                        }

                        kbData = new ISInputData(ISInputCode.IS_KEYDOWN, (short)keyboardStruct.scanCode, 0);
                        break;
                    }
                case WM_SYSKEYUP:
                case WM_KEYUP:
                    {
                        if (keyboardStruct.scanCode == (int)ScanCode.Control)
                        {
                            currentModifiers &= ~Hotkey.Modifiers.Ctrl;
                        }
                        else if (keyboardStruct.scanCode == (int)ScanCode.Alt)
                        {
                            currentModifiers &= ~Hotkey.Modifiers.Alt;
                        }
                        else if (keyboardStruct.scanCode == (int)ScanCode.LShift | keyboardStruct.scanCode == (int)ScanCode.RShift)
                        {
                            currentModifiers &= ~Hotkey.Modifiers.Shift;
                        }
                        kbData = new ISInputData(ISInputCode.IS_KEYUP, (short)keyboardStruct.scanCode, 0);
                        break;
                    }
                default:
                    {
                        ISLogger.Write("Unexpected windows keyboard input code " + code);
                        return;
                    }
            }


            if (UserInputBlocked)
            {
                InputReceived?.Invoke(this, kbData);
            }
        }

        private ISInputData mouseData = new ISInputData();
        private void IsThreadHandleMouse(uint wParam, MSLLHOOKSTRUCT mouseStruct)
        {
            int code = (int)wParam;

            switch (code)
            {
                case WM_MOUSEMOVE:
                    {
                        short relX = (short)(mouseStruct.pt.X - mPos.X);
                        short relY = (short)(mouseStruct.pt.Y - mPos.Y);

                        //Mouse hooks can sometimes report a mouse move that isnt enough for the mouse to move a single pixel, we ignore these messages
                        //Sometimes the hook reports movements that make no sense (eg 1000X 1000Y) we also want to ingore these.
                        if ((relX == 0 && relY == 0) || relX > 100 || relY > 100 || relX < -100 || relY < -100)
                        {
                            mouseData = new ISInputData(ISInputCode.IS_UNKNOWN, 0, 0);
                            return;
                        }

                        mouseData = new ISInputData(ISInputCode.IS_MOUSEMOVERELATIVE, relX, relY);
                        break;
                    }
                case WM_LMOUSEDOWN:
                    {
                        mouseData = new ISInputData(ISInputCode.IS_MOUSELDOWN, 0, 0);
                        break;
                    }
                case WM_LMOUSEUP:
                    {
                        mouseData = new ISInputData(ISInputCode.IS_MOUSELUP, 0, 0);
                        break;
                    }
                case WM_RMOUSEDOWN:
                    {
                        mouseData = new ISInputData(ISInputCode.IS_MOUSERDOWN, 0, 0);
                        break;
                    }
                case WM_RMOUSEUP:
                    {
                        mouseData = new ISInputData(ISInputCode.IS_MOUSERUP, 0, 0);
                        break;
                    }
                case WM_MBUTTONDOWN:
                    {
                        mouseData = new ISInputData(ISInputCode.IS_MOUSEMDOWN, 0, 0);
                        break;
                    }
                case WM_MBUTTONUP:
                    {
                        mouseData = new ISInputData(ISInputCode.IS_MOUSEMUP, 0, 0);
                        break;
                    }
                case WM_MOUSEWHEEL:
                    {
                        //TODO - implement X axis scrolling
                        mouseData = new ISInputData(ISInputCode.IS_MOUSEYSCROLL, unchecked((short)((long)mouseStruct.mouseData >> 16)), 0);
                        break;
                    }
                case WM_XBUTTONDOWN:
                    {
                        mouseData = new ISInputData(ISInputCode.IS_MOUSEXDOWN, unchecked((short)((long)mouseStruct.mouseData >> 16)), 0);
                        break;
                    }
                case WM_XBUTTONUP:
                    {
                        mouseData = new ISInputData(ISInputCode.IS_MOUSEXUP, unchecked((short)((long)mouseStruct.mouseData >> 16)), 0);
                        break;
                    }
                default:
                    ISLogger.Write("Unexpected windows mouse input code " + code.ToString("X"));
                    mouseData = new ISInputData(ISInputCode.IS_UNKNOWN, 0, 0);
                    return;
            }


            if (UserInputBlocked)
            {
                if (mouseData.Code != ISInputCode.IS_UNKNOWN)
                {
                    InputReceived?.Invoke(this, mouseData);
                }
                else
                {
                    ISLogger.Write($"Error: invalid mouse input data {mouseData.Code}:{mouseData.Param1}:{mouseData.Param2}");
                }
            }
        }

        public override void Start()
        {
            if (Running)
                throw new InvalidOperationException("Windows input manager was already running");

            hotkeyList = new List<Hotkey>();

            mouseCallback = MouseCallback;
            keyboardCallback = KeyboardCallback;
            msgWindow = new WinWindow();
            if (DEBUG_DISABLEHOOK)
                msgWindow.CreateWindow(true, null, null, false, true);
            else
                msgWindow.CreateWindow(true, mouseCallback, keyboardCallback, false, true);
            Running = true;
            msgWindow.ClipboardContentChanged += MsgWindow_ClipboardContentChanged;


            mPosTimer = new Timer(MPosTimerCallback, null, 0, 100);
            inputQueue = new BlockingCollection<Win32Msg>();
            isInputThreadTokenSource = new CancellationTokenSource();
            isInputThread = new Thread(() => ThreadLoop(isInputThreadTokenSource.Token));
            isInputThread.Name = "Input thread";
            isInputThread.IsBackground = false;
            isInputThread.Start();
           
        }

        private void MsgWindow_ClipboardContentChanged(object sender, EventArgs e)
        {
            string text = WinClipboard.ReadText();
            if (text == null)
                return;

            ISLogger.Write("INPUTMANAGER->OnClipboardCopied");
            inputQueue?.Add(new Win32Msg { wParam = INPUTSHARE_CLIPBOARDTEXTCOPY, cbText = text });
        }

        private void MPosTimerCallback(object sync)
        {
            if (UserInputBlocked)
                GetCursorPos(out mPos);
        }


        private IntPtr MouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (UserInputBlocked)
            {
                MSLLHOOKSTRUCT str = new MSLLHOOKSTRUCT();
                Marshal.PtrToStructure(lParam, str);
                inputQueue?.Add(new Win32Msg { wParam = (uint)wParam, lParam = str });
                return new IntPtr(-1);
            }

            return IntPtr.Zero;
        }

        private IntPtr KeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            KBDLLHOOKSTRUCT str = new KBDLLHOOKSTRUCT();
            Marshal.PtrToStructure(lParam, str);
            inputQueue?.Add(new Win32Msg { wParam = (uint)wParam, lParam = str });

            if (UserInputBlocked)
                return new IntPtr(-1);

            return IntPtr.Zero;
        }

        public override void Stop()
        {
            mPosTimer?.Dispose();
            msgWindow.CloseWindow();
            if (isInputThread != null)
            {
                isInputThreadTokenSource.Cancel();
                isInputThread = null;
            }
        }
    }
}
