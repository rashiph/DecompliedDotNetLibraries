namespace System.ServiceModel.Security
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Threading;

    internal sealed class TlsSspiNegotiation : ISspiNegotiation, IDisposable
    {
        private SspiContextFlags attributes;
        private X509Certificate2 clientCertificate;
        private bool clientCertRequired;
        private static SspiContextFlags ClientStandardFlags = ((StandardFlags | SspiContextFlags.AcceptIdentify) | SspiContextFlags.AcceptExtendedError);
        private SslConnectionInfo connectionInfo;
        private SafeFreeCredentials credentialsHandle;
        private string destination;
        private bool disposed;
        private string incomingValueTypeUri;
        private bool isCompleted;
        private bool isServer;
        private SchProtocols protocolFlags;
        private X509Certificate2 remoteCertificate;
        private X509Certificate2Collection remoteCertificateChain;
        private SafeDeleteContext securityContext;
        private const string SecurityPackage = "Microsoft Unified Security Protocol Provider";
        private X509Certificate2 serverCertificate;
        private static SspiContextFlags ServerStandardFlags = ((StandardFlags | SspiContextFlags.AcceptExtendedError) | SspiContextFlags.AcceptStream);
        private static SspiContextFlags StandardFlags = (SspiContextFlags.AllocateMemory | SspiContextFlags.Confidentiality | SspiContextFlags.ReplayDetect);
        private System.IdentityModel.StreamSizes streamSizes;
        private object syncObject;
        private bool wasClientCertificateSent;

        public TlsSspiNegotiation(SchProtocols protocolFlags, X509Certificate2 serverCertificate, bool clientCertRequired) : this(null, true, protocolFlags, serverCertificate, null, clientCertRequired)
        {
        }

        public TlsSspiNegotiation(string destination, SchProtocols protocolFlags, X509Certificate2 clientCertificate) : this(destination, false, protocolFlags, null, clientCertificate, false)
        {
        }

        private TlsSspiNegotiation(string destination, bool isServer, SchProtocols protocolFlags, X509Certificate2 serverCertificate, X509Certificate2 clientCertificate, bool clientCertRequired)
        {
            this.syncObject = new object();
            SspiWrapper.GetVerifyPackageInfo("Microsoft Unified Security Protocol Provider");
            this.destination = destination;
            this.isServer = isServer;
            this.protocolFlags = protocolFlags;
            this.serverCertificate = serverCertificate;
            this.clientCertificate = clientCertificate;
            this.clientCertRequired = clientCertRequired;
            this.securityContext = null;
            if (isServer)
            {
                this.ValidateServerCertificate();
            }
            else
            {
                this.ValidateClientCertificate();
            }
            if (this.isServer)
            {
                try
                {
                    this.AcquireServerCredentials();
                }
                catch (Win32Exception exception)
                {
                    if (exception.NativeErrorCode != -2146893043)
                    {
                        throw;
                    }
                    if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
                    {
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    Thread.Sleep(0);
                    this.AcquireServerCredentials();
                }
            }
            else
            {
                this.AcquireDummyCredentials();
            }
        }

        private void AcquireClientCredentials()
        {
            SecureCredential scc = new SecureCredential(4, this.ClientCertificate, SecureCredential.Flags.NoDefaultCred | SecureCredential.Flags.ValidateManual, this.protocolFlags);
            this.credentialsHandle = SspiWrapper.AcquireCredentialsHandle("Microsoft Unified Security Protocol Provider", CredentialUse.Outbound, scc);
        }

        private void AcquireDummyCredentials()
        {
            SecureCredential scc = new SecureCredential(4, null, SecureCredential.Flags.NoDefaultCred | SecureCredential.Flags.ValidateManual, this.protocolFlags);
            this.credentialsHandle = SspiWrapper.AcquireCredentialsHandle("Microsoft Unified Security Protocol Provider", CredentialUse.Outbound, scc);
        }

        private void AcquireServerCredentials()
        {
            SecureCredential scc = new SecureCredential(4, this.serverCertificate, SecureCredential.Flags.Zero, this.protocolFlags);
            this.credentialsHandle = SspiWrapper.AcquireCredentialsHandle("Microsoft Unified Security Protocol Provider", CredentialUse.Inbound, scc);
        }

        public byte[] Decrypt(byte[] encryptedContent)
        {
            int num2;
            this.ThrowIfDisposed();
            byte[] dst = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(encryptedContent.Length);
            Buffer.BlockCopy(encryptedContent, 0, dst, 0, encryptedContent.Length);
            int dataLen = 0;
            this.DecryptInPlace(dst, out num2, out dataLen);
            byte[] buffer2 = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(dataLen);
            Buffer.BlockCopy(dst, num2, buffer2, 0, dataLen);
            return buffer2;
        }

        internal void DecryptInPlace(byte[] encryptedContent, out int dataStartOffset, out int dataLen)
        {
            this.ThrowIfDisposed();
            dataStartOffset = this.StreamSizes.header;
            dataLen = 0;
            byte[] data = new byte[0];
            byte[] buffer2 = new byte[0];
            byte[] buffer3 = new byte[0];
            SecurityBuffer[] input = new SecurityBuffer[] { new SecurityBuffer(encryptedContent, 0, encryptedContent.Length, System.IdentityModel.BufferType.Data), new SecurityBuffer(data, System.IdentityModel.BufferType.Empty), new SecurityBuffer(buffer2, System.IdentityModel.BufferType.Empty), new SecurityBuffer(buffer3, System.IdentityModel.BufferType.Empty) };
            int error = SspiWrapper.DecryptMessage(this.securityContext, input, 0, false);
            if (error != 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].type == System.IdentityModel.BufferType.Data)
                {
                    dataLen = input[i].size;
                    return;
                }
            }
            this.OnBadData();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (this.syncObject)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    if (disposing)
                    {
                        if (this.securityContext != null)
                        {
                            this.securityContext.Close();
                            this.securityContext = null;
                        }
                        if (this.credentialsHandle != null)
                        {
                            this.credentialsHandle.Close();
                            this.credentialsHandle = null;
                        }
                    }
                    this.connectionInfo = null;
                    this.destination = null;
                    this.streamSizes = null;
                }
            }
        }

        public byte[] Encrypt(byte[] input)
        {
            this.ThrowIfDisposed();
            byte[] dst = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray((input.Length + this.StreamSizes.header) + this.StreamSizes.trailer);
            Buffer.BlockCopy(input, 0, dst, this.StreamSizes.header, input.Length);
            int encryptedDataLen = 0;
            this.EncryptInPlace(dst, 0, input.Length, out encryptedDataLen);
            if (encryptedDataLen == dst.Length)
            {
                return dst;
            }
            byte[] buffer2 = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(encryptedDataLen);
            Buffer.BlockCopy(dst, 0, buffer2, 0, encryptedDataLen);
            return buffer2;
        }

        internal void EncryptInPlace(byte[] buffer, int bufferStartOffset, int dataLen, out int encryptedDataLen)
        {
            this.ThrowIfDisposed();
            encryptedDataLen = 0;
            if ((((bufferStartOffset + dataLen) + this.StreamSizes.header) + this.StreamSizes.trailer) > buffer.Length)
            {
                this.OnBadData();
            }
            byte[] data = new byte[0];
            int offset = (bufferStartOffset + this.StreamSizes.header) + dataLen;
            SecurityBuffer[] input = new SecurityBuffer[] { new SecurityBuffer(buffer, bufferStartOffset, this.StreamSizes.header, System.IdentityModel.BufferType.Header), new SecurityBuffer(buffer, bufferStartOffset + this.StreamSizes.header, dataLen, System.IdentityModel.BufferType.Data), new SecurityBuffer(buffer, offset, this.StreamSizes.trailer, System.IdentityModel.BufferType.Trailer), new SecurityBuffer(data, System.IdentityModel.BufferType.Empty) };
            int error = SspiWrapper.EncryptMessage(this.securityContext, input, 0);
            if (error != 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            int size = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].type == System.IdentityModel.BufferType.Trailer)
                {
                    size = input[i].size;
                    encryptedDataLen = (this.StreamSizes.header + dataLen) + size;
                    return;
                }
            }
            this.OnBadData();
        }

        private SafeFreeCertContext ExtractCertificateHandle(ContextAttribute contextAttribute)
        {
            return (SspiWrapper.QueryContextAttributes(this.securityContext, contextAttribute) as SafeFreeCertContext);
        }

        private void ExtractRemoteCertificate()
        {
            SafeFreeCertContext certContext = null;
            this.remoteCertificate = null;
            this.remoteCertificateChain = null;
            try
            {
                certContext = this.ExtractCertificateHandle(ContextAttribute.RemoteCertificate);
                if ((certContext != null) && !certContext.IsInvalid)
                {
                    this.remoteCertificateChain = UnmanagedCertificateContext.GetStore(certContext);
                    this.remoteCertificate = new X509Certificate2(certContext.DangerousGetHandle());
                }
            }
            finally
            {
                if (certContext != null)
                {
                    certContext.Close();
                }
            }
        }

        public byte[] GetOutgoingBlob(byte[] incomingBlob, ChannelBinding channelbinding, ExtendedProtectionPolicy protectionPolicy)
        {
            this.ThrowIfDisposed();
            SecurityBuffer inputBuffer = null;
            if (incomingBlob != null)
            {
                inputBuffer = new SecurityBuffer(incomingBlob, System.IdentityModel.BufferType.Token);
            }
            SecurityBuffer outputBuffer = new SecurityBuffer(null, System.IdentityModel.BufferType.Token);
            this.remoteCertificate = null;
            int error = 0;
            if (this.isServer)
            {
                error = SspiWrapper.AcceptSecurityContext(this.credentialsHandle, ref this.securityContext, ServerStandardFlags | (this.clientCertRequired ? SspiContextFlags.MutualAuth : SspiContextFlags.Zero), Endianness.Native, inputBuffer, outputBuffer, ref this.attributes);
            }
            else
            {
                error = SspiWrapper.InitializeSecurityContext(this.credentialsHandle, ref this.securityContext, this.destination, ClientStandardFlags, Endianness.Native, inputBuffer, outputBuffer, ref this.attributes);
            }
            if ((error & -2147483648) != 0)
            {
                this.Dispose();
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            if (error == 0)
            {
                if (System.ServiceModel.Security.SecurityUtils.ShouldValidateSslCipherStrength())
                {
                    SslConnectionInfo info = (SslConnectionInfo) SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.ConnectionInfo);
                    if (info == null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("CannotObtainSslConnectionInfo")));
                    }
                    System.ServiceModel.Security.SecurityUtils.ValidateSslCipherStrength(info.DataKeySize);
                }
                this.isCompleted = true;
            }
            else
            {
                if (error == 0x90320)
                {
                    this.AcquireClientCredentials();
                    if (this.ClientCertificate != null)
                    {
                        this.wasClientCertificateSent = true;
                    }
                    return this.GetOutgoingBlob(incomingBlob, channelbinding, protectionPolicy);
                }
                if (error != 0x90312)
                {
                    this.Dispose();
                    if (error == -2146893052)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, System.ServiceModel.SR.GetString("LsaAuthorityNotContacted")));
                    }
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
            }
            return outputBuffer.token;
        }

        public string GetRemoteIdentityName()
        {
            if (!this.IsValidContext)
            {
                return string.Empty;
            }
            X509Certificate2 remoteCertificate = this.RemoteCertificate;
            if (remoteCertificate == null)
            {
                return string.Empty;
            }
            return System.ServiceModel.Security.SecurityUtils.GetCertificateId(remoteCertificate);
        }

        private void OnBadData()
        {
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("BadData")));
        }

        private void ThrowIfDisposed()
        {
            lock (this.syncObject)
            {
                if (this.disposed)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(null));
                }
            }
        }

        internal bool TryGetContextIdentity(out WindowsIdentity mappedIdentity)
        {
            bool flag;
            if (!this.IsValidContext)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(-2146893055));
            }
            SafeCloseHandle token = null;
            try
            {
                if (SspiWrapper.QuerySecurityContextToken(this.securityContext, out token) != 0)
                {
                    mappedIdentity = null;
                    return false;
                }
                mappedIdentity = new WindowsIdentity(token.DangerousGetHandle(), "SSL/PCT");
                flag = true;
            }
            finally
            {
                if (token != null)
                {
                    token.Close();
                }
            }
            return flag;
        }

        private void ValidateClientCertificate()
        {
            if (this.clientCertificate != null)
            {
                ValidatePrivateKey(this.clientCertificate);
            }
        }

        private static void ValidatePrivateKey(X509Certificate2 certificate)
        {
            bool flag = false;
            try
            {
                flag = (certificate != null) && (certificate.PrivateKey != null);
            }
            catch (SecurityException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SslCertMayNotDoKeyExchange", new object[] { certificate.SubjectName.Name }), exception));
            }
            catch (CryptographicException exception2)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SslCertMayNotDoKeyExchange", new object[] { certificate.SubjectName.Name }), exception2));
            }
            if (!flag)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SslCertMustHavePrivateKey", new object[] { certificate.SubjectName.Name })));
            }
        }

        private void ValidateServerCertificate()
        {
            if (this.serverCertificate == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serverCertificate");
            }
            ValidatePrivateKey(this.serverCertificate);
        }

        public X509Certificate2 ClientCertificate
        {
            get
            {
                this.ThrowIfDisposed();
                return this.clientCertificate;
            }
        }

        public bool ClientCertRequired
        {
            get
            {
                this.ThrowIfDisposed();
                return this.clientCertRequired;
            }
        }

        internal SslConnectionInfo ConnectionInfo
        {
            get
            {
                this.ThrowIfDisposed();
                if (!this.IsValidContext)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(-2146893055));
                }
                if (this.connectionInfo != null)
                {
                    return this.connectionInfo;
                }
                SslConnectionInfo info = SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.ConnectionInfo) as SslConnectionInfo;
                if (this.IsCompleted)
                {
                    this.connectionInfo = info;
                }
                return info;
            }
        }

        public string Destination
        {
            get
            {
                this.ThrowIfDisposed();
                return this.destination;
            }
        }

        public DateTime ExpirationTimeUtc
        {
            get
            {
                this.ThrowIfDisposed();
                return System.ServiceModel.Security.SecurityUtils.MaxUtcDateTime;
            }
        }

        internal string IncomingValueTypeUri
        {
            get
            {
                return this.incomingValueTypeUri;
            }
            set
            {
                this.incomingValueTypeUri = value;
            }
        }

        public bool IsCompleted
        {
            get
            {
                this.ThrowIfDisposed();
                return this.isCompleted;
            }
        }

        public bool IsMutualAuthFlag
        {
            get
            {
                this.ThrowIfDisposed();
                return ((this.attributes & SspiContextFlags.MutualAuth) != SspiContextFlags.Zero);
            }
        }

        public bool IsValidContext
        {
            get
            {
                return ((this.securityContext != null) && !this.securityContext.IsInvalid);
            }
        }

        public string KeyEncryptionAlgorithm
        {
            get
            {
                return "http://schemas.xmlsoap.org/2005/02/trust/tlsnego#TLS_Wrap";
            }
        }

        public X509Certificate2 RemoteCertificate
        {
            get
            {
                this.ThrowIfDisposed();
                if (!this.IsValidContext)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(-2146893055));
                }
                if (this.remoteCertificate == null)
                {
                    this.ExtractRemoteCertificate();
                }
                return this.remoteCertificate;
            }
        }

        public X509Certificate2Collection RemoteCertificateChain
        {
            get
            {
                this.ThrowIfDisposed();
                if (!this.IsValidContext)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(-2146893055));
                }
                if (this.remoteCertificateChain == null)
                {
                    this.ExtractRemoteCertificate();
                }
                return this.remoteCertificateChain;
            }
        }

        public X509Certificate2 ServerCertificate
        {
            get
            {
                this.ThrowIfDisposed();
                return this.serverCertificate;
            }
        }

        internal System.IdentityModel.StreamSizes StreamSizes
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.streamSizes != null)
                {
                    return this.streamSizes;
                }
                System.IdentityModel.StreamSizes sizes = (System.IdentityModel.StreamSizes) SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.StreamSizes);
                if (this.IsCompleted)
                {
                    this.streamSizes = sizes;
                }
                return sizes;
            }
        }

        public bool WasClientCertificateSent
        {
            get
            {
                this.ThrowIfDisposed();
                return this.wasClientCertificateSent;
            }
        }

        private static class UnmanagedCertificateContext
        {
            internal static X509Certificate2Collection GetStore(SafeFreeCertContext certContext)
            {
                X509Certificate2Collection certificates = new X509Certificate2Collection();
                if (!certContext.IsInvalid)
                {
                    _CERT_CONTEXT _cert_context = (_CERT_CONTEXT) Marshal.PtrToStructure(certContext.DangerousGetHandle(), typeof(_CERT_CONTEXT));
                    if (!(_cert_context.hCertStore != IntPtr.Zero))
                    {
                        return certificates;
                    }
                    X509Store store = null;
                    try
                    {
                        store = new X509Store(_cert_context.hCertStore);
                        certificates = store.Certificates;
                    }
                    finally
                    {
                        if (store != null)
                        {
                            store.Close();
                        }
                    }
                }
                return certificates;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct _CERT_CONTEXT
            {
                internal int dwCertEncodingType;
                internal IntPtr pbCertEncoded;
                internal int cbCertEncoded;
                internal IntPtr pCertInfo;
                internal IntPtr hCertStore;
            }
        }
    }
}

