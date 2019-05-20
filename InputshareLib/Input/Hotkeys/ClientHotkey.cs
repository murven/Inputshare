using System;
using System.Collections.Generic;
using System.Text;

namespace InputshareLib.Input.Hotkeys
{
    public class ClientHotkey : Hotkey
    {
        public ClientHotkey(ScanCode HkScan, Modifiers mods,  Guid targetClient) : base(HkScan, mods)
        {
            TargetClient = targetClient;
        }

        public Guid TargetClient { get; }
    }
}
