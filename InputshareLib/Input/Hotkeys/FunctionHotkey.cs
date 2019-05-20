namespace InputshareLib.Input.Hotkeys
{
    public class FunctionHotkey : Hotkey
    {
        public FunctionHotkey(ScanCode hkScan, Modifiers mods, HotkeyFunction function) : base(hkScan, mods)
        {
            Function = function;
        }

        public HotkeyFunction Function { get; }
    }
}
