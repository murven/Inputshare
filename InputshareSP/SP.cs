using InputshareLib;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using InputshareLib.AnonIPC;
using System.Security.Principal;

namespace InputshareSP
{
    class SP
    {
        private DesktopCursorMonitor curMonitor;
        private spOutputThread outThread;
        private DefaultDesktopThread dDeskThread;

        private WinWindow winLogonMsgWindow;

        private volatile bool ignoreClipboardData = false;

        private AnonPipeClientRead ipcRead;
        private AnonPipeClient ipcWrite;

        public SP()
        {
            //y
            ISLogger.Write($"InputshareSP started");
        }
        public void Run(string[] args)
        {
            Thread.CurrentThread.Name = "Main";

            if(WindowsIdentity.GetCurrent().Name != @"NT AUTHORITY\SYSTEM")
            {
                ISLogger.Write("Current user: " + WindowsIdentity.GetCurrent().Name);
                ISLogger.Write("InputshareSP can only be launched from the inputshare service");
                Thread.Sleep(3000);
                return;
            }

            //We need to check that the process was launched with the correct args (read and write pipe handles)
            ISLogger.Write("Process args: ({0})", args.Length);
            if (args.Length != 2)
            {
                ISLogger.Write("Started with invalid args... exiting");
                return;
            }
            else
            {
                foreach (var arg in args)
                {
                    ISLogger.Write("Param: " + arg);
                }
            }

            WinDesktop.DESKTOPOBJECT desk = WinDesktop.GetThreadDesktop();
            ISLogger.Write($"Current desktop: '{desk.Name}' Input desktop: {desk.InputDesktop}");

            //We don't want to create a window on a thread that belongs to the default desktop to
            //prevent shatter attacks. SP will exit if the current thread is not winlogon
            //TODO - fix spaces in name returned from windesktop.GetThreadDesktop()
            if(desk.Name.Contains("WinLogon"))
            {
                ISLogger.Write($"Failed to start: Current desktop is not 'Winlogon' ({desk.Name})");
                return;
            }

            dDeskThread = new DefaultDesktopThread();
            winLogonMsgWindow = new WinWindow();
            winLogonMsgWindow.CreateWindow(true, null, null, true, true);
            winLogonMsgWindow.DesktopSwitched += WinLogonMsgWindow_DesktopSwitched;
            winLogonMsgWindow.ClipboardContentChanged += WinLogonMsgWindow_ClipboardContentChanged;

            outThread = new spOutputThread();
            outThread.CreateThread();
            curMonitor = new DesktopCursorMonitor();
            curMonitor.EdgeHit += CurMonitor_EdgeHit;
            curMonitor.Start();

            curMonitor.SwitchToInputDesktop = true;
            outThread.SwitchThreadDesktop = true;

            ipcRead = new AnonPipeClientRead(args[1]);
            ipcRead.InputDataReceived += IpcClient_InputDataReceived;
            ipcRead.CopyToClipboardReceived += IpcClient_CopyToClipboardReceived;

            ipcWrite = new AnonPipeClient(args[0]);

            ISLogger.Write("IPC client created");
            ISLogger.Write("IPC server started");

            ISLogger.Write("SP running");
        }

        //To set the clipboard text, we need to be on the default desktop
        //We will invoke the setclipboard onto 
        //the dedicated thread running on the default desktop
        private void IpcClient_CopyToClipboardReceived(object sender, string cbText)
        {
            ISLogger.Write("IPC->Copying clipboard text");
            ignoreClipboardData = true;
            dDeskThread.InvokeAction(() => SetClipboard(cbText));
        }

        /// <summary>
        /// Called when the IPC client receives input data from the service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="input"></param>
        private void IpcClient_InputDataReceived(object sender, InputshareLib.Input.ISInputData input)
        {
            outThread?.outputQueue.Add(input);
        }

        //When the message only window running on winlogon desktop detects a desktop switch,
        //Both the cursor monitor and output thread need to be switched to the new input desktop
        private void WinLogonMsgWindow_DesktopSwitched(object sender, EventArgs e)
        {
            curMonitor.SwitchToInputDesktop = true;
            outThread.SwitchThreadDesktop = true;
        }

        private void WinLogonMsgWindow_ClipboardContentChanged(object sender, EventArgs e)
        {
            if (ignoreClipboardData)
            {
                ignoreClipboardData = false;
                return;
            }

            ISLogger.Write("Clipboard content changed");
            dDeskThread.InvokeAction(ReadClipboard);
        }

        //This method reads the text (if any) from the clipboard
        //--This method needs be invoked on the dedicated desktop thread!
        private void ReadClipboard()
        {
            string str = WinClipboard.ReadText();
            if(str == null)
            {
                ISLogger.Write("GetClipboardText returned null");
                return;
            }

            ipcWrite.SendClipboardTextCopied(str);
        }

        //Writes a string to the clipboard
        //--This method needs be invoked on the dedicated desktop thread!
        private void SetClipboard(string text)
        {
            ignoreClipboardData = true;
            if (!WinClipboard.SetText(text))
            {
                ISLogger.Write("Failed to set clipboard text: Win32 code {0}", Marshal.GetLastWin32Error().ToString("X"));
                return;
            }

            ISLogger.Write("Clipboard text set");
        }

        private void CurMonitor_EdgeHit(object sender, BoundEdge edge)
        {
            //Tell the service that the cursor has hit the specific edge
            ipcWrite.SendEdgeHit(edge);
        }
    }
}
