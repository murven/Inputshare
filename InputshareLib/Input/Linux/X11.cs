using System;
using System.Runtime.InteropServices;

namespace Inputshare.Input.Linux
{
    static class X11
    {
        public const int GrabModeSync = 0;
        public const int GrabModeAsync = 1;


        [DllImport("libX11.so.6")]
        public static extern IntPtr XOpenDisplay(string display_name);

        [DllImport("libX11.so.6")]
        public static extern int XGrabKeyboard(IntPtr display, uint window, bool owner_events, 
            int pointer_mode, int keyboard_mode, int time);

        [DllImport("libX11.so.6")]
        public static extern int XGrabPointer(IntPtr display, uint window, bool owner_events, 
            XEventMask event_mask, int pointer_mode, int keyboard_mode,IntPtr confine_to, IntPtr cursor, int time);
            
        [DllImport("libX11.so.6")]
        public static extern void XUngrabKeyboard(IntPtr display, int time);

        [DllImport("libX11.so.6")]
        public static extern void XUngrabPointer(IntPtr display, int time);
        
        [DllImport("libX11.so.6")]
        public static extern uint XDefaultRootWindow(IntPtr display);

        [DllImport("libX11.so.6")]
        public static extern void XSelectInput(IntPtr display, uint window, XEventMask event_mask);
        
        [DllImport("libX11.so.6")]
        public static extern void XAllowEvents(IntPtr display, XEventMode event_mode, int time);

        [DllImport("libX11.so.6")]
        public static extern void XNextEvent(IntPtr display, out XAnyEvent event_return);
        [DllImport("libX11.so.6")]
        public static extern void XWarpPointer(IntPtr display, IntPtr src_w, IntPtr dest_w);
        [Flags]
        public enum XEventMask{
            NoEventMask = 0,
            KeyPressMask = 1,
            KeyReleaseMask = 2,
            ButtonPressMask = 4,
            ButtonReleaseMask = 8,
            EnterWindowMask = 16,
            LeaveWindowMask = 32,
            PointerMotionMask = 64,
            PointerMotionHintMask = 128,
        }

#pragma warning disable 0649

        public struct XAnyEvent{
            public int type;
            public uint serial;
            public bool send_event;
            public IntPtr display;
            public uint window;
        }


        public enum XEventMode{
            AsyncPointer = 0,
            SyncPointer = 1,
            ReplayPointer = 2,
            AsyncKeyboard = 3,
            SyncKeyboard = 4,
            ReplayKeyboard = 5,
            AsyncBoth = 6,
            SyncBoth = 7
        }

#pragma warning enable 0649
    }
}