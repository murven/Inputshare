using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace InputshareLib.AnonIPC
{
    public class AnonPipeClient
    {
        public AnonymousPipeClientStream pWrite { get; private set; }

        public AnonPipeClient(string pipeHandle)
        {
            pWrite = new AnonymousPipeClientStream(PipeDirection.Out, pipeHandle);
        }

        public void SendClipboardTextCopied(string text)
        {
            try
            {

                int len = Encoding.Unicode.GetByteCount(text);
                byte[] data = new byte[len + 5];
                Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, data, 0, 4);
                data[4] = (byte)IPCMessageType.ClipboardTextCopied;
                Buffer.BlockCopy(Encoding.Unicode.GetBytes(text), 0, data, 5, len);
                pWrite.BeginWrite(data, 0, data.Length, WriteCallback, null);
                pWrite.Flush();
            }catch(Exception ex)
            {
                ISLogger.Write("IPC->Failed to sent clipboard text: " + ex.Message);
            }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            try
            {
                pWrite.EndWrite(ar);
            }catch(Exception ex)
            {
                ISLogger.Write("IPC->Failed to write to pipe: " + ex.Message);
            }
        }

        public void SendEdgeHit(BoundEdge edge)
        {
            try
            {
                byte[] data = new byte[5];
                Buffer.BlockCopy(BitConverter.GetBytes(5), 0, data, 0, 4);
                switch (edge)
                {
                    case BoundEdge.Bottom:
                        data[4] = (byte)IPCMessageType.EdgeHitBottom;
                        break;
                    case BoundEdge.Left:
                        data[4] = (byte)IPCMessageType.EdgeHitLeft;
                        break;
                    case BoundEdge.Right:
                        data[4] = (byte)IPCMessageType.EdgeHitRight;
                        break;
                    case BoundEdge.Top:
                        data[4] = (byte)IPCMessageType.EdgeHitTop;
                        break;
                }
                pWrite.Write(data, 0, data.Length);
                pWrite.Flush();
            }catch(Exception ex)
            {
                ISLogger.Write("IPC->Failed to send edge hit message: " + ex.Message);
            }
        }
    }
}
