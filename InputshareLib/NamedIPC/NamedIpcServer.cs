using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InputshareLib.NamedIPC
{
    public class IpcConnectArgs
    {
        public IpcConnectArgs(IPEndPoint server, string clientName, Guid clientGuid)
        {
            Server = server;
            ClientName = clientName;
            ClientGuid = clientGuid;
        }

        public IPEndPoint Server { get; }
        public string ClientName { get; }
        public Guid ClientGuid { get; }
    }

    public class NamedIpcServer : IDisposable
    {
        public event EventHandler<IpcConnectArgs> Ipc_Connect;
        public event EventHandler Ipc_Disconnect;
        public event EventHandler RequestedState;

        private NamedPipeServerStream serverWrite = new NamedPipeServerStream("IsPipeW", PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        private NamedPipeServerStream serverRead = new NamedPipeServerStream("IsPipeR", PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

        private byte[] serverBuff = new byte[10240];

        public bool Active { get => writeConnected && readConnected; }

        private bool writeConnected = false;
        private bool readConnected = false;

        private Thread invokeThread;
        private CancellationTokenSource invokeThreadCancelToken;
        private BlockingCollection<Action> invokeQueue;
        public NamedIpcServer()
        {
            invokeQueue = new BlockingCollection<Action>();
            invokeThreadCancelToken = new CancellationTokenSource();
            invokeThread = new Thread(InvokeThreadLoop);
            invokeThread.Start();
            serverWrite.BeginWaitForConnection(ServerWaitForConnectionCallback, null);
            serverRead.BeginWaitForConnection(ServerReadWaitForConnectionCallback, null);
        }

        private void InvokeThreadLoop()
        {
            while (!disposedValue)
            {
                try
                {
                    Action invoke = invokeQueue.Take(invokeThreadCancelToken.Token);
                    invoke();
                }catch(OperationCanceledException)
                {
                    
                }catch(Exception ex)
                {
                    ISLogger.Write($"NamedIpc->Error on invoke thread: {ex.Message}");
                }
            }
        }

        public void SendObject(object obj)
        {
            try
            {
                byte[] data = ObjectSerializer.Serialize(obj);
                //ISLogger.Write($"NamedIPC->sent {data.Length}");
                serverWrite.Write(data, 0, data.Length);
            }
            catch(Exception ex)
            {
                ISLogger.Write($"NamedIpc->Error writing to pipe: {ex.Message}");
            }
            
        }

        public void Stop()
        {
            ISLogger.Write($"Closing IpcServer pipes");
            invokeThreadCancelToken.Cancel();
            serverWrite.Dispose();
            serverRead.Dispose();
        }

        private void ServerReadCallback(IAsyncResult ar)
        {
            try
            {
                int bIn = serverRead.EndRead(ar);

                if(bIn == 0)
                {
                    if (disposedValue)
                        return;

                    throw new Exception("Client disconnected");
                }

                invokeQueue.Add(new Action(() => { ProcessObject(serverBuff); }));
                

                serverRead.BeginRead(serverBuff, 0, serverBuff.Length, ServerReadCallback, null);
            }catch(Exception ex)
            {
                ISLogger.Write($"NamedIpc->Read error: {ex.Message}");
                OnConnectionError();
            }
        }

        private void ProcessObject(byte[] data)
        {
            try
            {
                object obj = ObjectSerializer.Deserialize<object>(data);
                Type objType = obj.GetType();
                //ISLogger.Write($"NamedIpc->{objType}");
                if (objType == typeof(NIpcConnectMessage))
                {
                    HandleConnectMessage(obj as NIpcConnectMessage);
                    return;
                }else if(objType == typeof(NIpcBasicMessage))
                {
                    HandleBasicMessage((NIpcBasicMessage)obj);
                    return;
                }  
            }
            catch (Exception ex)
            {
                ISLogger.Write($"NamedIpc->Error reading object: {ex.Message}");
            }
        }

        private void HandleBasicMessage(NIpcBasicMessage message)
        {
            //ISLogger.Write($"NamedIpc->{message}");
            switch (message)
            {
                case NIpcBasicMessage.GetState:
                    {
                        invokeQueue.Add(new Action(() => { RequestedState?.Invoke(this, null); }));
                        return;
                    }
                case NIpcBasicMessage.Disconnect:
                    invokeQueue.Add(new Action(() => { Ipc_Disconnect?.Invoke(this, null); }));
                    return;

            }
        }

       

        private void HandleConnectMessage(NIpcConnectMessage message)
        {
            IPAddress address = null;

            if (message.AddressType == InputshareLib.NamedIPC.NIpcConnectMessage.AddressMode.IP)
            {
                IPAddress.TryParse(message.Address, out address);

                if (address == null)
                {
                    ISLogger.Write($"NamedIpc->Ipc sent invalid address");
                    SendObject(NIpcBasicMessage.InvalidAddress);
                    return;
                }
            }
            else
            {
                IPHostEntry host = Dns.GetHostEntry(message.Address);
                if (host.AddressList.Length > 0)
                {
                    address = host.AddressList[0];
                }
                else
                {
                    ISLogger.Write($"NamedIpc->Ipc sent invalid address");
                    return;
                }
            }

            if (message.ClientId == null)
            {
                ISLogger.Write($"NamedIpc->Ipc sent invalid client guid");
                SendObject(NIpcBasicMessage.InvalidGuid);
                return;
            }

            if (message.ClientName.Length > 32 || message.ClientName.Length < 3)
            {
                ISLogger.Write($"NamedIpc->Ipc sent invalid client name");
                SendObject(NIpcBasicMessage.InvalidName);
                return;

            }

            if (message.Port == 0 || message.Port > 65535)
            {
                ISLogger.Write($"Service->Ipc sent invalid server port");
                SendObject(NIpcBasicMessage.InvalidPort);
                return;
            }

            //Ipc_Connect?.BeginInvoke(this, new IpcConnectArgs(new IPEndPoint(address, message.Port), message.ClientName, message.ClientId), ConnectEventInvokeCallback, null);
            //Ipc_Connect(this, new IpcConnectArgs(new IPEndPoint(address, message.Port), message.ClientName, message.ClientId));
            invokeQueue.Add(new Action(() => { Ipc_Connect(this, new IpcConnectArgs(new IPEndPoint(address, message.Port), message.ClientName, message.ClientId)); }));
        }





        private void OnConnectionError()
        {
            invokeThreadCancelToken.Cancel();
            readConnected = false;
            writeConnected = false;
            serverWrite.Close();
            serverRead.Close();

            serverWrite = new NamedPipeServerStream("IsPipeW", PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            serverRead = new NamedPipeServerStream("IsPipeR", PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            invokeQueue = new BlockingCollection<Action>();
            invokeThreadCancelToken = new CancellationTokenSource();
            invokeThread = new Thread(InvokeThreadLoop);
            invokeThread.Start();
            serverWrite.BeginWaitForConnection(ServerWaitForConnectionCallback, null);
            serverRead.BeginWaitForConnection(ServerReadWaitForConnectionCallback, null);
        }

        private void ServerWaitForConnectionCallback(IAsyncResult ar)
        {
            try
            {
                serverWrite.EndWaitForConnection(ar);
                ISLogger.Write("NamedIpc->write client connected");
                writeConnected = true;
            }catch(Exception ex)
            {
                if (disposedValue)
                    return;

                ISLogger.Write($"NamedIpc->Error waiting for write pipe connection: {ex.Message}");
            }
            
        }
        private void ServerReadWaitForConnectionCallback(IAsyncResult ar)
        {
            try
            {
                serverRead.EndWaitForConnection(ar);
                ISLogger.Write("NamedIpc->read client connected");
                readConnected = true;
                serverRead.BeginRead(serverBuff, 0, serverBuff.Length, ServerReadCallback, null);
            }
            catch(Exception ex)
            {
                if (disposedValue)
                    return;

                ISLogger.Write($"NamedIpc->Error waiting for read pipe connection: {ex.Message}");
            }
            
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    invokeThreadCancelToken.Cancel();
                    serverWrite.Dispose();
                    serverRead.Dispose();
                }


                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NamedIpcServer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
