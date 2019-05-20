using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.Input
{
    public enum ISInputCode : byte
    {
        /// <summary>
        /// Mouse move relative to last mouse position
        /// p1 = X
        /// p2 = Y
        /// </summary>
        IS_MOUSEMOVERELATIVE,

        /// <summary>
        /// Mouse move to specified position
        /// p1 = X
        /// p2 = Y
        /// </summary>
        IS_MOUSEMOVEABSOLUTE,

        /// <summary>
        /// Key pressed
        /// p1 = key scan code
        /// p2 = windows virtual key code
        /// </summary>
        IS_KEYDOWN,

        /// <summary>
        /// Key Released
        /// p1 = virtual key code
        /// </summary>
        IS_KEYUP,

        /// <summary>
        /// Left mouse button pressed
        /// </summary>
        IS_MOUSELDOWN,
        /// <summary>
        /// Left mouse button released
        /// </summary>
        IS_MOUSELUP,

        /// <summary>
        ///  /// <summary>
        /// Right mouse button pressed
        /// </summary>
        /// </summary>
        IS_MOUSERDOWN,

        /// <summary>
        /// Right mouse button released
        /// </summary>
        IS_MOUSERUP,

        /// <summary>
        /// Middle mouse button pressed (scroll wheel)
        /// </summary>
        IS_MOUSEMDOWN,
        /// <summary>
        /// Middle mouse button released (scroll wheel)
        /// </summary>
        IS_MOUSEMUP,

        /// <summary>
        /// Mouse vertical scroll
        /// p1 = direction (120 = up; -120 = down)
        /// </summary>
        IS_MOUSEYSCROLL,

        /// <summary>
        /// Mouse horizontal scroll
        /// TODO - this
        /// </summary>
        IS_MOUSEXSCROLL,

        /// <summary>
        /// Mouse X button pressed
        /// p1 = X button ID
        /// </summary>
        IS_MOUSEXDOWN,

        /// <summary>
        /// Mouse X button released
        /// p1 = X button ID
        /// </summary>
        IS_MOUSEXUP,

        IS_SENDSAS,

        IS_RELEASEALL,

        IS_UNKNOWN,
    }
}
