using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.ClipboardMonitor
{
    public interface IClipboardManager
    {
        bool Monitoring { get; }

        event EventHandler<string> TextCopied;
        void SetText(string text);
        void StartMonitoring();
        void StopMonitoring();
    }
}
