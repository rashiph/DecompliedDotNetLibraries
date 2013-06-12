namespace System.Net.Security
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    internal class _SslStream
    {
        private byte[] _InternalBuffer;
        private int _InternalBufferCount;
        private int _InternalOffset;
        private static AsyncCallback _MulitpleWriteCallback = new AsyncCallback(_SslStream.MulitpleWriteCallback);
        private int _NestedRead;
        private int _NestedWrite;
        private FixedSizeReader _Reader;
        private static AsyncProtocolCallback _ReadFrameCallback = new AsyncProtocolCallback(_SslStream.ReadFrameCallback);
        private static AsyncProtocolCallback _ReadHeaderCallback = new AsyncProtocolCallback(_SslStream.ReadHeaderCallback);
        private static AsyncProtocolCallback _ResumeAsyncReadCallback = new AsyncProtocolCallback(_SslStream.ResumeAsyncReadCallback);
        private static AsyncProtocolCallback _ResumeAsyncWriteCallback = new AsyncProtocolCallback(_SslStream.ResumeAsyncWriteCallback);
        private SslState _SslState;
        private static AsyncCallback _WriteCallback = new AsyncCallback(_SslStream.WriteCallback);

        internal _SslStream(SslState sslState)
        {
            this._SslState = sslState;
            this._Reader = new FixedSizeReader(this._SslState.InnerStream);
        }

        internal IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            BufferAsyncResult userAsyncResult = new BufferAsyncResult(this, buffer, offset, count, asyncState, asyncCallback);
            AsyncProtocolRequest asyncRequest = new AsyncProtocolRequest(userAsyncResult);
            this.ProcessRead(buffer, offset, count, asyncRequest);
            return userAsyncResult;
        }

        internal IAsyncResult BeginWrite(BufferOffsetSize[] buffers, AsyncCallback asyncCallback, object asyncState)
        {
            LazyAsyncResult userAsyncResult = new LazyAsyncResult(this, asyncState, asyncCallback);
            SplitWriteAsyncProtocolRequest asyncRequest = new SplitWriteAsyncProtocolRequest(userAsyncResult);
            this.ProcessWrite(buffers, asyncRequest);
            return userAsyncResult;
        }

        internal IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            LazyAsyncResult userAsyncResult = new LazyAsyncResult(this, asyncState, asyncCallback);
            AsyncProtocolRequest asyncRequest = new AsyncProtocolRequest(userAsyncResult);
            this.ProcessWrite(buffer, offset, count, asyncRequest);
            return userAsyncResult;
        }

        private void DecrementInternalBufferCount(int decrCount)
        {
            this._InternalOffset += decrCount;
            this._InternalBufferCount -= decrCount;
        }

        private BufferOffsetSize[] EncryptBuffers(BufferOffsetSize[] buffers, byte[] lastHandshakePayload)
        {
            List<BufferOffsetSize> list = null;
            SecurityStatus oK = SecurityStatus.OK;
            foreach (BufferOffsetSize size in buffers)
            {
                int num2;
                int count = Math.Min(size.Size, this._SslState.MaxDataSize);
                byte[] outBuffer = null;
                oK = this._SslState.EncryptData(size.Buffer, size.Offset, count, ref outBuffer, out num2);
                if (oK != SecurityStatus.OK)
                {
                    break;
                }
                if ((count != size.Size) || (list != null))
                {
                    if (list == null)
                    {
                        list = new List<BufferOffsetSize>(buffers.Length * ((size.Size / count) + 1));
                        if (lastHandshakePayload != null)
                        {
                            list.Add(new BufferOffsetSize(lastHandshakePayload, false));
                        }
                        foreach (BufferOffsetSize size2 in buffers)
                        {
                            if (size2 == size)
                            {
                                break;
                            }
                            list.Add(size2);
                        }
                    }
                    list.Add(new BufferOffsetSize(outBuffer, 0, num2, false));
                    while ((size.Size -= count) != 0)
                    {
                        size.Offset += count;
                        count = Math.Min(size.Size, this._SslState.MaxDataSize);
                        oK = this._SslState.EncryptData(size.Buffer, size.Offset, count, ref outBuffer, out num2);
                        if (oK != SecurityStatus.OK)
                        {
                            break;
                        }
                        list.Add(new BufferOffsetSize(outBuffer, 0, num2, false));
                    }
                }
                else
                {
                    size.Buffer = outBuffer;
                    size.Offset = 0;
                    size.Size = num2;
                }
                if (oK != SecurityStatus.OK)
                {
                    break;
                }
            }
            if (oK != SecurityStatus.OK)
            {
                ProtocolToken token = new ProtocolToken(null, oK);
                throw new IOException(SR.GetString("net_io_encrypt"), token.GetException());
            }
            if (list != null)
            {
                buffers = list.ToArray();
                return buffers;
            }
            if (lastHandshakePayload != null)
            {
                BufferOffsetSize[] destinationArray = new BufferOffsetSize[buffers.Length + 1];
                Array.Copy(buffers, 0, destinationArray, 1, buffers.Length);
                destinationArray[0] = new BufferOffsetSize(lastHandshakePayload, false);
                buffers = destinationArray;
            }
            return buffers;
        }

        internal int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            BufferAsyncResult result = asyncResult as BufferAsyncResult;
            if (result == null)
            {
                throw new ArgumentException(SR.GetString("net_io_async_result", new object[] { asyncResult.GetType().FullName }), "asyncResult");
            }
            if (Interlocked.Exchange(ref this._NestedRead, 0) == 0)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndRead" }));
            }
            result.InternalWaitForCompletion();
            if (!(result.Result is Exception))
            {
                return (int) result.Result;
            }
            if (result.Result is IOException)
            {
                throw ((Exception) result.Result);
            }
            throw new IOException(SR.GetString("net_io_write"), (Exception) result.Result);
        }

        internal void EndWrite(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result = asyncResult as LazyAsyncResult;
            if (result == null)
            {
                throw new ArgumentException(SR.GetString("net_io_async_result", new object[] { asyncResult.GetType().FullName }), "asyncResult");
            }
            if (Interlocked.Exchange(ref this._NestedWrite, 0) == 0)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndWrite" }));
            }
            result.InternalWaitForCompletion();
            if (result.Result is Exception)
            {
                if (result.Result is IOException)
                {
                    throw ((Exception) result.Result);
                }
                throw new IOException(SR.GetString("net_io_write"), (Exception) result.Result);
            }
        }

        private void EnsureInternalBufferSize(int curOffset, int addSize)
        {
            if ((this._InternalBuffer == null) || (this._InternalBuffer.Length < (addSize + curOffset)))
            {
                byte[] src = this._InternalBuffer;
                this._InternalBuffer = new byte[addSize + curOffset];
                if ((src != null) && (curOffset != 0))
                {
                    Buffer.BlockCopy(src, 0, this._InternalBuffer, 0, curOffset);
                }
            }
            this._InternalOffset = curOffset;
            this._InternalBufferCount = curOffset + addSize;
        }

        private static void MulitpleWriteCallback(IAsyncResult transportResult)
        {
            if (!transportResult.CompletedSynchronously)
            {
                SplitWriteAsyncProtocolRequest asyncState = (SplitWriteAsyncProtocolRequest) transportResult.AsyncState;
                _SslStream asyncObject = (_SslStream) asyncState.AsyncObject;
                try
                {
                    ((NetworkStream) asyncObject._SslState.InnerStream).EndMultipleWrite(transportResult);
                    asyncObject._SslState.FinishWrite();
                    asyncObject.StartWriting(asyncState.SplitWritesState, asyncState);
                }
                catch (Exception exception)
                {
                    if (asyncState.IsUserCompleted)
                    {
                        throw;
                    }
                    asyncObject._SslState.FinishWrite();
                    asyncState.CompleteWithError(exception);
                }
            }
        }

        private int ProcessFrameBody(int readBytes, byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            if (readBytes == 0)
            {
                throw new IOException(SR.GetString("net_io_eof"));
            }
            readBytes += this._SslState.HeaderSize;
            int num = 0;
            SecurityStatus errorCode = this._SslState.DecryptData(this.InternalBuffer, ref num, ref readBytes);
            if (errorCode != SecurityStatus.OK)
            {
                byte[] dst = null;
                if (readBytes != 0)
                {
                    dst = new byte[readBytes];
                    Buffer.BlockCopy(this.InternalBuffer, num, dst, 0, readBytes);
                }
                this.DecrementInternalBufferCount(this.InternalBufferCount);
                return this.ProcessReadErrorCode(errorCode, buffer, offset, count, asyncRequest, dst);
            }
            if ((readBytes == 0) && (count != 0))
            {
                this.DecrementInternalBufferCount(this.InternalBufferCount);
                return -1;
            }
            this.EnsureInternalBufferSize(0, num + readBytes);
            this.DecrementInternalBufferCount(num);
            if (readBytes > count)
            {
                readBytes = count;
            }
            Buffer.BlockCopy(this.InternalBuffer, this.InternalOffset, buffer, offset, readBytes);
            this.DecrementInternalBufferCount(readBytes);
            this._SslState.FinishRead(null);
            if (asyncRequest != null)
            {
                asyncRequest.CompleteUser(readBytes);
            }
            return readBytes;
        }

        private int ProcessRead(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            int num2;
            this.ValidateParameters(buffer, offset, count);
            if (Interlocked.Exchange(ref this._NestedRead, 1) == 1)
            {
                throw new NotSupportedException(SR.GetString("net_io_invalidnestedcall", new object[] { (asyncRequest != null) ? "BeginRead" : "Read", "read" }));
            }
            bool flag = false;
            try
            {
                if (this.InternalBufferCount != 0)
                {
                    int num = (this.InternalBufferCount > count) ? count : this.InternalBufferCount;
                    if (num != 0)
                    {
                        Buffer.BlockCopy(this.InternalBuffer, this.InternalOffset, buffer, offset, num);
                        this.DecrementInternalBufferCount(num);
                    }
                    if (asyncRequest != null)
                    {
                        asyncRequest.CompleteUser(num);
                    }
                    return num;
                }
                num2 = this.StartReading(buffer, offset, count, asyncRequest);
            }
            catch (Exception exception)
            {
                this._SslState.FinishRead(null);
                flag = true;
                if (exception is IOException)
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_read"), exception);
            }
            finally
            {
                if ((asyncRequest == null) || flag)
                {
                    this._NestedRead = 0;
                }
            }
            return num2;
        }

        private int ProcessReadErrorCode(SecurityStatus errorCode, byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest, byte[] extraBuffer)
        {
            ProtocolToken token = new ProtocolToken(null, errorCode);
            if (token.Renegotiate)
            {
                this._SslState.ReplyOnReAuthentication(extraBuffer);
                return -1;
            }
            if (!token.CloseConnection)
            {
                throw new IOException(SR.GetString("net_io_decrypt"), token.GetException());
            }
            this._SslState.FinishRead(null);
            if (asyncRequest != null)
            {
                asyncRequest.CompleteUser(0);
            }
            return 0;
        }

        private void ProcessWrite(BufferOffsetSize[] buffers, SplitWriteAsyncProtocolRequest asyncRequest)
        {
            foreach (BufferOffsetSize size in buffers)
            {
                this.ValidateParameters(size.Buffer, size.Offset, size.Size);
            }
            if (Interlocked.Exchange(ref this._NestedWrite, 1) == 1)
            {
                throw new NotSupportedException(SR.GetString("net_io_invalidnestedcall", new object[] { (asyncRequest != null) ? "BeginWrite" : "Write", "write" }));
            }
            bool flag = false;
            try
            {
                SplitWritesState splitWritesState = new SplitWritesState(buffers);
                if (asyncRequest != null)
                {
                    asyncRequest.SetNextRequest(splitWritesState, _ResumeAsyncWriteCallback);
                }
                this.StartWriting(splitWritesState, asyncRequest);
            }
            catch (Exception exception)
            {
                this._SslState.FinishWrite();
                flag = true;
                if (exception is IOException)
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_write"), exception);
            }
            finally
            {
                if ((asyncRequest == null) || flag)
                {
                    this._NestedWrite = 0;
                }
            }
        }

        private void ProcessWrite(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            if (this._SslState.LastPayload != null)
            {
                BufferOffsetSize[] buffers = new BufferOffsetSize[] { new BufferOffsetSize(buffer, offset, count, false) };
                if (asyncRequest != null)
                {
                    this.ProcessWrite(buffers, new SplitWriteAsyncProtocolRequest(asyncRequest.UserAsyncResult));
                }
                else
                {
                    this.ProcessWrite(buffers, null);
                }
            }
            else
            {
                this.ValidateParameters(buffer, offset, count);
                if (Interlocked.Exchange(ref this._NestedWrite, 1) == 1)
                {
                    throw new NotSupportedException(SR.GetString("net_io_invalidnestedcall", new object[] { (asyncRequest != null) ? "BeginWrite" : "Write", "write" }));
                }
                bool flag = false;
                try
                {
                    this.StartWriting(buffer, offset, count, asyncRequest);
                }
                catch (Exception exception)
                {
                    this._SslState.FinishWrite();
                    flag = true;
                    if (exception is IOException)
                    {
                        throw;
                    }
                    throw new IOException(SR.GetString("net_io_write"), exception);
                }
                finally
                {
                    if ((asyncRequest == null) || flag)
                    {
                        this._NestedWrite = 0;
                    }
                }
            }
        }

        internal int Read(byte[] buffer, int offset, int count)
        {
            return this.ProcessRead(buffer, offset, count, null);
        }

        private static void ReadFrameCallback(AsyncProtocolRequest asyncRequest)
        {
            try
            {
                _SslStream asyncObject = (_SslStream) asyncRequest.AsyncObject;
                BufferAsyncResult userAsyncResult = (BufferAsyncResult) asyncRequest.UserAsyncResult;
                if (-1 == asyncObject.ProcessFrameBody(asyncRequest.Result, userAsyncResult.Buffer, userAsyncResult.Offset, userAsyncResult.Count, asyncRequest))
                {
                    asyncObject.StartReading(userAsyncResult.Buffer, userAsyncResult.Offset, userAsyncResult.Count, asyncRequest);
                }
            }
            catch (Exception exception)
            {
                if (asyncRequest.IsUserCompleted)
                {
                    throw;
                }
                asyncRequest.CompleteWithError(exception);
            }
        }

        private static void ReadHeaderCallback(AsyncProtocolRequest asyncRequest)
        {
            try
            {
                _SslStream asyncObject = (_SslStream) asyncRequest.AsyncObject;
                BufferAsyncResult userAsyncResult = (BufferAsyncResult) asyncRequest.UserAsyncResult;
                if (-1 == asyncObject.StartFrameBody(asyncRequest.Result, userAsyncResult.Buffer, userAsyncResult.Offset, userAsyncResult.Count, asyncRequest))
                {
                    asyncObject.StartReading(userAsyncResult.Buffer, userAsyncResult.Offset, userAsyncResult.Count, asyncRequest);
                }
            }
            catch (Exception exception)
            {
                if (asyncRequest.IsUserCompleted)
                {
                    throw;
                }
                asyncRequest.CompleteWithError(exception);
            }
        }

        private static void ResumeAsyncReadCallback(AsyncProtocolRequest request)
        {
            try
            {
                ((_SslStream) request.AsyncObject).StartReading(request.Buffer, request.Offset, request.Count, request);
            }
            catch (Exception exception)
            {
                if (request.IsUserCompleted)
                {
                    throw;
                }
                ((_SslStream) request.AsyncObject)._SslState.FinishRead(null);
                request.CompleteWithError(exception);
            }
        }

        private static void ResumeAsyncWriteCallback(AsyncProtocolRequest asyncRequest)
        {
            try
            {
                SplitWriteAsyncProtocolRequest request = asyncRequest as SplitWriteAsyncProtocolRequest;
                if (request != null)
                {
                    ((_SslStream) asyncRequest.AsyncObject).StartWriting(request.SplitWritesState, request);
                }
                else
                {
                    ((_SslStream) asyncRequest.AsyncObject).StartWriting(asyncRequest.Buffer, asyncRequest.Offset, asyncRequest.Count, asyncRequest);
                }
            }
            catch (Exception exception)
            {
                if (asyncRequest.IsUserCompleted)
                {
                    throw;
                }
                ((_SslStream) asyncRequest.AsyncObject)._SslState.FinishWrite();
                asyncRequest.CompleteWithError(exception);
            }
        }

        private int StartFrameBody(int readBytes, byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            if (readBytes == 0)
            {
                this.DecrementInternalBufferCount(this.InternalBufferCount);
                if (asyncRequest != null)
                {
                    asyncRequest.CompleteUser(0);
                }
                return 0;
            }
            readBytes = this._SslState.GetRemainingFrameSize(this.InternalBuffer, readBytes);
            if (readBytes < 0)
            {
                throw new IOException(SR.GetString("net_frame_read_size"));
            }
            this.EnsureInternalBufferSize(this._SslState.HeaderSize, readBytes);
            if (asyncRequest != null)
            {
                asyncRequest.SetNextRequest(this.InternalBuffer, this._SslState.HeaderSize, readBytes, _ReadFrameCallback);
                this._Reader.AsyncReadPacket(asyncRequest);
                if (!asyncRequest.MustCompleteSynchronously)
                {
                    return 0;
                }
                readBytes = asyncRequest.Result;
            }
            else
            {
                readBytes = this._Reader.ReadPacket(this.InternalBuffer, this._SslState.HeaderSize, readBytes);
            }
            return this.ProcessFrameBody(readBytes, buffer, offset, count, asyncRequest);
        }

        private int StartFrameHeader(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            int readBytes = 0;
            this.EnsureInternalBufferSize(0, this._SslState.HeaderSize);
            if (asyncRequest != null)
            {
                asyncRequest.SetNextRequest(this.InternalBuffer, 0, this._SslState.HeaderSize, _ReadHeaderCallback);
                this._Reader.AsyncReadPacket(asyncRequest);
                if (!asyncRequest.MustCompleteSynchronously)
                {
                    return 0;
                }
                readBytes = asyncRequest.Result;
            }
            else
            {
                readBytes = this._Reader.ReadPacket(this.InternalBuffer, 0, this._SslState.HeaderSize);
            }
            return this.StartFrameBody(readBytes, buffer, offset, count, asyncRequest);
        }

        private int StartReading(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            int num = 0;
        Label_0002:
            if (asyncRequest != null)
            {
                asyncRequest.SetNextRequest(buffer, offset, count, _ResumeAsyncReadCallback);
            }
            int userResult = this._SslState.CheckEnqueueRead(buffer, offset, count, asyncRequest);
            switch (userResult)
            {
                case 0:
                    return 0;

                case -1:
                    num = this.StartFrameHeader(buffer, offset, count, asyncRequest);
                    if (num == -1)
                    {
                        goto Label_0002;
                    }
                    return num;
            }
            if (asyncRequest != null)
            {
                asyncRequest.CompleteUser(userResult);
            }
            return userResult;
        }

        private void StartWriting(SplitWritesState splitWrite, SplitWriteAsyncProtocolRequest asyncRequest)
        {
            while (!splitWrite.IsDone)
            {
                if (this._SslState.CheckEnqueueWrite(asyncRequest))
                {
                    return;
                }
                byte[] lastHandshakePayload = null;
                if (this._SslState.LastPayload != null)
                {
                    lastHandshakePayload = this._SslState.LastPayload;
                    this._SslState.LastPayloadConsumed();
                }
                BufferOffsetSize[] nextBuffers = splitWrite.GetNextBuffers();
                nextBuffers = this.EncryptBuffers(nextBuffers, lastHandshakePayload);
                if (asyncRequest != null)
                {
                    IAsyncResult asyncResult = ((NetworkStream) this._SslState.InnerStream).BeginMultipleWrite(nextBuffers, _MulitpleWriteCallback, asyncRequest);
                    if (!asyncResult.CompletedSynchronously)
                    {
                        return;
                    }
                    ((NetworkStream) this._SslState.InnerStream).EndMultipleWrite(asyncResult);
                }
                else
                {
                    ((NetworkStream) this._SslState.InnerStream).MultipleWrite(nextBuffers);
                }
                this._SslState.FinishWrite();
            }
            if (asyncRequest != null)
            {
                asyncRequest.CompleteUser();
            }
        }

        private void StartWriting(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            if (asyncRequest != null)
            {
                asyncRequest.SetNextRequest(buffer, offset, count, _ResumeAsyncWriteCallback);
            }
            if (count >= 0)
            {
                byte[] outBuffer = null;
                do
                {
                    int num2;
                    if (this._SslState.CheckEnqueueWrite(asyncRequest))
                    {
                        return;
                    }
                    int num = Math.Min(count, this._SslState.MaxDataSize);
                    SecurityStatus errorCode = this._SslState.EncryptData(buffer, offset, num, ref outBuffer, out num2);
                    if (errorCode != SecurityStatus.OK)
                    {
                        ProtocolToken token = new ProtocolToken(null, errorCode);
                        throw new IOException(SR.GetString("net_io_encrypt"), token.GetException());
                    }
                    if (asyncRequest != null)
                    {
                        asyncRequest.SetNextRequest(buffer, offset + num, count - num, _ResumeAsyncWriteCallback);
                        IAsyncResult asyncResult = this._SslState.InnerStream.BeginWrite(outBuffer, 0, num2, _WriteCallback, asyncRequest);
                        if (!asyncResult.CompletedSynchronously)
                        {
                            return;
                        }
                        this._SslState.InnerStream.EndWrite(asyncResult);
                    }
                    else
                    {
                        this._SslState.InnerStream.Write(outBuffer, 0, num2);
                    }
                    offset += num;
                    count -= num;
                    this._SslState.FinishWrite();
                }
                while (count != 0);
            }
            if (asyncRequest != null)
            {
                asyncRequest.CompleteUser();
            }
        }

        private void ValidateParameters(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (count > (buffer.Length - offset))
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("net_offset_plus_count"));
            }
        }

        internal void Write(BufferOffsetSize[] buffers)
        {
            this.ProcessWrite(buffers, null);
        }

        internal void Write(byte[] buffer, int offset, int count)
        {
            this.ProcessWrite(buffer, offset, count, null);
        }

        private static void WriteCallback(IAsyncResult transportResult)
        {
            if (!transportResult.CompletedSynchronously)
            {
                AsyncProtocolRequest asyncState = (AsyncProtocolRequest) transportResult.AsyncState;
                _SslStream asyncObject = (_SslStream) asyncState.AsyncObject;
                try
                {
                    asyncObject._SslState.InnerStream.EndWrite(transportResult);
                    asyncObject._SslState.FinishWrite();
                    if (asyncState.Count == 0)
                    {
                        asyncState.Count = -1;
                    }
                    asyncObject.StartWriting(asyncState.Buffer, asyncState.Offset, asyncState.Count, asyncState);
                }
                catch (Exception exception)
                {
                    if (asyncState.IsUserCompleted)
                    {
                        throw;
                    }
                    asyncObject._SslState.FinishWrite();
                    asyncState.CompleteWithError(exception);
                }
            }
        }

        internal bool DataAvailable
        {
            get
            {
                return (this.InternalBufferCount != 0);
            }
        }

        private byte[] InternalBuffer
        {
            get
            {
                return this._InternalBuffer;
            }
        }

        private int InternalBufferCount
        {
            get
            {
                return this._InternalBufferCount;
            }
        }

        private int InternalOffset
        {
            get
            {
                return this._InternalOffset;
            }
        }

        private class SplitWriteAsyncProtocolRequest : AsyncProtocolRequest
        {
            internal System.Net.SplitWritesState SplitWritesState;

            internal SplitWriteAsyncProtocolRequest(LazyAsyncResult userAsyncResult) : base(userAsyncResult)
            {
            }

            internal void SetNextRequest(System.Net.SplitWritesState splitWritesState, AsyncProtocolCallback callback)
            {
                this.SplitWritesState = splitWritesState;
                base.SetNextRequest(null, 0, 0, callback);
            }
        }
    }
}

