namespace System.Net.Security
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;

    public class NegotiateStream : AuthenticatedStream
    {
        private FixedSizeReader _FrameReader;
        private byte[] _InternalBuffer;
        private int _InternalBufferCount;
        private int _InternalOffset;
        private NegoState _NegoState;
        private int _NestedRead;
        private int _NestedWrite;
        private string _Package;
        private static AsyncProtocolCallback _ReadCallback = new AsyncProtocolCallback(NegotiateStream.ReadCallback);
        private byte[] _ReadHeader;
        private IIdentity _RemoteIdentity;
        private static AsyncCallback _WriteCallback = new AsyncCallback(NegotiateStream.WriteCallback);

        public NegotiateStream(Stream innerStream) : this(innerStream, false)
        {
        }

        public NegotiateStream(Stream innerStream, bool leaveInnerStreamOpen) : base(innerStream, leaveInnerStreamOpen)
        {
            this._NegoState = new NegoState(innerStream, leaveInnerStreamOpen);
            this._Package = NegoState.DefaultPackage;
            this.InitializeStreamPart();
        }

        private void AdjustInternalBufferOffsetSize(int bytes, int offset)
        {
            this._InternalBufferCount = bytes;
            this._InternalOffset = offset;
        }

        public virtual void AuthenticateAsClient()
        {
            this.AuthenticateAsClient((NetworkCredential) CredentialCache.DefaultCredentials, null, string.Empty, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
        }

        public virtual void AuthenticateAsClient(NetworkCredential credential, string targetName)
        {
            this.AuthenticateAsClient(credential, null, targetName, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
        }

        public virtual void AuthenticateAsClient(NetworkCredential credential, ChannelBinding binding, string targetName)
        {
            this.AuthenticateAsClient(credential, binding, targetName, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
        }

        public virtual void AuthenticateAsClient(NetworkCredential credential, string targetName, ProtectionLevel requiredProtectionLevel, TokenImpersonationLevel allowedImpersonationLevel)
        {
            this.AuthenticateAsClient(credential, null, targetName, requiredProtectionLevel, allowedImpersonationLevel);
        }

        public virtual void AuthenticateAsClient(NetworkCredential credential, ChannelBinding binding, string targetName, ProtectionLevel requiredProtectionLevel, TokenImpersonationLevel allowedImpersonationLevel)
        {
            this._NegoState.ValidateCreateContext(this._Package, false, credential, targetName, binding, requiredProtectionLevel, allowedImpersonationLevel);
            this._NegoState.ProcessAuthentication(null);
        }

        public virtual void AuthenticateAsServer()
        {
            this.AuthenticateAsServer((NetworkCredential) CredentialCache.DefaultCredentials, null, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
        }

        public virtual void AuthenticateAsServer(ExtendedProtectionPolicy policy)
        {
            this.AuthenticateAsServer((NetworkCredential) CredentialCache.DefaultCredentials, policy, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
        }

        public virtual void AuthenticateAsServer(NetworkCredential credential, ProtectionLevel requiredProtectionLevel, TokenImpersonationLevel requiredImpersonationLevel)
        {
            this.AuthenticateAsServer(credential, null, requiredProtectionLevel, requiredImpersonationLevel);
        }

        public virtual void AuthenticateAsServer(NetworkCredential credential, ExtendedProtectionPolicy policy, ProtectionLevel requiredProtectionLevel, TokenImpersonationLevel requiredImpersonationLevel)
        {
            if (!ComNetOS.IsWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
            }
            this._NegoState.ValidateCreateContext(this._Package, credential, string.Empty, policy, requiredProtectionLevel, requiredImpersonationLevel);
            this._NegoState.ProcessAuthentication(null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginAuthenticateAsClient((NetworkCredential) CredentialCache.DefaultCredentials, null, string.Empty, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(NetworkCredential credential, string targetName, AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginAuthenticateAsClient(credential, null, targetName, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(NetworkCredential credential, ChannelBinding binding, string targetName, AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginAuthenticateAsClient(credential, binding, targetName, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(NetworkCredential credential, string targetName, ProtectionLevel requiredProtectionLevel, TokenImpersonationLevel allowedImpersonationLevel, AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginAuthenticateAsClient(credential, null, targetName, requiredProtectionLevel, allowedImpersonationLevel, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(NetworkCredential credential, ChannelBinding binding, string targetName, ProtectionLevel requiredProtectionLevel, TokenImpersonationLevel allowedImpersonationLevel, AsyncCallback asyncCallback, object asyncState)
        {
            this._NegoState.ValidateCreateContext(this._Package, false, credential, targetName, binding, requiredProtectionLevel, allowedImpersonationLevel);
            LazyAsyncResult lazyResult = new LazyAsyncResult(this._NegoState, asyncState, asyncCallback);
            this._NegoState.ProcessAuthentication(lazyResult);
            return lazyResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsServer(AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginAuthenticateAsServer((NetworkCredential) CredentialCache.DefaultCredentials, null, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsServer(ExtendedProtectionPolicy policy, AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginAuthenticateAsServer((NetworkCredential) CredentialCache.DefaultCredentials, policy, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsServer(NetworkCredential credential, ProtectionLevel requiredProtectionLevel, TokenImpersonationLevel requiredImpersonationLevel, AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginAuthenticateAsServer(credential, null, requiredProtectionLevel, requiredImpersonationLevel, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsServer(NetworkCredential credential, ExtendedProtectionPolicy policy, ProtectionLevel requiredProtectionLevel, TokenImpersonationLevel requiredImpersonationLevel, AsyncCallback asyncCallback, object asyncState)
        {
            if (!ComNetOS.IsWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
            }
            this._NegoState.ValidateCreateContext(this._Package, credential, string.Empty, policy, requiredProtectionLevel, requiredImpersonationLevel);
            LazyAsyncResult lazyResult = new LazyAsyncResult(this._NegoState, asyncState, asyncCallback);
            this._NegoState.ProcessAuthentication(lazyResult);
            return lazyResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            this._NegoState.CheckThrow(true);
            if (!this._NegoState.CanGetSecureStream)
            {
                return base.InnerStream.BeginRead(buffer, offset, count, asyncCallback, asyncState);
            }
            BufferAsyncResult userAsyncResult = new BufferAsyncResult(this, buffer, offset, count, asyncState, asyncCallback);
            AsyncProtocolRequest asyncRequest = new AsyncProtocolRequest(userAsyncResult);
            this.ProcessRead(buffer, offset, count, asyncRequest);
            return userAsyncResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            this._NegoState.CheckThrow(true);
            if (!this._NegoState.CanGetSecureStream)
            {
                return base.InnerStream.BeginWrite(buffer, offset, count, asyncCallback, asyncState);
            }
            BufferAsyncResult userAsyncResult = new BufferAsyncResult(this, buffer, offset, count, true, asyncState, asyncCallback);
            AsyncProtocolRequest asyncRequest = new AsyncProtocolRequest(userAsyncResult);
            this.ProcessWrite(buffer, offset, count, asyncRequest);
            return userAsyncResult;
        }

        private void DecrementInternalBufferCount(int decrCount)
        {
            this._InternalOffset += decrCount;
            this._InternalBufferCount -= decrCount;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this._NegoState.Close();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public virtual void EndAuthenticateAsClient(IAsyncResult asyncResult)
        {
            this._NegoState.EndProcessAuthentication(asyncResult);
        }

        public virtual void EndAuthenticateAsServer(IAsyncResult asyncResult)
        {
            this._NegoState.EndProcessAuthentication(asyncResult);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            this._NegoState.CheckThrow(true);
            if (!this._NegoState.CanGetSecureStream)
            {
                return base.InnerStream.EndRead(asyncResult);
            }
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

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this._NegoState.CheckThrow(true);
            if (!this._NegoState.CanGetSecureStream)
            {
                base.InnerStream.EndWrite(asyncResult);
            }
            else
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
        }

        private void EnsureInternalBufferSize(int bytes)
        {
            this._InternalBufferCount = bytes;
            this._InternalOffset = 0;
            if ((this.InternalBuffer == null) || (this.InternalBuffer.Length < bytes))
            {
                this._InternalBuffer = new byte[bytes];
            }
        }

        public override void Flush()
        {
            base.InnerStream.Flush();
        }

        private void InitializeStreamPart()
        {
            this._ReadHeader = new byte[4];
            this._FrameReader = new FixedSizeReader(base.InnerStream);
        }

        private int ProcessFrameBody(int readBytes, byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            int num;
            if (readBytes == 0)
            {
                throw new IOException(SR.GetString("net_io_eof"));
            }
            readBytes = this._NegoState.DecryptData(this.InternalBuffer, 0, readBytes, out num);
            this.AdjustInternalBufferOffsetSize(readBytes, num);
            if ((readBytes == 0) && (count != 0))
            {
                return -1;
            }
            if (readBytes > count)
            {
                readBytes = count;
            }
            Buffer.BlockCopy(this.InternalBuffer, this.InternalOffset, buffer, offset, readBytes);
            this.DecrementInternalBufferCount(readBytes);
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

        private void ProcessWrite(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            this._NegoState.CheckThrow(true);
            if (!this._NegoState.CanGetSecureStream)
            {
                return base.InnerStream.Read(buffer, offset, count);
            }
            return this.ProcessRead(buffer, offset, count, null);
        }

        private static void ReadCallback(AsyncProtocolRequest asyncRequest)
        {
            try
            {
                NegotiateStream asyncObject = (NegotiateStream) asyncRequest.AsyncObject;
                BufferAsyncResult userAsyncResult = (BufferAsyncResult) asyncRequest.UserAsyncResult;
                if (asyncRequest.Buffer == asyncObject._ReadHeader)
                {
                    asyncObject.StartFrameBody(asyncRequest.Result, userAsyncResult.Buffer, userAsyncResult.Offset, userAsyncResult.Count, asyncRequest);
                }
                else if (-1 == asyncObject.ProcessFrameBody(asyncRequest.Result, userAsyncResult.Buffer, userAsyncResult.Offset, userAsyncResult.Count, asyncRequest))
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

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        public override void SetLength(long value)
        {
            base.InnerStream.SetLength(value);
        }

        private int StartFrameBody(int readBytes, byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            if (readBytes == 0)
            {
                if (asyncRequest != null)
                {
                    asyncRequest.CompleteUser(0);
                }
                return 0;
            }
            readBytes = this._ReadHeader[3];
            readBytes = (readBytes << 8) | this._ReadHeader[2];
            readBytes = (readBytes << 8) | this._ReadHeader[1];
            readBytes = (readBytes << 8) | this._ReadHeader[0];
            if ((readBytes <= 4) || (readBytes > 0x10000))
            {
                throw new IOException(SR.GetString("net_frame_read_size"));
            }
            this.EnsureInternalBufferSize(readBytes);
            if (asyncRequest != null)
            {
                asyncRequest.SetNextRequest(this.InternalBuffer, 0, readBytes, _ReadCallback);
                this._FrameReader.AsyncReadPacket(asyncRequest);
                if (!asyncRequest.MustCompleteSynchronously)
                {
                    return 0;
                }
                readBytes = asyncRequest.Result;
            }
            else
            {
                readBytes = this._FrameReader.ReadPacket(this.InternalBuffer, 0, readBytes);
            }
            return this.ProcessFrameBody(readBytes, buffer, offset, count, asyncRequest);
        }

        private int StartFrameHeader(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            int readBytes = 0;
            if (asyncRequest != null)
            {
                asyncRequest.SetNextRequest(this._ReadHeader, 0, this._ReadHeader.Length, _ReadCallback);
                this._FrameReader.AsyncReadPacket(asyncRequest);
                if (!asyncRequest.MustCompleteSynchronously)
                {
                    return 0;
                }
                readBytes = asyncRequest.Result;
            }
            else
            {
                readBytes = this._FrameReader.ReadPacket(this._ReadHeader, 0, this._ReadHeader.Length);
            }
            return this.StartFrameBody(readBytes, buffer, offset, count, asyncRequest);
        }

        private int StartReading(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            int num;
            while ((num = this.StartFrameHeader(buffer, offset, count, asyncRequest)) == -1)
            {
            }
            return num;
        }

        private void StartWriting(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            if (count >= 0)
            {
                byte[] outBuffer = null;
                do
                {
                    int num2;
                    int num = Math.Min(count, 0xfc00);
                    try
                    {
                        num2 = this._NegoState.EncryptData(buffer, offset, num, ref outBuffer);
                    }
                    catch (Exception exception)
                    {
                        throw new IOException(SR.GetString("net_io_encrypt"), exception);
                    }
                    if (asyncRequest != null)
                    {
                        asyncRequest.SetNextRequest(buffer, offset + num, count - num, null);
                        IAsyncResult asyncResult = base.InnerStream.BeginWrite(outBuffer, 0, num2, _WriteCallback, asyncRequest);
                        if (!asyncResult.CompletedSynchronously)
                        {
                            return;
                        }
                        base.InnerStream.EndWrite(asyncResult);
                    }
                    else
                    {
                        base.InnerStream.Write(outBuffer, 0, num2);
                    }
                    offset += num;
                    count -= num;
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._NegoState.CheckThrow(true);
            if (!this._NegoState.CanGetSecureStream)
            {
                base.InnerStream.Write(buffer, offset, count);
            }
            else
            {
                this.ProcessWrite(buffer, offset, count, null);
            }
        }

        private static void WriteCallback(IAsyncResult transportResult)
        {
            if (!transportResult.CompletedSynchronously)
            {
                AsyncProtocolRequest asyncState = (AsyncProtocolRequest) transportResult.AsyncState;
                try
                {
                    NegotiateStream asyncObject = (NegotiateStream) asyncState.AsyncObject;
                    asyncObject.InnerStream.EndWrite(transportResult);
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
                    asyncState.CompleteWithError(exception);
                }
            }
        }

        public override bool CanRead
        {
            get
            {
                return (this.IsAuthenticated && base.InnerStream.CanRead);
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return base.InnerStream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return (this.IsAuthenticated && base.InnerStream.CanWrite);
            }
        }

        public virtual TokenImpersonationLevel ImpersonationLevel
        {
            get
            {
                return this._NegoState.AllowedImpersonation;
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

        public override bool IsAuthenticated
        {
            get
            {
                return this._NegoState.IsAuthenticated;
            }
        }

        public override bool IsEncrypted
        {
            get
            {
                return this._NegoState.IsEncrypted;
            }
        }

        public override bool IsMutuallyAuthenticated
        {
            get
            {
                return this._NegoState.IsMutuallyAuthenticated;
            }
        }

        public override bool IsServer
        {
            get
            {
                return this._NegoState.IsServer;
            }
        }

        public override bool IsSigned
        {
            get
            {
                return this._NegoState.IsSigned;
            }
        }

        public override long Length
        {
            get
            {
                return base.InnerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return base.InnerStream.Position;
            }
            set
            {
                throw new NotSupportedException(SR.GetString("net_noseek"));
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return base.InnerStream.ReadTimeout;
            }
            set
            {
                base.InnerStream.ReadTimeout = value;
            }
        }

        public virtual IIdentity RemoteIdentity
        {
            get
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                if (this._RemoteIdentity == null)
                {
                    this._RemoteIdentity = this._NegoState.GetIdentity();
                }
                return this._RemoteIdentity;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return base.InnerStream.WriteTimeout;
            }
            set
            {
                base.InnerStream.WriteTimeout = value;
            }
        }
    }
}

