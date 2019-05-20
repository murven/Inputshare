using System;

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
