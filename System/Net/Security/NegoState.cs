namespace System.Net.Security
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.Threading;

    internal class NegoState
    {
        private bool _CanRetryAuthentication;
        private NTAuthentication _Context;
        private static readonly byte[] _EmptyMessage = new byte[0];
        private Exception _Exception;
        private TokenImpersonationLevel _ExpectedImpersonationLevel;
        private ProtectionLevel _ExpectedProtectionLevel;
        private ExtendedProtectionPolicy _ExtendedProtectionPolicy;
        private StreamFramer _Framer;
        private Stream _InnerStream;
        private bool _LeaveStreamOpen;
        private int _NestedAuth;
        private static readonly AsyncCallback _ReadCallback = new AsyncCallback(NegoState.ReadCallback);
        private uint _ReadSequenceNumber;
        private bool _RemoteOk;
        private static readonly AsyncCallback _WriteCallback = new AsyncCallback(NegoState.WriteCallback);
        private uint _WriteSequenceNumber;
        internal const int c_MaxReadFrameSize = 0x10000;
        internal const int c_MaxWriteDataSize = 0xfc00;
        private const int ERROR_TRUST_FAILURE = 0x6fe;

        internal NegoState(Stream innerStream, bool leaveStreamOpen)
        {
            if (innerStream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this._InnerStream = innerStream;
            this._LeaveStreamOpen = leaveStreamOpen;
        }

        private void CheckCompletionBeforeNextReceive(LazyAsyncResult lazyResult)
        {
            if (this.HandshakeComplete && this._RemoteOk)
            {
                if (lazyResult != null)
                {
                    lazyResult.InvokeCallback();
                }
            }
            else
            {
                this.StartReceiveBlob(lazyResult);
            }
        }

        private void CheckCompletionBeforeNextSend(byte[] message, LazyAsyncResult lazyResult)
        {
            if (this.HandshakeComplete)
            {
                if (!this._RemoteOk)
                {
                    throw new AuthenticationException(SR.GetString("net_io_header_id", new object[] { "MessageId", this._Framer.ReadHeader.MessageId, 20 }), null);
                }
                if (lazyResult != null)
                {
                    lazyResult.InvokeCallback();
                }
            }
            else
            {
                this.StartSendBlob(message, lazyResult);
            }
        }

        private bool CheckSpn()
        {
            if (this._Context.IsKerberos)
            {
                return true;
            }
            if ((this._ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Never) || (this._ExtendedProtectionPolicy.CustomServiceNames == null))
            {
                return true;
            }
            if (!AuthenticationManager.OSSupportsExtendedProtection)
            {
                return true;
            }
            string clientSpecifiedSpn = this._Context.ClientSpecifiedSpn;
            if (string.IsNullOrEmpty(clientSpecifiedSpn))
            {
                if (this._ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.WhenSupported)
                {
                    return true;
                }
            }
            else
            {
                foreach (string str2 in this._ExtendedProtectionPolicy.CustomServiceNames)
                {
                    if (string.Compare(clientSpecifiedSpn, str2, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
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

        internal void Close()
        {
            this._Exception = new ObjectDisposedException("NegotiateStream");
            if (this._Context != null)
            {
                this._Context.CloseContext();
            }
        }

        internal int DecryptData(byte[] buffer, int offset, int count, out int newOffset)
        {
            this.CheckThrow(true);
            this._ReadSequenceNumber++;
            return this._Context.Decrypt(buffer, offset, count, out newOffset, this._ReadSequenceNumber);
        }

        internal int EncryptData(byte[] buffer, int offset, int count, ref byte[] outBuffer)
        {
            this.CheckThrow(true);
            this._WriteSequenceNumber++;
            return this._Context.Encrypt(buffer, offset, count, ref outBuffer, this._WriteSequenceNumber);
        }

        internal void EndProcessAuthentication(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result2 = result as LazyAsyncResult;
            if (result2 == null)
            {
                throw new ArgumentException(SR.GetString("net_io_async_result", new object[] { result.GetType().FullName }), "asyncResult");
            }
            if (Interlocked.Exchange(ref this._NestedAuth, 0) == 0)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndAuthenticate" }));
            }
            result2.InternalWaitForCompletion();
            Exception e = result2.Result as Exception;
            if (e != null)
            {
                throw this.SetException(e);
            }
        }

        internal IIdentity GetIdentity()
        {
            this.CheckThrow(true);
            string name = this._Context.IsServer ? this._Context.AssociatedName : this._Context.Spn;
            string type = "NTLM";
            if (!ComNetOS.IsWin9x)
            {
                type = this._Context.ProtocolName;
            }
            if (this._Context.IsServer && !ComNetOS.IsWin9x)
            {
                SafeCloseHandle contextToken = null;
                try
                {
                    contextToken = this._Context.GetContextToken();
                    return new WindowsIdentity(contextToken.DangerousGetHandle(), this._Context.ProtocolName, WindowsAccountType.Normal, true);
                }
                catch (SecurityException)
                {
                }
                finally
                {
                    if (contextToken != null)
                    {
                        contextToken.Close();
                    }
                }
            }
            return new GenericIdentity(name, type);
        }

        private byte[] GetOutgoingBlob(byte[] incomingBlob, ref Win32Exception e)
        {
            SecurityStatus status;
            byte[] buffer = this._Context.GetOutgoingBlob(incomingBlob, false, out status);
            if ((status & ((SecurityStatus) (-2147483648))) != SecurityStatus.OK)
            {
                e = new Win32Exception((int) status);
                buffer = new byte[8];
                for (int i = buffer.Length - 1; i >= 0; i--)
                {
                    buffer[i] = (byte) (status & ((SecurityStatus) 0xff));
                    status = (SecurityStatus) (((int) status) >> 8);
                }
            }
            if ((buffer != null) && (buffer.Length == 0))
            {
                buffer = _EmptyMessage;
            }
            return buffer;
        }

        internal void ProcessAuthentication(LazyAsyncResult lazyResult)
        {
            this.CheckThrow(false);
            if (Interlocked.Exchange(ref this._NestedAuth, 1) == 1)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidnestedcall", new object[] { (lazyResult == null) ? "BeginAuthenticate" : "Authenticate", "authenticate" }));
            }
            try
            {
                if (this._Context.IsServer)
                {
                    this.StartReceiveBlob(lazyResult);
                }
                else
                {
                    this.StartSendBlob(null, lazyResult);
                }
            }
            catch (Exception exception)
            {
                exception = this.SetException(exception);
                throw;
            }
            finally
            {
                if ((lazyResult == null) || (this._Exception != null))
                {
                    this._NestedAuth = 0;
                }
            }
        }

        private void ProcessReceivedBlob(byte[] message, LazyAsyncResult lazyResult)
        {
            if (message == null)
            {
                throw new AuthenticationException(SR.GetString("net_auth_eof"), null);
            }
            if (this._Framer.ReadHeader.MessageId == 0x15)
            {
                Win32Exception innerException = null;
                if (message.Length >= 8)
                {
                    long num = 0L;
                    for (int i = 0; i < 8; i++)
                    {
                        num = (num << 8) + message[i];
                    }
                    innerException = new Win32Exception((int) num);
                }
                if (innerException != null)
                {
                    if (innerException.NativeErrorCode == -2146893044)
                    {
                        throw new InvalidCredentialException(SR.GetString("net_auth_bad_client_creds"), innerException);
                    }
                    if (innerException.NativeErrorCode == 0x6fe)
                    {
                        throw new AuthenticationException(SR.GetString("net_auth_context_expectation_remote"), innerException);
                    }
                }
                throw new AuthenticationException(SR.GetString("net_auth_alert"), innerException);
            }
            if (this._Framer.ReadHeader.MessageId == 20)
            {
                this._RemoteOk = true;
            }
            else if (this._Framer.ReadHeader.MessageId != 0x16)
            {
                throw new AuthenticationException(SR.GetString("net_io_header_id", new object[] { "MessageId", this._Framer.ReadHeader.MessageId, 0x16 }), null);
            }
            this.CheckCompletionBeforeNextSend(message, lazyResult);
        }

        private static void ReadCallback(IAsyncResult transportResult)
        {
            if (!transportResult.CompletedSynchronously)
            {
                LazyAsyncResult asyncState = (LazyAsyncResult) transportResult.AsyncState;
                try
                {
                    NegoState asyncObject = (NegoState) asyncState.AsyncObject;
                    byte[] message = asyncObject._Framer.EndReadMessage(transportResult);
                    asyncObject.ProcessReceivedBlob(message, asyncState);
                }
                catch (Exception exception)
                {
                    if (asyncState.InternalPeekCompleted)
                    {
                        throw;
                    }
                    asyncState.InvokeCallback(exception);
                }
            }
        }

        private Exception SetException(Exception e)
        {
            if ((this._Exception == null) || !(this._Exception is ObjectDisposedException))
            {
                this._Exception = e;
            }
            if ((this._Exception != null) && (this._Context != null))
            {
                this._Context.CloseContext();
            }
            return this._Exception;
        }

        private void StartReceiveBlob(LazyAsyncResult lazyResult)
        {
            byte[] buffer;
            if (lazyResult == null)
            {
                buffer = this._Framer.ReadMessage();
            }
            else
            {
                IAsyncResult asyncResult = this._Framer.BeginReadMessage(_ReadCallback, lazyResult);
                if (!asyncResult.CompletedSynchronously)
                {
                    return;
                }
                buffer = this._Framer.EndReadMessage(asyncResult);
            }
            this.ProcessReceivedBlob(buffer, lazyResult);
        }

        private void StartSendAuthResetSignal(LazyAsyncResult lazyResult, byte[] message, Exception exception)
        {
            this._Framer.WriteHeader.MessageId = 0x15;
            Win32Exception exception2 = exception as Win32Exception;
            if ((exception2 != null) && (exception2.NativeErrorCode == -2146893044))
            {
                if (this.IsServer)
                {
                    exception = new InvalidCredentialException(SR.GetString("net_auth_bad_client_creds"), exception);
                }
                else
                {
                    exception = new InvalidCredentialException(SR.GetString("net_auth_bad_client_creds_or_target_mismatch"), exception);
                }
            }
            if (!(exception is AuthenticationException))
            {
                exception = new AuthenticationException(SR.GetString("net_auth_SSPI"), exception);
            }
            if (lazyResult == null)
            {
                this._Framer.WriteMessage(message);
            }
            else
            {
                lazyResult.Result = exception;
                IAsyncResult asyncResult = this._Framer.BeginWriteMessage(message, _WriteCallback, lazyResult);
                if (!asyncResult.CompletedSynchronously)
                {
                    return;
                }
                this._Framer.EndWriteMessage(asyncResult);
            }
            this._CanRetryAuthentication = true;
            throw exception;
        }

        private void StartSendBlob(byte[] message, LazyAsyncResult lazyResult)
        {
            Win32Exception e = null;
            if (message != _EmptyMessage)
            {
                message = this.GetOutgoingBlob(message, ref e);
            }
            if (e != null)
            {
                this.StartSendAuthResetSignal(lazyResult, message, e);
            }
            else
            {
                if (this.HandshakeComplete)
                {
                    if (this._Context.IsServer && !this.CheckSpn())
                    {
                        Exception exception = new AuthenticationException(SR.GetString("net_auth_bad_client_creds_or_target_mismatch"));
                        int num = 0x6fe;
                        message = new byte[8];
                        for (int i = message.Length - 1; i >= 0; i--)
                        {
                            message[i] = (byte) (num & 0xff);
                            num = num >> 8;
                        }
                        this.StartSendAuthResetSignal(lazyResult, message, exception);
                        return;
                    }
                    if (this.PrivateImpersonationLevel < this._ExpectedImpersonationLevel)
                    {
                        Exception exception3 = new AuthenticationException(SR.GetString("net_auth_context_expectation", new object[] { this._ExpectedImpersonationLevel.ToString(), this.PrivateImpersonationLevel.ToString() }));
                        int num3 = 0x6fe;
                        message = new byte[8];
                        for (int j = message.Length - 1; j >= 0; j--)
                        {
                            message[j] = (byte) (num3 & 0xff);
                            num3 = num3 >> 8;
                        }
                        this.StartSendAuthResetSignal(lazyResult, message, exception3);
                        return;
                    }
                    ProtectionLevel level = this._Context.IsConfidentialityFlag ? ProtectionLevel.EncryptAndSign : (this._Context.IsIntegrityFlag ? ProtectionLevel.Sign : ProtectionLevel.None);
                    if (level < this._ExpectedProtectionLevel)
                    {
                        Exception exception4 = new AuthenticationException(SR.GetString("net_auth_context_expectation", new object[] { level.ToString(), this._ExpectedProtectionLevel.ToString() }));
                        int num5 = 0x6fe;
                        message = new byte[8];
                        for (int k = message.Length - 1; k >= 0; k--)
                        {
                            message[k] = (byte) (num5 & 0xff);
                            num5 = num5 >> 8;
                        }
                        this.StartSendAuthResetSignal(lazyResult, message, exception4);
                        return;
                    }
                    this._Framer.WriteHeader.MessageId = 20;
                    if (this._Context.IsServer)
                    {
                        this._RemoteOk = true;
                        if (message == null)
                        {
                            message = _EmptyMessage;
                        }
                    }
                }
                else if ((message == null) || (message == _EmptyMessage))
                {
                    throw new InternalException();
                }
                if (message != null)
                {
                    if (lazyResult == null)
                    {
                        this._Framer.WriteMessage(message);
                    }
                    else
                    {
                        IAsyncResult asyncResult = this._Framer.BeginWriteMessage(message, _WriteCallback, lazyResult);
                        if (!asyncResult.CompletedSynchronously)
                        {
                            return;
                        }
                        this._Framer.EndWriteMessage(asyncResult);
                    }
                }
                this.CheckCompletionBeforeNextReceive(lazyResult);
            }
        }

        internal void ValidateCreateContext(string package, NetworkCredential credential, string servicePrincipalName, ExtendedProtectionPolicy policy, ProtectionLevel protectionLevel, TokenImpersonationLevel impersonationLevel)
        {
            if (policy != null)
            {
                if (!AuthenticationManager.OSSupportsExtendedProtection)
                {
                    if (policy.PolicyEnforcement == PolicyEnforcement.Always)
                    {
                        throw new PlatformNotSupportedException(SR.GetString("security_ExtendedProtection_NoOSSupport"));
                    }
                }
                else if ((policy.CustomChannelBinding == null) && (policy.CustomServiceNames == null))
                {
                    throw new ArgumentException(SR.GetString("net_auth_must_specify_extended_protection_scheme"), "policy");
                }
                this._ExtendedProtectionPolicy = policy;
            }
            else
            {
                this._ExtendedProtectionPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);
            }
            this.ValidateCreateContext(package, true, credential, servicePrincipalName, this._ExtendedProtectionPolicy.CustomChannelBinding, protectionLevel, impersonationLevel);
        }

        internal void ValidateCreateContext(string package, bool isServer, NetworkCredential credential, string servicePrincipalName, ChannelBinding channelBinding, ProtectionLevel protectionLevel, TokenImpersonationLevel impersonationLevel)
        {
            if ((this._Exception != null) && !this._CanRetryAuthentication)
            {
                throw this._Exception;
            }
            if ((this._Context != null) && this._Context.IsValidContext)
            {
                throw new InvalidOperationException(SR.GetString("net_auth_reauth"));
            }
            if (credential == null)
            {
                throw new ArgumentNullException("credential");
            }
            if (servicePrincipalName == null)
            {
                throw new ArgumentNullException("servicePrincipalName");
            }
            if (ComNetOS.IsWin9x && (protectionLevel != ProtectionLevel.None))
            {
                throw new NotSupportedException(SR.GetString("net_auth_no_protection_on_win9x"));
            }
            if (((impersonationLevel != TokenImpersonationLevel.Identification) && (impersonationLevel != TokenImpersonationLevel.Impersonation)) && (impersonationLevel != TokenImpersonationLevel.Delegation))
            {
                throw new ArgumentOutOfRangeException("impersonationLevel", impersonationLevel.ToString(), SR.GetString("net_auth_supported_impl_levels"));
            }
            if ((this._Context != null) && (this.IsServer != isServer))
            {
                throw new InvalidOperationException(SR.GetString("net_auth_client_server"));
            }
            this._Exception = null;
            this._RemoteOk = false;
            this._Framer = new StreamFramer(this._InnerStream);
            this._Framer.WriteHeader.MessageId = 0x16;
            this._ExpectedProtectionLevel = protectionLevel;
            this._ExpectedImpersonationLevel = isServer ? impersonationLevel : TokenImpersonationLevel.None;
            this._WriteSequenceNumber = 0;
            this._ReadSequenceNumber = 0;
            ContextFlags connection = ContextFlags.Connection;
            if ((protectionLevel == ProtectionLevel.None) && !isServer)
            {
                package = "NTLM";
            }
            else if (protectionLevel == ProtectionLevel.EncryptAndSign)
            {
                connection |= ContextFlags.Confidentiality;
            }
            else if (protectionLevel == ProtectionLevel.Sign)
            {
                connection |= ContextFlags.AcceptStream | ContextFlags.SequenceDetect | ContextFlags.ReplayDetect;
            }
            if (isServer)
            {
                if (this._ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.WhenSupported)
                {
                    connection |= ContextFlags.AllowMissingBindings;
                }
                if ((this._ExtendedProtectionPolicy.PolicyEnforcement != PolicyEnforcement.Never) && (this._ExtendedProtectionPolicy.ProtectionScenario == ProtectionScenario.TrustedProxy))
                {
                    connection |= ContextFlags.ProxyBindings;
                }
            }
            else
            {
                if (protectionLevel != ProtectionLevel.None)
                {
                    connection |= ContextFlags.MutualAuth;
                }
                if (impersonationLevel == TokenImpersonationLevel.Identification)
                {
                    connection |= ContextFlags.AcceptIntegrity;
                }
                if (impersonationLevel == TokenImpersonationLevel.Delegation)
                {
                    connection |= ContextFlags.Delegate;
                }
            }
            this._CanRetryAuthentication = false;
            if (!(credential is SystemNetworkCredential))
            {
                ExceptionHelper.ControlPrincipalPermission.Demand();
            }
            try
            {
                this._Context = new NTAuthentication(isServer, package, credential, servicePrincipalName, connection, channelBinding);
            }
            catch (Win32Exception exception)
            {
                throw new AuthenticationException(SR.GetString("net_auth_SSPI"), exception);
            }
        }

        private static void WriteCallback(IAsyncResult transportResult)
        {
            if (!transportResult.CompletedSynchronously)
            {
                LazyAsyncResult asyncState = (LazyAsyncResult) transportResult.AsyncState;
                try
                {
                    NegoState asyncObject = (NegoState) asyncState.AsyncObject;
                    asyncObject._Framer.EndWriteMessage(transportResult);
                    if (asyncState.Result is Exception)
                    {
                        asyncObject._CanRetryAuthentication = true;
                        throw ((Exception) asyncState.Result);
                    }
                    asyncObject.CheckCompletionBeforeNextReceive(asyncState);
                }
                catch (Exception exception)
                {
                    if (asyncState.InternalPeekCompleted)
                    {
                        throw;
                    }
                    asyncState.InvokeCallback(exception);
                }
            }
        }

        internal TokenImpersonationLevel AllowedImpersonation
        {
            get
            {
                this.CheckThrow(true);
                return this.PrivateImpersonationLevel;
            }
        }

        internal bool CanGetSecureStream
        {
            get
            {
                if (!this._Context.IsConfidentialityFlag)
                {
                    return this._Context.IsIntegrityFlag;
                }
                return true;
            }
        }

        internal static string DefaultPackage
        {
            get
            {
                if (!ComNetOS.IsWin9x)
                {
                    return "Negotiate";
                }
                return "NTLM";
            }
        }

        private bool HandshakeComplete
        {
            get
            {
                return (this._Context.IsCompleted && this._Context.IsValidContext);
            }
        }

        internal bool IsAuthenticated
        {
            get
            {
                return ((((this._Context != null) && this.HandshakeComplete) && (this._Exception == null)) && this._RemoteOk);
            }
        }

        internal bool IsEncrypted
        {
            get
            {
                return (this.IsAuthenticated && this._Context.IsConfidentialityFlag);
            }
        }

        internal bool IsMutuallyAuthenticated
        {
            get
            {
                if (!this.IsAuthenticated)
                {
                    return false;
                }
                if (ComNetOS.IsWin9x)
                {
                    return false;
                }
                if (this._Context.IsNTLM)
                {
                    return false;
                }
                return this._Context.IsMutualAuthFlag;
            }
        }

        internal bool IsServer
        {
            get
            {
                return ((this._Context != null) && this._Context.IsServer);
            }
        }

        internal bool IsSigned
        {
            get
            {
                if (!this.IsAuthenticated)
                {
                    return false;
                }
                if (!this._Context.IsIntegrityFlag)
                {
                    return this._Context.IsConfidentialityFlag;
                }
                return true;
            }
        }

        private TokenImpersonationLevel PrivateImpersonationLevel
        {
            get
            {
                if (this._Context.IsDelegationFlag && (this._Context.ProtocolName != "NTLM"))
                {
                    return TokenImpersonationLevel.Delegation;
                }
                if (this._Context.IsIdentifyFlag)
                {
                    return TokenImpersonationLevel.Identification;
                }
                if (ComNetOS.IsWin9x && this._Context.IsServer)
                {
                    return TokenImpersonationLevel.Identification;
                }
                return TokenImpersonationLevel.Impersonation;
            }
        }
    }
}

