using InputshareLib.Input.Hotkeys;
using System;
using System.Collections.Generic;

namespace InputshareLib.Input
{
    public abstract class InputManager
    {
        public abstract event EventHandler<ISInputData> InputReceived;
        public abstract event EventHandler<FunctionHotkey> FunctionHotkeyPressed;
        public abstract event EventHandler<ClientHotkey> ClientHotkeyPressed;
        public abstract event EventHandler<string> ClipboardTextCopied;

        public bool Running { get; protected set; }
        public IEnumerable<Hotkey> AssignedHotkeys { get => hotkeyList.ToArray(); }
        public bool UserInputBlocked { get; protected set; }

        protected List<Hotkey> hotkeyList = new List<Hotkey>();
        protected Hotkey.Modifiers currentModifiers = 0;

        public abstract void Start();
        public abstract void Stop();

        public abstract void BlockUserInput(bool block);

        public void SetFunctionHotkey(Hotkey newKey, HotkeyFunction function)
        {
            if (!Running)
            {
                throw new InvalidOperationException("InputManager not started");
            }

            if (DoesHotkeyExistForFunction(function))
            {
                FunctionHotkey hk = GetFunctionHotkey(function);
                hotkeyList.Remove(hk);
            }

            ISLogger.Write(string.Format("Function {0} hotkey set to {1}", function, newKey));
            hotkeyList.Add(new FunctionHotkey(newKey.HkScan, newKey.Mods, function));
        }

        public FunctionHotkey GetFunctionHotkey(HotkeyFunction function)
        {
            if (!Running)
            {
                throw new InvalidOperationException("InputManager not started");
            }

            for (int i = 0; i < hotkeyList.Count; i++)
            {
                if (hotkeyList[i] is FunctionHotkey)
                {
                    FunctionHotkey hk = hotkeyList[i] as FunctionHotkey;
                    if (hk.Function == function)
                    {
                        return hk;
                    }
                }
            }
            throw new InvalidOperationException("Hotkey not found");
        }

        public void SetUpdateClientHotkey(Hotkey newKey, Guid targetClient)
        {
            if (!Running)
            {
                throw new InvalidOperationException("InputManager not started");
            }

            if (DoesHotkeyExistForClient(targetClient))
            {
                ClientHotkey hk = GetClientHotkey(targetClient);
                hotkeyList.Remove(hk);
            }

            if (IsHotkeyInUse(newKey))
            {
                ISLogger.Write("Cannot assign hotkey {0}... hotkey already in use", newKey);
                return;
                //throw new HotkeyInUseException("Hotkey " + newKey + " already in use");
            }

            ISLogger.Write(string.Format("Client {0} hotkey set to {1}", targetClient, newKey));
            hotkeyList.Add(new ClientHotkey(newKey.HkScan, newKey.Mods, targetClient));
        }

        public ClientHotkey GetClientHotkey(Guid targetClient)
        {
            if (!Running)
            {
                throw new InvalidOperationException("InputManager not started");
            }

            for (int i = 0; i < hotkeyList.Count; i++)
            {
                if (hotkeyList[i] is ClientHotkey)
                {
                    ClientHotkey hk = hotkeyList[i] as ClientHotkey;
                    if (hk.TargetClient == targetClient)
                        return hk;
                }
            }

            return null;
        }

        public void RemoveClientHotkey(Guid targetClient)
        {
            if (!Running)
            {
                throw new InvalidOperationException("InputManager not started");
            }

            if (!DoesHotkeyExistForClient(targetClient))
            {
                return;
            }

            ClientHotkey hk = GetClientHotkey(targetClient);
            hotkeyList.Remove(hk);
        }

        public bool IsHotkeyInUse(Hotkey key)
        {
            if (!Running)
            {
                throw new InvalidOperationException("InputManager not started");
            }

            for (int i = 0; i < hotkeyList.Count; i++)
            {
                if (hotkeyList[i].HkScan == key.HkScan && hotkeyList[i].Mods == key.Mods)
                {
                    return true;
                }
            }
            return false;
        }

        public bool DoesHotkeyExistForClient(Guid targetClient)
        {
            if (!Running)
            {
                throw new InvalidOperationException("InputManager not started");
            }

            for (int i = 0; i < hotkeyList.Count; i++)
            {
                if (hotkeyList[i] is ClientHotkey)
                {
                    ClientHotkey hk = hotkeyList[i] as ClientHotkey;
                    if (hk.TargetClient == targetClient)
                        return true;
                }
            }
            return false;
        }

        public bool DoesHotkeyExistForFunction(HotkeyFunction function)
        {
            if (!Running)
            {
                throw new InvalidOperationException("InputManager not started");
            }
            for (int i = 0; i < hotkeyList.Count; i++)
            {
                if (hotkeyList[i] is FunctionHotkey)
                {
                    FunctionHotkey hk = hotkeyList[i] as FunctionHotkey;
                    if (hk.Function == function)
                        return true;
                }
            }

            return false;
        }

        public class HotkeyInUseException : Exception
        {
            public HotkeyInUseException(string message) : base(message)
            {

            }
        }

    }
}
