using System;
using System.Runtime.InteropServices;

namespace InputshareLib.ClipboardMonitor
{
    class WindowsClipboardManager : IClipboardManager
    {
        public bool Monitoring { get; private set; }

        public event EventHandler<string> TextCopied;

        private WinWindow cbMonitorWindow;

        public void StartMonitoring()
        {
            if (Monitoring)
                return;
            cbMonitorWindow = new WinWindow();
            cbMonitorWindow.CreateWindow(true, null, null, false, true);
            cbMonitorWindow.ClipboardContentChanged += CbMonitorWindow_ClipboardContentChanged;
        }

        public void SetText(string text)
        {
            if (!WinClipboard.SetText(text))
            {
                ISLogger.Write("WindowsClipboardMonitor: Failed to set text: Win32 code {0}", Marshal.GetLastWin32Error().ToString("X"));
                return;
            }
        }

        private void CbMonitorWindow_ClipboardContentChanged(object sender, EventArgs e)
        {
            string text = WinClipboard.ReadText();

            if (text == null)
            {
                ISLogger.Write("WindowsClipboardMonitor: Failed to read clipboard: win32 code {0}", Marshal.GetLastWin32Error().ToString("X"));
                return;
            }

            TextCopied?.Invoke(this, text);
        }

        public void StopMonitoring()
        {
            if (!Monitoring)
                return;

            cbMonitorWindow.CloseWindow();
        }
    }
}
