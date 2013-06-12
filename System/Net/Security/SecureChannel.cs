namespace System.Net.Security
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Principal;

    internal class SecureChannel
    {
        private const int ChainRevocationCheckExcludeRoot = 0x40000000;
        private ContextFlags m_Attributes;
        private LocalCertSelectionCallback m_CertSelectionDelegate;
        private bool m_CheckCertName;
        private bool m_CheckCertRevocation;
        private readonly X509CertificateCollection m_ClientCertificates;
        private SslConnectionInfo m_ConnectionInfo;
        private SafeFreeCredentials m_CredentialsHandle;
        private readonly string m_Destination;
        private readonly EncryptionPolicy m_EncryptionPolicy;
        private int m_HeaderSize = 5;
        private readonly string m_HostName;
        private bool m_IsRemoteCertificateAvailable;
        private int m_MaxDataSize = 0x3fe2;
        private readonly SchProtocols m_ProtocolFlags;
        private bool m_RefreshCredentialNeeded;
        private readonly bool m_RemoteCertRequired;
        private SafeDeleteContext m_SecurityContext;
        private X509Certificate m_SelectedClientCertificate;
        private X509Certificate m_ServerCertificate;
        private readonly bool m_ServerMode;
        private int m_TrailerSize = 0x10;
        private const ContextFlags RequiredFlags = (ContextFlags.AllocateMemory | ContextFlags.Confidentiality | ContextFlags.SequenceDetect | ContextFlags.ReplayDetect);
        private static X509Store s_MyCertStoreEx;
        private static X509Store s_MyMachineCertStoreEx;
        private static readonly object s_SyncObject = new object();
        internal const string SecurityPackage = "Microsoft Unified Security Protocol Provider";
        private const ContextFlags ServerRequiredFlags = (ContextFlags.AcceptStream | ContextFlags.AllocateMemory | ContextFlags.Confidentiality | ContextFlags.SequenceDetect | ContextFlags.ReplayDetect);

        internal SecureChannel(string hostname, bool serverMode, SchProtocols protocolFlags, X509Certificate serverCertificate, X509CertificateCollection clientCertificates, bool remoteCertRequired, bool checkCertName, bool checkCertRevocationStatus, EncryptionPolicy encryptionPolicy, LocalCertSelectionCallback certSelectionDelegate)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, ".ctor", string.Concat(new object[] { "hostname=", hostname, ", #clientCertificates=", (clientCertificates == null) ? "0" : clientCertificates.Count.ToString(NumberFormatInfo.InvariantInfo), ", encryptionPolicy=", encryptionPolicy }));
            }
            SSPIWrapper.GetVerifyPackageInfo(GlobalSSPI.SSPISecureChannel, "Microsoft Unified Security Protocol Provider", true);
            if (ComNetOS.IsWin9x && (clientCertificates.Count > 0))
            {
                this.m_Destination = hostname + "+" + clientCertificates.GetHashCode();
            }
            else
            {
                this.m_Destination = hostname;
            }
            this.m_HostName = hostname;
            this.m_ServerMode = serverMode;
            if (serverMode)
            {
                this.m_ProtocolFlags = protocolFlags & SchProtocols.ServerMask;
            }
            else
            {
                this.m_ProtocolFlags = protocolFlags & SchProtocols.ClientMask;
            }
            this.m_ServerCertificate = serverCertificate;
            this.m_ClientCertificates = clientCertificates;
            this.m_RemoteCertRequired = remoteCertRequired;
            this.m_SecurityContext = null;
            this.m_CheckCertRevocation = checkCertRevocationStatus;
            this.m_CheckCertName = checkCertName;
            this.m_CertSelectionDelegate = certSelectionDelegate;
            this.m_RefreshCredentialNeeded = true;
            this.m_EncryptionPolicy = encryptionPolicy;
        }

        [StorePermission(SecurityAction.Assert, Unrestricted=true)]
        private bool AcquireClientCredentials(ref byte[] thumbPrint)
        {
            X509Certificate certificate = null;
            ArrayList list = new ArrayList();
            string[] acceptableIssuers = null;
            bool flag = false;
            if (this.m_CertSelectionDelegate != null)
            {
                if (acceptableIssuers == null)
                {
                    acceptableIssuers = this.GetIssuers();
                }
                X509Certificate2 remoteCertificate = null;
                try
                {
                    X509Certificate2Collection certificates;
                    remoteCertificate = this.GetRemoteCertificate(out certificates);
                    certificate = this.m_CertSelectionDelegate(this.m_HostName, this.ClientCertificates, remoteCertificate, acceptableIssuers);
                }
                finally
                {
                    if (remoteCertificate != null)
                    {
                        remoteCertificate.Reset();
                    }
                }
                if (certificate != null)
                {
                    if (this.m_CredentialsHandle == null)
                    {
                        flag = true;
                    }
                    list.Add(certificate);
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_got_certificate_from_delegate"));
                    }
                }
                else if (this.ClientCertificates.Count == 0)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_no_delegate_and_have_no_client_cert"));
                    }
                    flag = true;
                }
                else if (Logging.On)
                {
                    Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_no_delegate_but_have_client_cert"));
                }
            }
            else if (((this.m_CredentialsHandle == null) && (this.m_ClientCertificates != null)) && (this.m_ClientCertificates.Count > 0))
            {
                certificate = this.ClientCertificates[0];
                flag = true;
                if (certificate != null)
                {
                    list.Add(certificate);
                }
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_attempting_restart_using_cert", new object[] { (certificate == null) ? "null" : certificate.ToString(true) }));
                }
            }
            else if ((this.m_ClientCertificates != null) && (this.m_ClientCertificates.Count > 0))
            {
                if (acceptableIssuers == null)
                {
                    acceptableIssuers = this.GetIssuers();
                }
                if (Logging.On)
                {
                    if ((acceptableIssuers == null) || (acceptableIssuers.Length == 0))
                    {
                        Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_no_issuers_try_all_certs"));
                    }
                    else
                    {
                        Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_server_issuers_look_for_matching_certs", new object[] { acceptableIssuers.Length }));
                    }
                }
                for (int j = 0; j < this.m_ClientCertificates.Count; j++)
                {
                    if ((acceptableIssuers != null) && (acceptableIssuers.Length != 0))
                    {
                        X509Certificate2 certificate3 = null;
                        X509Chain chain = null;
                        try
                        {
                            certificate3 = MakeEx(this.m_ClientCertificates[j]);
                            if (certificate3 == null)
                            {
                                continue;
                            }
                            chain = new X509Chain {
                                ChainPolicy = { RevocationMode = X509RevocationMode.NoCheck, VerificationFlags = X509VerificationFlags.IgnoreInvalidName }
                            };
                            chain.Build(certificate3);
                            bool flag2 = false;
                            if (chain.ChainElements.Count > 0)
                            {
                                for (int k = 0; k < chain.ChainElements.Count; k++)
                                {
                                    string issuer = chain.ChainElements[k].Certificate.Issuer;
                                    flag2 = Array.IndexOf<string>(acceptableIssuers, issuer) != -1;
                                    if (flag2)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (!flag2)
                            {
                                continue;
                            }
                        }
                        finally
                        {
                            if (chain != null)
                            {
                                chain.Reset();
                            }
                            if ((certificate3 != null) && (certificate3 != this.m_ClientCertificates[j]))
                            {
                                certificate3.Reset();
                            }
                        }
                    }
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_selected_cert", new object[] { this.m_ClientCertificates[j].ToString(true) }));
                    }
                    list.Add(this.m_ClientCertificates[j]);
                }
            }
            X509Certificate2 certificate4 = null;
            certificate = null;
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_n_certs_after_filtering", new object[] { list.Count }));
                if (list.Count != 0)
                {
                    Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_finding_matching_certs"));
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                certificate = list[i] as X509Certificate;
                certificate4 = this.EnsurePrivateKey(certificate);
                if (certificate4 != null)
                {
                    break;
                }
                certificate = null;
                certificate4 = null;
            }
            try
            {
                byte[] buffer = (certificate4 == null) ? null : certificate4.GetCertHash();
                SafeFreeCredentials credentials = SslSessionsCache.TryCachedCredential(buffer, this.m_ProtocolFlags, this.m_EncryptionPolicy);
                if ((flag && (credentials == null)) && (certificate4 != null))
                {
                    if (certificate != certificate4)
                    {
                        certificate4.Reset();
                    }
                    buffer = null;
                    certificate4 = null;
                    certificate = null;
                }
                if (credentials != null)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.Web, SR.GetString("net_log_using_cached_credential"));
                    }
                    this.m_CredentialsHandle = credentials;
                    this.m_SelectedClientCertificate = certificate;
                    return true;
                }
                SecureCredential secureCredential = new SecureCredential(4, certificate4, SecureCredential.Flags.NoDefaultCred | SecureCredential.Flags.ValidateManual, this.m_ProtocolFlags, this.m_EncryptionPolicy);
                this.m_CredentialsHandle = this.AcquireCredentialsHandle(CredentialUse.Outbound, ref secureCredential);
                thumbPrint = buffer;
                this.m_SelectedClientCertificate = certificate;
            }
            finally
            {
                if ((certificate4 != null) && (certificate != certificate4))
                {
                    certificate4.Reset();
                }
            }
            return false;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        private SafeFreeCredentials AcquireCredentialsHandle(CredentialUse credUsage, ref SecureCredential secureCredential)
        {
            SafeFreeCredentials credentials;
            try
            {
                using (WindowsIdentity.Impersonate(IntPtr.Zero))
                {
                    credentials = SSPIWrapper.AcquireCredentialsHandle(GlobalSSPI.SSPISecureChannel, "Microsoft Unified Security Protocol Provider", credUsage, secureCredential);
                }
            }
            catch
            {
                credentials = SSPIWrapper.AcquireCredentialsHandle(GlobalSSPI.SSPISecureChannel, "Microsoft Unified Security Protocol Provider", credUsage, secureCredential);
            }
            return credentials;
        }

        [StorePermission(SecurityAction.Assert, Unrestricted=true)]
        private bool AcquireServerCredentials(ref byte[] thumbPrint)
        {
            X509Certificate serverCertificate = null;
            if (this.m_CertSelectionDelegate != null)
            {
                X509CertificateCollection localCertificates = new X509CertificateCollection();
                localCertificates.Add(this.m_ServerCertificate);
                serverCertificate = this.m_CertSelectionDelegate(string.Empty, localCertificates, null, new string[0]);
            }
            else
            {
                serverCertificate = this.m_ServerCertificate;
            }
            if (serverCertificate == null)
            {
                throw new NotSupportedException(SR.GetString("net_ssl_io_no_server_cert"));
            }
            X509Certificate2 certificate2 = this.EnsurePrivateKey(serverCertificate);
            if (certificate2 == null)
            {
                throw new NotSupportedException(SR.GetString("net_ssl_io_no_server_cert"));
            }
            byte[] certHash = certificate2.GetCertHash();
            try
            {
                SafeFreeCredentials credentials = SslSessionsCache.TryCachedCredential(certHash, this.m_ProtocolFlags, this.m_EncryptionPolicy);
                if (credentials != null)
                {
                    this.m_CredentialsHandle = credentials;
                    this.m_ServerCertificate = serverCertificate;
                    return true;
                }
                SecureCredential secureCredential = new SecureCredential(4, certificate2, SecureCredential.Flags.Zero, this.m_ProtocolFlags, this.m_EncryptionPolicy);
                this.m_CredentialsHandle = this.AcquireCredentialsHandle(CredentialUse.Inbound, ref secureCredential);
                thumbPrint = certHash;
                this.m_ServerCertificate = serverCertificate;
            }
            finally
            {
                if (serverCertificate != certificate2)
                {
                    certificate2.Reset();
                }
            }
            return false;
        }

        internal void Close()
        {
            if (this.m_SecurityContext != null)
            {
                this.m_SecurityContext.Close();
            }
            if (this.m_CredentialsHandle != null)
            {
                this.m_CredentialsHandle.Close();
            }
        }

        internal SecurityStatus Decrypt(byte[] payload, ref int offset, ref int count)
        {
            if ((offset < 0) || (offset > ((payload == null) ? 0 : payload.Length)))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (count > ((payload == null) ? 0 : (payload.Length - offset))))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            SecurityBuffer[] input = new SecurityBuffer[] { new SecurityBuffer(payload, offset, count, BufferType.Data), new SecurityBuffer(null, BufferType.Empty), new SecurityBuffer(null, BufferType.Empty), new SecurityBuffer(null, BufferType.Empty) };
            int num = SSPIWrapper.DecryptMessage(GlobalSSPI.SSPISecureChannel, this.m_SecurityContext, input, 0);
            count = 0;
            BufferType data = BufferType.Data;
            if (num != 0)
            {
                data = BufferType.Extra;
            }
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].type == data)
                {
                    count = input[i].size;
                    break;
                }
            }
            if ((data == BufferType.Data) && (count > 0))
            {
                offset += this.m_HeaderSize;
            }
            return (SecurityStatus) num;
        }

        internal SecurityStatus Encrypt(byte[] buffer, int offset, int size, ref byte[] output, out int resultSize)
        {
            byte[] buffer2;
            try
            {
                if ((offset < 0) || (offset > ((buffer == null) ? 0 : buffer.Length)))
                {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if ((size < 0) || (size > ((buffer == null) ? 0 : (buffer.Length - offset))))
                {
                    throw new ArgumentOutOfRangeException("size");
                }
                resultSize = 0;
                buffer2 = new byte[(size + this.m_HeaderSize) + this.m_TrailerSize];
                Buffer.BlockCopy(buffer, offset, buffer2, this.m_HeaderSize, size);
            }
            catch (Exception exception)
            {
                NclUtilities.IsFatal(exception);
                throw;
            }
            SecurityBuffer[] input = new SecurityBuffer[] { new SecurityBuffer(buffer2, 0, this.m_HeaderSize, BufferType.Header), new SecurityBuffer(buffer2, this.m_HeaderSize, size, BufferType.Data), new SecurityBuffer(buffer2, this.m_HeaderSize + size, this.m_TrailerSize, BufferType.Trailer), new SecurityBuffer(null, BufferType.Empty) };
            int num = SSPIWrapper.EncryptMessage(GlobalSSPI.SSPISecureChannel, this.m_SecurityContext, input, 0);
            if (num != 0)
            {
                return (SecurityStatus) num;
            }
            output = buffer2;
            resultSize = (size + this.m_HeaderSize) + input[2].size;
            return SecurityStatus.OK;
        }

        private X509Certificate2 EnsurePrivateKey(X509Certificate certificate)
        {
            if (certificate != null)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_locating_private_key_for_certificate", new object[] { certificate.ToString(true) }));
                }
                try
                {
                    X509Certificate2Collection certificates;
                    X509Certificate2 certificate2 = certificate as X509Certificate2;
                    Type type = certificate.GetType();
                    string findValue = null;
                    if ((type != typeof(X509Certificate2)) && (type != typeof(X509Certificate)))
                    {
                        if (certificate.Handle != IntPtr.Zero)
                        {
                            certificate2 = new X509Certificate2(certificate);
                            findValue = certificate2.GetCertHashString();
                        }
                    }
                    else
                    {
                        findValue = certificate.GetCertHashString();
                    }
                    if (certificate2 != null)
                    {
                        if (certificate2.HasPrivateKey)
                        {
                            if (Logging.On)
                            {
                                Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_cert_is_of_type_2"));
                            }
                            return certificate2;
                        }
                        if (certificate != certificate2)
                        {
                            certificate2.Reset();
                        }
                    }
                    ExceptionHelper.KeyContainerPermissionOpen.Demand();
                    X509Store store = EnsureStoreOpened(this.m_ServerMode);
                    if (store != null)
                    {
                        certificates = store.Certificates.Find(X509FindType.FindByThumbprint, findValue, false);
                        if ((certificates.Count > 0) && (certificates[0].PrivateKey != null))
                        {
                            if (Logging.On)
                            {
                                Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_found_cert_in_store", new object[] { this.m_ServerMode ? "LocalMachine" : "CurrentUser" }));
                            }
                            return certificates[0];
                        }
                    }
                    store = EnsureStoreOpened(!this.m_ServerMode);
                    if (store != null)
                    {
                        certificates = store.Certificates.Find(X509FindType.FindByThumbprint, findValue, false);
                        if ((certificates.Count > 0) && (certificates[0].PrivateKey != null))
                        {
                            if (Logging.On)
                            {
                                Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_found_cert_in_store", new object[] { this.m_ServerMode ? "CurrentUser" : "LocalMachine" }));
                            }
                            return certificates[0];
                        }
                    }
                }
                catch (CryptographicException)
                {
                }
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_did_not_find_cert_in_store"));
                }
            }
            return null;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        internal static X509Store EnsureStoreOpened(bool isMachineStore)
        {
            X509Store store = isMachineStore ? s_MyMachineCertStoreEx : s_MyCertStoreEx;
            if (store == null)
            {
                lock (s_SyncObject)
                {
                    store = isMachineStore ? s_MyMachineCertStoreEx : s_MyCertStoreEx;
                    if (store != null)
                    {
                        return store;
                    }
                    StoreLocation storeLocation = isMachineStore ? StoreLocation.LocalMachine : StoreLocation.CurrentUser;
                    store = new X509Store(StoreName.My, storeLocation);
                    try
                    {
                        try
                        {
                            using (WindowsIdentity.Impersonate(IntPtr.Zero))
                            {
                                store.Open(OpenFlags.OpenExistingOnly);
                            }
                        }
                        catch
                        {
                            throw;
                        }
                        if (isMachineStore)
                        {
                            s_MyMachineCertStoreEx = store;
                            return store;
                        }
                        s_MyCertStoreEx = store;
                        return store;
                    }
                    catch (Exception exception)
                    {
                        if ((exception is CryptographicException) || (exception is SecurityException))
                        {
                            return null;
                        }
                        if (Logging.On)
                        {
                            Logging.PrintError(Logging.Web, SR.GetString("net_log_open_store_failed", new object[] { storeLocation, exception }));
                        }
                        throw;
                    }
                }
            }
            return store;
        }

        private SecurityStatus GenerateToken(byte[] input, int offset, int count, ref byte[] output)
        {
            if ((offset < 0) || (offset > ((input == null) ? 0 : input.Length)))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (count > ((input == null) ? 0 : (input.Length - offset))))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            SecurityBuffer inputBuffer = null;
            SecurityBuffer[] inputBuffers = null;
            if (input != null)
            {
                inputBuffer = new SecurityBuffer(input, offset, count, BufferType.Token);
                inputBuffers = new SecurityBuffer[] { inputBuffer, new SecurityBuffer(null, 0, 0, BufferType.Empty) };
            }
            SecurityBuffer outputBuffer = new SecurityBuffer(null, BufferType.Token);
            int num = 0;
            bool flag = false;
            byte[] thumbPrint = null;
            try
            {
                do
                {
                    thumbPrint = null;
                    if (this.m_RefreshCredentialNeeded)
                    {
                        flag = this.m_ServerMode ? this.AcquireServerCredentials(ref thumbPrint) : this.AcquireClientCredentials(ref thumbPrint);
                    }
                    if (this.m_ServerMode)
                    {
                        num = SSPIWrapper.AcceptSecurityContext(GlobalSSPI.SSPISecureChannel, ref this.m_CredentialsHandle, ref this.m_SecurityContext, (ContextFlags.AcceptStream | ContextFlags.AllocateMemory | ContextFlags.Confidentiality | ContextFlags.SequenceDetect | ContextFlags.ReplayDetect) | (this.m_RemoteCertRequired ? ContextFlags.MutualAuth : ContextFlags.Zero), Endianness.Native, inputBuffer, outputBuffer, ref this.m_Attributes);
                    }
                    else if (inputBuffer == null)
                    {
                        num = SSPIWrapper.InitializeSecurityContext(GlobalSSPI.SSPISecureChannel, ref this.m_CredentialsHandle, ref this.m_SecurityContext, this.m_Destination, ContextFlags.AcceptIdentify | ContextFlags.AllocateMemory | ContextFlags.Confidentiality | ContextFlags.SequenceDetect | ContextFlags.ReplayDetect, Endianness.Native, inputBuffer, outputBuffer, ref this.m_Attributes);
                    }
                    else
                    {
                        num = SSPIWrapper.InitializeSecurityContext(GlobalSSPI.SSPISecureChannel, this.m_CredentialsHandle, ref this.m_SecurityContext, this.m_Destination, ContextFlags.AcceptIdentify | ContextFlags.AllocateMemory | ContextFlags.Confidentiality | ContextFlags.SequenceDetect | ContextFlags.ReplayDetect, Endianness.Native, inputBuffers, outputBuffer, ref this.m_Attributes);
                    }
                }
                while (flag && (this.m_CredentialsHandle == null));
            }
            finally
            {
                if (this.m_RefreshCredentialNeeded)
                {
                    this.m_RefreshCredentialNeeded = false;
                    if (this.m_CredentialsHandle != null)
                    {
                        this.m_CredentialsHandle.Close();
                    }
                    if ((!flag && (this.m_SecurityContext != null)) && (!this.m_SecurityContext.IsInvalid && !this.m_CredentialsHandle.IsInvalid))
                    {
                        SslSessionsCache.CacheCredential(this.m_CredentialsHandle, thumbPrint, this.m_ProtocolFlags, this.m_EncryptionPolicy);
                    }
                }
            }
            output = outputBuffer.token;
            return (SecurityStatus) num;
        }

        internal ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            ChannelBinding binding = null;
            if (this.m_SecurityContext != null)
            {
                binding = SSPIWrapper.QueryContextChannelBinding(GlobalSSPI.SSPISecureChannel, this.m_SecurityContext, (ContextAttribute) kind);
            }
            return binding;
        }

        private unsafe string[] GetIssuers()
        {
            string[] strArray = new string[0];
            if (this.IsValidContext)
            {
                IssuerListInfoEx ex = (IssuerListInfoEx) SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPISecureChannel, this.m_SecurityContext, ContextAttribute.IssuerListInfoEx);
                try
                {
                    if (ex.cIssuers <= 0)
                    {
                        return strArray;
                    }
                    uint cIssuers = ex.cIssuers;
                    strArray = new string[ex.cIssuers];
                    _CERT_CHAIN_ELEMENT* handle = (_CERT_CHAIN_ELEMENT*) ex.aIssuers.DangerousGetHandle();
                    for (int i = 0; i < cIssuers; i++)
                    {
                        _CERT_CHAIN_ELEMENT* _cert_chain_elementPtr2 = handle + i;
                        uint cbSize = _cert_chain_elementPtr2->cbSize;
                        byte* pCertContext = (byte*) _cert_chain_elementPtr2->pCertContext;
                        byte[] encodedDistinguishedName = new byte[cbSize];
                        for (int j = 0; j < cbSize; j++)
                        {
                            encodedDistinguishedName[j] = pCertContext[j];
                        }
                        X500DistinguishedName name = new X500DistinguishedName(encodedDistinguishedName);
                        strArray[i] = name.Name;
                    }
                }
                finally
                {
                    if (ex.aIssuers != null)
                    {
                        ex.aIssuers.Close();
                    }
                }
            }
            return strArray;
        }

        internal X509Certificate2 GetRemoteCertificate(out X509Certificate2Collection remoteCertificateStore)
        {
            remoteCertificateStore = null;
            if (this.m_SecurityContext == null)
            {
                return null;
            }
            X509Certificate2 certificate = null;
            SafeFreeCertContext certContext = null;
            try
            {
                certContext = SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPISecureChannel, this.m_SecurityContext, ContextAttribute.RemoteCertificate) as SafeFreeCertContext;
                if ((certContext != null) && !certContext.IsInvalid)
                {
                    certificate = new X509Certificate2(certContext.DangerousGetHandle());
                }
            }
            finally
            {
                if (certContext != null)
                {
                    remoteCertificateStore = UnmanagedCertificateContext.GetStore(certContext);
                    certContext.Close();
                }
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, SR.GetString("net_log_remote_certificate", new object[] { (certificate == null) ? "null" : certificate.ToString(true) }));
            }
            return certificate;
        }

        private static X509Certificate2 MakeEx(X509Certificate certificate)
        {
            if (certificate.GetType() == typeof(X509Certificate2))
            {
                return (X509Certificate2) certificate;
            }
            X509Certificate2 certificate2 = null;
            try
            {
                if (certificate.Handle != IntPtr.Zero)
                {
                    certificate2 = new X509Certificate2(certificate);
                }
            }
            catch (SecurityException)
            {
            }
            catch (CryptographicException)
            {
            }
            return certificate2;
        }

        internal ProtocolToken NextMessage(byte[] incoming, int offset, int count)
        {
            byte[] output = null;
            SecurityStatus errorCode = this.GenerateToken(incoming, offset, count, ref output);
            if (!this.m_ServerMode && (errorCode == SecurityStatus.CredentialsNeeded))
            {
                this.SetRefreshCredentialNeeded();
                errorCode = this.GenerateToken(incoming, offset, count, ref output);
            }
            return new ProtocolToken(output, errorCode);
        }

        internal void ProcessHandshakeSuccess()
        {
            StreamSizes sizes = SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPISecureChannel, this.m_SecurityContext, ContextAttribute.StreamSizes) as StreamSizes;
            if (sizes != null)
            {
                try
                {
                    this.m_HeaderSize = sizes.header;
                    this.m_TrailerSize = sizes.trailer;
                    this.m_MaxDataSize = sizes.maximumMessage - (this.m_HeaderSize + this.m_TrailerSize);
                }
                catch (Exception exception)
                {
                    NclUtilities.IsFatal(exception);
                    throw;
                }
            }
            this.m_ConnectionInfo = SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPISecureChannel, this.m_SecurityContext, ContextAttribute.ConnectionInfo) as SslConnectionInfo;
        }

        internal void SetRefreshCredentialNeeded()
        {
            this.m_RefreshCredentialNeeded = true;
        }

        [StorePermission(SecurityAction.Assert, Unrestricted=true)]
        internal unsafe bool VerifyRemoteCertificate(RemoteCertValidationCallback remoteCertValidationCallback)
        {
            SslPolicyErrors none = SslPolicyErrors.None;
            bool flag = false;
            X509Chain chain = null;
            X509Certificate2 remoteCertificate = null;
            try
            {
                X509Certificate2Collection certificates;
                remoteCertificate = this.GetRemoteCertificate(out certificates);
                this.m_IsRemoteCertificateAvailable = remoteCertificate != null;
                if (remoteCertificate == null)
                {
                    none |= SslPolicyErrors.RemoteCertificateNotAvailable;
                }
                else
                {
                    chain = new X509Chain {
                        ChainPolicy = { RevocationMode = this.m_CheckCertRevocation ? X509RevocationMode.Online : X509RevocationMode.NoCheck, RevocationFlag = X509RevocationFlag.ExcludeRoot }
                    };
                    if (certificates != null)
                    {
                        chain.ChainPolicy.ExtraStore.AddRange(certificates);
                    }
                    if (!chain.Build(remoteCertificate) && (chain.ChainContext == IntPtr.Zero))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                    if (this.m_CheckCertName)
                    {
                        ChainPolicyParameter cpp = new ChainPolicyParameter {
                            cbSize = ChainPolicyParameter.StructSize,
                            dwFlags = 0
                        };
                        SSL_EXTRA_CERT_CHAIN_POLICY_PARA ssl_extra_cert_chain_policy_para = new SSL_EXTRA_CERT_CHAIN_POLICY_PARA(this.IsServer);
                        cpp.pvExtraPolicyPara = &ssl_extra_cert_chain_policy_para;
                        fixed (char* str = ((char*) this.m_HostName))
                        {
                            char* chPtr = str;
                            ssl_extra_cert_chain_policy_para.pwszServerName = chPtr;
                            cpp.dwFlags |= 0xfbf;
                            SafeFreeCertChain chainContext = new SafeFreeCertChain(chain.ChainContext);
                            if (PolicyWrapper.VerifyChainPolicy(chainContext, ref cpp) == 0x800b010f)
                            {
                                none |= SslPolicyErrors.RemoteCertificateNameMismatch;
                            }
                        }
                    }
                    X509ChainStatus[] chainStatus = chain.ChainStatus;
                    if ((chainStatus != null) && (chainStatus.Length != 0))
                    {
                        none |= SslPolicyErrors.RemoteCertificateChainErrors;
                    }
                }
                if (remoteCertValidationCallback != null)
                {
                    flag = remoteCertValidationCallback(this.m_HostName, remoteCertificate, chain, none);
                }
                else if ((none == SslPolicyErrors.RemoteCertificateNotAvailable) && !this.m_RemoteCertRequired)
                {
                    flag = true;
                }
                else
                {
                    flag = none == SslPolicyErrors.None;
                }
                if (!Logging.On)
                {
                    return flag;
                }
                if (none != SslPolicyErrors.None)
                {
                    Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_remote_cert_has_errors"));
                    if ((none & SslPolicyErrors.RemoteCertificateNotAvailable) != SslPolicyErrors.None)
                    {
                        Logging.PrintInfo(Logging.Web, this, "\t" + SR.GetString("net_log_remote_cert_not_available"));
                    }
                    if ((none & SslPolicyErrors.RemoteCertificateNameMismatch) != SslPolicyErrors.None)
                    {
                        Logging.PrintInfo(Logging.Web, this, "\t" + SR.GetString("net_log_remote_cert_name_mismatch"));
                    }
                    if ((none & SslPolicyErrors.RemoteCertificateChainErrors) != SslPolicyErrors.None)
                    {
                        foreach (X509ChainStatus status in chain.ChainStatus)
                        {
                            Logging.PrintInfo(Logging.Web, this, "\t" + status.StatusInformation);
                        }
                    }
                }
                if (flag)
                {
                    if (remoteCertValidationCallback != null)
                    {
                        Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_remote_cert_user_declared_valid"));
                        return flag;
                    }
                    Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_remote_cert_has_no_errors"));
                    return flag;
                }
                if (remoteCertValidationCallback != null)
                {
                    Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_remote_cert_user_declared_invalid"));
                }
            }
            finally
            {
                if (chain != null)
                {
                    chain.Reset();
                }
                if (remoteCertificate != null)
                {
                    remoteCertificate.Reset();
                }
            }
            return flag;
        }

        internal bool CheckCertRevocationStatus
        {
            get
            {
                return this.m_CheckCertRevocation;
            }
        }

        internal X509CertificateCollection ClientCertificates
        {
            get
            {
                return this.m_ClientCertificates;
            }
        }

        internal SslConnectionInfo ConnectionInfo
        {
            get
            {
                return this.m_ConnectionInfo;
            }
        }

        internal int HeaderSize
        {
            get
            {
                return this.m_HeaderSize;
            }
        }

        internal bool IsRemoteCertificateAvailable
        {
            get
            {
                return this.m_IsRemoteCertificateAvailable;
            }
        }

        internal bool IsServer
        {
            get
            {
                return this.m_ServerMode;
            }
        }

        internal bool IsValidContext
        {
            get
            {
                return ((this.m_SecurityContext != null) && !this.m_SecurityContext.IsInvalid);
            }
        }

        internal X509Certificate LocalClientCertificate
        {
            get
            {
                return this.m_SelectedClientCertificate;
            }
        }

        internal X509Certificate LocalServerCertificate
        {
            get
            {
                return this.m_ServerCertificate;
            }
        }

        internal int MaxDataSize
        {
            get
            {
                return this.m_MaxDataSize;
            }
        }

        internal bool RemoteCertRequired
        {
            get
            {
                return this.m_RemoteCertRequired;
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

