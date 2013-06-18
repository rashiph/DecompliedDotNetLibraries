namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal sealed class WindowsSspiNegotiation : ISspiNegotiation, IDisposable
    {
        private bool allowNtlm;
        private string clientPackageName;
        private SspiContextFlags contextFlags;
        private SafeFreeCredentials credentialsHandle;
        private const int DefaultMaxPromptAttempts = 1;
        private bool disposed;
        private bool doMutualAuth;
        private TokenImpersonationLevel impersonationLevel;
        private bool interactiveNegoLogonEnabled;
        private bool isCompleted;
        private bool isServer;
        private System.IdentityModel.LifeSpan lifespan;
        private int MaxPromptAttempts;
        private string protocolName;
        private bool saveClientCredentialsOnSspiUi;
        private SafeDeleteContext securityContext;
        private string servicePrincipalName;
        private SecSizes sizes;
        private object syncObject;
        private int tokenSize;

        internal WindowsSspiNegotiation(string package, SafeFreeCredentials credentialsHandle, string defaultServiceBinding) : this(true, package, credentialsHandle, TokenImpersonationLevel.Delegation, defaultServiceBinding, false, false, true)
        {
        }

        internal WindowsSspiNegotiation(string package, SafeFreeCredentials credentialsHandle, TokenImpersonationLevel impersonationLevel, string servicePrincipalName, bool doMutualAuth, bool interactiveLogonEnabled, bool ntlmEnabled) : this(false, package, credentialsHandle, impersonationLevel, servicePrincipalName, doMutualAuth, interactiveLogonEnabled, ntlmEnabled)
        {
        }

        private WindowsSspiNegotiation(bool isServer, string package, SafeFreeCredentials credentialsHandle, TokenImpersonationLevel impersonationLevel, string servicePrincipalName, bool doMutualAuth, bool interactiveLogonEnabled, bool ntlmEnabled)
        {
            this.syncObject = new object();
            this.interactiveNegoLogonEnabled = true;
            this.saveClientCredentialsOnSspiUi = true;
            this.tokenSize = SspiWrapper.GetVerifyPackageInfo(package).MaxToken;
            this.isServer = isServer;
            this.servicePrincipalName = servicePrincipalName;
            this.securityContext = null;
            if (isServer)
            {
                this.impersonationLevel = TokenImpersonationLevel.Delegation;
                this.doMutualAuth = false;
            }
            else
            {
                this.impersonationLevel = impersonationLevel;
                this.doMutualAuth = doMutualAuth;
                this.interactiveNegoLogonEnabled = interactiveLogonEnabled;
                this.clientPackageName = package;
                this.allowNtlm = ntlmEnabled;
            }
            this.credentialsHandle = credentialsHandle;
        }

        internal void CloseContext()
        {
            this.ThrowIfDisposed();
            try
            {
                if (this.securityContext != null)
                {
                    this.securityContext.Close();
                }
            }
            finally
            {
                this.securityContext = null;
            }
        }

        public byte[] Decrypt(byte[] encryptedContent)
        {
            if (encryptedContent == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encryptedContent");
            }
            this.ThrowIfDisposed();
            SecurityBuffer[] input = new SecurityBuffer[] { new SecurityBuffer(encryptedContent, 0, encryptedContent.Length, BufferType.Stream), new SecurityBuffer(0, BufferType.Data) };
            int error = SspiWrapper.DecryptMessage(this.securityContext, input, 0, true);
            if (error != 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].type == BufferType.Data)
                {
                    return input[i].token;
                }
            }
            this.OnBadData();
            return null;
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
                    if (disposing)
                    {
                        this.CloseContext();
                    }
                    this.protocolName = null;
                    this.servicePrincipalName = null;
                    this.sizes = null;
                    this.disposed = true;
                }
            }
        }

        public byte[] Encrypt(byte[] input)
        {
            if (input == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("input");
            }
            this.ThrowIfDisposed();
            SecurityBuffer[] bufferArray = new SecurityBuffer[3];
            byte[] data = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(this.SecuritySizes.SecurityTrailer);
            bufferArray[0] = new SecurityBuffer(data, 0, data.Length, BufferType.Token);
            byte[] dst = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(input.Length);
            Buffer.BlockCopy(input, 0, dst, 0, input.Length);
            bufferArray[1] = new SecurityBuffer(dst, 0, dst.Length, BufferType.Data);
            byte[] buffer3 = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(this.SecuritySizes.BlockSize);
            bufferArray[2] = new SecurityBuffer(buffer3, 0, buffer3.Length, BufferType.Padding);
            int error = SspiWrapper.EncryptMessage(this.securityContext, bufferArray, 0);
            if (error != 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            int count = 0;
            int size = 0;
            for (int i = 0; i < bufferArray.Length; i++)
            {
                if (bufferArray[i].type == BufferType.Token)
                {
                    count = bufferArray[i].size;
                }
                else if (bufferArray[i].type == BufferType.Padding)
                {
                    size = bufferArray[i].size;
                }
            }
            byte[] buffer4 = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray((count + dst.Length) + size);
            Buffer.BlockCopy(data, 0, buffer4, 0, count);
            Buffer.BlockCopy(dst, 0, buffer4, count, dst.Length);
            Buffer.BlockCopy(buffer3, 0, buffer4, count + dst.Length, size);
            return buffer4;
        }

        internal SafeCloseHandle GetContextToken()
        {
            SafeCloseHandle handle;
            if (!this.IsValidContext)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(-2146893055));
            }
            SecurityStatus status = (SecurityStatus) SspiWrapper.QuerySecurityContextToken(this.securityContext, out handle);
            if (status != SecurityStatus.OK)
            {
                Utility.CloseInvalidOutSafeHandle(handle);
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int) status));
            }
            return handle;
        }

        public byte[] GetOutgoingBlob(byte[] incomingBlob, ChannelBinding channelbinding, ExtendedProtectionPolicy protectionPolicy)
        {
            this.ThrowIfDisposed();
            int error = 0;
            SspiContextFlags inFlags = SspiContextFlags.Confidentiality | SspiContextFlags.SequenceDetect | SspiContextFlags.ReplayDetect;
            if (this.doMutualAuth)
            {
                inFlags |= SspiContextFlags.MutualAuth;
            }
            if (this.impersonationLevel == TokenImpersonationLevel.Delegation)
            {
                inFlags |= SspiContextFlags.Delegate;
            }
            else if (!this.isServer && (this.impersonationLevel == TokenImpersonationLevel.Identification))
            {
                inFlags |= SspiContextFlags.InitIdentify;
            }
            else if (!this.isServer && (this.impersonationLevel == TokenImpersonationLevel.Anonymous))
            {
                inFlags |= SspiContextFlags.InitAnonymous;
            }
            ExtendedProtectionPolicyHelper helper = new ExtendedProtectionPolicyHelper(channelbinding, protectionPolicy);
            if (this.isServer)
            {
                if (((helper.PolicyEnforcement == PolicyEnforcement.Always) && (helper.ChannelBinding == null)) && (helper.ProtectionScenario != ProtectionScenario.TrustedProxy))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.ServiceModel.SR.GetString("SecurityChannelBindingMissing")));
                }
                if (helper.PolicyEnforcement == PolicyEnforcement.WhenSupported)
                {
                    inFlags |= SspiContextFlags.ChannelBindingAllowMissingBindings;
                }
                if (helper.ProtectionScenario == ProtectionScenario.TrustedProxy)
                {
                    inFlags |= SspiContextFlags.ChannelBindingProxyBindings;
                }
            }
            List<SecurityBuffer> list = new List<SecurityBuffer>(2);
            if (incomingBlob != null)
            {
                list.Add(new SecurityBuffer(incomingBlob, BufferType.Token));
            }
            if (this.isServer)
            {
                if (helper.ShouldAddChannelBindingToASC())
                {
                    list.Add(new SecurityBuffer(helper.ChannelBinding));
                }
            }
            else if (helper.ChannelBinding != null)
            {
                list.Add(new SecurityBuffer(helper.ChannelBinding));
            }
            SecurityBuffer[] inputBuffers = null;
            if (list.Count > 0)
            {
                inputBuffers = list.ToArray();
            }
            SecurityBuffer outputBuffer = new SecurityBuffer(this.tokenSize, BufferType.Token);
            if (!this.isServer)
            {
                error = SspiWrapper.InitializeSecurityContext(this.credentialsHandle, ref this.securityContext, this.servicePrincipalName, inFlags, Endianness.Network, inputBuffers, outputBuffer, ref this.contextFlags);
            }
            else
            {
                bool flag = this.securityContext == null;
                SspiContextFlags contextFlags = this.contextFlags;
                error = SspiWrapper.AcceptSecurityContext(this.credentialsHandle, ref this.securityContext, inFlags, Endianness.Network, inputBuffers, outputBuffer, ref this.contextFlags);
                if ((error == -2146893048) && !flag)
                {
                    this.contextFlags = contextFlags;
                    this.CloseContext();
                    error = SspiWrapper.AcceptSecurityContext(this.credentialsHandle, ref this.securityContext, inFlags, Endianness.Network, inputBuffers, outputBuffer, ref this.contextFlags);
                }
            }
            if ((error & -2147483648) != 0)
            {
                if (((!this.isServer && this.interactiveNegoLogonEnabled) && (System.ServiceModel.Security.SecurityUtils.IsOSGreaterThanOrEqualToWin7() && SspiWrapper.IsSspiPromptingNeeded((uint) error))) && SspiWrapper.IsNegotiateExPackagePresent())
                {
                    if (this.MaxPromptAttempts >= 1)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, System.ServiceModel.SR.GetString("InvalidClientCredentials")));
                    }
                    IntPtr zero = IntPtr.Zero;
                    uint num2 = SspiWrapper.SspiPromptForCredential(this.servicePrincipalName, this.clientPackageName, out zero, ref this.saveClientCredentialsOnSspiUi);
                    if (num2 == 0)
                    {
                        IntPtr ppNewAuthIdentity = IntPtr.Zero;
                        if (!this.allowNtlm)
                        {
                            UnsafeNativeMethods.SspiExcludePackage(zero, "NTLM", out ppNewAuthIdentity);
                        }
                        else
                        {
                            ppNewAuthIdentity = zero;
                        }
                        this.credentialsHandle = SspiWrapper.AcquireCredentialsHandle(this.clientPackageName, CredentialUse.Outbound, ref ppNewAuthIdentity);
                        if (IntPtr.Zero != ppNewAuthIdentity)
                        {
                            UnsafeNativeMethods.SspiFreeAuthIdentity(ppNewAuthIdentity);
                        }
                        this.CloseContext();
                        this.MaxPromptAttempts++;
                        return this.GetOutgoingBlob(null, channelbinding, protectionPolicy);
                    }
                    if (IntPtr.Zero != zero)
                    {
                        UnsafeNativeMethods.SspiFreeAuthIdentity(zero);
                    }
                    this.CloseContext();
                    this.isCompleted = true;
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int) num2, System.ServiceModel.SR.GetString("SspiErrorOrInvalidClientCredentials")));
                }
                this.CloseContext();
                this.isCompleted = true;
                if (this.isServer || (((error != -2146893042) && (error != -2146893053)) && (error != -2146893022)))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, System.ServiceModel.SR.GetString("InvalidSspiNegotiation")));
                }
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, System.ServiceModel.SR.GetString("IncorrectSpnOrUpnSpecified", new object[] { this.servicePrincipalName })));
            }
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                if (this.isServer)
                {
                    SecurityTraceRecordHelper.TraceServiceOutgoingSpnego(this);
                }
                else
                {
                    SecurityTraceRecordHelper.TraceClientOutgoingSpnego(this);
                }
            }
            if (error == 0)
            {
                this.isCompleted = true;
                if ((this.isServer && ((this.contextFlags & SspiContextFlags.AcceptAnonymous) == SspiContextFlags.Zero)) && ((string.Compare(this.ProtocolName, "Kerberos", StringComparison.OrdinalIgnoreCase) != 0) && helper.ShouldCheckServiceBinding))
                {
                    helper.CheckServiceBinding(this.securityContext, this.servicePrincipalName);
                }
            }
            return outputBuffer.token;
        }

        public string GetRemoteIdentityName()
        {
            if (!this.isServer)
            {
                return this.servicePrincipalName;
            }
            if (this.IsValidContext)
            {
                using (SafeCloseHandle handle = this.GetContextToken())
                {
                    using (WindowsIdentity identity = new WindowsIdentity(handle.DangerousGetHandle(), this.ProtocolName))
                    {
                        return identity.Name;
                    }
                }
            }
            return string.Empty;
        }

        public void ImpersonateContext()
        {
            this.ThrowIfDisposed();
            if (!this.IsValidContext)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(-2146893055));
            }
            SspiWrapper.ImpersonateSecurityContext(this.securityContext);
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

        public DateTime ExpirationTimeUtc
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.LifeSpan == null)
                {
                    return System.ServiceModel.Security.SecurityUtils.MaxUtcDateTime;
                }
                return this.LifeSpan.ExpiryTimeUtc;
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

        public bool IsDelegationFlag
        {
            get
            {
                this.ThrowIfDisposed();
                return ((this.contextFlags & SspiContextFlags.Delegate) != SspiContextFlags.Zero);
            }
        }

        public bool IsIdentifyFlag
        {
            get
            {
                this.ThrowIfDisposed();
                return ((this.contextFlags & (this.isServer ? SspiContextFlags.AcceptIdentify : SspiContextFlags.InitIdentify)) != SspiContextFlags.Zero);
            }
        }

        public bool IsMutualAuthFlag
        {
            get
            {
                this.ThrowIfDisposed();
                return ((this.contextFlags & SspiContextFlags.MutualAuth) != SspiContextFlags.Zero);
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
                return "http://schemas.xmlsoap.org/2005/02/trust/spnego#GSS_Wrap";
            }
        }

        public System.IdentityModel.LifeSpan LifeSpan
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.lifespan != null)
                {
                    return this.lifespan;
                }
                System.IdentityModel.LifeSpan span = (System.IdentityModel.LifeSpan) SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.Lifespan);
                if (this.IsCompleted)
                {
                    this.lifespan = span;
                }
                return span;
            }
        }

        public string ProtocolName
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.protocolName != null)
                {
                    return this.protocolName;
                }
                NegotiationInfoClass class2 = SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.NegotiationInfo) as NegotiationInfoClass;
                if (this.IsCompleted)
                {
                    this.protocolName = class2.AuthenticationPackage;
                }
                return class2.AuthenticationPackage;
            }
        }

        private SecSizes SecuritySizes
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.sizes != null)
                {
                    return this.sizes;
                }
                SecSizes sizes = (SecSizes) SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.Sizes);
                if (this.IsCompleted)
                {
                    this.sizes = sizes;
                }
                return sizes;
            }
        }

        public string ServicePrincipalName
        {
            get
            {
                this.ThrowIfDisposed();
                return this.servicePrincipalName;
            }
        }
    }
}

