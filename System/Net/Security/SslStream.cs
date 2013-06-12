namespace System.Net.Security
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Authentication;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    public class SslStream : AuthenticatedStream
    {
        private SslState _SslState;
        private LocalCertificateSelectionCallback _userCertificateSelectionCallback;
        private RemoteCertificateValidationCallback _userCertificateValidationCallback;
        private object m_RemoteCertificateOrBytes;

        public SslStream(Stream innerStream) : this(innerStream, false, null, null)
        {
        }

        public SslStream(Stream innerStream, bool leaveInnerStreamOpen) : this(innerStream, leaveInnerStreamOpen, null, null, EncryptionPolicy.RequireEncryption)
        {
        }

        public SslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback) : this(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, null, EncryptionPolicy.RequireEncryption)
        {
        }

        public SslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback) : this(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, userCertificateSelectionCallback, EncryptionPolicy.RequireEncryption)
        {
        }

        public SslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback, EncryptionPolicy encryptionPolicy) : base(innerStream, leaveInnerStreamOpen)
        {
            if (((encryptionPolicy != EncryptionPolicy.RequireEncryption) && (encryptionPolicy != EncryptionPolicy.AllowNoEncryption)) && (encryptionPolicy != EncryptionPolicy.NoEncryption))
            {
                throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "EncryptionPolicy" }), "encryptionPolicy");
            }
            this._userCertificateValidationCallback = userCertificateValidationCallback;
            this._userCertificateSelectionCallback = userCertificateSelectionCallback;
            RemoteCertValidationCallback certValidationCallback = new RemoteCertValidationCallback(this.userCertValidationCallbackWrapper);
            LocalCertSelectionCallback certSelectionCallback = (userCertificateSelectionCallback == null) ? null : new LocalCertSelectionCallback(this.userCertSelectionCallbackWrapper);
            this._SslState = new SslState(innerStream, certValidationCallback, certSelectionCallback, encryptionPolicy);
        }

        public virtual void AuthenticateAsClient(string targetHost)
        {
            this.AuthenticateAsClient(targetHost, new X509CertificateCollection(), SslProtocols.Default, false);
        }

        public virtual void AuthenticateAsClient(string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
        {
            this._SslState.ValidateCreateContext(false, targetHost, enabledSslProtocols, null, clientCertificates, true, checkCertificateRevocation);
            this._SslState.ProcessAuthentication(null);
        }

        public virtual void AuthenticateAsServer(X509Certificate serverCertificate)
        {
            this.AuthenticateAsServer(serverCertificate, false, SslProtocols.Default, false);
        }

        public virtual void AuthenticateAsServer(X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
        {
            if (!ComNetOS.IsWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
            }
            this._SslState.ValidateCreateContext(true, string.Empty, enabledSslProtocols, serverCertificate, null, clientCertificateRequired, checkCertificateRevocation);
            this._SslState.ProcessAuthentication(null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(string targetHost, AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginAuthenticateAsClient(targetHost, new X509CertificateCollection(), SslProtocols.Default, false, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsClient(string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
        {
            this._SslState.ValidateCreateContext(false, targetHost, enabledSslProtocols, null, clientCertificates, true, checkCertificateRevocation);
            LazyAsyncResult lazyResult = new LazyAsyncResult(this._SslState, asyncState, asyncCallback);
            this._SslState.ProcessAuthentication(lazyResult);
            return lazyResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsServer(X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginAuthenticateAsServer(serverCertificate, false, SslProtocols.Default, false, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public virtual IAsyncResult BeginAuthenticateAsServer(X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
        {
            if (!ComNetOS.IsWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
            }
            this._SslState.ValidateCreateContext(true, string.Empty, enabledSslProtocols, serverCertificate, null, clientCertificateRequired, checkCertificateRevocation);
            LazyAsyncResult lazyResult = new LazyAsyncResult(this._SslState, asyncState, asyncCallback);
            this._SslState.ProcessAuthentication(lazyResult);
            return lazyResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            return this._SslState.SecureStream.BeginRead(buffer, offset, count, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            return this._SslState.SecureStream.BeginWrite(buffer, offset, count, asyncCallback, asyncState);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this._SslState.Close();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public virtual void EndAuthenticateAsClient(IAsyncResult asyncResult)
        {
            this._SslState.EndProcessAuthentication(asyncResult);
        }

        public virtual void EndAuthenticateAsServer(IAsyncResult asyncResult)
        {
            this._SslState.EndProcessAuthentication(asyncResult);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this._SslState.SecureStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this._SslState.SecureStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            this._SslState.Flush();
        }

        internal ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            return this._SslState.GetChannelBinding(kind);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this._SslState.SecureStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        public override void SetLength(long value)
        {
            base.InnerStream.SetLength(value);
        }

        private X509Certificate userCertSelectionCallbackWrapper(string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return this._userCertificateSelectionCallback(this, targetHost, localCertificates, remoteCertificate, acceptableIssuers);
        }

        private bool userCertValidationCallbackWrapper(string hostName, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            this.m_RemoteCertificateOrBytes = (certificate == null) ? null : certificate.GetRawCertData();
            if (this._userCertificateValidationCallback != null)
            {
                return this._userCertificateValidationCallback(this, certificate, chain, sslPolicyErrors);
            }
            if (!this._SslState.RemoteCertRequired)
            {
                sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateNotAvailable;
            }
            return (sslPolicyErrors == SslPolicyErrors.None);
        }

        public void Write(byte[] buffer)
        {
            this._SslState.SecureStream.Write(buffer, 0, buffer.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._SslState.SecureStream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get
            {
                return (this._SslState.IsAuthenticated && base.InnerStream.CanRead);
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
                return (this._SslState.IsAuthenticated && base.InnerStream.CanWrite);
            }
        }

        public virtual bool CheckCertRevocationStatus
        {
            get
            {
                return this._SslState.CheckCertRevocationStatus;
            }
        }

        public virtual CipherAlgorithmType CipherAlgorithm
        {
            get
            {
                return this._SslState.CipherAlgorithm;
            }
        }

        public virtual int CipherStrength
        {
            get
            {
                return this._SslState.CipherStrength;
            }
        }

        public virtual HashAlgorithmType HashAlgorithm
        {
            get
            {
                return this._SslState.HashAlgorithm;
            }
        }

        public virtual int HashStrength
        {
            get
            {
                return this._SslState.HashStrength;
            }
        }

        public override bool IsAuthenticated
        {
            get
            {
                return this._SslState.IsAuthenticated;
            }
        }

        public override bool IsEncrypted
        {
            get
            {
                return this.IsAuthenticated;
            }
        }

        public override bool IsMutuallyAuthenticated
        {
            get
            {
                return this._SslState.IsMutuallyAuthenticated;
            }
        }

        public override bool IsServer
        {
            get
            {
                return this._SslState.IsServer;
            }
        }

        public override bool IsSigned
        {
            get
            {
                return this.IsAuthenticated;
            }
        }

        public virtual ExchangeAlgorithmType KeyExchangeAlgorithm
        {
            get
            {
                return this._SslState.KeyExchangeAlgorithm;
            }
        }

        public virtual int KeyExchangeStrength
        {
            get
            {
                return this._SslState.KeyExchangeStrength;
            }
        }

        public override long Length
        {
            get
            {
                return base.InnerStream.Length;
            }
        }

        public virtual X509Certificate LocalCertificate
        {
            get
            {
                return this._SslState.LocalCertificate;
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

        public virtual X509Certificate RemoteCertificate
        {
            get
            {
                this._SslState.CheckThrow(true);
                object remoteCertificateOrBytes = this.m_RemoteCertificateOrBytes;
                if ((remoteCertificateOrBytes != null) && (remoteCertificateOrBytes.GetType() == typeof(byte[])))
                {
                    return (this.m_RemoteCertificateOrBytes = new X509Certificate((byte[]) remoteCertificateOrBytes));
                }
                return (remoteCertificateOrBytes as X509Certificate);
            }
        }

        public virtual SslProtocols SslProtocol
        {
            get
            {
                return this._SslState.SslProtocol;
            }
        }

        public System.Net.TransportContext TransportContext
        {
            get
            {
                return new SslStreamContext(this);
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

