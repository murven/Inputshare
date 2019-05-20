using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace InputshareLib.AnonIPC
{
    public class AnonPipeServerRead
    {
        private AnonymousPipeServerStream pRead;
        private byte[] pipeBuff;

        public string PipeHandleAsString { get => pRead.GetClientHandleAsString(); }

        public event EventHandler<string> ClipboardTextCopied;
        public event EventHandler<BoundEdge> EdgeHit;

        private bool closed = false;
        public AnonPipeServerRead()
        {
            pRead = new AnonymousPipeServerStream(PipeDirection.In, System.IO.HandleInheritability.Inheritable);
            pipeBuff = new byte[10240];
            pRead.BeginRead(pipeBuff, 0, 4, PipeReadCallback, null);
        }
        public void RemoveHandle()
        {
            pRead.DisposeLocalCopyOfClientHandle();
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
                if (bytesIn == 0)
                {
                    if (closed)
                        return;

                    throw new Exception("Pipe disconnected");
                }

                int pSize = BitConverter.ToInt32(pipeBuff, 0);

                if (pSize >= 10240)
                {
                    pipeBuff = new byte[pSize + 1];
                    ISLogger.Write("Pipe size set to " + (pSize + 1));
                }

                pRead.Read(pipeBuff, 0, pSize);

                switch ((IPCMessageType)pipeBuff[0])
                {
                    case IPCMessageType.ClipboardTextCopied:
                        ClipboardTextCopied?.Invoke(this, Encoding.Unicode.GetString(pipeBuff, 1, pipeBuff.Length-1));
                        pipeBuff = new byte[10240];
                        break;
                    case IPCMessageType.EdgeHitBottom:
                        EdgeHit?.Invoke(this, BoundEdge.Bottom);
                        break;
                    case IPCMessageType.EdgeHitLeft:
                        EdgeHit?.Invoke(this, BoundEdge.Left);
                        break;
                    case IPCMessageType.EdgeHitRight:
                        EdgeHit?.Invoke(this, BoundEdge.Right);
                        break;
                    case IPCMessageType.EdgeHitTop:
                        EdgeHit?.Invoke(this, BoundEdge.Top);
                        break;
                    default:
                        ISLogger.Write("IPC->Unexpected message type " + (IPCMessageType)pipeBuff[0]);
                        break;
                }

                if (pipeBuff.Length != 10240)
                {
                    pipeBuff = new byte[10240];
                }

                pRead.BeginRead(pipeBuff, 0, 4, PipeReadCallback, null);
            }
            catch (Exception ex)
            {
                if (closed)
                    return;

                ISLogger.Write("Anon IPC Exception!\n {0}", ex.Message);
                ISLogger.Write($"{ex.Source}");
                ISLogger.Write($"{ex.StackTrace}");
            }
        }
    }
}
