using System.ServiceProcess;
using System.IO.Pipes;
using System.Threading;
using InputshareLib;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Net;
using InputshareLib.Input;
using System.ComponentModel;
using System.Text;
using InputshareLib.Net.Messages;
using Microsoft.Win32;
using InputshareLib.AnonIPC;
using InputshareLib.NamedIPC;
using static InputshareLib.ServerSocket;
using System.Threading.Tasks;

namespace InputshareService
{
    class IsService : ServiceBase
    {
        [DllImport("SAS.dll")]
        static extern void SendSAS(bool asUser);

        private SpMonitor spMon;
        private SpLauncher launcher;
        private ServerSocket cSocket;

        private AnonPipeServer ipcWrite;
        private AnonPipeServerRead ipcRead;

        private NamedIpcServer namedIpc;

        private string clientName = Environment.MachineName;
        private Guid clientId = Guid.Empty;
        private IPEndPoint lastServer;

        /// <summary>
        /// If true, the service will keep trying to connect to a service if the connection fails or if there is an error
        /// </summary>
        private bool keepRetryingConnection = false;

        public IsService()
        {
            ServiceName = "InputshareService";
            CanHandleSessionChangeEvent = true;
        }

        protected override void OnStart(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            string path = AppDomain.CurrentDomain.BaseDirectory;

            if(!Directory.Exists(path + "\\logs"))
            {
                try
                {
                    Directory.CreateDirectory("logs");
                }catch(Exception ex)
                {
                    ISLogger.Write($"Failed to create log file folder: {ex.Message}");
                }
            }

            ISLogger.SetLogFileName(path + @"\logs\InputshareService.log");
            ISLogger.EnableConsole = false;
            ISLogger.EnableLogFile = true;
            ISLogger.Write("Service->Service started");

            try
            {
                namedIpc = new NamedIpcServer();
                namedIpc.Ipc_Connect += NamedIpc_Connect;
                namedIpc.RequestedState += NamedIpc_RequestedState;
                namedIpc.Ipc_Disconnect += NamedIpc_Disconnect;
                ISLogger.Write("Service->Named IPC server started");
            }catch(Exception ex)
            {
                ISLogger.Write($"Service->Failed to create NamedIPC server: " + ex.Message);
            }

            if (!File.Exists(path + "\\inputsharesp.exe"))
            {
                ISLogger.Write("Service->Error: inputsharepSP not found! stopping service!");
                Stop();
                return;
            }

            CreatePipes();
            EnableSASHKey();

            cSocket = new ServerSocket();
            spMon = new SpMonitor();
            spMon.StartMonitoring();
            spMon.SpStarted += SpMon_SpStarted;
            spMon.SpStopped += SpMon_SpStopped;

            launcher = new SpLauncher();

            if (!launcher.SpRunning)
            {
                ISLogger.Write("Service->SP not found... launching");

                try
                {
                    launcher.LaunchSp(ipcWrite.PipeHandleAsString, ipcRead.PipeHandleAsString);
                    ipcWrite.RemoveHandle();
                    ipcRead.RemoveHandle();
                }
                catch (Exception ex)
                {
                    if (ex is InvalidOperationException || ex is Win32Exception)
                    {
                        ISLogger.Write($"Failed to launch inputsharesp: {ex.Message}");
                    }
                }
            }

            cSocket.InputReceived += CSocket_InputReceived;
            cSocket.ClipboardTextReceived += CSocket_ClipboardTextReceived;
            cSocket.MessageReceived += CSocket_MessageReceived;
            cSocket.Connected += CSocket_Connected;
            cSocket.Disconnected += CSocket_Disconnected;
            cSocket.ConnectionFailed += CSocket_ConnectionFailed;
            cSocket.ConnectionError += CSocket_ConnectionError;

            ISLogger.Write("Service->Service started");

            //Read initial config
            string lastAddr = ConfigManager.ReadConfig("LastAddress");
            string lastName = ConfigManager.ReadConfig("LastName");
            string lastGuidStr = ConfigManager.ReadConfig("LastGuid");

            if (lastAddr == null || lastName == null || lastGuidStr == null)
                return;

            IPEndPoint.TryParse(lastAddr, out IPEndPoint lastEp);
            if(lastEp == null)
            {
                ISLogger.Write($"Service->Config file contained invalid address");
                return;
            }

            Guid.TryParse(lastGuidStr, out Guid lastGuid);
            if(lastGuid == null)
            {
                ISLogger.Write($"Service->Config file contained invalid guid");
                return;
            }

            if(lastName == "" && lastName.Length > 32)
            {
                ISLogger.Write($"Serivce->Config file contained invalid username");
                return;
            }

            lastServer = lastEp;
            keepRetryingConnection = true;
            Ipc_Connect(lastEp, lastName, lastGuid);
        }
        
        private void DelayReconnect(int ms)
        {
            Task.Run(new Action(() => { Thread.Sleep(ms);
                cSocket.Connect(lastServer, clientName, clientId);
            }));
        }

        

        private void NamedIpc_Disconnect(object sender, EventArgs e)
        {
            ISLogger.Write($"Service->Ipc requested disconnect");
            if (!cSocket.IsStateConnected())
            {
                if (namedIpc.Active)
                {
                    namedIpc.SendObject(NIpcBasicMessage.AlreadyDisconnected);
                }
                
                return;
            }

            
            cSocket.Disconnect();
        }

        private void NamedIpc_RequestedState(object sender, EventArgs e)
        {
            SendState();
        }

        private void SendState()
        {
            try
            {
                if (cSocket.IsStateConnected())
                {
                    NIpcServiceStateMessage message = new NIpcServiceStateMessage(
                    cSocket.IsStateConnected(), cSocket.serverAddress.Address.ToString(), cSocket.serverAddress.Port,
                    clientName, clientId);
                    namedIpc.SendObject(message);
                }
                else
                {
                    NIpcServiceStateMessage message = new NIpcServiceStateMessage(
                    cSocket.IsStateConnected(), null, 0,
                    clientName, clientId);
                    namedIpc.SendObject(message);
                }

            }
            catch (Exception ex)
            {
                ISLogger.Write($"Server->Error sending service state to IPC: {ex.Message}");
            }
        }

        private void NamedIpc_Connect(object sender, IpcConnectArgs args)
        {
            ISLogger.Write($"Service->Ipc requested connect to {args.Server}");

            if (cSocket.IsStateConnected())
            {
                namedIpc.SendObject(NIpcBasicMessage.AlreadyConnected);
                ISLogger.Write($"Service->Cannot connect: already connected or attempting to connect {cSocket.State}");
                return;
            }
            lastServer = args.Server;
            clientName = args.ClientName;

            if(args.ClientGuid == Guid.Empty)
            {
                if(clientId == Guid.Empty)
                {
                    ISLogger.Write($"Creating new GUID");
                    clientId = Guid.NewGuid();
                }
            }
            else
            {
                clientId = args.ClientGuid;
            }


            Ipc_Connect(args.Server, args.ClientName, clientId);
            namedIpc.SendObject(NIpcBasicMessage.AttemptingConnection);
        }

        private void CreatePipes()
        {
            ipcWrite = new AnonPipeServer();
            ipcRead = new AnonPipeServerRead();
            ipcRead.ClipboardTextCopied += IpcRead_ClipboardTextCopied;
            ipcRead.EdgeHit += IpcRead_EdgeHit;
        }

        private void IpcRead_EdgeHit(object sender, BoundEdge edge)
        {
            EdgeHit(edge);
        }

        private void IpcRead_ClipboardTextCopied(object sender, string text)
        {
            ISLogger.Write("IPC->Clipboard text copied");
            if (cSocket.IsStateConnected())
            {
                cSocket.SendClipboardText(text);
            }
        }

        private void EnableSASHKey()
        {
            string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
            int keyVal = 3;

            try
            {
                Registry.SetValue(keyPath, "SoftwareSASGeneration", keyVal);
                ISLogger.Write(@"Written registry value HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System\SoftwareSASGeneration = 3");

            }catch(Exception ex)
            {
                ISLogger.Write("Failed to allow SoftwareSASGeneration: " + ex.Message);
            }
        }

        private void EdgeHit(BoundEdge edge)
        {
            if (cSocket.IsStateConnected())
            {
                switch (edge) {
                    case BoundEdge.Bottom:
                        cSocket?.SendCommand(MessageType.ClientBoundsBottom);
                        break;
                    case BoundEdge.Left:
                        cSocket?.SendCommand(MessageType.ClientBoundsLeft);
                        break;
                    case BoundEdge.Right:
                        cSocket?.SendCommand(MessageType.ClientBoundsRight);
                        break;
                    case BoundEdge.Top:
                        cSocket?.SendCommand(MessageType.ClientBoundsTop);
                        break;
                }
            }
        }

        private void CSocket_ConnectionFailed(object sender, EventArgs e)
        {
            ISLogger.Write($"Service->Failed to connect to {cSocket.serverAddress}");

            if (namedIpc.Active) 
            {
                namedIpc.SendObject(NIpcBasicMessage.ConnectionFailed);
            }
            if (keepRetryingConnection)
            {
                try
                {
                    ISLogger.Write($"Service->Auto retry enabled... trying to reconnect in 2s");
                    DelayReconnect(2000);
                }
                catch (Exception ex)
                {
                    ISLogger.Write($"Service->Auto retry error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// This method runs on a session change and launches a new instance of SP if required.
        /// </summary>
        /// <param name="changeDescription"></param>
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            if (!launcher.SpRunning)
            {
                ISLogger.Write("SP not found... launching");

                try
                {
                    launcher.LaunchSp(ipcWrite.PipeHandleAsString, ipcRead.PipeHandleAsString);
                }
                catch (Exception ex)
                {
                    if (ex is InvalidOperationException || ex is Win32Exception)
                    {
                        ISLogger.Write($"Failed to launch inputsharesp: {ex.Message}");
                    }
                }
            }


            base.OnSessionChange(changeDescription);
        }

        public void Ipc_Connect(IPEndPoint dest, string name, Guid id)
        {
            lastServer = dest;
            clientName = name;
            

            if(id != Guid.Empty)
            {
                clientId = id;
            }

            cSocket.Connect(dest, name, clientId);
        }

        private void CSocket_Disconnected(object sender, EventArgs e)
        {
            if (namedIpc.Active)
            {
                namedIpc.SendObject(NIpcBasicMessage.Disconnected);
            }


        }

        private void CSocket_ConnectionError(object sender, EventArgs e)
        {
            ISLogger.Write($"Service->Socket error. disconnected");

            if (namedIpc.Active)
            {
                namedIpc.SendObject(NIpcBasicMessage.ConnectionError);
            }

            if (keepRetryingConnection)
            {
                try
                {
                    ISLogger.Write($"Service->Auto retry enabled... trying to reconnect");
                    DelayReconnect(2000);
                }
                catch (Exception ex)
                {
                    ISLogger.Write($"Service->Auto retry error: {ex.Message}");
                }
            }
        }

        private void CSocket_Connected(object sender, EventArgs e)
        {
            ISLogger.Write($"Service->Socket connected to: {cSocket.serverAddress}");

            ConfigManager.WriteConfig("LastName", clientName);
            ConfigManager.WriteConfig("LastGuid", clientId.ToString());
            ConfigManager.WriteConfig("LastAddress", cSocket.serverAddress.ToString());

            if (namedIpc.Active)
            {
                namedIpc.SendObject(NIpcBasicMessage.Connected);
            }
        }

        private void CSocket_MessageReceived(object sender, MessageType msg)
        {
            switch (msg)
            {
                case MessageType.ClientInFocus:
                    OnFocus();
                    break;
                case MessageType.ClientOutOfFocus:
                    OnUnfocus();
                    break;
            }
        }

        private void OnFocus()
        {
            //Notifies SP that the server input is directed at this client
            ipcWrite.SendInput(new ISInputData(ISInputCode.IS_RELEASEALL, 0, 0));
        }

        private void OnUnfocus()
        {
            //Notifies SP that the server input is no longer directed at this client
            ipcWrite.SendInput(new ISInputData(ISInputCode.IS_RELEASEALL, 0, 0));
        }

        private void CSocket_ClipboardTextReceived(object sender, string cbText)
        {
            if (ipcWrite.Connected)
            {
                //Send clipboard text to SP
                ipcWrite.SendClipboardText(cbText);
                ISLogger.Write("IPC->Sending clipboard text");
            }
            
        }

        private void CSocket_InputReceived(object sender, InputshareLib.Input.ISInputData input)
        {

            //Check if SAS (alt+ctrl+del) was received
            if (input.Code == ISInputCode.IS_SENDSAS)
            {
                OnSASReceived();
                return;
            }
            if (ipcWrite.Connected)
            {
                //send input to SP
                ipcWrite.SendInput(input);
            }
            
        }

        private void OnSASReceived()
        {
            ISLogger.Write("Sending SAS");
            SendSAS(false); //Sends alt+ctrl+del
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ISLogger.Write("UNHANDLED EXCEPTION");
            Exception ex = e.ExceptionObject as Exception;
            while(ex.InnerException != null) { ex = ex.InnerException; }

            ISLogger.Write(ex.Message);
            ISLogger.Write(ex.Source);
            ISLogger.Write(ex.StackTrace);

            Thread.Sleep(2000); //Give islogger time to finish writing log
            Stop();
        }

        private void SpMon_SpStopped(object sender, EventArgs e)
        {
            ISLogger.Write("Service->SP process stopped");

            //Relaunch SP if needed
            if (!launcher.SpRunning)
            {
                //We need to create new anonymous pipes to connect to the newly made SP process
                CreatePipes();
                launcher.LaunchSp(ipcWrite.PipeHandleAsString, ipcRead.PipeHandleAsString);
                ipcRead.RemoveHandle();
                ipcWrite.RemoveHandle();
            }
        }

        private void SpMon_SpStarted(object sender, EventArgs e)
        {
            ISLogger.Write($"SP process started");
        }

        protected override void OnStop()
        {
            try
            {
                ISLogger.Write($"Stopping service...");
                ipcRead?.Close();
                spMon?.StopMonitoring();
                launcher.KillSp();
                namedIpc?.Dispose();
                cSocket?.Dispose();
                ISLogger.Exit();
            }catch(Exception) { }
            
        }
    }
}
