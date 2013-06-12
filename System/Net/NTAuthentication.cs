namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    internal class NTAuthentication
    {
        private System.Security.Authentication.ExtendedProtection.ChannelBinding m_ChannelBinding;
        private string m_ClientSpecifiedSpn;
        private ContextFlags m_ContextFlags;
        private SafeFreeCredentials m_CredentialsHandle;
        private bool m_IsCompleted;
        private bool m_IsServer;
        private string m_LastProtocolName;
        private string m_Package;
        private string m_ProtocolName;
        private ContextFlags m_RequestedContextFlags;
        private SafeDeleteContext m_SecurityContext;
        private SecSizes m_Sizes;
        private string m_Spn;
        private int m_TokenSize;
        private string m_UniqueUserId;
        private static ContextCallback s_InitializeCallback = new ContextCallback(NTAuthentication.InitializeCallback);
        private static int s_UniqueGroupId = 1;

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        internal NTAuthentication(bool isServer, string package, string spn, ContextFlags requestedContextFlags, System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding)
        {
            try
            {
                using (WindowsIdentity.Impersonate(IntPtr.Zero))
                {
                    this.Initialize(isServer, package, SystemNetworkCredential.defaultCredential, spn, requestedContextFlags, channelBinding);
                }
            }
            catch
            {
                throw;
            }
        }

        internal NTAuthentication(string package, NetworkCredential networkCredential, string spn, WebRequest request, System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding) : this(false, package, networkCredential, spn, GetHttpContextFlags(request), request.GetWritingContext(), channelBinding)
        {
            if ((package == "NTLM") || (package == "Negotiate"))
            {
                this.m_UniqueUserId = Interlocked.Increment(ref s_UniqueGroupId).ToString(NumberFormatInfo.InvariantInfo) + this.m_UniqueUserId;
            }
        }

        internal NTAuthentication(bool isServer, string package, NetworkCredential credential, string spn, ContextFlags requestedContextFlags, System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding)
        {
            this.Initialize(isServer, package, credential, spn, requestedContextFlags, channelBinding);
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        internal NTAuthentication(bool isServer, string package, NetworkCredential credential, string spn, ContextFlags requestedContextFlags, ContextAwareResult context, System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding)
        {
            if ((credential is SystemNetworkCredential) && ComNetOS.IsWinNt)
            {
                WindowsIdentity identity = (context == null) ? null : context.Identity;
                try
                {
                    IDisposable disposable = (identity == null) ? null : identity.Impersonate();
                    if (disposable != null)
                    {
                        using (disposable)
                        {
                            this.Initialize(isServer, package, credential, spn, requestedContextFlags, channelBinding);
                            return;
                        }
                    }
                    ExecutionContext executionContext = (context == null) ? null : context.ContextCopy;
                    if (executionContext == null)
                    {
                        this.Initialize(isServer, package, credential, spn, requestedContextFlags, channelBinding);
                    }
                    else
                    {
                        ExecutionContext.Run(executionContext, s_InitializeCallback, new InitializeCallbackContext(this, isServer, package, credential, spn, requestedContextFlags, channelBinding));
                    }
                    return;
                }
                catch
                {
                    throw;
                }
            }
            this.Initialize(isServer, package, credential, spn, requestedContextFlags, channelBinding);
        }

        internal void CloseContext()
        {
            if ((this.m_SecurityContext != null) && !this.m_SecurityContext.IsClosed)
            {
                this.m_SecurityContext.Close();
            }
        }

        internal int Decrypt(byte[] payload, int offset, int count, out int newOffset, uint expectedSeqNumber)
        {
            int num;
            if ((offset < 0) || (offset > ((payload == null) ? 0 : payload.Length)))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (count > ((payload == null) ? 0 : (payload.Length - offset))))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.IsNTLM)
            {
                return this.DecryptNtlm(payload, offset, count, out newOffset, expectedSeqNumber);
            }
            SecurityBuffer[] input = new SecurityBuffer[] { new SecurityBuffer(payload, offset, count, BufferType.Stream), new SecurityBuffer(0, BufferType.Data) };
            if (this.IsConfidentialityFlag)
            {
                num = SSPIWrapper.DecryptMessage(GlobalSSPI.SSPIAuth, this.m_SecurityContext, input, expectedSeqNumber);
            }
            else
            {
                num = SSPIWrapper.VerifySignature(GlobalSSPI.SSPIAuth, this.m_SecurityContext, input, expectedSeqNumber);
            }
            if (num != 0)
            {
                throw new Win32Exception(num);
            }
            if (input[1].type != BufferType.Data)
            {
                throw new InternalException();
            }
            newOffset = input[1].offset;
            return input[1].size;
        }

        private int DecryptNtlm(byte[] payload, int offset, int count, out int newOffset, uint expectedSeqNumber)
        {
            int num;
            if (count < 0x10)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            SecurityBuffer[] input = new SecurityBuffer[] { new SecurityBuffer(payload, offset, 0x10, BufferType.Token), new SecurityBuffer(payload, offset + 0x10, count - 0x10, BufferType.Data) };
            BufferType data = BufferType.Data;
            if (this.IsConfidentialityFlag)
            {
                num = SSPIWrapper.DecryptMessage(GlobalSSPI.SSPIAuth, this.m_SecurityContext, input, expectedSeqNumber);
            }
            else
            {
                data |= BufferType.ReadOnlyFlag;
                input[1].type = data;
                num = SSPIWrapper.VerifySignature(GlobalSSPI.SSPIAuth, this.m_SecurityContext, input, expectedSeqNumber);
            }
            if (num != 0)
            {
                throw new Win32Exception(num);
            }
            if (input[1].type != data)
            {
                throw new InternalException();
            }
            newOffset = input[1].offset;
            return input[1].size;
        }

        internal int Encrypt(byte[] buffer, int offset, int count, ref byte[] output, uint sequenceNumber)
        {
            int num3;
            SecSizes sizes = this.Sizes;
            try
            {
                int num = (0x7ffffffb - sizes.BlockSize) - sizes.SecurityTrailer;
                if ((count > num) || (count < 0))
                {
                    throw new ArgumentOutOfRangeException("count", SR.GetString("net_io_out_range", new object[] { num }));
                }
            }
            catch (Exception exception)
            {
                NclUtilities.IsFatal(exception);
                throw;
            }
            int size = (count + sizes.SecurityTrailer) + sizes.BlockSize;
            if ((output == null) || (output.Length < (size + 4)))
            {
                output = new byte[size + 4];
            }
            Buffer.BlockCopy(buffer, offset, output, 4 + sizes.SecurityTrailer, count);
            SecurityBuffer[] input = new SecurityBuffer[] { new SecurityBuffer(output, 4, sizes.SecurityTrailer, BufferType.Token), new SecurityBuffer(output, 4 + sizes.SecurityTrailer, count, BufferType.Data), new SecurityBuffer(output, (4 + sizes.SecurityTrailer) + count, sizes.BlockSize, BufferType.Padding) };
            if (this.IsConfidentialityFlag)
            {
                num3 = SSPIWrapper.EncryptMessage(GlobalSSPI.SSPIAuth, this.m_SecurityContext, input, sequenceNumber);
            }
            else
            {
                if (this.IsNTLM)
                {
                    SecurityBuffer buffer1 = input[1];
                    buffer1.type |= BufferType.ReadOnlyFlag;
                }
                num3 = SSPIWrapper.MakeSignature(GlobalSSPI.SSPIAuth, this.m_SecurityContext, input, 0);
            }
            if (num3 != 0)
            {
                throw new Win32Exception(num3);
            }
            size = input[0].size;
            bool flag = false;
            if (size != sizes.SecurityTrailer)
            {
                flag = true;
                Buffer.BlockCopy(output, input[1].offset, output, 4 + size, input[1].size);
            }
            size += input[1].size;
            if ((input[2].size != 0) && (flag || (size != (count + sizes.SecurityTrailer))))
            {
                Buffer.BlockCopy(output, input[2].offset, output, 4 + size, input[2].size);
            }
            size += input[2].size;
            output[0] = (byte) (size & 0xff);
            output[1] = (byte) ((size >> 8) & 0xff);
            output[2] = (byte) ((size >> 0x10) & 0xff);
            output[3] = (byte) ((size >> 0x18) & 0xff);
            return (size + 4);
        }

        private string GetClientSpecifiedSpn()
        {
            return (SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, this.m_SecurityContext, ContextAttribute.ClientSpecifiedSpn) as string);
        }

        internal SafeCloseHandle GetContextToken()
        {
            SecurityStatus status;
            SafeCloseHandle contextToken = this.GetContextToken(out status);
            if (status != SecurityStatus.OK)
            {
                throw new Win32Exception((int) status);
            }
            return contextToken;
        }

        internal SafeCloseHandle GetContextToken(out SecurityStatus status)
        {
            if (!this.IsValidContext)
            {
                throw new Win32Exception(-2146893055);
            }
            SafeCloseHandle token = null;
            status = (SecurityStatus) SSPIWrapper.QuerySecurityContextToken(GlobalSSPI.SSPIAuth, this.m_SecurityContext, out token);
            return token;
        }

        private static ContextFlags GetHttpContextFlags(WebRequest request)
        {
            ContextFlags connection = ContextFlags.Connection;
            if (request.ImpersonationLevel == TokenImpersonationLevel.Anonymous)
            {
                throw new NotSupportedException(SR.GetString("net_auth_no_anonymous_support"));
            }
            if (request.ImpersonationLevel == TokenImpersonationLevel.Identification)
            {
                connection |= ContextFlags.AcceptIntegrity;
            }
            else if (request.ImpersonationLevel == TokenImpersonationLevel.Delegation)
            {
                connection |= ContextFlags.Delegate;
            }
            if ((request.AuthenticationLevel != AuthenticationLevel.MutualAuthRequested) && (request.AuthenticationLevel != AuthenticationLevel.MutualAuthRequired))
            {
                return connection;
            }
            return (connection | ContextFlags.MutualAuth);
        }

        internal string GetOutgoingBlob(string incomingBlob)
        {
            byte[] buffer = null;
            if ((incomingBlob != null) && (incomingBlob.Length > 0))
            {
                buffer = Convert.FromBase64String(incomingBlob);
            }
            byte[] inArray = null;
            if ((this.IsValidContext || this.IsCompleted) && (buffer == null))
            {
                this.m_IsCompleted = true;
            }
            else
            {
                SecurityStatus status;
                inArray = this.GetOutgoingBlob(buffer, true, out status);
            }
            string str = null;
            if ((inArray != null) && (inArray.Length > 0))
            {
                str = Convert.ToBase64String(inArray);
            }
            if (this.IsCompleted)
            {
                string protocolName = this.ProtocolName;
                this.CloseContext();
            }
            return str;
        }

        internal byte[] GetOutgoingBlob(byte[] incomingBlob, bool throwOnError, out SecurityStatus statusCode)
        {
            List<SecurityBuffer> list = new List<SecurityBuffer>(2);
            if (incomingBlob != null)
            {
                list.Add(new SecurityBuffer(incomingBlob, BufferType.Token));
            }
            if (this.m_ChannelBinding != null)
            {
                list.Add(new SecurityBuffer(this.m_ChannelBinding));
            }
            SecurityBuffer[] inputBuffers = null;
            if (list.Count > 0)
            {
                inputBuffers = list.ToArray();
            }
            SecurityBuffer outputBuffer = new SecurityBuffer(this.m_TokenSize, BufferType.Token);
            bool flag = this.m_SecurityContext == null;
            try
            {
                if (!this.m_IsServer)
                {
                    statusCode = (SecurityStatus) SSPIWrapper.InitializeSecurityContext(GlobalSSPI.SSPIAuth, this.m_CredentialsHandle, ref this.m_SecurityContext, this.m_Spn, this.m_RequestedContextFlags, Endianness.Network, inputBuffers, outputBuffer, ref this.m_ContextFlags);
                    if (statusCode == SecurityStatus.CompleteNeeded)
                    {
                        SecurityBuffer[] bufferArray2 = new SecurityBuffer[] { outputBuffer };
                        statusCode = (SecurityStatus) SSPIWrapper.CompleteAuthToken(GlobalSSPI.SSPIAuth, ref this.m_SecurityContext, bufferArray2);
                        outputBuffer.token = null;
                    }
                }
                else
                {
                    statusCode = (SecurityStatus) SSPIWrapper.AcceptSecurityContext(GlobalSSPI.SSPIAuth, this.m_CredentialsHandle, ref this.m_SecurityContext, this.m_RequestedContextFlags, Endianness.Network, inputBuffers, outputBuffer, ref this.m_ContextFlags);
                }
            }
            finally
            {
                if (flag && (this.m_CredentialsHandle != null))
                {
                    this.m_CredentialsHandle.Close();
                }
            }
            if ((statusCode & ((SecurityStatus) (-2147483648))) != SecurityStatus.OK)
            {
                this.CloseContext();
                this.m_IsCompleted = true;
                if (throwOnError)
                {
                    Win32Exception exception = new Win32Exception((int) statusCode);
                    throw exception;
                }
                return null;
            }
            if (flag && (this.m_CredentialsHandle != null))
            {
                SSPIHandleCache.CacheCredential(this.m_CredentialsHandle);
            }
            if (statusCode == SecurityStatus.OK)
            {
                this.m_IsCompleted = true;
            }
            return outputBuffer.token;
        }

        internal string GetOutgoingDigestBlob(string incomingBlob, string requestMethod, string requestedUri, string realm, bool isClientPreAuth, bool throwOnError, out SecurityStatus statusCode)
        {
            SecurityBuffer[] inputBuffers = null;
            SecurityBuffer outputBuffer = new SecurityBuffer(this.m_TokenSize, isClientPreAuth ? BufferType.Parameters : BufferType.Token);
            bool flag = this.m_SecurityContext == null;
            try
            {
                if (!this.m_IsServer)
                {
                    if (!isClientPreAuth)
                    {
                        if (incomingBlob != null)
                        {
                            List<SecurityBuffer> list = new List<SecurityBuffer>(5) {
                                new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(incomingBlob), 2),
                                new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(requestMethod), 3),
                                new SecurityBuffer(null, 3),
                                new SecurityBuffer(Encoding.Unicode.GetBytes(this.m_Spn), 0x10)
                            };
                            if (this.m_ChannelBinding != null)
                            {
                                list.Add(new SecurityBuffer(this.m_ChannelBinding));
                            }
                            inputBuffers = list.ToArray();
                        }
                        statusCode = (SecurityStatus) SSPIWrapper.InitializeSecurityContext(GlobalSSPI.SSPIAuth, this.m_CredentialsHandle, ref this.m_SecurityContext, requestedUri, this.m_RequestedContextFlags, Endianness.Network, inputBuffers, outputBuffer, ref this.m_ContextFlags);
                    }
                    else
                    {
                        statusCode = SecurityStatus.OK;
                    }
                }
                else
                {
                    List<SecurityBuffer> list2 = new List<SecurityBuffer>(6) {
                        (incomingBlob == null) ? new SecurityBuffer(0, BufferType.Token) : new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(incomingBlob), BufferType.Token),
                        (requestMethod == null) ? new SecurityBuffer(0, BufferType.Parameters) : new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(requestMethod), BufferType.Parameters),
                        (requestedUri == null) ? new SecurityBuffer(0, BufferType.Parameters) : new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(requestedUri), BufferType.Parameters),
                        new SecurityBuffer(0, BufferType.Parameters),
                        (realm == null) ? new SecurityBuffer(0, BufferType.Parameters) : new SecurityBuffer(Encoding.Unicode.GetBytes(realm), BufferType.Parameters)
                    };
                    if (this.m_ChannelBinding != null)
                    {
                        list2.Add(new SecurityBuffer(this.m_ChannelBinding));
                    }
                    inputBuffers = list2.ToArray();
                    statusCode = (SecurityStatus) SSPIWrapper.AcceptSecurityContext(GlobalSSPI.SSPIAuth, this.m_CredentialsHandle, ref this.m_SecurityContext, this.m_RequestedContextFlags, Endianness.Network, inputBuffers, outputBuffer, ref this.m_ContextFlags);
                    if (statusCode == SecurityStatus.CompleteNeeded)
                    {
                        inputBuffers[4] = outputBuffer;
                        statusCode = (SecurityStatus) SSPIWrapper.CompleteAuthToken(GlobalSSPI.SSPIAuth, ref this.m_SecurityContext, inputBuffers);
                        outputBuffer.token = null;
                    }
                }
            }
            finally
            {
                if (flag && (this.m_CredentialsHandle != null))
                {
                    this.m_CredentialsHandle.Close();
                }
            }
            if ((statusCode & ((SecurityStatus) (-2147483648))) != SecurityStatus.OK)
            {
                this.CloseContext();
                if (throwOnError)
                {
                    Win32Exception exception = new Win32Exception((int) statusCode);
                    throw exception;
                }
                return null;
            }
            if (flag && (this.m_CredentialsHandle != null))
            {
                SSPIHandleCache.CacheCredential(this.m_CredentialsHandle);
            }
            if (statusCode == SecurityStatus.OK)
            {
                this.m_IsCompleted = true;
            }
            byte[] token = outputBuffer.token;
            string str = null;
            if ((token != null) && (token.Length > 0))
            {
                str = WebHeaderCollection.HeaderEncoding.GetString(token, 0, outputBuffer.size);
            }
            return str;
        }

        private void Initialize(bool isServer, string package, NetworkCredential credential, string spn, ContextFlags requestedContextFlags, System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding)
        {
            this.m_TokenSize = SSPIWrapper.GetVerifyPackageInfo(GlobalSSPI.SSPIAuth, package, true).MaxToken;
            this.m_IsServer = isServer;
            this.m_Spn = spn;
            this.m_SecurityContext = null;
            this.m_RequestedContextFlags = requestedContextFlags;
            this.m_Package = package;
            this.m_ChannelBinding = channelBinding;
            if (credential is SystemNetworkCredential)
            {
                this.m_CredentialsHandle = SSPIWrapper.AcquireDefaultCredential(GlobalSSPI.SSPIAuth, package, this.m_IsServer ? CredentialUse.Inbound : CredentialUse.Outbound);
                this.m_UniqueUserId = "/S";
            }
            else
            {
                string userName = credential.InternalGetUserName();
                string domain = credential.InternalGetDomain();
                AuthIdentity authdata = new AuthIdentity(userName, credential.InternalGetPassword(), ((package == "WDigest") && ((domain == null) || (domain.Length == 0))) ? null : domain);
                this.m_UniqueUserId = domain + "/" + userName + "/U";
                this.m_CredentialsHandle = SSPIWrapper.AcquireCredentialsHandle(GlobalSSPI.SSPIAuth, package, this.m_IsServer ? CredentialUse.Inbound : CredentialUse.Outbound, ref authdata);
            }
        }

        private static void InitializeCallback(object state)
        {
            InitializeCallbackContext context = (InitializeCallbackContext) state;
            context.thisPtr.Initialize(context.isServer, context.package, context.credential, context.spn, context.requestedContextFlags, context.channelBinding);
        }

        internal int MakeSignature(byte[] buffer, int offset, int count, ref byte[] output)
        {
            SecSizes sizes = this.Sizes;
            int num = count + sizes.MaxSignature;
            if ((output == null) || (output.Length < num))
            {
                output = new byte[num];
            }
            Buffer.BlockCopy(buffer, offset, output, sizes.MaxSignature, count);
            SecurityBuffer[] input = new SecurityBuffer[] { new SecurityBuffer(output, 0, sizes.MaxSignature, BufferType.Token), new SecurityBuffer(output, sizes.MaxSignature, count, BufferType.Data) };
            int error = SSPIWrapper.MakeSignature(GlobalSSPI.SSPIAuth, this.m_SecurityContext, input, 0);
            if (error != 0)
            {
                throw new Win32Exception(error);
            }
            return (input[0].size + input[1].size);
        }

        internal int VerifySignature(byte[] buffer, int offset, int count)
        {
            if ((offset < 0) || (offset > ((buffer == null) ? 0 : buffer.Length)))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (count > ((buffer == null) ? 0 : (buffer.Length - offset))))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            SecurityBuffer[] input = new SecurityBuffer[] { new SecurityBuffer(buffer, offset, count, BufferType.Stream), new SecurityBuffer(0, BufferType.Data) };
            int error = SSPIWrapper.VerifySignature(GlobalSSPI.SSPIAuth, this.m_SecurityContext, input, 0);
            if (error != 0)
            {
                throw new Win32Exception(error);
            }
            if (input[1].type != BufferType.Data)
            {
                throw new InternalException();
            }
            return input[1].size;
        }

        internal string AssociatedName
        {
            get
            {
                if (!this.IsValidContext || !this.IsCompleted)
                {
                    throw new Win32Exception(-2146893055);
                }
                return (SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, this.m_SecurityContext, ContextAttribute.Names) as string);
            }
        }

        internal System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
        {
            get
            {
                return this.m_ChannelBinding;
            }
        }

        internal string ClientSpecifiedSpn
        {
            get
            {
                if (this.m_ClientSpecifiedSpn == null)
                {
                    this.m_ClientSpecifiedSpn = this.GetClientSpecifiedSpn();
                }
                return this.m_ClientSpecifiedSpn;
            }
        }

        internal bool IsCompleted
        {
            get
            {
                return this.m_IsCompleted;
            }
        }

        internal bool IsConfidentialityFlag
        {
            get
            {
                return ((this.m_ContextFlags & ContextFlags.Confidentiality) != ContextFlags.Zero);
            }
        }

        internal bool IsDelegationFlag
        {
            get
            {
                return ((this.m_ContextFlags & ContextFlags.Delegate) != ContextFlags.Zero);
            }
        }

        internal bool IsIdentifyFlag
        {
            get
            {
                return ((this.m_ContextFlags & (this.m_IsServer ? ContextFlags.AcceptIdentify : ContextFlags.AcceptIntegrity)) != ContextFlags.Zero);
            }
        }

        internal bool IsIntegrityFlag
        {
            get
            {
                return ((this.m_ContextFlags & (this.m_IsServer ? ContextFlags.AcceptIntegrity : ContextFlags.AcceptStream)) != ContextFlags.Zero);
            }
        }

        internal bool IsKerberos
        {
            get
            {
                if (this.m_LastProtocolName == null)
                {
                    this.m_LastProtocolName = this.ProtocolName;
                }
                return (this.m_LastProtocolName == "Kerberos");
            }
        }

        internal bool IsMutualAuthFlag
        {
            get
            {
                return ((this.m_ContextFlags & ContextFlags.MutualAuth) != ContextFlags.Zero);
            }
        }

        internal bool IsNTLM
        {
            get
            {
                if (this.m_LastProtocolName == null)
                {
                    this.m_LastProtocolName = this.ProtocolName;
                }
                return (this.m_LastProtocolName == "NTLM");
            }
        }

        internal bool IsServer
        {
            get
            {
                return this.m_IsServer;
            }
        }

        internal bool IsValidContext
        {
            get
            {
                return ((this.m_SecurityContext != null) && !this.m_SecurityContext.IsInvalid);
            }
        }

        internal bool OSSupportsExtendedProtection
        {
            get
            {
                int num;
                SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, this.m_SecurityContext, ContextAttribute.ClientSpecifiedSpn, out num);
                return (num != -2146893054);
            }
        }

        internal string Package
        {
            get
            {
                return this.m_Package;
            }
        }

        internal string ProtocolName
        {
            get
            {
                if (this.m_ProtocolName != null)
                {
                    return this.m_ProtocolName;
                }
                NegotiationInfoClass class2 = null;
                if (this.IsValidContext)
                {
                    class2 = SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, this.m_SecurityContext, ContextAttribute.NegotiationInfo) as NegotiationInfoClass;
                    if (this.IsCompleted)
                    {
                        if (class2 == null)
                        {
                            if (ComNetOS.IsWin9x)
                            {
                                this.m_ProtocolName = "NTLM";
                                return this.m_ProtocolName;
                            }
                        }
                        else
                        {
                            this.m_ProtocolName = class2.AuthenticationPackage;
                        }
                    }
                }
                if (class2 != null)
                {
                    return class2.AuthenticationPackage;
                }
                return string.Empty;
            }
        }

        internal SecSizes Sizes
        {
            get
            {
                if (this.m_Sizes == null)
                {
                    this.m_Sizes = SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, this.m_SecurityContext, ContextAttribute.Sizes) as SecSizes;
                }
                return this.m_Sizes;
            }
        }

        internal string Spn
        {
            get
            {
                return this.m_Spn;
            }
        }

        internal string UniqueUserId
        {
            get
            {
                return this.m_UniqueUserId;
            }
        }

        private class InitializeCallbackContext
        {
            internal readonly ChannelBinding channelBinding;
            internal readonly NetworkCredential credential;
            internal readonly bool isServer;
            internal readonly string package;
            internal readonly ContextFlags requestedContextFlags;
            internal readonly string spn;
            internal readonly NTAuthentication thisPtr;

            internal InitializeCallbackContext(NTAuthentication thisPtr, bool isServer, string package, NetworkCredential credential, string spn, ContextFlags requestedContextFlags, ChannelBinding channelBinding)
            {
                this.thisPtr = thisPtr;
                this.isServer = isServer;
                this.package = package;
                this.credential = credential;
                this.spn = spn;
                this.requestedContextFlags = requestedContextFlags;
                this.channelBinding = channelBinding;
            }
        }
    }
}

