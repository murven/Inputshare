using InputshareLib.Input;
using System;
using System.IO.Pipes;
using System.Text;

namespace InputshareLib.AnonIPC
{
    public class AnonPipeServer
    {
        private AnonymousPipeServerStream pWrite;
        public string PipeHandleAsString { get => pWrite.GetClientHandleAsString(); }
        public bool Connected { get => pWrite.IsConnected; }

        public AnonPipeServer()
        {
            pWrite = new AnonymousPipeServerStream(PipeDirection.Out, System.IO.HandleInheritability.Inheritable);
        }

        public void RemoveHandle()
        {
            pWrite.DisposeLocalCopyOfClientHandle();
        }

        private byte[] header6Bytes = BitConverter.GetBytes(6);
        public void SendInput(ISInputData input)
        {
            try
            {
                byte[] data = new byte[10];
                Buffer.BlockCopy(header6Bytes, 0, data, 0, 4);
                data[4] = (byte)IPCMessageType.Input;
                data[5] = (byte)input.Code;
                Buffer.BlockCopy(BitConverter.GetBytes(input.Param1), 0, data, 6, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(input.Param2), 0, data, 8, 2);
                pWrite.BeginWrite(data, 0, data.Length, WriteCallback, null);
                pWrite.Flush();
            }catch(Exception ex)
            {
                ISLogger.Write("IPC->Failed to send input data: " + ex.Message);
            }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            try
            {
                pWrite.EndWrite(ar);
            }catch(Exception ex)
            {
                ISLogger.Write("IPC->Error writing to pipe: " + ex.Message);
            }
        }

        public void SendClipboardText(string text)
        {
            try
            {
                int len = Encoding.Unicode.GetByteCount(text);
                byte[] data = new byte[len + 5];
                Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, data, 0, 4);
                data[4] = (byte)IPCMessageType.CopyToClipboard;
                Buffer.BlockCopy(Encoding.Unicode.GetBytes(text), 0, data, 5, len);
                pWrite.Write(data, 0, data.Length);
                pWrite.Flush();
            }catch(Exception ex)
            {
                ISLogger.Write("IPC->Failed to sent SetClipboardText: " + ex.Message);
            }
            
        }
        
    }
}
