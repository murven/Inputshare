using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.Input.Hotkeys
{
    public class Hotkey
    {
        public Hotkey(ScanCode hkScan, Modifiers mods)
        {
            HkScan = hkScan;
            Mods = mods;
        }

        public override string ToString()
        {
            if(Mods == 0)
            {
                return HkScan.ToString();
            }
            return string.Format("{1} + {0}", HkScan, Mods);
        }

        public string ToSettingsString()
        {
            return string.Format($"{(int)HkScan}:{(uint)Mods}");
        }

        public static Hotkey FromSettingsString(string hk)
        {
            try
            {
                string[] e = hk.Split(':');
                if (e.Length != 2)
                    return null;

                int.TryParse(e[0], out int key);
                uint.TryParse(e[1], out uint mods);

                return new Hotkey((ScanCode)key, (Modifiers)mods);
            }catch(Exception) { return null; }
            
        }

        /// <summary>
        /// Scancode of specified hotkey
        /// </summary>
        public ScanCode HkScan { get; }

        /// <summary>
        /// Required modifier keys for hotkey
        /// </summary>
        public Modifiers Mods { get; }

        public static bool operator ==(Hotkey hk1, Hotkey hk2)
        {
            if(ReferenceEquals(hk1, null))
            {
                if(ReferenceEquals(hk2, null))
                {
                    return true;
                }
                return false;
            }

            if (ReferenceEquals(hk2, null))
            {
                if (ReferenceEquals(hk1, null))
                {
                    return true;
                }
                return false;
            }

            if ((hk1.HkScan == hk2.HkScan) && (hk1.Mods == hk2.Mods))
                return true;
            else
                return false;
        }
        public static bool operator !=(Hotkey hk1, Hotkey hk2)
        {
            return !(hk1 == hk2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }


        [FlagsAttribute]
        public enum Modifiers
        {
            Alt = 0x0001,
            Ctrl = 0x0002,
            Shift = 0x0004,
            Windows = 0x0008
        }
    }
}
