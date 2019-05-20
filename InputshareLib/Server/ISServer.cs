using InputshareLib.Server.Net;
using System;
using static InputshareLib.Settings;
using InputshareLib.Input.Hotkeys;
using InputshareLib.Input;
using System.Diagnostics;
using InputshareLib.CursorMonitor;
using InputshareLib.Ouput;
using InputshareLib.ProcessMonitor;

namespace InputshareLib.Server
{
    public class ISServer
    {
        public bool Running { get; private set; }
        public event EventHandler<ConnectedClientInfo> ClientConnected;
        public event EventHandler<ClientDisconnectedArgs> ClientDisconnected;
        public event EventHandler InputClientSwitched;
        public event EventHandler DisplayConfigChanged;

        public event EventHandler ServerStopped;

        private ClientListener tcpListener;
        private InputManager inputMan;
        private ClientManager clientMan;
        private ICursorMonitor curMonitor;
        private IOutputManager outManager;
        private IProcessMonitor procMonitor;

        private ConnectedClient currentInputClient = null;
        public bool LocalInput { get; private set; } = true;
        public int ServerPort { get; private set; }
        public bool EnableMouseSwitchClients { get; private set; } = true;
        public bool ReleaseInputsOnClientSwitch { get; private set; } = true;
        public bool DisableMouseSwitchWhileFullscreen { get; private set; } = true;

        private bool ignoreClipboard = false;

        public struct ISServerSettings
        {
            public ISServerSettings(bool enableCursorSwitch, bool releaseAllOnSwitch, bool disableSwitchWhileFullscreen)
            {
                EnableCursorSwitch = enableCursorSwitch;
                ReleaseAllOnSwitch = releaseAllOnSwitch;
                DisableSwitchWhileFullscreen = disableSwitchWhileFullscreen;
            }

            public bool EnableCursorSwitch { get; }
            public bool ReleaseAllOnSwitch { get; }
            public bool DisableSwitchWhileFullscreen { get; }
        }

        public void SetSettings(ISServerSettings settings)
        {
            EnableMouseSwitchClients = settings.EnableCursorSwitch;
            ReleaseInputsOnClientSwitch = settings.ReleaseAllOnSwitch;
            DisableMouseSwitchWhileFullscreen = settings.DisableSwitchWhileFullscreen;
        }

        public ISServerSettings GetSettings()
        {
            return new ISServer.ISServerSettings(EnableMouseSwitchClients, ReleaseInputsOnClientSwitch, DisableMouseSwitchWhileFullscreen);
        }

        public void Start(int port)
        {
            if (Running)
            {
                throw new InvalidOperationException("Server already running");
            }

            try
            {
                Process.GetCurrentProcess().PriorityClass = ServerBasePriority;
            }catch(Exception ex)
            {
                ISLogger.Write("Cannot set process priority to {0}: {1}", ServerBasePriority, ex.Message);
            }

            
            ConnectedClient.LocalHost = new ConnectedClient(true);
            ISLogger.Write("Starting server...");
            ServerPort = port;
            clientMan = new ClientManager(ServerDefaultMaxClients);
            clientMan.AddClient(ConnectedClient.LocalHost);
            tcpListener = new ClientListener();
            tcpListener.ClientConnected += TcpListener_ClientConnected;


            tcpListener.Start(port);


            SetConsoleText("Current client: localhost");
            
            //We need to determine which OS is being used
            OSHelper.Os os = OSHelper.GetOsVersion();

            switch (os.System) {
                case OSHelper.Platform.Windows:
                    {

                        inputMan = new WindowsInputManager();
                        curMonitor = new WindowsCursorMonitor();
                        outManager = new WindowsOutputManager();
                        procMonitor = new WindowsProcessMonitor();
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
            inputMan.Start();

            curMonitor.Start();
            curMonitor.EdgeHit += LocalHost_EdgeHit;

            procMonitor.StartMonitoring();
            procMonitor.ProcessEnteredFullscreen += ProcMonitor_ProcessEnteredFullscreen;
            procMonitor.ProcessExitedFullscreen += ProcMonitor_ProcessExitedFullscreen;
            Running = true;
            
            inputMan.InputReceived += InputMan_InputReceived;
            inputMan.ClientHotkeyPressed += InputMan_ClientHotkeyPressed;
            inputMan.FunctionHotkeyPressed += InputMan_FunctionHotkeyPressed;
            inputMan.ClipboardTextCopied += InputMan_ClipboardTextCopied;

            LoadHotkeySettings();
        }

        private void LoadHotkeySettings()
        {
            string exitHk = ConfigManager.ReadConfig("HotkeyExit");
            if(exitHk != null)
            {
                Hotkey hk = Hotkey.FromSettingsString(exitHk);
                if(hk != null)
                {
                    inputMan.SetFunctionHotkey(hk, HotkeyFunction.Exit);
                }
                else
                {
                    inputMan.SetFunctionHotkey(ServerDefaultExitHotkey, HotkeyFunction.Exit);
                }
            }
            else
            {
                inputMan.SetFunctionHotkey(ServerDefaultExitHotkey, HotkeyFunction.Exit);
            }

            string localHk = ConfigManager.ReadConfig("HotkeyLocal");
            if (localHk != null)
            {
                Hotkey hk = Hotkey.FromSettingsString(localHk);
                if (hk != null)
                {
                    inputMan.SetFunctionHotkey(hk, HotkeyFunction.SwitchToLocalInput);
                }
                else
                {
                    inputMan.SetFunctionHotkey(ServerDefaultSwitchLocalHotkey, HotkeyFunction.SwitchToLocalInput);
                }
            }
            else
            {
                inputMan.SetFunctionHotkey(ServerDefaultSwitchLocalHotkey, HotkeyFunction.SwitchToLocalInput);
            }

            string sasHk = ConfigManager.ReadConfig("HotkeySas");
            if(exitHk == null)
            {
                Hotkey hk = Hotkey.FromSettingsString(sasHk);
                if(hk != null)
                {
                    inputMan.SetFunctionHotkey(hk, HotkeyFunction.SAS);
                }
                else
                {
                    inputMan.SetFunctionHotkey(ServerDefaultSASHotkey, HotkeyFunction.SAS);
                }
            }
            else
            {
                inputMan.SetFunctionHotkey(ServerDefaultSASHotkey, HotkeyFunction.SAS);
            }

        }

        public void SetClientEdge(ConnectedClientInfo clientA, BoundEdge sideof, ConnectedClientInfo clientB)
        {
            ConnectedClient a = null;
            if(clientA != null)
                a = clientMan.GetClientFromGuid(clientA.ClientId);

            ConnectedClient b = clientMan.GetClientFromGuid(clientB.ClientId);


            if (a == null)
            {
                switch (sideof)
                {
                    case BoundEdge.Left:
                        if (b.LeftClient != null)
                            b.LeftClient.RightClient = null;
                        break;
                    case BoundEdge.Right:
                        if (b.RightClient != null)
                            b.RightClient.LeftClient= null;
                        break;
                    case BoundEdge.Bottom:
                        if (b.BelowClient != null)
                            b.BelowClient.AboveClient = null;
                        break;
                    case BoundEdge.Top:
                        if (b.AboveClient != null)
                            b.AboveClient.BelowClient = null;
                        break;

                }
            }

            switch (sideof) {
                case BoundEdge.Bottom:
                    b.BelowClient = a;
                    if(a != null)
                        a.AboveClient = b;
                    break;
                case BoundEdge.Right:
                    b.RightClient = a;
                    if (a != null)
                        a.LeftClient = b;
                    break;
                case BoundEdge.Left:
                    b.LeftClient = a;
                    if (a != null)
                        a.RightClient = b;
                    break;
                case BoundEdge.Top:
                    b.AboveClient = a;
                    if (a != null)
                        a.BelowClient = b;
                    break;
            }


            if (a == null)
                ISLogger.Write("Set None {0}of {1}", sideof, b.ClientName);
            else
                ISLogger.Write("Set {0} {1}of {2}", a.ClientName, sideof, b.ClientName);
            DisplayConfigChanged(this, null);
        }

        private void ProcMonitor_ProcessExitedFullscreen(object sender, Process proc)
        {
            //ISLogger.Write($"{proc.ProcessName} exited fullscreen");
        }

        private void ProcMonitor_ProcessEnteredFullscreen(object sender, Process proc)
        {
            //ISLogger.Write($"{proc.ProcessName} entered fullscreen");
        }

        public ConnectedClientInfo GetInputClient()
        {
            if (LocalInput)
            {
                return CreateClientInfo(ConnectedClient.LocalHost, true);
            }

            return CreateClientInfo(currentInputClient, true);
        }

        public void Stop()
        {
            if (!Running)
            {
                throw new InvalidOperationException("Server not running");
            }

            ISLogger.Write("Stopping server");

            foreach(ConnectedClient client in clientMan?.AllClients)
            {
                clientMan.RemoveClient(client);
                client.Dispose();
            }
            tcpListener?.Stop();

            if (inputMan.Running)
                inputMan?.Stop();
            inputMan = null;

            if(curMonitor.Running)
                curMonitor?.Stop();
            curMonitor = null;
            if(procMonitor.Monitoring)
                procMonitor?.StopMonitoring();
            procMonitor = null;
            
            tcpListener = null;
            Running = false;
            ServerStopped?.Invoke(this, null);
        }

        public void SetClientHotkey(ConnectedClientInfo client, Hotkey key)
        {
            if(client.ClientId == Guid.Empty)
            {
                inputMan.SetFunctionHotkey(key, HotkeyFunction.SwitchToLocalInput);
                ConfigManager.WriteConfig("HotkeyLocal", key.ToSettingsString());
                return;
            }

            inputMan.SetUpdateClientHotkey(key, client.ClientId);
            ConfigManager.WriteConfig(client.ClientId.ToString() + "Hotkey", key.ToSettingsString());
        }

        public Hotkey GetclientHotkey(ConnectedClientInfo client)
        {
            if(client.ClientId == Guid.Empty)
            {
                return inputMan.GetFunctionHotkey(HotkeyFunction.SwitchToLocalInput);
            }

            return inputMan.GetClientHotkey(client.ClientId);
        }

        public FunctionHotkey GetFunctionHotkey(HotkeyFunction function)
        {
            return inputMan.GetFunctionHotkey(function);
        }

        public ConnectedClientInfo[] GetClients()
        {
            if (clientMan == null)
                return new ConnectedClientInfo[1]{ ConnectedClientInfo.None };

            ConnectedClientInfo[] list = new ConnectedClientInfo[clientMan.AllClients.Length];
            int index = 0;
            foreach (var client in clientMan.AllClients)
            {
                list[index] = CreateClientInfo(client, true);
                index++;
            }
            return list;
        }

        public ConnectedClientInfo GetFocusedClient()
        {
            if (LocalInput)
            {
                return CreateClientInfo(ConnectedClient.LocalHost, true);
            }

            if(currentInputClient != null)
            {
                return CreateClientInfo(currentInputClient, true);
            }
            throw new InvalidOperationException("Focused client not found");
        }

        private void InputMan_ClipboardTextCopied(object sender, string cbText)
        {
            
            if (ignoreClipboard)
            {
                ignoreClipboard = false;
                return;
            }
            foreach (var client in clientMan.AllClients)
            {
                if(client != ConnectedClient.LocalHost)
                {
                    client.SetClipboardText(cbText);
                }
            }
        }

        private void InputMan_InputReceived(object sender, InputshareLib.Input.ISInputData input)
        {
            if (LocalInput)
                return;

            currentInputClient?.SendInput(input);
        }

        private void InputMan_ClientHotkeyPressed(object sender, ClientHotkey e)
        {
            if (!LocalInput && currentInputClient?.ClientGuid == e.TargetClient)
                return;

            SwitchInputToClient(e.TargetClient);
        }

        private void InputMan_FunctionHotkeyPressed(object sender, FunctionHotkey e)
        {
            switch (e.Function)
            {
                case HotkeyFunction.Exit:
                    Stop();
                    break;
                case HotkeyFunction.SwitchToLocalInput:
                    if(!LocalInput)
                        SwitchLocalInput();
                    break;
                case HotkeyFunction.SAS:
                    if (!LocalInput)
                        currentInputClient?.SendInput(new ISInputData(ISInputCode.IS_SENDSAS, 0, 0));
                    break;
            } 
        }

        private void SwitchLocalInput()
        {
            LocalInput = true;

            if (!curMonitor.Running)
                curMonitor.Start();

            if (currentInputClient != null && currentInputClient.Connected)
            {
                currentInputClient.SendMessage(InputshareLib.Net.Messages.MessageType.ClientOutOfFocus);
            }
            currentInputClient = null;

            if (ReleaseInputsOnClientSwitch)
                outManager.ReleaseAllKeys();

            inputMan.BlockUserInput(false);
            SetConsoleText("Current client: localhost");
            InputClientSwitched?.Invoke(this, null);
        }

        private void SwitchInputToClient(Guid client)
        {
            if (client == Guid.Empty)
            {
                SwitchLocalInput();
                return;
            }

            foreach(var c in clientMan.AllClients)
            {
                if(c.ClientGuid == client)
                {
                    if(!LocalInput && currentInputClient != null && currentInputClient.Connected)
                    {
                        currentInputClient.SendMessage(InputshareLib.Net.Messages.MessageType.ClientOutOfFocus);
                    }

                    LocalInput = false;
                    currentInputClient = c;
                    c.SendMessage(InputshareLib.Net.Messages.MessageType.ClientInFocus);
                    if (ReleaseInputsOnClientSwitch)
                        outManager.ReleaseAllKeys();

                    inputMan.BlockUserInput(true);
                    SetConsoleText("Current client: " + c.ClientName);
                    InputClientSwitched?.Invoke(this, null);
                    return;
                }
            }

            //inputMan.RemoveClientHotkey(client);
            ISLogger.Write("Could not switch input to client... client not found");
        }

        private void SetConsoleText(string text)
        {
            try
            {
                //Console.Title = text;
            }
            catch (Exception) { }
        }

        private void TcpListener_ClientConnected(object sender, ClientConnectedArgs e)
        {
            ConnectedClient c = new ConnectedClient(e.ClientSocket, e.ClientName, e.ClientId);

            try
            {
                clientMan.AddClient(c);
            }
            catch (ClientManager.DuplicateNameException)
            {
                ISLogger.Write("Declining client {0}: Name already in use", c.ClientName);
                c.SendMessage(InputshareLib.Net.Messages.MessageType.ClientDuplicateName);
                c.Dispose();
                return;
            }
            catch (ClientManager.DuplicateGuidException)
            {
                ISLogger.Write("Declining client {0}: Guid already in use", c.ClientName);
                c.SendMessage(InputshareLib.Net.Messages.MessageType.ClientDuplicateGuid);
                c.Dispose();
                return;
            }
            catch (ClientManager.MaxClientsReachedException)
            {
                ISLogger.Write("Declining client {0}: Max clients reached", c.ClientName);
                c.SendMessage(InputshareLib.Net.Messages.MessageType.ClientLimitReached);
                c.Dispose();
                return;
            }
            ApplyClientConfig(c.ClientGuid);
            c.SendMessage(InputshareLib.Net.Messages.MessageType.ServerOK);
            ISLogger.Write("{0} connected from {1}", e.ClientName, e.ClientSocket.RemoteEndPoint);
            ConnectedClientInfo info = CreateClientInfo(c, true);

            ClientConnected?.Invoke(this, CreateClientInfo(c, true));



            c.ClipboardTextCopied += C_ClipboardTextCopied;
            c.ConnectionError += C_ConnectionError;
            c.ClientEdgeHit += OnAnyEdgeHit;
        }

        private void ApplyClientConfig(Guid clientGiud)
        {
            string clientId = clientGiud.ToString();
            ISLogger.Write($"Loading config for {clientId}");
            //Reading client config
            string left = ConfigManager.ReadConfig(clientId + "BoundLeft");
            string right = ConfigManager.ReadConfig(clientId + "BoundRight");
            string above = ConfigManager.ReadConfig(clientId + "BoundAbove");
            string below = ConfigManager.ReadConfig(clientId + "BoundBelow");
            string hk = ConfigManager.ReadConfig(clientId + "Hotkey");

            if (hk != null)
            {
                Hotkey key = Hotkey.FromSettingsString(hk);
                if (key != null)
                {
                    inputMan?.SetUpdateClientHotkey(key, clientGiud);
                    ISLogger.Write($"IsServer->Loaded saved hotkey for client");
                }
                else
                {
                    ISLogger.Write($"IsServer->Error reading client hotkey from config");
                }
            }
            else
            {
                ISLogger.Write($"IsServer->Error reading client hotkey from config (hotkey was null)");
            }
        }

        private void C_ClipboardTextCopied(object sender, string cbText)
        {
            if(cbText == null)
            {
                ISLogger.Write("Warning: Copied null string from clipboard");
                return;
            }
            //ISLogger.Write($"{(sender as ConnectedClient).ClientName} copied {e}");

            ignoreClipboard = true;
            if (!WinClipboard.SetText(cbText))
            {
                ISLogger.Write("IsServer: Failed to set clipboard text");
            }
            ConnectedClient senderClient = sender as ConnectedClient;

            foreach(var client in clientMan.AllClients)
            {
                if(client != senderClient && client != ConnectedClient.LocalHost)
                {
                    client?.SetClipboardText(cbText);
                }
            }
        }

        private void OnAnyEdgeHit(object sender, BoundEdge edge)
        {
            if (!EnableMouseSwitchClients)
                return;

            ConnectedClient c = sender as ConnectedClient;

            if (c != currentInputClient && c != ConnectedClient.LocalHost)        
                //Make sure the client that reports this is actually the focused client
                return;

            //Check that no application is in fullscreen mode
            if (procMonitor.FullscreenApplicationRunning && DisableMouseSwitchWhileFullscreen && c == ConnectedClient.LocalHost)
            {
                return;
            }

            switch (edge) {
                case BoundEdge.Bottom:
                    {
                        if (c.BelowClient != null)
                        {
                            if (!c.BelowClient.Connected)
                            {
                                c.BelowClient = null;
                                break;
                            }

                            //Console.WriteLine("Switching to {0} (under {1})", c.BelowClient.ClientName, c.ClientName);
                            SwitchInputToClient(c.BelowClient.ClientGuid);
                        }
                        break;
                    }
                case BoundEdge.Top:
                    {
                        if (c.AboveClient != null)
                        {
                            if (!c.AboveClient.Connected)
                            {
                                c.AboveClient = null;
                                break;
                            }
                            //Console.WriteLine("Switching to {0} (above {1})", c.AboveClient.ClientName, c.ClientName);
                            SwitchInputToClient(c.AboveClient.ClientGuid); ;
                        }
                        break;
                    }
                case BoundEdge.Left:
                    {
                        if (c.LeftClient != null)
                        {
                            if (!c.LeftClient.Connected)
                            {
                                c.LeftClient = null;
                                break;
                            }
                            //Console.WriteLine("Switching to {0} (left of {1})", c.LeftClient.ClientName, c.ClientName);
                            SwitchInputToClient(c.LeftClient.ClientGuid);
                        }
                        break;
                    }
                case BoundEdge.Right:
                    {
                        if (c.RightClient != null)
                        {
                            if (!c.RightClient.Connected)
                            {
                                c.RightClient = null;
                                break;
                            }
                            //Console.WriteLine("Switching to {0} (right of {1})", c.RightClient.ClientName, c.ClientName);
                            SwitchInputToClient(c.RightClient.ClientGuid);
                        }
                        break;
                    }
            }

        }

        private void LocalHost_EdgeHit(object sender, BoundEdge edge)
        {
            if (!LocalInput)
                return;

            OnAnyEdgeHit(ConnectedClient.LocalHost, edge);
        }

        private void C_ConnectionError(object sender, EventArgs e)
        {
            ConnectedClient c = sender as ConnectedClient;

            if(currentInputClient == c)
            {
                SwitchLocalInput();
            }

            try
            {
                inputMan.RemoveClientHotkey(c.ClientGuid);
            }
            catch (InvalidOperationException ex)
            {
                ISLogger.Write("Could not remove hotkey for client {0}: {1}", c.ClientName, ex.Message);
            }
            ISLogger.Write("{0} disconnected: Connection error", c.ClientName);
            clientMan.RemoveClient(c);
            ClientDisconnected?.Invoke(this, new ClientDisconnectedArgs(CreateClientInfo(c, true), "Connection error"));
            c.Dispose();
        }

        private ConnectedClientInfo CreateClientInfo(ConnectedClient client, bool includeEdges)
        {
            if (client == null)
                return ConnectedClientInfo.None;

            ClientHotkey hk = new ClientHotkey(0, 0, Guid.Empty);
            try
            {
               hk = inputMan.GetClientHotkey(client.ClientGuid);
            }
            catch (InvalidOperationException)
            {
            }

            if(client == ConnectedClient.LocalHost)
            {
                FunctionHotkey fhk = inputMan.GetFunctionHotkey(HotkeyFunction.SwitchToLocalInput);
                hk = new ClientHotkey(fhk.HkScan, fhk.Mods, Guid.Empty);
            }

            if (includeEdges)
            {
                ConnectedClientInfo lc;
                if (client.LeftClient == null)
                    lc = ConnectedClientInfo.None;
                else
                    lc = CreateClientInfo(client.LeftClient, false);

                ConnectedClientInfo rc;
                if (client.RightClient == null)
                    rc = ConnectedClientInfo.None;
                else
                    rc = CreateClientInfo(client.RightClient, false);

                ConnectedClientInfo ac;
                if (client.AboveClient == null)
                    ac = ConnectedClientInfo.None;
                else
                    ac = CreateClientInfo(client.AboveClient, false);
                ConnectedClientInfo bc;
                if (client.BelowClient == null)
                    bc = ConnectedClientInfo.None;
                else
                    bc = CreateClientInfo(client.BelowClient, false);

                return new ConnectedClientInfo(client.ClientName, client.ClientGuid, client.ClientEndPoint.Address, hk,
                    lc, rc, ac, bc);
            }
            else
            {
                return new ConnectedClientInfo(client.ClientName, client.ClientGuid, client.ClientEndPoint.Address, hk,
                   null, null, null, null);
            }

            
        }

        public class ClientDisconnectedArgs : EventArgs
        {
            public ClientDisconnectedArgs(ConnectedClientInfo client, string reason)
            {
                Client = client;
                Reason = reason;
            }

            public ConnectedClientInfo Client { get; }
            public string Reason { get; }
        }
    }

    
}
