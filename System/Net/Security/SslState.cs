namespace System.Net.Security
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    internal class SslState
    {
        private CachedSessionStatus _CachedSession;
        private bool _CanRetryAuthentication;
        private LocalCertSelectionCallback _CertSelectionDelegate;
        private RemoteCertValidationCallback _CertValidationDelegate;
        private bool _CertValidationFailed;
        private const int _ConstMaxQueuedReadBytes = 0x20000;
        private SecureChannel _Context;
        private readonly EncryptionPolicy _EncryptionPolicy;
        private Exception _Exception;
        private bool _ForceBufferingLastHandshakePayload;
        private Framing _Framing;
        private bool _HandshakeCompleted;
        private Stream _InnerStream;
        private byte[] _LastPayload;
        private int _LockReadState;
        private int _LockWriteState;
        private int _NestedAuth;
        private static AsyncProtocolCallback _PartialFrameCallback = new AsyncProtocolCallback(SslState.PartialFrameCallback);
        private bool _PendingReHandshake;
        private int _QueuedReadCount;
        private byte[] _QueuedReadData;
        private object _QueuedReadStateRequest;
        private object _QueuedWriteStateRequest;
        private FixedSizeReader _Reader;
        private static AsyncProtocolCallback _ReadFrameCallback = new AsyncProtocolCallback(SslState.ReadFrameCallback);
        private _SslStream _SecureStream;
        private SecurityStatus _SecurityStatus;
        private static AsyncCallback _WriteCallback = new AsyncCallback(SslState.WriteCallback);
        private const int LockHandshake = 2;
        private const int LockNone = 0;
        private const int LockPendingRead = 6;
        private const int LockPendingWrite = 3;
        private const int LockRead = 4;
        private const int LockWrite = 1;
        private static int UniqueNameInteger = 0x7b;

        internal SslState(Stream innerStream, bool isHTTP, EncryptionPolicy encryptionPolicy) : this(innerStream, null, null, encryptionPolicy)
        {
            this._ForceBufferingLastHandshakePayload = isHTTP;
        }

        internal SslState(Stream innerStream, RemoteCertValidationCallback certValidationCallback, LocalCertSelectionCallback certSelectionCallback, EncryptionPolicy encryptionPolicy)
        {
            this._InnerStream = innerStream;
            this._Reader = new FixedSizeReader(innerStream);
            this._CertValidationDelegate = certValidationCallback;
            this._CertSelectionDelegate = certSelectionCallback;
            this._EncryptionPolicy = encryptionPolicy;
        }

        private void AsyncResumeHandshake(object state)
        {
            AsyncProtocolRequest asyncRequest = state as AsyncProtocolRequest;
            this.ForceAuthentication(this.Context.IsServer, asyncRequest.Buffer, asyncRequest);
        }

        private void AsyncResumeHandshakeRead(object state)
        {
            AsyncProtocolRequest asyncRequest = (AsyncProtocolRequest) state;
            try
            {
                if (this._PendingReHandshake)
                {
                    this.StartReceiveBlob(asyncRequest.Buffer, asyncRequest);
                }
                else
                {
                    this.ProcessReceivedBlob(asyncRequest.Buffer, (asyncRequest.Buffer == null) ? 0 : asyncRequest.Buffer.Length, asyncRequest);
                }
            }
            catch (Exception exception)
            {
                if (asyncRequest.IsUserCompleted)
                {
                    throw;
                }
                this.FinishHandshake(exception, asyncRequest);
            }
        }

        private void CheckCompletionBeforeNextReceive(ProtocolToken message, AsyncProtocolRequest asyncRequest)
        {
            if (message.Failed)
            {
                this.StartSendAuthResetSignal(null, asyncRequest, new AuthenticationException(SR.GetString("net_auth_SSPI"), message.GetException()));
            }
            else if (message.Done && !this._PendingReHandshake)
            {
                if (this.CheckWin9xCachedSession())
                {
                    this._PendingReHandshake = true;
                    this.Win9xSessionRestarted();
                    this.ForceAuthentication(false, null, asyncRequest);
                }
                else if (!this.CompleteHandshake())
                {
                    this.StartSendAuthResetSignal(null, asyncRequest, new AuthenticationException(SR.GetString("net_ssl_io_cert_validation"), null));
                }
                else
                {
                    this.FinishHandshake(null, asyncRequest);
                }
            }
            else
            {
                this.StartReceiveBlob(message.Payload, asyncRequest);
            }
        }

        private bool CheckEnqueueHandshake(byte[] buffer, AsyncProtocolRequest asyncRequest)
        {
            LazyAsyncResult result = null;
            lock (this)
            {
                if (this._LockWriteState == 3)
                {
                    return false;
                }
                if (Interlocked.Exchange(ref this._LockWriteState, 2) != 1)
                {
                    return false;
                }
                if (asyncRequest != null)
                {
                    asyncRequest.Buffer = buffer;
                    this._QueuedWriteStateRequest = asyncRequest;
                    return true;
                }
                result = new LazyAsyncResult(null, null, null);
                this._QueuedWriteStateRequest = result;
            }
            result.InternalWaitForCompletion();
            return false;
        }

        private bool CheckEnqueueHandshakeRead(ref byte[] buffer, AsyncProtocolRequest request)
        {
            LazyAsyncResult result = null;
            lock (this)
            {
                if (this._LockReadState == 6)
                {
                    return false;
                }
                if (Interlocked.Exchange(ref this._LockReadState, 2) != 4)
                {
                    return false;
                }
                if (request != null)
                {
                    this._QueuedReadStateRequest = request;
                    return true;
                }
                result = new LazyAsyncResult(null, null, null);
                this._QueuedReadStateRequest = result;
            }
            result.InternalWaitForCompletion();
            buffer = (byte[]) result.Result;
            return false;
        }

        internal int CheckEnqueueRead(byte[] buffer, int offset, int count, AsyncProtocolRequest request)
        {
            if (Interlocked.CompareExchange(ref this._LockReadState, 4, 0) != 2)
            {
                return this.CheckOldKeyDecryptedData(buffer, offset, count);
            }
            LazyAsyncResult result = null;
            lock (this)
            {
                int num2 = this.CheckOldKeyDecryptedData(buffer, offset, count);
                if (num2 != -1)
                {
                    return num2;
                }
                if (this._LockReadState != 2)
                {
                    this._LockReadState = 4;
                    return -1;
                }
                this._LockReadState = 6;
                if (request != null)
                {
                    this._QueuedReadStateRequest = request;
                    return 0;
                }
                result = new LazyAsyncResult(null, null, null);
                this._QueuedReadStateRequest = result;
            }
            result.InternalWaitForCompletion();
            lock (this)
            {
                return this.CheckOldKeyDecryptedData(buffer, offset, count);
            }
        }

        internal bool CheckEnqueueWrite(AsyncProtocolRequest asyncRequest)
        {
            this._QueuedWriteStateRequest = null;
            if (Interlocked.CompareExchange(ref this._LockWriteState, 1, 0) == 2)
            {
                LazyAsyncResult result = null;
                lock (this)
                {
                    if (this._LockWriteState == 1)
                    {
                        this.CheckThrow(true);
                        return false;
                    }
                    this._LockWriteState = 3;
                    if (asyncRequest != null)
                    {
                        this._QueuedWriteStateRequest = asyncRequest;
                        return true;
                    }
                    result = new LazyAsyncResult(null, null, null);
                    this._QueuedWriteStateRequest = result;
                }
                result.InternalWaitForCompletion();
                this.CheckThrow(true);
            }
            return false;
        }

        internal int CheckOldKeyDecryptedData(byte[] buffer, int offset, int count)
        {
            this.CheckThrow(true);
            if (this._QueuedReadData == null)
            {
                return -1;
            }
            int num = Math.Min(this._QueuedReadCount, count);
            Buffer.BlockCopy(this._QueuedReadData, 0, buffer, offset, num);
            this._QueuedReadCount -= num;
            if (this._QueuedReadCount == 0)
            {
                this._QueuedReadData = null;
                return num;
            }
            Buffer.BlockCopy(this._QueuedReadData, num, this._QueuedReadData, 0, this._QueuedReadCount);
            return num;
        }

        internal void CheckThrow(bool authSucessCheck)
        {
            if (this._Exception != null)
            {
                throw this._Exception;
            }
            if (authSucessCheck && !this.IsAuthenticated)
            {
                throw new InvalidOperationException(SR.GetString("net_auth_noauth"));
            }
        }

        private bool CheckWin9xCachedSession()
        {
            if ((ComNetOS.IsWin9x && (this._CachedSession == CachedSessionStatus.IsCached)) && (this.Context.IsServer && this.Context.RemoteCertRequired))
            {
                X509Certificate2 remoteCertificate = null;
                try
                {
                    X509Certificate2Collection certificates;
                    remoteCertificate = this.Context.GetRemoteCertificate(out certificates);
                    if (remoteCertificate == null)
                    {
                        return true;
                    }
                }
                finally
                {
                    if (remoteCertificate != null)
                    {
                        remoteCertificate.Reset();
                    }
                }
            }
            return false;
        }

        internal void Close()
        {
            this._Exception = new ObjectDisposedException("SslStream");
            if (this.Context != null)
            {
                this.Context.Close();
            }
        }

        private bool CompleteHandshake()
        {
            this.Context.ProcessHandshakeSuccess();
            if (!this.Context.VerifyRemoteCertificate(this._CertValidationDelegate))
            {
                this._HandshakeCompleted = false;
                this._CertValidationFailed = true;
                return false;
            }
            this._CertValidationFailed = false;
            this._HandshakeCompleted = true;
            return true;
        }

        private void CompleteRequestWaitCallback(object state)
        {
            AsyncProtocolRequest request = (AsyncProtocolRequest) state;
            if (request.MustCompleteSynchronously)
            {
                throw new InternalException();
            }
            request.CompleteRequest(0);
        }

        internal SecurityStatus DecryptData(byte[] buffer, ref int offset, ref int count)
        {
            this.CheckThrow(true);
            return this.PrivateDecryptData(buffer, ref offset, ref count);
        }

        private Framing DetectFraming(byte[] bytes, int length)
        {
            int num = -1;
            if ((bytes[0] == 0x16) || (bytes[0] == 0x17))
            {
                if (length >= 3)
                {
                    num = (bytes[1] << 8) | bytes[2];
                    if ((num >= 0x300) && (num < 0x500))
                    {
                        return Framing.SinceSSL3;
                    }
                }
                return Framing.Invalid;
            }
            if (length < 3)
            {
                return Framing.Invalid;
            }
            if (bytes[2] > 8)
            {
                return Framing.Invalid;
            }
            if (bytes[2] == 1)
            {
                if (length >= 5)
                {
                    num = (bytes[3] << 8) | bytes[4];
                }
            }
            else if ((bytes[2] == 4) && (length >= 7))
            {
                num = (bytes[5] << 8) | bytes[6];
            }
            if (num != -1)
            {
                if (this._Framing == Framing.None)
                {
                    if ((num != 2) && ((num < 0x200) || (num >= 0x500)))
                    {
                        return Framing.Invalid;
                    }
                }
                else if (num != 2)
                {
                    return Framing.Invalid;
                }
            }
            if (this.Context.IsServer && (this._Framing != Framing.Unified))
            {
                return Framing.Unified;
            }
            return Framing.BeforeSSL3;
        }

        internal SecurityStatus EncryptData(byte[] buffer, int offset, int count, ref byte[] outBuffer, out int outSize)
        {
            this.CheckThrow(true);
            return this.Context.Encrypt(buffer, offset, count, ref outBuffer, out outSize);
        }

        internal void EndProcessAuthentication(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult lazyResult = result as LazyAsyncResult;
            if (lazyResult == null)
            {
                throw new ArgumentException(SR.GetString("net_io_async_result", new object[] { result.GetType().FullName }), "asyncResult");
            }
            if (Interlocked.Exchange(ref this._NestedAuth, 0) == 0)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndAuthenticate" }));
            }
            this.InternalEndProcessAuthentication(lazyResult);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_selected_cipher_suite", new object[] { "EndProcessAuthentication", this.SslProtocol, this.CipherAlgorithm, this.CipherStrength, this.HashAlgorithm, this.HashStrength, this.KeyExchangeAlgorithm, this.KeyExchangeStrength }));
            }
        }

        private Exception EnqueueOldKeyDecryptedData(byte[] buffer, int offset, int count)
        {
            lock (this)
            {
                if ((this._QueuedReadCount + count) > 0x20000)
                {
                    object[] args = new object[] { 0x20000.ToString(NumberFormatInfo.CurrentInfo) };
                    return new IOException(SR.GetString("net_auth_ignored_reauth", args));
                }
                if (count != 0)
                {
                    this._QueuedReadData = EnsureBufferSize(this._QueuedReadData, this._QueuedReadCount, this._QueuedReadCount + count);
                    Buffer.BlockCopy(buffer, offset, this._QueuedReadData, this._QueuedReadCount, count);
                    this._QueuedReadCount += count;
                    this.FinishHandshakeRead(2);
                }
            }
            return null;
        }

        private static byte[] EnsureBufferSize(byte[] buffer, int copyCount, int size)
        {
            if ((buffer == null) || (buffer.Length < size))
            {
                byte[] src = buffer;
                buffer = new byte[size];
                if ((src != null) && (copyCount != 0))
                {
                    Buffer.BlockCopy(src, 0, buffer, 0, copyCount);
                }
            }
            return buffer;
        }

        private void FinishHandshake(Exception e, AsyncProtocolRequest asyncRequest)
        {
            try
            {
                lock (this)
                {
                    if (e != null)
                    {
                        this.SetException(e);
                    }
                    this.FinishHandshakeRead(0);
                    if (Interlocked.CompareExchange(ref this._LockWriteState, 0, 2) == 3)
                    {
                        this._LockWriteState = 1;
                        object state = this._QueuedWriteStateRequest;
                        if (state != null)
                        {
                            this._QueuedWriteStateRequest = null;
                            if (state is LazyAsyncResult)
                            {
                                ((LazyAsyncResult) state).InvokeCallback();
                            }
                            else
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(this.CompleteRequestWaitCallback), state);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (asyncRequest != null)
                {
                    if (e != null)
                    {
                        asyncRequest.CompleteWithError(e);
                    }
                    else
                    {
                        asyncRequest.CompleteUser();
                    }
                }
            }
        }

        private void FinishHandshakeRead(int newState)
        {
            lock (this)
            {
                if (Interlocked.Exchange(ref this._LockReadState, newState) == 6)
                {
                    this._LockReadState = 4;
                    object state = this._QueuedReadStateRequest;
                    if (state != null)
                    {
                        this._QueuedReadStateRequest = null;
                        if (state is LazyAsyncResult)
                        {
                            ((LazyAsyncResult) state).InvokeCallback();
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(this.CompleteRequestWaitCallback), state);
                        }
                    }
                }
            }
        }

        internal void FinishRead(byte[] renegotiateBuffer)
        {
            if (Interlocked.CompareExchange(ref this._LockReadState, 0, 4) == 2)
            {
                lock (this)
                {
                    LazyAsyncResult result = this._QueuedReadStateRequest as LazyAsyncResult;
                    if (result != null)
                    {
                        this._QueuedReadStateRequest = null;
                        result.InvokeCallback(renegotiateBuffer);
                    }
                    else
                    {
                        AsyncProtocolRequest state = (AsyncProtocolRequest) this._QueuedReadStateRequest;
                        state.Buffer = renegotiateBuffer;
                        this._QueuedReadStateRequest = null;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.AsyncResumeHandshakeRead), state);
                    }
                }
            }
        }

        internal void FinishWrite()
        {
            if (Interlocked.CompareExchange(ref this._LockWriteState, 0, 1) == 2)
            {
                lock (this)
                {
                    object state = this._QueuedWriteStateRequest;
                    if (state != null)
                    {
                        this._QueuedWriteStateRequest = null;
                        if (state is LazyAsyncResult)
                        {
                            ((LazyAsyncResult) state).InvokeCallback();
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(this.AsyncResumeHandshake), state);
                        }
                    }
                }
            }
        }

        internal void Flush()
        {
            this.InnerStream.Flush();
        }

        private void ForceAuthentication(bool receiveFirst, byte[] buffer, AsyncProtocolRequest asyncRequest)
        {
            if (!this.CheckEnqueueHandshake(buffer, asyncRequest))
            {
                this._Framing = Framing.None;
                try
                {
                    if (receiveFirst)
                    {
                        this.StartReceiveBlob(buffer, asyncRequest);
                    }
                    else
                    {
                        this.StartSendBlob(buffer, (buffer == null) ? 0 : buffer.Length, asyncRequest);
                    }
                }
                catch (Exception exception)
                {
                    this._Framing = Framing.None;
                    this._HandshakeCompleted = false;
                    if (this.SetException(exception) == exception)
                    {
                        throw;
                    }
                    throw this._Exception;
                }
                finally
                {
                    if (this._Exception != null)
                    {
                        this.FinishHandshake(null, null);
                    }
                }
            }
        }

        internal ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            if (this.Context != null)
            {
                return this.Context.GetChannelBinding(kind);
            }
            return null;
        }

        internal int GetRemainingFrameSize(byte[] buffer, int dataSize)
        {
            int num = -1;
            switch (this._Framing)
            {
                case Framing.BeforeSSL3:
                case Framing.Unified:
                    if (dataSize < 2)
                    {
                        throw new IOException(SR.GetString("net_ssl_io_frame"));
                    }
                    if ((buffer[0] & 0x80) != 0)
                    {
                        num = (((buffer[0] & 0x7f) << 8) | buffer[1]) + 2;
                        return (num - dataSize);
                    }
                    num = (((buffer[0] & 0x3f) << 8) | buffer[1]) + 3;
                    return (num - dataSize);

                case Framing.SinceSSL3:
                    if (dataSize < 5)
                    {
                        throw new IOException(SR.GetString("net_ssl_io_frame"));
                    }
                    num = ((buffer[3] << 8) | buffer[4]) + 5;
                    return (num - dataSize);
            }
            return num;
        }

        internal void InternalEndProcessAuthentication(LazyAsyncResult lazyResult)
        {
            lazyResult.InternalWaitForCompletion();
            Exception result = lazyResult.Result as Exception;
            if (result != null)
            {
                this._Framing = Framing.None;
                this._HandshakeCompleted = false;
                throw this.SetException(result);
            }
        }

        internal void LastPayloadConsumed()
        {
            this._LastPayload = null;
        }

        private static void PartialFrameCallback(AsyncProtocolRequest asyncRequest)
        {
            SslState asyncObject = (SslState) asyncRequest.AsyncObject;
            try
            {
                asyncObject.StartReadFrame(asyncRequest.Buffer, asyncRequest.Result, asyncRequest);
            }
            catch (Exception exception)
            {
                if (asyncRequest.IsUserCompleted)
                {
                    throw;
                }
                asyncObject.FinishHandshake(exception, asyncRequest);
            }
        }

        private SecurityStatus PrivateDecryptData(byte[] buffer, ref int offset, ref int count)
        {
            return this.Context.Decrypt(buffer, ref offset, ref count);
        }

        internal void ProcessAuthentication(LazyAsyncResult lazyResult)
        {
            if (Interlocked.Exchange(ref this._NestedAuth, 1) == 1)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidnestedcall", new object[] { (lazyResult == null) ? "BeginAuthenticate" : "Authenticate", "authenticate" }));
            }
            try
            {
                this.CheckThrow(false);
                AsyncProtocolRequest asyncRequest = null;
                if (lazyResult != null)
                {
                    asyncRequest = new AsyncProtocolRequest(lazyResult) {
                        Buffer = null
                    };
                }
                this._CachedSession = CachedSessionStatus.Unknown;
                this.ForceAuthentication(this.Context.IsServer, null, asyncRequest);
                if ((lazyResult == null) && Logging.On)
                {
                    Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_selected_cipher_suite", new object[] { "ProcessAuthentication", this.SslProtocol, this.CipherAlgorithm, this.CipherStrength, this.HashAlgorithm, this.HashStrength, this.KeyExchangeAlgorithm, this.KeyExchangeStrength }));
                }
            }
            finally
            {
                if ((lazyResult == null) || (this._Exception != null))
                {
                    this._NestedAuth = 0;
                }
            }
        }

        private void ProcessReceivedBlob(byte[] buffer, int count, AsyncProtocolRequest asyncRequest)
        {
            if (count == 0)
            {
                throw new AuthenticationException(SR.GetString("net_auth_eof"), null);
            }
            if (this._PendingReHandshake)
            {
                int offset = 0;
                SecurityStatus errorCode = this.PrivateDecryptData(buffer, ref offset, ref count);
                if (errorCode == SecurityStatus.OK)
                {
                    Exception exception = this.EnqueueOldKeyDecryptedData(buffer, offset, count);
                    if (exception != null)
                    {
                        this.StartSendAuthResetSignal(null, asyncRequest, exception);
                        return;
                    }
                    this._Framing = Framing.None;
                    this.StartReceiveBlob(buffer, asyncRequest);
                    return;
                }
                if (errorCode != SecurityStatus.Renegotiate)
                {
                    ProtocolToken token = new ProtocolToken(null, errorCode);
                    this.StartSendAuthResetSignal(null, asyncRequest, new AuthenticationException(SR.GetString("net_auth_SSPI"), token.GetException()));
                    return;
                }
                this._PendingReHandshake = false;
                if (offset != 0)
                {
                    Buffer.BlockCopy(buffer, offset, buffer, 0, count);
                }
            }
            this.StartSendBlob(buffer, count, asyncRequest);
        }

        private static void ReadFrameCallback(AsyncProtocolRequest asyncRequest)
        {
            SslState asyncObject = (SslState) asyncRequest.AsyncObject;
            try
            {
                if (asyncRequest.Result == 0)
                {
                    asyncRequest.Offset = 0;
                }
                asyncObject.ProcessReceivedBlob(asyncRequest.Buffer, asyncRequest.Offset + asyncRequest.Result, asyncRequest);
            }
            catch (Exception exception)
            {
                if (asyncRequest.IsUserCompleted)
                {
                    throw;
                }
                asyncObject.FinishHandshake(exception, asyncRequest);
            }
        }

        private void RehandshakeCompleteCallback(IAsyncResult result)
        {
            LazyAsyncResult result2 = (LazyAsyncResult) result;
            Exception e = result2.InternalWaitForCompletion() as Exception;
            if (e != null)
            {
                this.FinishHandshake(e, null);
            }
        }

        internal void ReplyOnReAuthentication(byte[] buffer)
        {
            lock (this)
            {
                this._LockReadState = 2;
                if (this._PendingReHandshake)
                {
                    this.FinishRead(buffer);
                    return;
                }
            }
            AsyncProtocolRequest asyncRequest = new AsyncProtocolRequest(new LazyAsyncResult(this, null, new AsyncCallback(this.RehandshakeCompleteCallback))) {
                Buffer = buffer
            };
            this.ForceAuthentication(false, buffer, asyncRequest);
        }

        internal void SetCertValidationDelegate(RemoteCertValidationCallback certValidationCallback)
        {
            this._CertValidationDelegate = certValidationCallback;
        }

        private Exception SetException(Exception e)
        {
            if (this._Exception == null)
            {
                this._Exception = e;
            }
            if ((this._Exception != null) && (this.Context != null))
            {
                this.Context.Close();
            }
            return this._Exception;
        }

        private void StartReadFrame(byte[] buffer, int readBytes, AsyncProtocolRequest asyncRequest)
        {
            if (readBytes == 0)
            {
                throw new IOException(SR.GetString("net_auth_eof"));
            }
            if (this._Framing == Framing.None)
            {
                this._Framing = this.DetectFraming(buffer, readBytes);
            }
            int remainingFrameSize = this.GetRemainingFrameSize(buffer, readBytes);
            if (remainingFrameSize < 0)
            {
                throw new IOException(SR.GetString("net_ssl_io_frame"));
            }
            if (remainingFrameSize == 0)
            {
                throw new AuthenticationException(SR.GetString("net_auth_eof"), null);
            }
            buffer = EnsureBufferSize(buffer, readBytes, readBytes + remainingFrameSize);
            if (asyncRequest == null)
            {
                remainingFrameSize = this._Reader.ReadPacket(buffer, readBytes, remainingFrameSize);
            }
            else
            {
                asyncRequest.SetNextRequest(buffer, readBytes, remainingFrameSize, _ReadFrameCallback);
                this._Reader.AsyncReadPacket(asyncRequest);
                if (!asyncRequest.MustCompleteSynchronously)
                {
                    return;
                }
                remainingFrameSize = asyncRequest.Result;
                if (remainingFrameSize == 0)
                {
                    readBytes = 0;
                }
            }
            this.ProcessReceivedBlob(buffer, readBytes + remainingFrameSize, asyncRequest);
        }

        private void StartReceiveBlob(byte[] buffer, AsyncProtocolRequest asyncRequest)
        {
            if (this._PendingReHandshake)
            {
                if (this.CheckEnqueueHandshakeRead(ref buffer, asyncRequest))
                {
                    return;
                }
                if (!this._PendingReHandshake)
                {
                    this.ProcessReceivedBlob(buffer, buffer.Length, asyncRequest);
                    return;
                }
            }
            buffer = EnsureBufferSize(buffer, 0, this.Context.HeaderSize);
            int readBytes = 0;
            if (asyncRequest == null)
            {
                readBytes = this._Reader.ReadPacket(buffer, 0, this.Context.HeaderSize);
            }
            else
            {
                asyncRequest.SetNextRequest(buffer, 0, this.Context.HeaderSize, _PartialFrameCallback);
                this._Reader.AsyncReadPacket(asyncRequest);
                if (!asyncRequest.MustCompleteSynchronously)
                {
                    return;
                }
                readBytes = asyncRequest.Result;
            }
            this.StartReadFrame(buffer, readBytes, asyncRequest);
        }

        private void StartSendAuthResetSignal(ProtocolToken message, AsyncProtocolRequest asyncRequest, Exception exception)
        {
            if ((message == null) || (message.Size == 0))
            {
                throw exception;
            }
            if (asyncRequest == null)
            {
                this.InnerStream.Write(message.Payload, 0, message.Size);
            }
            else
            {
                asyncRequest.AsyncState = exception;
                IAsyncResult asyncResult = this.InnerStream.BeginWrite(message.Payload, 0, message.Size, _WriteCallback, asyncRequest);
                if (!asyncResult.CompletedSynchronously)
                {
                    return;
                }
                this.InnerStream.EndWrite(asyncResult);
            }
            throw exception;
        }

        private void StartSendBlob(byte[] incoming, int count, AsyncProtocolRequest asyncRequest)
        {
            ProtocolToken message = this.Context.NextMessage(incoming, 0, count);
            this._SecurityStatus = message.Status;
            if (message.Size != 0)
            {
                if (this.Context.IsServer && (this._CachedSession == CachedSessionStatus.Unknown))
                {
                    this._CachedSession = (message.Size < 200) ? CachedSessionStatus.IsCached : CachedSessionStatus.IsNotCached;
                }
                if (this._Framing == Framing.Unified)
                {
                    this._Framing = this.DetectFraming(message.Payload, message.Payload.Length);
                }
                if (((message.Done && this._ForceBufferingLastHandshakePayload) && ((this.InnerStream.GetType() == typeof(NetworkStream)) && !this._PendingReHandshake)) && !this.CheckWin9xCachedSession())
                {
                    this._LastPayload = message.Payload;
                }
                else if (asyncRequest == null)
                {
                    this.InnerStream.Write(message.Payload, 0, message.Size);
                }
                else
                {
                    asyncRequest.AsyncState = message;
                    IAsyncResult asyncResult = this.InnerStream.BeginWrite(message.Payload, 0, message.Size, _WriteCallback, asyncRequest);
                    if (!asyncResult.CompletedSynchronously)
                    {
                        return;
                    }
                    this.InnerStream.EndWrite(asyncResult);
                }
            }
            this.CheckCompletionBeforeNextReceive(message, asyncRequest);
        }

        internal void ValidateCreateContext(bool isServer, string targetHost, SslProtocols enabledSslProtocols, X509Certificate serverCertificate, X509CertificateCollection clientCertificates, bool remoteCertRequired, bool checkCertRevocationStatus)
        {
            this.ValidateCreateContext(isServer, targetHost, enabledSslProtocols, serverCertificate, clientCertificates, remoteCertRequired, checkCertRevocationStatus, !isServer);
        }

        internal void ValidateCreateContext(bool isServer, string targetHost, SslProtocols enabledSslProtocols, X509Certificate serverCertificate, X509CertificateCollection clientCertificates, bool remoteCertRequired, bool checkCertRevocationStatus, bool checkCertName)
        {
            if ((this._Exception != null) && !this._CanRetryAuthentication)
            {
                throw this._Exception;
            }
            if ((this.Context != null) && this.Context.IsValidContext)
            {
                throw new InvalidOperationException(SR.GetString("net_auth_reauth"));
            }
            if ((this.Context != null) && (this.IsServer != isServer))
            {
                throw new InvalidOperationException(SR.GetString("net_auth_client_server"));
            }
            if (targetHost == null)
            {
                throw new ArgumentNullException("targetHost");
            }
            if (isServer)
            {
                enabledSslProtocols &= 0x40000055;
                if (serverCertificate == null)
                {
                    throw new ArgumentNullException("serverCertificate");
                }
            }
            else
            {
                enabledSslProtocols &= -2147483478;
            }
            if (enabledSslProtocols == SslProtocols.None)
            {
                throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SslProtocolType" }), "sslProtocolType");
            }
            if (clientCertificates == null)
            {
                clientCertificates = new X509CertificateCollection();
            }
            if (targetHost.Length == 0)
            {
                targetHost = "?" + Interlocked.Increment(ref UniqueNameInteger).ToString(NumberFormatInfo.InvariantInfo);
            }
            this._Exception = null;
            try
            {
                this._Context = new SecureChannel(targetHost, isServer, (SchProtocols) enabledSslProtocols, serverCertificate, clientCertificates, remoteCertRequired, checkCertName, checkCertRevocationStatus, this._EncryptionPolicy, this._CertSelectionDelegate);
            }
            catch (Win32Exception exception)
            {
                throw new AuthenticationException(SR.GetString("net_auth_SSPI"), exception);
            }
        }

        private void Win9xSessionRestarted()
        {
            this._CachedSession = CachedSessionStatus.Renegotiated;
        }

        private static void WriteCallback(IAsyncResult transportResult)
        {
            if (!transportResult.CompletedSynchronously)
            {
                AsyncProtocolRequest asyncState = (AsyncProtocolRequest) transportResult.AsyncState;
                SslState asyncObject = (SslState) asyncState.AsyncObject;
                try
                {
                    asyncObject.InnerStream.EndWrite(transportResult);
                    object obj2 = asyncState.AsyncState;
                    Exception exception = obj2 as Exception;
                    if (exception != null)
                    {
                        throw exception;
                    }
                    asyncObject.CheckCompletionBeforeNextReceive((ProtocolToken) obj2, asyncState);
                }
                catch (Exception exception2)
                {
                    if (asyncState.IsUserCompleted)
                    {
                        throw;
                    }
                    asyncObject.FinishHandshake(exception2, asyncState);
                }
            }
        }

        internal bool CheckCertRevocationStatus
        {
            get
            {
                return ((this.Context != null) && this.Context.CheckCertRevocationStatus);
            }
        }

        internal CipherAlgorithmType CipherAlgorithm
        {
            get
            {
                this.CheckThrow(true);
                SslConnectionInfo connectionInfo = this.Context.ConnectionInfo;
                if (connectionInfo == null)
                {
                    return CipherAlgorithmType.None;
                }
                return (CipherAlgorithmType) connectionInfo.DataCipherAlg;
            }
        }

        internal int CipherStrength
        {
            get
            {
                this.CheckThrow(true);
                SslConnectionInfo connectionInfo = this.Context.ConnectionInfo;
                if (connectionInfo == null)
                {
                    return 0;
                }
                return connectionInfo.DataKeySize;
            }
        }

        private SecureChannel Context
        {
            get
            {
                return this._Context;
            }
        }

        internal bool DataAvailable
        {
            get
            {
                if (!this.IsAuthenticated)
                {
                    return false;
                }
                if (!this.SecureStream.DataAvailable)
                {
                    return (this._QueuedReadCount != 0);
                }
                return true;
            }
        }

        private bool HandshakeCompleted
        {
            get
            {
                return this._HandshakeCompleted;
            }
        }

        internal HashAlgorithmType HashAlgorithm
        {
            get
            {
                this.CheckThrow(true);
                SslConnectionInfo connectionInfo = this.Context.ConnectionInfo;
                if (connectionInfo == null)
                {
                    return HashAlgorithmType.None;
                }
                return (HashAlgorithmType) connectionInfo.DataHashAlg;
            }
        }

        internal int HashStrength
        {
            get
            {
                this.CheckThrow(true);
                SslConnectionInfo connectionInfo = this.Context.ConnectionInfo;
                if (connectionInfo == null)
                {
                    return 0;
                }
                return connectionInfo.DataHashKeySize;
            }
        }

        internal int HeaderSize
        {
            get
            {
                return this.Context.HeaderSize;
            }
        }

        internal Stream InnerStream
        {
            get
            {
                return this._InnerStream;
            }
        }

        internal X509Certificate InternalLocalCertificate
        {
            get
            {
                if (!this.Context.IsServer)
                {
                    return this.Context.LocalClientCertificate;
                }
                return this.Context.LocalServerCertificate;
            }
        }

        internal bool IsAuthenticated
        {
            get
            {
                return ((((this._Context != null) && this._Context.IsValidContext) && (this._Exception == null)) && this.HandshakeCompleted);
            }
        }

        internal bool IsCertValidationFailed
        {
            get
            {
                return this._CertValidationFailed;
            }
        }

        internal bool IsMutuallyAuthenticated
        {
            get
            {
                return ((this.IsAuthenticated && ((this.Context.IsServer ? this.Context.LocalServerCertificate : this.Context.LocalClientCertificate) != null)) && this.Context.IsRemoteCertificateAvailable);
            }
        }

        internal bool IsServer
        {
            get
            {
                return ((this.Context != null) && this.Context.IsServer);
            }
        }

        internal ExchangeAlgorithmType KeyExchangeAlgorithm
        {
            get
            {
                this.CheckThrow(true);
                SslConnectionInfo connectionInfo = this.Context.ConnectionInfo;
                if (connectionInfo == null)
                {
                    return ExchangeAlgorithmType.None;
                }
                return (ExchangeAlgorithmType) connectionInfo.KeyExchangeAlg;
            }
        }

        internal int KeyExchangeStrength
        {
            get
            {
                this.CheckThrow(true);
                SslConnectionInfo connectionInfo = this.Context.ConnectionInfo;
                if (connectionInfo == null)
                {
                    return 0;
                }
                return connectionInfo.KeyExchKeySize;
            }
        }

        internal byte[] LastPayload
        {
            get
            {
                return this._LastPayload;
            }
        }

        internal SecurityStatus LastSecurityStatus
        {
            get
            {
                return this._SecurityStatus;
            }
        }

        internal X509Certificate LocalCertificate
        {
            get
            {
                this.CheckThrow(true);
                return this.InternalLocalCertificate;
            }
        }

        internal int MaxDataSize
        {
            get
            {
                return this.Context.MaxDataSize;
            }
        }

        internal bool RemoteCertRequired
        {
            get
            {
                if (this.Context != null)
                {
                    return this.Context.RemoteCertRequired;
                }
                return true;
            }
        }

        internal _SslStream SecureStream
        {
            get
            {
                this.CheckThrow(true);
                if (this._SecureStream == null)
                {
                    Interlocked.CompareExchange<_SslStream>(ref this._SecureStream, new _SslStream(this), null);
                }
                return this._SecureStream;
            }
        }

        internal SslProtocols SslProtocol
        {
            get
            {
                this.CheckThrow(true);
                SslConnectionInfo connectionInfo = this.Context.ConnectionInfo;
                if (connectionInfo == null)
                {
                    return SslProtocols.None;
                }
                SslProtocols protocol = (SslProtocols) connectionInfo.Protocol;
                if ((protocol & SslProtocols.Ssl2) != SslProtocols.None)
                {
                    protocol |= SslProtocols.Ssl2;
                }
                if ((protocol & SslProtocols.Ssl3) != SslProtocols.None)
                {
                    protocol |= SslProtocols.Ssl3;
                }
                if ((protocol & SslProtocols.Tls) != SslProtocols.None)
                {
                    protocol |= SslProtocols.Tls;
                }
                return protocol;
            }
        }

        private enum CachedSessionStatus : byte
        {
            IsCached = 2,
            IsNotCached = 1,
            Renegotiated = 3,
            Unknown = 0
        }

        private enum FrameType : byte
        {
            Alert = 0x15,
            AppData = 0x17,
            ChangeCipherSpec = 20,
            Handshake = 0x16
        }

        private enum Framing
        {
            None,
            BeforeSSL3,
            SinceSSL3,
            Unified,
            Invalid
        }
    }
}

