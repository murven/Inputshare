using System;
using System.IO.Pipes;

namespace InputshareLib.NamedIPC
{
    public class NamedIpcClient : IDisposable
    {
        public event EventHandler<NIpcServiceStateMessage> ServiceStateReceived;
        public event EventHandler<NIpcBasicMessage> MessageReceived;

        private NamedPipeClientStream clientRead = new NamedPipeClientStream(".", "IsPipeW", PipeDirection.In);
        private NamedPipeClientStream clientWrite = new NamedPipeClientStream(".", "IsPipeR", PipeDirection.Out);

        public event EventHandler Disconnected;
        public event EventHandler Connected;

        private object threadLock = new object();

        private byte[] clientBuff = new byte[10240];

        public void Connect(int timeout)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException("NamedIpcClient");
            }

            clientRead.Connect(timeout);
            clientWrite.Connect(timeout);
            clientRead.BeginRead(clientBuff, 0, clientBuff.Length, ClientReadCallback, null);
        }

        public void SendObject(object obj)
        {
            if (disposedValue)
            {
                return;
            }
            lock (threadLock)
            {
                byte[] data = ObjectSerializer.Serialize(obj);
                //ISLogger.Write($"NamedIPC->sent {data.Length} bytes");
                clientWrite.Write(data, 0, data.Length);
            }
            
        }

        private void ClientReadCallback(IAsyncResult ar)
        {
            lock (threadLock)
            {
                if (disposedValue)
                    return;

                int bIn = clientRead.EndRead(ar);

                if (bIn == 0)
                {
                    OnConnectionError();
                    return;
                }
                ProcessObject(clientBuff);
                if (disposedValue)
                    return;
                clientRead.BeginRead(clientBuff, 0, clientBuff.Length, ClientReadCallback, null);
            }
        }

        private void ProcessObject(byte[] data)
        {
            try
            {
                object obj = ObjectSerializer.Deserialize<object>(data);
                
                Type objType = obj.GetType();
                //ISLogger.Write($"Service: {objType.Name}");
                if (objType == typeof(NIpcServiceStateMessage))
                {
                    ServiceStateReceived?.Invoke(this, obj as NIpcServiceStateMessage);
                }else if(objType == typeof(NIpcBasicMessage))
                {
                    NIpcBasicMessage msg = (NIpcBasicMessage)obj;
                    //ISLogger.Write($"Service: {msg}");
                    HandleBasicMessage(msg);
                }

            }catch(Exception ex)
            {
                ISLogger.Write($"NamedIpc->Error reading object: {ex.Message}");
            }
        }

        private void HandleBasicMessage(NIpcBasicMessage message)
        {
            try
            {
                MessageReceived?.Invoke(this, message);
            }
            catch(Exception ex)
            {
                ISLogger.Write($"NamedIpc->Error handling message: {ex.Message}");
            }
        }

        

        private void OnConnectionError()
        {
            if (disposedValue)
                return;
            lock (threadLock)
            {
                Disconnected?.Invoke(this, null);
                ISLogger.Write($"NamedIpc->Reconnecting to service");
                while (!disposedValue)
                {
                    try
                    {
                        if (!clientRead.IsConnected)
                            clientRead.Connect(500);

                        if (!clientWrite.IsConnected)
                            clientWrite.Connect(500);
                    }
                    catch (Exception) { }

                }

                if (disposedValue)
                    return;

                Connected?.Invoke(this, null);
                ISLogger.Write($"NamedIpc->Reconnected to service");
                clientRead.BeginRead(clientBuff, 0, clientBuff.Length, ClientReadCallback, null);
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
                    clientRead?.Dispose();
                    clientWrite?.Dispose();
                    clientBuff = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NamedIpcClient()
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
