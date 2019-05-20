using InputshareLib.Input;
using System;
using System.IO.Pipes;
using System.Text;

namespace InputshareLib.AnonIPC
{
    public class AnonPipeClientRead 
    {
        public AnonymousPipeClientStream pRead { get; private set; }
        private byte[] pipeBuff;

        public event EventHandler<ISInputData> InputDataReceived;
        public event EventHandler<string> CopyToClipboardReceived;

        private bool closed = false;

        public AnonPipeClientRead(string pipeHandle)
        {
            pRead = new AnonymousPipeClientStream(PipeDirection.In, pipeHandle);
            pipeBuff = new byte[10240];
            pRead.BeginRead(pipeBuff, 0, 4, PipeReadCallback, null);
        }

        public void Close()
        {
            closed = true;
            pRead?.Close();
            pipeBuff = null;
        }

        private void PipeReadCallback(IAsyncResult ar)
        {
            try
            {
                int bytesIn = pRead.EndRead(ar);
                if(bytesIn == 0)
                {
                    throw new Exception("Pipe disconnected");
                }

                int pSize = BitConverter.ToInt32(pipeBuff, 0);
                if(pSize >= 10240)
                {
                    pipeBuff = new byte[pSize+1];
                    ISLogger.Write("Pipe size set to " + (pSize+1));
                }

                if(pRead.Read(pipeBuff, 0, pSize) == 0)
                {
                    throw new Exception("Pipe disconnected");
                }

                switch((IPCMessageType)pipeBuff[0])
                {
                    case IPCMessageType.Input:
                        InputDataReceived?.Invoke(this, new ISInputData((ISInputCode)pipeBuff[1], BitConverter.ToInt16(pipeBuff, 2), BitConverter.ToInt16(pipeBuff, 4)));
                        break;
                    case IPCMessageType.CopyToClipboard:
                        CopyToClipboardReceived?.Invoke(this, Encoding.Unicode.GetString(pipeBuff, 1, pipeBuff.Length - 1));
                        pipeBuff = new byte[10240]; //We need to make sure that the string doesnt sit in the buffer
                        //So that remaining text is not copied after a short string
                        break;
                    default:
                        ISLogger.Write("IPC->Unexpected message type " + (IPCMessageType)pipeBuff[0]);
                        break;
                }

                if(pipeBuff.Length != 10240)
                {
                    pipeBuff = new byte[10240];
                }

                pRead.BeginRead(pipeBuff, 0, 4, PipeReadCallback, null);
            }catch(Exception ex)
            {
                if (closed)
                    return;

                ISLogger.Write("Anon IPC Exception!\n {0}", ex.Message);
                ISLogger.Write("Source: " + ex.Source);
                ISLogger.Write("Trace: " + ex.StackTrace);
            }
        }
    }
}
