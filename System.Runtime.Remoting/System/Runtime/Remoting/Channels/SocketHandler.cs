namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.Remoting;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    internal abstract class SocketHandler
    {
        private IAsyncResult _beginReadAsyncResult;
        private AsyncCallback _beginReadCallback;
        private byte[] _byteBuffer;
        private int _controlCookie;
        private DateTime _creationTime;
        private WaitCallback _dataArrivedCallback;
        private object _dataArrivedCallbackState;
        private byte[] _dataBuffer;
        private int _dataBufferSize;
        private int _dataCount;
        private int _dataOffset;
        private WindowsIdentity _impersonationIdentity;
        private RequestQueue _requestQueue;
        protected Socket NetSocket;
        protected Stream NetStream;

        private SocketHandler()
        {
            this._byteBuffer = new byte[4];
            this._controlCookie = 1;
        }

        public SocketHandler(Socket socket, Stream netStream)
        {
            this._byteBuffer = new byte[4];
            this._controlCookie = 1;
            this._beginReadCallback = new AsyncCallback(this.BeginReadMessageCallback);
            this._creationTime = DateTime.UtcNow;
            this.NetSocket = socket;
            this.NetStream = netStream;
            this._dataBuffer = CoreChannel.BufferPool.GetBuffer();
            this._dataBufferSize = this._dataBuffer.Length;
            this._dataOffset = 0;
            this._dataCount = 0;
        }

        internal SocketHandler(Socket socket, RequestQueue requestQueue, Stream netStream) : this(socket, netStream)
        {
            this._requestQueue = requestQueue;
        }

        public void BeginReadMessage()
        {
            bool flag = false;
            try
            {
                if (this._requestQueue != null)
                {
                    this._requestQueue.ScheduleMoreWorkIfNeeded();
                }
                this.PrepareForNewMessage();
                if (this._dataCount == 0)
                {
                    this._beginReadAsyncResult = this.NetStream.BeginRead(this._dataBuffer, 0, this._dataBufferSize, this._beginReadCallback, null);
                }
                else
                {
                    flag = true;
                }
            }
            catch (Exception exception)
            {
                this.CloseOnFatalError(exception);
            }
            if (flag)
            {
                if (this._requestQueue != null)
                {
                    this._requestQueue.ProcessNextRequest(this);
                }
                else
                {
                    this.ProcessRequestNow();
                }
                this._beginReadAsyncResult = null;
            }
        }

        public void BeginReadMessageCallback(IAsyncResult ar)
        {
            bool flag = false;
            try
            {
                this._beginReadAsyncResult = null;
                this._dataOffset = 0;
                this._dataCount = this.NetStream.EndRead(ar);
                if (this._dataCount <= 0)
                {
                    this.Close();
                }
                else
                {
                    flag = true;
                }
            }
            catch (Exception exception)
            {
                this.CloseOnFatalError(exception);
            }
            if (flag)
            {
                if (this._requestQueue != null)
                {
                    this._requestQueue.ProcessNextRequest(this);
                }
                else
                {
                    this.ProcessRequestNow();
                }
            }
        }

        private int BufferMoreData()
        {
            int num = this.ReadFromSocket(this._dataBuffer, 0, this._dataBufferSize);
            this._dataOffset = 0;
            this._dataCount = num;
            return num;
        }

        public virtual void Close()
        {
            if (this._requestQueue != null)
            {
                this._requestQueue.ScheduleMoreWorkIfNeeded();
            }
            if (this.NetStream != null)
            {
                this.NetStream.Close();
                this.NetStream = null;
            }
            if (this.NetSocket != null)
            {
                this.NetSocket.Close();
                this.NetSocket = null;
            }
            if (this._dataBuffer != null)
            {
                CoreChannel.BufferPool.ReturnBuffer(this._dataBuffer);
                this._dataBuffer = null;
            }
        }

        internal void CloseOnFatalError(Exception e)
        {
            try
            {
                this.SendErrorMessageIfPossible(e);
                this.Close();
            }
            catch
            {
                try
                {
                    this.Close();
                }
                catch
                {
                }
            }
        }

        internal bool CustomErrorsEnabled()
        {
            try
            {
                return RemotingConfiguration.CustomErrorsEnabled(this.IsLocalhost());
            }
            catch
            {
                return true;
            }
        }

        internal bool IsLocal()
        {
            return ((this.NetSocket == null) || IPAddress.IsLoopback(((IPEndPoint) this.NetSocket.RemoteEndPoint).Address));
        }

        internal bool IsLocalhost()
        {
            if ((this.NetSocket != null) && (this.NetSocket.RemoteEndPoint != null))
            {
                IPAddress address = ((IPEndPoint) this.NetSocket.RemoteEndPoint).Address;
                if (!IPAddress.IsLoopback(address))
                {
                    return CoreChannel.IsLocalIpAddress(address);
                }
            }
            return true;
        }

        public virtual void OnInputStreamClosed()
        {
        }

        protected abstract void PrepareForNewMessage();
        internal void ProcessRequestNow()
        {
            try
            {
                WaitCallback callback = this._dataArrivedCallback;
                if (callback != null)
                {
                    callback(this);
                }
            }
            catch (Exception exception)
            {
                this.CloseOnFatalError(exception);
            }
        }

        public bool RaceForControl()
        {
            return (1 == Interlocked.Exchange(ref this._controlCookie, 0));
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int num = 0;
            if (this._dataCount > 0)
            {
                int num2 = Math.Min(this._dataCount, count);
                StreamHelper.BufferCopy(this._dataBuffer, this._dataOffset, buffer, offset, num2);
                this._dataCount -= num2;
                this._dataOffset += num2;
                count -= num2;
                offset += num2;
                num += num2;
            }
            while (count > 0)
            {
                if (count < 0x100)
                {
                    this.BufferMoreData();
                    int num3 = Math.Min(this._dataCount, count);
                    StreamHelper.BufferCopy(this._dataBuffer, this._dataOffset, buffer, offset, num3);
                    this._dataCount -= num3;
                    this._dataOffset += num3;
                    count -= num3;
                    offset += num3;
                    num += num3;
                }
                else
                {
                    int num4 = this.ReadFromSocket(buffer, offset, count);
                    count -= num4;
                    offset += num4;
                    num += num4;
                }
            }
            return num;
        }

        protected bool ReadAndMatchFourBytes(byte[] buffer)
        {
            this.Read(this._byteBuffer, 0, 4);
            return ((((this._byteBuffer[0] == buffer[0]) && (this._byteBuffer[1] == buffer[1])) && (this._byteBuffer[2] == buffer[2])) && (this._byteBuffer[3] == buffer[3]));
        }

        public int ReadByte()
        {
            if (this.Read(this._byteBuffer, 0, 1) != -1)
            {
                return this._byteBuffer[0];
            }
            return -1;
        }

        private int ReadFromSocket(byte[] buffer, int offset, int count)
        {
            int num = this.NetStream.Read(buffer, offset, count);
            if (num <= 0)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Socket_UnderlyingSocketClosed"));
            }
            return num;
        }

        public int ReadInt32()
        {
            this.Read(this._byteBuffer, 0, 4);
            return ((((this._byteBuffer[0] & 0xff) | (this._byteBuffer[1] << 8)) | (this._byteBuffer[2] << 0x10)) | (this._byteBuffer[3] << 0x18));
        }

        protected byte[] ReadToByte(byte b)
        {
            return this.ReadToByte(b, null);
        }

        protected byte[] ReadToByte(byte b, ValidateByteDelegate validator)
        {
            byte[] dest = null;
            if (this._dataCount == 0)
            {
                this.BufferMoreData();
            }
            int num = this._dataOffset + this._dataCount;
            int srcOffset = this._dataOffset;
            int index = srcOffset;
            bool flag = false;
            while (!flag)
            {
                bool flag2 = index == num;
                flag = !flag2 && (this._dataBuffer[index] == b);
                if (((validator != null) && !flag2) && (!flag && !validator(this._dataBuffer[index])))
                {
                    throw new RemotingException(CoreChannel.GetResourceString("Remoting_Http_InvalidDataReceived"));
                }
                if (flag2 || flag)
                {
                    int count = index - srcOffset;
                    if (dest == null)
                    {
                        dest = new byte[count];
                        StreamHelper.BufferCopy(this._dataBuffer, srcOffset, dest, 0, count);
                    }
                    else
                    {
                        int length = dest.Length;
                        byte[] buffer2 = new byte[length + count];
                        StreamHelper.BufferCopy(dest, 0, buffer2, 0, length);
                        StreamHelper.BufferCopy(this._dataBuffer, srcOffset, buffer2, length, count);
                        dest = buffer2;
                    }
                    this._dataOffset += count;
                    this._dataCount -= count;
                    if (flag2)
                    {
                        this.BufferMoreData();
                        num = this._dataOffset + this._dataCount;
                        srcOffset = this._dataOffset;
                        index = srcOffset;
                    }
                    else if (flag)
                    {
                        this._dataOffset++;
                        this._dataCount--;
                    }
                }
                else
                {
                    index++;
                }
            }
            return dest;
        }

        protected string ReadToChar(char ch)
        {
            return this.ReadToChar(ch, null);
        }

        protected string ReadToChar(char ch, ValidateByteDelegate validator)
        {
            byte[] bytes = this.ReadToByte((byte) ch, validator);
            if (bytes == null)
            {
                return null;
            }
            if (bytes.Length == 0)
            {
                return string.Empty;
            }
            return Encoding.ASCII.GetString(bytes);
        }

        public string ReadToEndOfLine()
        {
            string str = this.ReadToChar('\r');
            if (this.ReadByte() == 10)
            {
                return str;
            }
            return null;
        }

        public ushort ReadUInt16()
        {
            this.Read(this._byteBuffer, 0, 2);
            return (ushort) ((this._byteBuffer[0] & 0xff) | (this._byteBuffer[1] << 8));
        }

        internal void RejectRequestNowSinceServerIsBusy()
        {
            this.CloseOnFatalError(new RemotingException(CoreChannel.GetResourceString("Remoting_ServerIsBusy")));
        }

        public void ReleaseControl()
        {
            this._controlCookie = 1;
        }

        protected virtual void SendErrorMessageIfPossible(Exception e)
        {
        }

        public void WriteByte(byte value, Stream outputStream)
        {
            this._byteBuffer[0] = value;
            outputStream.Write(this._byteBuffer, 0, 1);
        }

        public void WriteInt32(int value, Stream outputStream)
        {
            this._byteBuffer[0] = (byte) value;
            this._byteBuffer[1] = (byte) (value >> 8);
            this._byteBuffer[2] = (byte) (value >> 0x10);
            this._byteBuffer[3] = (byte) (value >> 0x18);
            outputStream.Write(this._byteBuffer, 0, 4);
        }

        public void WriteUInt16(ushort value, Stream outputStream)
        {
            this._byteBuffer[0] = (byte) value;
            this._byteBuffer[1] = (byte) (value >> 8);
            outputStream.Write(this._byteBuffer, 0, 2);
        }

        public DateTime CreationTime
        {
            get
            {
                return this._creationTime;
            }
        }

        public WaitCallback DataArrivedCallback
        {
            set
            {
                this._dataArrivedCallback = value;
            }
        }

        public object DataArrivedCallbackState
        {
            get
            {
                return this._dataArrivedCallbackState;
            }
            set
            {
                this._dataArrivedCallbackState = value;
            }
        }

        public WindowsIdentity ImpersonationIdentity
        {
            get
            {
                return this._impersonationIdentity;
            }
            set
            {
                this._impersonationIdentity = value;
            }
        }
    }
}

