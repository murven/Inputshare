using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace InputshareLib
{
    public static class WinClipboard
    {
        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("User32.dll", SetLastError = true)]
        static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        static extern bool EmptyClipboard();

        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern int GlobalSize(IntPtr hMem);

        private const int MaxReadStringSize = 1024 * 10 * 10*10;   //set clipboard read limit to 1MB
        private const int MaxWriteStringSize = 1024 * 10 * 10 * 10;
        public static bool SetText(string text)
        {

            if(Encoding.Unicode.GetByteCount(text) > MaxWriteStringSize)
            {
                ISLogger.Write("Cannot write string to clipboard: String too large");
                return false;
            }

            OpenClipboard();

            EmptyClipboard();
            IntPtr hGlobal = default;
            try
            {
                var bytes = (text.Length + 1) * 2;
                hGlobal = Marshal.AllocHGlobal(bytes);

                if (hGlobal == default)
                {
                    return false;
                }

                var target = GlobalLock(hGlobal);

                if (target == default)
                {
                    return false;
                }

                try
                {
                    Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
                }
                finally
                {
                    GlobalUnlock(target);
                }

                if (SetClipboardData(13, hGlobal) == default)
                {
                    return false;
                }

                hGlobal = default;
            }
            finally
            {
                if (hGlobal != default)
                {
                    Marshal.FreeHGlobal(hGlobal);
                }

                CloseClipboard();
            }

            return true;
        }
        private static bool OpenClipboard()
        {
            var num = 10;
            while (true)
            {
                if (OpenClipboard(default))
                {
                    return true;
                }

                if (--num == 0)
                {
                    return false;
                }

                Thread.Sleep(50);
            }
        }
        public static string ReadText()
        {
            if (!IsClipboardFormatAvailable(13))
            {
                return null;
            }

            IntPtr handle = default;

            IntPtr pointer = default;
            try
            {
                OpenClipboard();
                handle = GetClipboardData(13);
                if (handle == default)
                {
                    return null;
                }

                pointer = GlobalLock(handle);
                if (pointer == default)
                {
                    return null;
                }

                var size = GlobalSize(handle);

                if(size > MaxReadStringSize)
                {
                    ISLogger.Write("Failed to read clipboard data: Clipboard contains string larger than limit");
                    GlobalUnlock(handle);
                    return null;
                }

                var buff = new byte[size];

                Marshal.Copy(pointer, buff, 0, size);

                return Encoding.Unicode.GetString(buff).TrimEnd('\0');
            }
            finally
            {
                if (pointer != default)
                {
                    GlobalUnlock(handle);
                }

                CloseClipboard();
            }
        }
    }
}
