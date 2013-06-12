namespace System.Net
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;

    internal class StreamFramer
    {
        private readonly AsyncCallback m_BeginWriteCallback;
        private FrameHeader m_CurReadHeader = new FrameHeader();
        private bool m_Eof;
        private NetworkStream m_NetworkStream;
        private readonly AsyncCallback m_ReadFrameCallback;
        private byte[] m_ReadHeaderBuffer;
        private FrameHeader m_ReadVerifier = new FrameHeader(-1, -1, -1);
        private Stream m_Transport;
        private FrameHeader m_WriteHeader = new FrameHeader();
        private byte[] m_WriteHeaderBuffer;

        public StreamFramer(Stream Transport)
        {
            if ((Transport == null) || (Transport == Stream.Null))
            {
                throw new ArgumentNullException("Transport");
            }
            this.m_Transport = Transport;
            if (this.m_Transport.GetType() == typeof(NetworkStream))
            {
                this.m_NetworkStream = Transport as NetworkStream;
            }
            this.m_ReadHeaderBuffer = new byte[this.m_CurReadHeader.Size];
            this.m_WriteHeaderBuffer = new byte[this.m_WriteHeader.Size];
            this.m_ReadFrameCallback = new AsyncCallback(this.ReadFrameCallback);
            this.m_BeginWriteCallback = new AsyncCallback(this.BeginWriteCallback);
        }

        public IAsyncResult BeginReadMessage(AsyncCallback asyncCallback, object stateObject)
        {
            WorkerAsyncResult result;
            if (this.m_Eof)
            {
                result = new WorkerAsyncResult(this, stateObject, asyncCallback, null, 0, 0);
                result.InvokeCallback(-1);
                return result;
            }
            result = new WorkerAsyncResult(this, stateObject, asyncCallback, this.m_ReadHeaderBuffer, 0, this.m_ReadHeaderBuffer.Length);
            IAsyncResult transportResult = this.Transport.BeginRead(this.m_ReadHeaderBuffer, 0, this.m_ReadHeaderBuffer.Length, this.m_ReadFrameCallback, result);
            if (transportResult.CompletedSynchronously)
            {
                this.ReadFrameComplete(transportResult);
            }
            return result;
        }

        private void BeginWriteCallback(IAsyncResult transportResult)
        {
            if (!transportResult.CompletedSynchronously)
            {
                WorkerAsyncResult asyncState = (WorkerAsyncResult) transportResult.AsyncState;
                try
                {
                    this.BeginWriteComplete(transportResult);
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    asyncState.InvokeCallback(exception);
                }
            }
        }

        private void BeginWriteComplete(IAsyncResult transportResult)
        {
            WorkerAsyncResult result;
        Label_0000:
            result = (WorkerAsyncResult) transportResult.AsyncState;
            this.Transport.EndWrite(transportResult);
            if (result.Offset == result.End)
            {
                result.InvokeCallback();
            }
            else
            {
                result.Offset = result.End;
                transportResult = this.Transport.BeginWrite(result.Buffer, 0, result.End, this.m_BeginWriteCallback, result);
                if (transportResult.CompletedSynchronously)
                {
                    goto Label_0000;
                }
            }
        }

        public IAsyncResult BeginWriteMessage(byte[] message, AsyncCallback asyncCallback, object stateObject)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            this.m_WriteHeader.PayloadSize = message.Length;
            this.m_WriteHeader.CopyTo(this.m_WriteHeaderBuffer, 0);
            if ((this.m_NetworkStream != null) && (message.Length != 0))
            {
                BufferOffsetSize[] buffers = new BufferOffsetSize[] { new BufferOffsetSize(this.m_WriteHeaderBuffer, 0, this.m_WriteHeaderBuffer.Length, false), new BufferOffsetSize(message, 0, message.Length, false) };
                return this.m_NetworkStream.BeginMultipleWrite(buffers, asyncCallback, stateObject);
            }
            if (message.Length == 0)
            {
                return this.Transport.BeginWrite(this.m_WriteHeaderBuffer, 0, this.m_WriteHeaderBuffer.Length, asyncCallback, stateObject);
            }
            WorkerAsyncResult state = new WorkerAsyncResult(this, stateObject, asyncCallback, message, 0, message.Length);
            IAsyncResult transportResult = this.Transport.BeginWrite(this.m_WriteHeaderBuffer, 0, this.m_WriteHeaderBuffer.Length, this.m_BeginWriteCallback, state);
            if (transportResult.CompletedSynchronously)
            {
                this.BeginWriteComplete(transportResult);
            }
            return state;
        }

        public byte[] EndReadMessage(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            WorkerAsyncResult result = asyncResult as WorkerAsyncResult;
            if (result == null)
            {
                throw new ArgumentException(SR.GetString("net_io_async_result", new object[] { typeof(WorkerAsyncResult).FullName }), "asyncResult");
            }
            if (!result.InternalPeekCompleted)
            {
                result.InternalWaitForCompletion();
            }
            if (result.Result is Exception)
            {
                throw ((Exception) result.Result);
            }
            switch (((int) result.Result))
            {
                case -1:
                    this.m_Eof = true;
                    return null;

                case 0:
                    return new byte[0];
            }
            return result.Buffer;
        }

        public void EndWriteMessage(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            WorkerAsyncResult result = asyncResult as WorkerAsyncResult;
            if (result != null)
            {
                if (!result.InternalPeekCompleted)
                {
                    result.InternalWaitForCompletion();
                }
                if (result.Result is Exception)
                {
                    throw ((Exception) result.Result);
                }
            }
            else
            {
                this.Transport.EndWrite(asyncResult);
            }
        }

        private void ReadFrameCallback(IAsyncResult transportResult)
        {
            if (!transportResult.CompletedSynchronously)
            {
                WorkerAsyncResult asyncState = (WorkerAsyncResult) transportResult.AsyncState;
                try
                {
                    this.ReadFrameComplete(transportResult);
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    if (!(exception is IOException))
                    {
                        exception = new IOException(SR.GetString("net_io_readfailure", new object[] { exception.Message }), exception);
                    }
                    asyncState.InvokeCallback(exception);
                }
            }
        }

        private void ReadFrameComplete(IAsyncResult transportResult)
        {
            WorkerAsyncResult result;
        Label_0000:
            result = (WorkerAsyncResult) transportResult.AsyncState;
            int num = this.Transport.EndRead(transportResult);
            result.Offset += num;
            if (num <= 0)
            {
                object obj2 = null;
                if (!result.HeaderDone && (result.Offset == 0))
                {
                    obj2 = -1;
                }
                else
                {
                    obj2 = new IOException(SR.GetString("net_frame_read_io"));
                }
                result.InvokeCallback(obj2);
            }
            else
            {
                if (result.Offset >= result.End)
                {
                    if (result.HeaderDone)
                    {
                        result.HeaderDone = false;
                        result.InvokeCallback(result.End);
                        return;
                    }
                    result.HeaderDone = true;
                    this.m_CurReadHeader.CopyFrom(result.Buffer, 0, this.m_ReadVerifier);
                    int payloadSize = this.m_CurReadHeader.PayloadSize;
                    if (payloadSize < 0)
                    {
                        result.InvokeCallback(new IOException(SR.GetString("net_frame_read_size")));
                    }
                    if (payloadSize == 0)
                    {
                        result.InvokeCallback(0);
                        return;
                    }
                    if (payloadSize > this.m_CurReadHeader.MaxMessageSize)
                    {
                        throw new InvalidOperationException(SR.GetString("net_frame_size", new object[] { this.m_CurReadHeader.MaxMessageSize.ToString(NumberFormatInfo.InvariantInfo), payloadSize.ToString(NumberFormatInfo.InvariantInfo) }));
                    }
                    byte[] buffer = new byte[payloadSize];
                    result.Buffer = buffer;
                    result.End = buffer.Length;
                    result.Offset = 0;
                }
                transportResult = this.Transport.BeginRead(result.Buffer, result.Offset, result.End - result.Offset, this.m_ReadFrameCallback, result);
                if (transportResult.CompletedSynchronously)
                {
                    goto Label_0000;
                }
            }
        }

        public byte[] ReadMessage()
        {
            int num2;
            if (this.m_Eof)
            {
                return null;
            }
            int offset = 0;
            byte[] readHeaderBuffer = this.m_ReadHeaderBuffer;
            while (offset < readHeaderBuffer.Length)
            {
                num2 = this.Transport.Read(readHeaderBuffer, offset, readHeaderBuffer.Length - offset);
                if (num2 == 0)
                {
                    if (offset != 0)
                    {
                        throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
                    }
                    this.m_Eof = true;
                    return null;
                }
                offset += num2;
            }
            this.m_CurReadHeader.CopyFrom(readHeaderBuffer, 0, this.m_ReadVerifier);
            if (this.m_CurReadHeader.PayloadSize > this.m_CurReadHeader.MaxMessageSize)
            {
                throw new InvalidOperationException(SR.GetString("net_frame_size", new object[] { this.m_CurReadHeader.MaxMessageSize.ToString(NumberFormatInfo.InvariantInfo), this.m_CurReadHeader.PayloadSize.ToString(NumberFormatInfo.InvariantInfo) }));
            }
            readHeaderBuffer = new byte[this.m_CurReadHeader.PayloadSize];
            for (offset = 0; offset < readHeaderBuffer.Length; offset += num2)
            {
                num2 = this.Transport.Read(readHeaderBuffer, offset, readHeaderBuffer.Length - offset);
                if (num2 == 0)
                {
                    throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
                }
            }
            return readHeaderBuffer;
        }

        public void WriteMessage(byte[] message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            this.m_WriteHeader.PayloadSize = message.Length;
            this.m_WriteHeader.CopyTo(this.m_WriteHeaderBuffer, 0);
            if ((this.m_NetworkStream != null) && (message.Length != 0))
            {
                BufferOffsetSize[] buffers = new BufferOffsetSize[] { new BufferOffsetSize(this.m_WriteHeaderBuffer, 0, this.m_WriteHeaderBuffer.Length, false), new BufferOffsetSize(message, 0, message.Length, false) };
                this.m_NetworkStream.MultipleWrite(buffers);
            }
            else
            {
                this.Transport.Write(this.m_WriteHeaderBuffer, 0, this.m_WriteHeaderBuffer.Length);
                if (message.Length != 0)
                {
                    this.Transport.Write(message, 0, message.Length);
                }
            }
        }

        public FrameHeader ReadHeader
        {
            get
            {
                return this.m_CurReadHeader;
            }
        }

        public Stream Transport
        {
            get
            {
                return this.m_Transport;
            }
        }

        public FrameHeader WriteHeader
        {
            get
            {
                return this.m_WriteHeader;
            }
        }
    }
}

