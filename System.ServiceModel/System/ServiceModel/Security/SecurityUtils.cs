namespace System.ServiceModel.Security
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.DirectoryServices.ActiveDirectory;
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal static class SecurityUtils
    {
        private static SecurityIdentifier administratorsSid;
        private static IIdentity anonymousIdentity;
        public const string AuthTypeAnonymous = "";
        public const string AuthTypeBasic = "Basic";
        public const string AuthTypeCertMap = "SSL/PCT";
        public const string AuthTypeKerberos = "Kerberos";
        public const string AuthTypeNegotiate = "Negotiate";
        public const string AuthTypeNTLM = "NTLM";
        private static byte[] combinedHashLabel;
        private static bool computedDomain;
        private static string currentDomain;
        private static NetworkCredential dummyNetworkCredential;
        private static object dummyNetworkCredentialLock = new object();
        private static int fipsAlgorithmPolicy = -1;
        private const string fipsPolicyRegistryKey = @"System\CurrentControlSet\Control\Lsa";
        public const string Identities = "Identities";
        private static volatile bool isSslValidationRequirementDetermined = false;
        private static readonly int MinimumSslCipherStrength = 0x80;
        private static X509SecurityTokenAuthenticator nonValidatingX509Authenticator;
        public const string Principal = "Principal";
        private const string ServicePack1 = "Service Pack 1";
        private const string ServicePack2 = "Service Pack 2";
        private static volatile bool shouldValidateSslCipherStrength;
        private const string suppressChannelBindingRegistryKey = @"System\CurrentControlSet\Control\Lsa";
        private const int WindowsServerMajorNumber = 5;
        private const int WindowsServerMinorNumber = 2;
        private const int XPMajorNumber = 5;
        private const int XPMinorNumber = 1;

        internal static void AbortTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator)
        {
            CloseCommunicationObject(tokenAuthenticator, true, TimeSpan.Zero);
        }

        internal static void AbortTokenProviderIfRequired(SecurityTokenProvider tokenProvider)
        {
            CloseCommunicationObject(tokenProvider, true, TimeSpan.Zero);
        }

        internal static bool AllowsImpersonation(WindowsIdentity windowsIdentity, TokenImpersonationLevel impersonationLevel)
        {
            if (windowsIdentity == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windowsIdentity");
            }
            TokenImpersonationLevelHelper.Validate(impersonationLevel);
            if (impersonationLevel == TokenImpersonationLevel.Identification)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("impersonationLevel"));
            }
            bool flag = true;
            switch (windowsIdentity.ImpersonationLevel)
            {
                case TokenImpersonationLevel.None:
                case TokenImpersonationLevel.Anonymous:
                case TokenImpersonationLevel.Identification:
                    return false;

                case TokenImpersonationLevel.Impersonation:
                    if (impersonationLevel == TokenImpersonationLevel.Delegation)
                    {
                        flag = false;
                    }
                    return flag;

                case TokenImpersonationLevel.Delegation:
                    return flag;
            }
            return false;
        }

        internal static void AppendCertificateIdentityName(StringBuilder str, X509Certificate2 certificate)
        {
            string name = certificate.SubjectName.Name;
            if (string.IsNullOrEmpty(name))
            {
                name = certificate.GetNameInfo(X509NameType.DnsName, false);
                if (string.IsNullOrEmpty(name))
                {
                    name = certificate.GetNameInfo(X509NameType.SimpleName, false);
                    if (string.IsNullOrEmpty(name))
                    {
                        name = certificate.GetNameInfo(X509NameType.EmailName, false);
                        if (string.IsNullOrEmpty(name))
                        {
                            name = certificate.GetNameInfo(X509NameType.UpnName, false);
                        }
                    }
                }
            }
            str.Append(string.IsNullOrEmpty(name) ? "<x509>" : name);
            str.Append("; ");
            str.Append(certificate.Thumbprint);
        }

        internal static void AppendIdentityName(StringBuilder str, IIdentity identity)
        {
            string name = null;
            try
            {
                name = identity.Name;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
            }
            str.Append(string.IsNullOrEmpty(name) ? "<null>" : name);
            WindowsIdentity identity2 = identity as WindowsIdentity;
            if (identity2 != null)
            {
                if (identity2.User != null)
                {
                    str.Append("; ");
                    str.Append(identity2.User.ToString());
                }
            }
            else
            {
                WindowsSidIdentity identity3 = identity as WindowsSidIdentity;
                if (identity3 != null)
                {
                    str.Append("; ");
                    str.Append(identity3.SecurityIdentifier.ToString());
                }
            }
        }

        [SecurityCritical]
        internal static string AppendWindowsAuthenticationInfo(string inputString, NetworkCredential credential, AuthenticationLevel authenticationLevel, TokenImpersonationLevel impersonationLevel)
        {
            if (IsDefaultNetworkCredential(credential))
            {
                string str = UnsafeGetCurrentUserSidAsString();
                return (inputString + "\0" + str + "\0" + AuthenticationLevelHelper.ToString(authenticationLevel) + "\0" + TokenImpersonationLevelHelper.ToString(impersonationLevel));
            }
            return (inputString + "\0" + NetworkCredentialHelper.UnsafeGetDomain(credential) + "\0" + NetworkCredentialHelper.UnsafeGetUsername(credential) + "\0" + NetworkCredentialHelper.UnsafeGetPassword(credential) + "\0" + AuthenticationLevelHelper.ToString(authenticationLevel) + "\0" + TokenImpersonationLevelHelper.ToString(impersonationLevel));
        }

        internal static bool AreSecurityTokenParametersSuitableForChannelBinding(Collection<SecurityTokenParameters> tokenParameters)
        {
            if (tokenParameters != null)
            {
                foreach (SecurityTokenParameters parameters in tokenParameters)
                {
                    if ((parameters is SspiSecurityTokenParameters) || (parameters is KerberosSecurityTokenParameters))
                    {
                        return true;
                    }
                    SecureConversationSecurityTokenParameters parameters2 = parameters as SecureConversationSecurityTokenParameters;
                    if (parameters2 != null)
                    {
                        return IsSecurityBindingSuitableForChannelBinding(parameters2.BootstrapSecurityBindingElement as TransportSecurityBindingElement);
                    }
                }
            }
            return false;
        }

        internal static IAsyncResult BeginCloseTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseCommunicationObjectAsyncResult(tokenAuthenticator, timeout, callback, state);
        }

        internal static IAsyncResult BeginCloseTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseCommunicationObjectAsyncResult(tokenProvider, timeout, callback, state);
        }

        internal static IAsyncResult BeginOpenTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenCommunicationObjectAsyncResult(tokenAuthenticator, timeout, callback, state);
        }

        internal static IAsyncResult BeginOpenTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenCommunicationObjectAsyncResult(tokenProvider, timeout, callback, state);
        }

        internal static byte[] CloneBuffer(byte[] buffer)
        {
            byte[] dst = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(buffer.Length);
            Buffer.BlockCopy(buffer, 0, dst, 0, buffer.Length);
            return dst;
        }

        [SecuritySafeCritical]
        internal static WindowsIdentity CloneWindowsIdentityIfNecessary(WindowsIdentity wid)
        {
            return CloneWindowsIdentityIfNecessary(wid, null);
        }

        [SecuritySafeCritical]
        internal static WindowsIdentity CloneWindowsIdentityIfNecessary(WindowsIdentity wid, string authType)
        {
            if (wid != null)
            {
                IntPtr token = UnsafeGetWindowsIdentityToken(wid);
                if (token != IntPtr.Zero)
                {
                    return UnsafeCreateWindowsIdentityFromToken(token, authType);
                }
            }
            return wid;
        }

        private static void CloseCommunicationObject(object obj, bool aborted, TimeSpan timeout)
        {
            if (obj != null)
            {
                ICommunicationObject obj2 = obj as ICommunicationObject;
                if (obj2 != null)
                {
                    if (!aborted)
                    {
                        obj2.Close(timeout);
                    }
                    else
                    {
                        try
                        {
                            obj2.Abort();
                        }
                        catch (CommunicationException exception)
                        {
                            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
                            {
                                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                            }
                        }
                    }
                }
                else if (obj is IDisposable)
                {
                    ((IDisposable) obj).Dispose();
                }
            }
        }

        internal static void CloseTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout)
        {
            CloseTokenAuthenticatorIfRequired(tokenAuthenticator, false, timeout);
        }

        internal static void CloseTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, bool aborted, TimeSpan timeout)
        {
            CloseCommunicationObject(tokenAuthenticator, aborted, timeout);
        }

        internal static void CloseTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout)
        {
            CloseCommunicationObject(tokenProvider, false, timeout);
        }

        internal static void CloseTokenProviderIfRequired(SecurityTokenProvider tokenProvider, bool aborted, TimeSpan timeout)
        {
            CloseCommunicationObject(tokenProvider, aborted, timeout);
        }

        private static bool ComputeSslCipherStrengthRequirementFlag()
        {
            if ((Environment.OSVersion.Version.Major > 5) || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor > 2)))
            {
                return false;
            }
            if ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 1))
            {
                if ((!(Environment.OSVersion.ServicePack == string.Empty) && !string.Equals(Environment.OSVersion.ServicePack, "Service Pack 1", StringComparison.OrdinalIgnoreCase)) && !string.Equals(Environment.OSVersion.ServicePack, "Service Pack 2", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                return true;
            }
            if (((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 2)) && (!(Environment.OSVersion.ServicePack == string.Empty) && !string.Equals(Environment.OSVersion.ServicePack, "Service Pack 1", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            return true;
        }

        private static Exception CreateCertificateLoadException(StoreName storeName, StoreLocation storeLocation, X509FindType findType, object findValue, EndpointAddress target, int certCount)
        {
            if (certCount == 0)
            {
                if (target == null)
                {
                    return new InvalidOperationException(System.ServiceModel.SR.GetString("CannotFindCert", new object[] { storeName, storeLocation, findType, findValue }));
                }
                return new InvalidOperationException(System.ServiceModel.SR.GetString("CannotFindCertForTarget", new object[] { storeName, storeLocation, findType, findValue, target }));
            }
            if (target == null)
            {
                return new InvalidOperationException(System.ServiceModel.SR.GetString("FoundMultipleCerts", new object[] { storeName, storeLocation, findType, findValue }));
            }
            return new InvalidOperationException(System.ServiceModel.SR.GetString("FoundMultipleCertsForTarget", new object[] { storeName, storeLocation, findType, findValue, target }));
        }

        internal static IIdentity CreateIdentity(string name)
        {
            return new GenericIdentity(name);
        }

        internal static IIdentity CreateIdentity(string name, string authenticationType)
        {
            return new GenericIdentity(name, authenticationType);
        }

        internal static ReadOnlyCollection<IAuthorizationPolicy> CreatePrincipalNameAuthorizationPolicies(string principalName)
        {
            Claim claim;
            Claim claim2;
            if (principalName == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("principalName");
            }
            if (principalName.Contains("@") || principalName.Contains(@"\"))
            {
                claim = new Claim(ClaimTypes.Upn, principalName, Rights.Identity);
                claim2 = Claim.CreateUpnClaim(principalName);
            }
            else
            {
                claim = new Claim(ClaimTypes.Spn, principalName, Rights.Identity);
                claim2 = Claim.CreateSpnClaim(principalName);
            }
            List<Claim> claims = new List<Claim>(2) {
                claim,
                claim2
            };
            return new List<IAuthorizationPolicy>(1) { new UnconditionalPolicy(CreateIdentity(principalName), new DefaultClaimSet(ClaimSet.Anonymous, claims)) }.AsReadOnly();
        }

        internal static MessageFault CreateSecurityContextNotFoundFault(SecurityStandardsManager standardsManager, string action)
        {
            FaultReason reason;
            SecureConversationDriver secureConversationDriver = standardsManager.SecureConversationDriver;
            FaultCode subCode = new FaultCode(secureConversationDriver.BadContextTokenFaultCode.Value, secureConversationDriver.Namespace.Value);
            if (action != null)
            {
                reason = new FaultReason(System.ServiceModel.SR.GetString("BadContextTokenOrActionFaultReason", new object[] { action }), CultureInfo.CurrentCulture);
            }
            else
            {
                reason = new FaultReason(System.ServiceModel.SR.GetString("BadContextTokenFaultReason"), CultureInfo.CurrentCulture);
            }
            return MessageFault.CreateFault(FaultCode.CreateSenderFaultCode(subCode), reason);
        }

        internal static Exception CreateSecurityFaultException(Message unverifiedMessage)
        {
            return CreateSecurityFaultException(MessageFault.CreateFault(unverifiedMessage, 0x4000));
        }

        internal static Exception CreateSecurityFaultException(MessageFault fault)
        {
            return new MessageSecurityException(System.ServiceModel.SR.GetString("UnsecuredMessageFaultReceived"), FaultException.CreateFault(fault, new System.Type[] { typeof(string), typeof(object) }));
        }

        internal static MessageFault CreateSecurityMessageFault(Exception e, SecurityStandardsManager standardsManager)
        {
            FaultCode code;
            FaultReason reason;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            FaultException exception = null;
            while (e != null)
            {
                if (e is SecurityTokenValidationException)
                {
                    if (e is SecurityContextTokenValidationException)
                    {
                        return CreateSecurityContextNotFoundFault(SecurityStandardsManager.DefaultInstance, null);
                    }
                    flag = true;
                    flag2 = true;
                    break;
                }
                if (e is SecurityTokenException)
                {
                    flag = true;
                    flag3 = true;
                    break;
                }
                if (e is MessageSecurityException)
                {
                    MessageSecurityException exception2 = (MessageSecurityException) e;
                    if (exception2.Fault != null)
                    {
                        return exception2.Fault;
                    }
                    flag = true;
                }
                else if (e is FaultException)
                {
                    exception = (FaultException) e;
                    break;
                }
                e = e.InnerException;
            }
            if (!flag && (exception == null))
            {
                return null;
            }
            SecurityVersion securityVersion = standardsManager.SecurityVersion;
            if (flag2)
            {
                code = new FaultCode(securityVersion.FailedAuthenticationFaultCode.Value, securityVersion.HeaderNamespace.Value);
                reason = new FaultReason(System.ServiceModel.SR.GetString("FailedAuthenticationFaultReason"), CultureInfo.CurrentCulture);
            }
            else if (flag3)
            {
                code = new FaultCode(securityVersion.InvalidSecurityTokenFaultCode.Value, securityVersion.HeaderNamespace.Value);
                reason = new FaultReason(System.ServiceModel.SR.GetString("InvalidSecurityTokenFaultReason"), CultureInfo.CurrentCulture);
            }
            else
            {
                if (exception != null)
                {
                    return MessageFault.CreateFault(exception.Code, exception.Reason);
                }
                code = new FaultCode(securityVersion.InvalidSecurityFaultCode.Value, securityVersion.HeaderNamespace.Value);
                reason = new FaultReason(System.ServiceModel.SR.GetString("InvalidSecurityFaultReason"), CultureInfo.CurrentCulture);
            }
            return MessageFault.CreateFault(FaultCode.CreateSenderFaultCode(code), reason);
        }

        internal static SecurityStandardsManager CreateSecurityStandardsManager(SecurityTokenRequirement requirement, SecurityTokenManager tokenManager)
        {
            MessageSecurityTokenVersion property = requirement.GetProperty<MessageSecurityTokenVersion>(ServiceModelSecurityTokenRequirement.MessageSecurityVersionProperty);
            if (property == MessageSecurityTokenVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10)
            {
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10, tokenManager);
            }
            if (property == MessageSecurityTokenVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005)
            {
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11, tokenManager);
            }
            if (property == MessageSecurityTokenVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10)
            {
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10, tokenManager);
            }
            if (property == MessageSecurityTokenVersion.WSSecurity10WSTrust13WSSecureConversation13BasicSecurityProfile10)
            {
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10, tokenManager);
            }
            if (property == MessageSecurityTokenVersion.WSSecurity11WSTrust13WSSecureConversation13)
            {
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12, tokenManager);
            }
            if (property != MessageSecurityTokenVersion.WSSecurity11WSTrust13WSSecureConversation13BasicSecurityProfile10)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10, tokenManager);
        }

        internal static SecurityStandardsManager CreateSecurityStandardsManager(MessageSecurityVersion securityVersion, SecurityTokenManager tokenManager)
        {
            return new SecurityStandardsManager(securityVersion, tokenManager.CreateSecurityTokenSerializer(securityVersion.SecurityTokenVersion));
        }

        internal static SecurityStandardsManager CreateSecurityStandardsManager(MessageSecurityVersion securityVersion, SecurityTokenSerializer securityTokenSerializer)
        {
            if (securityVersion == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("securityVersion"));
            }
            if (securityTokenSerializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenSerializer");
            }
            return new SecurityStandardsManager(securityVersion, securityTokenSerializer);
        }

        internal static ReadOnlyCollection<SecurityKey> CreateSymmetricSecurityKeys(byte[] key)
        {
            return new List<SecurityKey>(1) { new InMemorySymmetricSecurityKey(key) }.AsReadOnly();
        }

        public static WrappedKeySecurityToken CreateTokenFromEncryptedKeyClause(EncryptedKeyIdentifierClause keyClause, SecurityToken unwrappingToken)
        {
            SecurityKeyIdentifier encryptingKeyIdentifier = keyClause.EncryptingKeyIdentifier;
            byte[] encryptedKey = keyClause.GetEncryptedKey();
            SecurityKey wrappingSecurityKey = unwrappingToken.SecurityKeys[0];
            string encryptionMethod = keyClause.EncryptionMethod;
            return new WrappedKeySecurityToken(GenerateId(), wrappingSecurityKey.DecryptKey(encryptionMethod, encryptedKey), encryptionMethod, unwrappingToken, encryptingKeyIdentifier, encryptedKey, wrappingSecurityKey);
        }

        internal static EndpointIdentity CreateWindowsIdentity()
        {
            return CreateWindowsIdentity(false);
        }

        internal static EndpointIdentity CreateWindowsIdentity(bool spnOnly)
        {
            using (WindowsIdentity identity2 = WindowsIdentity.GetCurrent())
            {
                bool flag = IsSystemAccount(identity2);
                if (spnOnly || flag)
                {
                    return EndpointIdentity.CreateSpnIdentity(string.Format(CultureInfo.InvariantCulture, "host/{0}", new object[] { DnsCache.MachineName }));
                }
                return new UpnEndpointIdentity(CloneWindowsIdentityIfNecessary(identity2));
            }
        }

        internal static EndpointIdentity CreateWindowsIdentity(NetworkCredential serverCredential)
        {
            string userName;
            if ((serverCredential == null) || NetworkCredentialHelper.IsDefault(serverCredential))
            {
                return CreateWindowsIdentity();
            }
            if ((serverCredential.Domain != null) && (serverCredential.Domain.Length > 0))
            {
                userName = serverCredential.UserName + "@" + serverCredential.Domain;
            }
            else
            {
                userName = serverCredential.UserName;
            }
            return EndpointIdentity.CreateUpnIdentity(userName);
        }

        internal static byte[] DecryptKey(SecurityToken unwrappingToken, string encryptionMethod, byte[] wrappedKey, out SecurityKey unwrappingSecurityKey)
        {
            unwrappingSecurityKey = null;
            if (unwrappingToken.SecurityKeys != null)
            {
                for (int i = 0; i < unwrappingToken.SecurityKeys.Count; i++)
                {
                    if (unwrappingToken.SecurityKeys[i].IsSupportedAlgorithm(encryptionMethod))
                    {
                        unwrappingSecurityKey = unwrappingToken.SecurityKeys[i];
                        break;
                    }
                }
            }
            if (unwrappingSecurityKey == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("CannotFindMatchingCrypto", new object[] { encryptionMethod })));
            }
            return unwrappingSecurityKey.DecryptKey(encryptionMethod, wrappedKey);
        }

        internal static byte[] EncryptKey(SecurityToken wrappingToken, string encryptionMethod, byte[] keyToWrap)
        {
            SecurityKey key = null;
            if (wrappingToken.SecurityKeys != null)
            {
                for (int i = 0; i < wrappingToken.SecurityKeys.Count; i++)
                {
                    if (wrappingToken.SecurityKeys[i].IsSupportedAlgorithm(encryptionMethod))
                    {
                        key = wrappingToken.SecurityKeys[i];
                        break;
                    }
                }
            }
            if (key == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("CannotFindMatchingCrypto", new object[] { encryptionMethod }));
            }
            return key.EncryptKey(encryptionMethod, keyToWrap);
        }

        internal static void EndCloseTokenAuthenticatorIfRequired(IAsyncResult result)
        {
            CloseCommunicationObjectAsyncResult.End(result);
        }

        internal static void EndCloseTokenProviderIfRequired(IAsyncResult result)
        {
            CloseCommunicationObjectAsyncResult.End(result);
        }

        internal static void EndOpenTokenAuthenticatorIfRequired(IAsyncResult result)
        {
            OpenCommunicationObjectAsyncResult.End(result);
        }

        internal static void EndOpenTokenProviderIfRequired(IAsyncResult result)
        {
            OpenCommunicationObjectAsyncResult.End(result);
        }

        internal static void EnsureCertificateCanDoKeyExchange(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            bool flag = false;
            Exception innerException = null;
            if (certificate.HasPrivateKey)
            {
                try
                {
                    RSACryptoServiceProvider privateKey = certificate.PrivateKey as RSACryptoServiceProvider;
                    if ((privateKey != null) && (privateKey.CspKeyContainerInfo.KeyNumber == KeyNumber.Exchange))
                    {
                        flag = true;
                    }
                }
                catch (SecurityException exception2)
                {
                    innerException = exception2;
                }
                catch (CryptographicException exception3)
                {
                    innerException = exception3;
                }
            }
            if (!flag)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SslCertMayNotDoKeyExchange", new object[] { certificate.SubjectName.Name }), innerException));
            }
        }

        internal static void EnsureExpectedSymmetricMatch(SecurityToken t1, SecurityToken t2, Message message)
        {
            if (((t1 != null) && (t2 != null)) && !object.ReferenceEquals(t1, t2))
            {
                SymmetricSecurityKey securityKey = GetSecurityKey<SymmetricSecurityKey>(t1);
                SymmetricSecurityKey key2 = GetSecurityKey<SymmetricSecurityKey>(t2);
                if (((securityKey == null) || (key2 == null)) || !System.ServiceModel.Security.CryptoHelper.IsEqual(securityKey.GetSymmetricKey(), key2.GetSymmetricKey()))
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenNotExpectedInSecurityHeader", new object[] { t2 })), message);
                }
            }
        }

        internal static void ErasePasswordInUsernameTokenIfPresent(SecurityMessageProperty messageProperty)
        {
            if (messageProperty != null)
            {
                if (messageProperty.TransportToken != null)
                {
                    UserNameSecurityToken securityToken = messageProperty.TransportToken.SecurityToken as UserNameSecurityToken;
                    if ((securityToken != null) && !messageProperty.TransportToken.SecurityToken.GetType().IsSubclassOf(typeof(UserNameSecurityToken)))
                    {
                        messageProperty.TransportToken = new SecurityTokenSpecification(new UserNameSecurityToken(securityToken.UserName, null, securityToken.Id), messageProperty.TransportToken.SecurityTokenPolicies);
                    }
                }
                if (messageProperty.ProtectionToken != null)
                {
                    UserNameSecurityToken token2 = messageProperty.ProtectionToken.SecurityToken as UserNameSecurityToken;
                    if ((token2 != null) && !messageProperty.ProtectionToken.SecurityToken.GetType().IsSubclassOf(typeof(UserNameSecurityToken)))
                    {
                        messageProperty.ProtectionToken = new SecurityTokenSpecification(new UserNameSecurityToken(token2.UserName, null, token2.Id), messageProperty.ProtectionToken.SecurityTokenPolicies);
                    }
                }
                if (messageProperty.HasIncomingSupportingTokens)
                {
                    for (int i = 0; i < messageProperty.IncomingSupportingTokens.Count; i++)
                    {
                        SupportingTokenSpecification specification = messageProperty.IncomingSupportingTokens[i];
                        UserNameSecurityToken token3 = specification.SecurityToken as UserNameSecurityToken;
                        if ((token3 != null) && !specification.SecurityToken.GetType().IsSubclassOf(typeof(UserNameSecurityToken)))
                        {
                            messageProperty.IncomingSupportingTokens[i] = new SupportingTokenSpecification(new UserNameSecurityToken(token3.UserName, null, token3.Id), specification.SecurityTokenPolicies, specification.SecurityTokenAttachmentMode, specification.SecurityTokenParameters);
                        }
                    }
                }
            }
        }

        [SecuritySafeCritical]
        internal static void FixNetworkCredential(ref NetworkCredential credential)
        {
            if (credential != null)
            {
                string str = NetworkCredentialHelper.UnsafeGetUsername(credential);
                string str2 = NetworkCredentialHelper.UnsafeGetDomain(credential);
                if (!string.IsNullOrEmpty(str) && string.IsNullOrEmpty(str2))
                {
                    string[] strArray = str.Split(new char[] { '\\' });
                    string[] strArray2 = str.Split(new char[] { '@' });
                    if ((strArray.Length == 2) && (strArray2.Length == 1))
                    {
                        if (!string.IsNullOrEmpty(strArray[0]) && !string.IsNullOrEmpty(strArray[1]))
                        {
                            credential = new NetworkCredential(strArray[1], NetworkCredentialHelper.UnsafeGetPassword(credential), strArray[0]);
                        }
                    }
                    else if (((strArray.Length == 1) && (strArray2.Length == 2)) && (!string.IsNullOrEmpty(strArray2[0]) && !string.IsNullOrEmpty(strArray2[1])))
                    {
                        credential = new NetworkCredential(strArray2[0], NetworkCredentialHelper.UnsafeGetPassword(credential), strArray2[1]);
                    }
                }
            }
        }

        internal static byte[] GenerateDerivedKey(SecurityToken tokenToDerive, string derivationAlgorithm, byte[] label, byte[] nonce, int keySize, int offset)
        {
            SymmetricSecurityKey securityKey = GetSecurityKey<SymmetricSecurityKey>(tokenToDerive);
            if ((securityKey == null) || !securityKey.IsSupportedAlgorithm(derivationAlgorithm))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("CannotFindMatchingCrypto", new object[] { derivationAlgorithm })));
            }
            return securityKey.GenerateDerivedKey(derivationAlgorithm, label, nonce, keySize, offset);
        }

        internal static string GenerateId()
        {
            return System.ServiceModel.Security.SecurityUniqueId.Create().Value;
        }

        internal static string GenerateIdWithPrefix(string prefix)
        {
            return System.ServiceModel.Security.SecurityUniqueId.Create(prefix).Value;
        }

        internal static UniqueId GenerateUniqueId()
        {
            return new UniqueId();
        }

        internal static X509Certificate2 GetCertificateFromStore(StoreName storeName, StoreLocation storeLocation, X509FindType findType, object findValue, EndpointAddress target)
        {
            X509Certificate2 certificate = GetCertificateFromStoreCore(storeName, storeLocation, findType, findValue, target, true);
            if (certificate == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotFindCert", new object[] { storeName, storeLocation, findType, findValue })));
            }
            return certificate;
        }

        private static X509Certificate2 GetCertificateFromStoreCore(StoreName storeName, StoreLocation storeLocation, X509FindType findType, object findValue, EndpointAddress target, bool throwIfMultipleOrNoMatch)
        {
            X509Certificate2 certificate;
            if (findValue == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("findValue");
            }
            X509CertificateStore store = new X509CertificateStore(storeName, storeLocation);
            X509Certificate2Collection certificates = null;
            try
            {
                store.Open(OpenFlags.ReadOnly);
                certificates = store.Find(findType, findValue, false);
                if (certificates.Count == 1)
                {
                    return new X509Certificate2(certificates[0]);
                }
                if (throwIfMultipleOrNoMatch)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateCertificateLoadException(storeName, storeLocation, findType, findValue, target, certificates.Count));
                }
                certificate = null;
            }
            finally
            {
                ResetAllCertificates(certificates);
                store.Close();
            }
            return certificate;
        }

        internal static string GetCertificateId(X509Certificate2 certificate)
        {
            StringBuilder str = new StringBuilder(0x100);
            AppendCertificateIdentityName(str, certificate);
            return str.ToString();
        }

        public static ChannelBinding GetChannelBindingFromMessage(Message message)
        {
            if (message == null)
            {
                return null;
            }
            ChannelBindingMessageProperty property = null;
            ChannelBindingMessageProperty.TryGet(message, out property);
            ChannelBinding channelBinding = null;
            if (property != null)
            {
                channelBinding = property.ChannelBinding;
            }
            return channelBinding;
        }

        internal static System.IdentityModel.SafeFreeCredentials GetCredentialsHandle(Binding binding, ClientCredentials clientCredentials)
        {
            SecurityBindingElement sbe = (binding == null) ? null : binding.CreateBindingElements().Find<SecurityBindingElement>();
            return GetCredentialsHandle(sbe, clientCredentials);
        }

        internal static System.IdentityModel.SafeFreeCredentials GetCredentialsHandle(Binding binding, KeyedByTypeCollection<IEndpointBehavior> behaviors)
        {
            ClientCredentials clientCredentials = (behaviors == null) ? null : behaviors.Find<ClientCredentials>();
            return GetCredentialsHandle(binding, clientCredentials);
        }

        internal static System.IdentityModel.SafeFreeCredentials GetCredentialsHandle(SecurityBindingElement sbe, BindingContext context)
        {
            ClientCredentials clientCredentials = (context == null) ? null : context.BindingParameters.Find<ClientCredentials>();
            return GetCredentialsHandle(sbe, clientCredentials);
        }

        internal static System.IdentityModel.SafeFreeCredentials GetCredentialsHandle(SecurityBindingElement sbe, ClientCredentials clientCredentials)
        {
            if (sbe == null)
            {
                return null;
            }
            bool flag = false;
            bool flag2 = false;
            foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(sbe, true))
            {
                if (parameters is SecureConversationSecurityTokenParameters)
                {
                    System.IdentityModel.SafeFreeCredentials credentialsHandle = GetCredentialsHandle(((SecureConversationSecurityTokenParameters) parameters).BootstrapSecurityBindingElement, clientCredentials);
                    if (credentialsHandle != null)
                    {
                        return credentialsHandle;
                    }
                }
                else if (parameters is IssuedSecurityTokenParameters)
                {
                    System.IdentityModel.SafeFreeCredentials credentials2 = GetCredentialsHandle(((IssuedSecurityTokenParameters) parameters).IssuerBinding, clientCredentials);
                    if (credentials2 != null)
                    {
                        return credentials2;
                    }
                }
                else
                {
                    if (parameters is SspiSecurityTokenParameters)
                    {
                        flag = true;
                        break;
                    }
                    if (parameters is KerberosSecurityTokenParameters)
                    {
                        flag2 = true;
                        break;
                    }
                }
            }
            if (!flag && !flag2)
            {
                return null;
            }
            NetworkCredential networkCredentialOrDefault = null;
            if (clientCredentials != null)
            {
                networkCredentialOrDefault = GetNetworkCredentialOrDefault(clientCredentials.Windows.ClientCredential);
            }
            if (!flag2)
            {
                if ((clientCredentials == null) || clientCredentials.Windows.AllowNtlm)
                {
                    return GetCredentialsHandle("Negotiate", networkCredentialOrDefault, false, new string[0]);
                }
                if (IsOsGreaterThanXP())
                {
                    return GetCredentialsHandle("Negotiate", networkCredentialOrDefault, false, new string[] { "!NTLM" });
                }
            }
            return GetCredentialsHandle("Kerberos", networkCredentialOrDefault, false, new string[0]);
        }

        internal static System.IdentityModel.SafeFreeCredentials GetCredentialsHandle(string package, NetworkCredential credential, bool isServer, params string[] additionalPackages)
        {
            System.IdentityModel.CredentialUse intent = isServer ? System.IdentityModel.CredentialUse.Inbound : System.IdentityModel.CredentialUse.Outbound;
            if ((credential == null) || NetworkCredentialHelper.IsDefault(credential))
            {
                AuthIdentityEx ex = new AuthIdentityEx(null, null, null, additionalPackages);
                return SspiWrapper.AcquireCredentialsHandle(package, intent, ref ex);
            }
            FixNetworkCredential(ref credential);
            AuthIdentityEx authdata = new AuthIdentityEx(credential.UserName, credential.Password, credential.Domain, new string[0]);
            return SspiWrapper.AcquireCredentialsHandle(package, intent, ref authdata);
        }

        [SecurityCritical, RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Lsa")]
        private static int GetFipsAlgorithmPolicyKeyFromRegistry()
        {
            int num = -1;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\Lsa", false))
            {
                if (key != null)
                {
                    object obj2 = key.GetValue("FIPSAlgorithmPolicy");
                    if (obj2 != null)
                    {
                        num = (int) obj2;
                    }
                }
            }
            return num;
        }

        internal static string GetIdentityName(IIdentity identity)
        {
            StringBuilder str = new StringBuilder(0x100);
            AppendIdentityName(str, identity);
            return str.ToString();
        }

        internal static string GetIdentityNamesFromContext(AuthorizationContext authContext)
        {
            if (authContext != null)
            {
                StringBuilder str = new StringBuilder(0x100);
                for (int i = 0; i < authContext.ClaimSets.Count; i++)
                {
                    ClaimSet set = authContext.ClaimSets[i];
                    WindowsClaimSet set2 = set as WindowsClaimSet;
                    if (set2 != null)
                    {
                        if (str.Length > 0)
                        {
                            str.Append(", ");
                        }
                        AppendIdentityName(str, set2.WindowsIdentity);
                    }
                    else
                    {
                        X509CertificateClaimSet set3 = set as X509CertificateClaimSet;
                        if (set3 != null)
                        {
                            if (str.Length > 0)
                            {
                                str.Append(", ");
                            }
                            AppendCertificateIdentityName(str, set3.X509Certificate);
                        }
                    }
                }
                if (str.Length <= 0)
                {
                    object obj2;
                    List<IIdentity> list = null;
                    if (authContext.Properties.TryGetValue("Identities", out obj2))
                    {
                        list = obj2 as List<IIdentity>;
                    }
                    if (list != null)
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            IIdentity identity = list[j];
                            if (identity != null)
                            {
                                if (str.Length > 0)
                                {
                                    str.Append(", ");
                                }
                                AppendIdentityName(str, identity);
                            }
                        }
                    }
                }
                if (str.Length > 0)
                {
                    return str.ToString();
                }
            }
            return string.Empty;
        }

        internal static string GetIdentityNamesFromPolicies(IList<IAuthorizationPolicy> authPolicies)
        {
            return GetIdentityNamesFromContext(AuthorizationContext.CreateDefaultAuthorizationContext(authPolicies));
        }

        public static SecurityBindingElement GetIssuerSecurityBindingElement(ServiceModelSecurityTokenRequirement requirement)
        {
            SecurityBindingElement secureConversationSecurityBindingElement = requirement.SecureConversationSecurityBindingElement;
            if (secureConversationSecurityBindingElement != null)
            {
                return secureConversationSecurityBindingElement;
            }
            Binding issuerBinding = requirement.IssuerBinding;
            if (issuerBinding == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("IssuerBindingNotPresentInTokenRequirement", new object[] { requirement }));
            }
            return issuerBinding.CreateBindingElements().Find<SecurityBindingElement>();
        }

        internal static string GetKeyDerivationAlgorithm(SecureConversationVersion version)
        {
            if (version == SecureConversationVersion.WSSecureConversationFeb2005)
            {
                return "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1";
            }
            if (version != SecureConversationVersion.WSSecureConversation13)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            return "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1";
        }

        internal static KeyedHashAlgorithm GetKeyedHashAlgorithm(string algorithm, SecurityToken token)
        {
            SymmetricSecurityKey securityKey = GetSecurityKey<SymmetricSecurityKey>(token);
            if ((securityKey != null) && securityKey.IsSupportedAlgorithm(algorithm))
            {
                return securityKey.GetKeyedHashAlgorithm(algorithm);
            }
            return null;
        }

        public static int GetMaxNegotiationBufferSize(BindingContext bindingContext)
        {
            TransportBindingElement element = bindingContext.RemainingBindingElements.Find<TransportBindingElement>();
            if (element is ConnectionOrientedTransportBindingElement)
            {
                return ((ConnectionOrientedTransportBindingElement) element).MaxBufferSize;
            }
            if (element is HttpTransportBindingElement)
            {
                return ((HttpTransportBindingElement) element).MaxBufferSize;
            }
            return 0x10000;
        }

        internal static NetworkCredential GetNetworkCredentialOrDefault(NetworkCredential credential)
        {
            if (NetworkCredentialHelper.IsNullOrEmpty(credential))
            {
                return CredentialCache.DefaultNetworkCredentials;
            }
            return credential;
        }

        [SecuritySafeCritical]
        internal static NetworkCredential GetNetworkCredentialsCopy(NetworkCredential networkCredential)
        {
            if ((networkCredential != null) && !NetworkCredentialHelper.IsDefault(networkCredential))
            {
                return new NetworkCredential(NetworkCredentialHelper.UnsafeGetUsername(networkCredential), NetworkCredentialHelper.UnsafeGetPassword(networkCredential), NetworkCredentialHelper.UnsafeGetDomain(networkCredential));
            }
            return networkCredential;
        }

        internal static string GetPrimaryDomain()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                return GetPrimaryDomain(IsSystemAccount(identity));
            }
        }

        internal static string GetPrimaryDomain(bool isSystemAccount)
        {
            if (!computedDomain)
            {
                try
                {
                    if (isSystemAccount)
                    {
                        currentDomain = Domain.GetComputerDomain().Name;
                    }
                    else
                    {
                        currentDomain = Domain.GetCurrentDomain().Name;
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                }
                finally
                {
                    computedDomain = true;
                }
            }
            return currentDomain;
        }

        internal static Claim GetPrimaryIdentityClaim(AuthorizationContext authContext)
        {
            if (authContext != null)
            {
                for (int i = 0; i < authContext.ClaimSets.Count; i++)
                {
                    ClaimSet set = authContext.ClaimSets[i];
                    using (IEnumerator<Claim> enumerator = set.FindClaims(null, Rights.Identity).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            return enumerator.Current;
                        }
                    }
                }
            }
            return null;
        }

        internal static Claim GetPrimaryIdentityClaim(ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            return GetPrimaryIdentityClaim(AuthorizationContext.CreateDefaultAuthorizationContext(authorizationPolicies));
        }

        internal static T GetSecurityKey<T>(SecurityToken token) where T: SecurityKey
        {
            T local = default(T);
            if (token.SecurityKeys != null)
            {
                for (int i = 0; i < token.SecurityKeys.Count; i++)
                {
                    T local2 = token.SecurityKeys[i] as T;
                    if (local2 != null)
                    {
                        if (local != null)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("MultipleMatchingCryptosFound", new object[] { typeof(T).ToString() })));
                        }
                        local = local2;
                    }
                }
            }
            return local;
        }

        internal static int GetServiceAddressAndViaHash(EndpointAddress sr)
        {
            if (sr == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sr");
            }
            return sr.GetHashCode();
        }

        internal static EndpointIdentity GetServiceCertificateIdentity(X509Certificate2 certificate)
        {
            using (X509CertificateClaimSet set = new X509CertificateClaimSet(certificate))
            {
                EndpointIdentity identity;
                if (!TryCreateIdentity(set, ClaimTypes.Dns, out identity))
                {
                    TryCreateIdentity(set, ClaimTypes.Rsa, out identity);
                }
                return identity;
            }
        }

        internal static string GetSpnFromIdentity(EndpointIdentity identity, EndpointAddress target)
        {
            bool flag = false;
            string resource = null;
            if (identity != null)
            {
                if (ClaimTypes.Spn.Equals(identity.IdentityClaim.ClaimType))
                {
                    resource = (string) identity.IdentityClaim.Resource;
                    flag = true;
                }
                else if (ClaimTypes.Upn.Equals(identity.IdentityClaim.ClaimType))
                {
                    resource = (string) identity.IdentityClaim.Resource;
                    flag = true;
                }
                else if (ClaimTypes.Dns.Equals(identity.IdentityClaim.ClaimType))
                {
                    resource = string.Format(CultureInfo.InvariantCulture, "host/{0}", new object[] { (string) identity.IdentityClaim.Resource });
                    flag = true;
                }
            }
            if (!flag)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("CannotDetermineSPNBasedOnAddress", new object[] { target })));
            }
            return resource;
        }

        internal static string GetSpnFromTarget(EndpointAddress target)
        {
            if (target == null)
            {
                throw Fx.AssertAndThrow("target should not be null - expecting an EndpointAddress");
            }
            return string.Format(CultureInfo.InvariantCulture, "host/{0}", new object[] { target.Uri.DnsSafeHost });
        }

        [SecurityCritical, RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Lsa")]
        internal static int GetSuppressChannelBindingValue()
        {
            int num = 0;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\Lsa", false))
                {
                    if (key != null)
                    {
                        object obj2 = key.GetValue("SuppressChannelBindingInfo");
                        if (obj2 != null)
                        {
                            num = (int) obj2;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
            }
            return num;
        }

        internal static SymmetricAlgorithm GetSymmetricAlgorithm(string algorithm, SecurityToken token)
        {
            SymmetricSecurityKey securityKey = GetSecurityKey<SymmetricSecurityKey>(token);
            if ((securityKey != null) && securityKey.IsSupportedAlgorithm(algorithm))
            {
                return securityKey.GetSymmetricAlgorithm(algorithm);
            }
            return null;
        }

        internal static bool HasSymmetricSecurityKey(SecurityToken token)
        {
            return (GetSecurityKey<SymmetricSecurityKey>(token) != null);
        }

        internal static bool IsCompositeDuplexBinding(BindingContext context)
        {
            if (context.Binding.Elements.Find<CompositeDuplexBindingElement>() == null)
            {
                return (context.Binding.Elements.Find<InternalDuplexBindingElement>() != null);
            }
            return true;
        }

        internal static bool IsCurrentlyTimeEffective(DateTime effectiveTime, DateTime expirationTime, TimeSpan maxClockSkew)
        {
            DateTime time = (effectiveTime < DateTime.MinValue.Add(maxClockSkew)) ? effectiveTime : effectiveTime.Subtract(maxClockSkew);
            DateTime time2 = (expirationTime > DateTime.MaxValue.Subtract(maxClockSkew)) ? expirationTime : expirationTime.Add(maxClockSkew);
            DateTime utcNow = DateTime.UtcNow;
            return ((time.ToUniversalTime() <= utcNow) && (utcNow < time2.ToUniversalTime()));
        }

        internal static bool IsDefaultNetworkCredential(NetworkCredential credential)
        {
            return NetworkCredentialHelper.IsDefault(credential);
        }

        internal static bool IsOSGreaterThanOrEqualToWin7()
        {
            Version version = new Version(6, 1, 0, 0);
            return ((Environment.OSVersion.Version.Major >= version.Major) && (Environment.OSVersion.Version.Minor >= version.Minor));
        }

        internal static bool IsOsGreaterThanXP()
        {
            return (((Environment.OSVersion.Version.Major >= 5) && (Environment.OSVersion.Version.Minor > 1)) || (Environment.OSVersion.Version.Major > 5));
        }

        internal static bool IsSecurityBindingSuitableForChannelBinding(TransportSecurityBindingElement securityBindingElement)
        {
            if (securityBindingElement == null)
            {
                return false;
            }
            return (AreSecurityTokenParametersSuitableForChannelBinding(securityBindingElement.EndpointSupportingTokenParameters.Endorsing) || (AreSecurityTokenParametersSuitableForChannelBinding(securityBindingElement.EndpointSupportingTokenParameters.Signed) || (AreSecurityTokenParametersSuitableForChannelBinding(securityBindingElement.EndpointSupportingTokenParameters.SignedEncrypted) || AreSecurityTokenParametersSuitableForChannelBinding(securityBindingElement.EndpointSupportingTokenParameters.SignedEndorsing))));
        }

        internal static bool IsSecurityFault(MessageFault fault, SecurityStandardsManager standardsManager)
        {
            if (fault.Code.IsSenderFault)
            {
                FaultCode subCode = fault.Code.SubCode;
                if (subCode != null)
                {
                    if ((!(subCode.Namespace == standardsManager.SecurityVersion.HeaderNamespace.Value) && !(subCode.Namespace == standardsManager.SecureConversationDriver.Namespace.Value)) && !(subCode.Namespace == standardsManager.TrustDriver.Namespace.Value))
                    {
                        return (subCode.Namespace == "http://schemas.microsoft.com/ws/2006/05/security");
                    }
                    return true;
                }
            }
            return false;
        }

        internal static bool IsSupportedAlgorithm(string algorithm, SecurityToken token)
        {
            if (token.SecurityKeys != null)
            {
                for (int i = 0; i < token.SecurityKeys.Count; i++)
                {
                    if (token.SecurityKeys[i].IsSupportedAlgorithm(algorithm))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsSystemAccount(WindowsIdentity self)
        {
            SecurityIdentifier user = self.User;
            if (user == null)
            {
                return false;
            }
            if ((!user.IsWellKnown(WellKnownSidType.LocalSystemSid) && !user.IsWellKnown(WellKnownSidType.NetworkServiceSid)) && !user.IsWellKnown(WellKnownSidType.LocalServiceSid))
            {
                return self.User.Value.StartsWith("S-1-5-82", StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        internal static void MatchRstWithEndpointFilter(Message rst, IMessageFilterTable<EndpointAddress> endpointFilterTable, Uri listenUri)
        {
            if (endpointFilterTable != null)
            {
                Collection<EndpointAddress> results = new Collection<EndpointAddress>();
                if (!endpointFilterTable.GetMatchingValues(rst, results))
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("RequestSecurityTokenDoesNotMatchEndpointFilters", new object[] { listenUri })), rst);
                }
            }
        }

        private static void OpenCommunicationObject(ICommunicationObject obj, TimeSpan timeout)
        {
            if (obj != null)
            {
                obj.Open(timeout);
            }
        }

        internal static void OpenTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout)
        {
            OpenCommunicationObject(tokenAuthenticator as ICommunicationObject, timeout);
        }

        internal static void OpenTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout)
        {
            OpenCommunicationObject(tokenProvider as ICommunicationObject, timeout);
        }

        internal static void PrepareNetworkCredential()
        {
            if (dummyNetworkCredential == null)
            {
                PrepareNetworkCredentialWorker();
            }
        }

        private static void PrepareNetworkCredentialWorker()
        {
            lock (dummyNetworkCredentialLock)
            {
                dummyNetworkCredential = new NetworkCredential("dummy", "dummy");
            }
        }

        internal static byte[] ReadContentAsBase64(XmlDictionaryReader reader, long maxBufferSize)
        {
            byte[] buffer;
            if (reader == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            byte[][] bufferArray = new byte[0x20][];
            int num = 0x180;
            int num2 = 0;
            int num3 = 0;
        Label_0026:
            buffer = new byte[num];
            bufferArray[num2++] = buffer;
            int index = 0;
            while (index < buffer.Length)
            {
                int num5 = reader.ReadContentAsBase64(buffer, index, buffer.Length - index);
                if (num5 == 0)
                {
                    break;
                }
                index += num5;
            }
            if (num3 > (maxBufferSize - index))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QuotaExceededException(System.ServiceModel.SR.GetString("BufferQuotaExceededReadingBase64", new object[] { maxBufferSize })));
            }
            num3 += index;
            if (index >= buffer.Length)
            {
                num *= 2;
                goto Label_0026;
            }
            buffer = new byte[num3];
            int dstOffset = 0;
            for (int i = 0; i < (num2 - 1); i++)
            {
                Buffer.BlockCopy(bufferArray[i], 0, buffer, dstOffset, bufferArray[i].Length);
                dstOffset += bufferArray[i].Length;
            }
            Buffer.BlockCopy(bufferArray[num2 - 1], 0, buffer, dstOffset, num3 - dstOffset);
            return buffer;
        }

        internal static void ResetAllCertificates(X509Certificate2Collection certificates)
        {
            if (certificates != null)
            {
                for (int i = 0; i < certificates.Count; i++)
                {
                    certificates[i].Reset();
                }
            }
        }

        internal static bool ShouldMatchRstWithEndpointFilter(SecurityBindingElement sbe)
        {
            foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(sbe, true))
            {
                if (parameters.HasAsymmetricKey)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ShouldValidateSslCipherStrength()
        {
            if (!isSslValidationRequirementDetermined)
            {
                shouldValidateSslCipherStrength = ComputeSslCipherStrengthRequirementFlag();
                Thread.MemoryBarrier();
                isSslValidationRequirementDetermined = true;
            }
            return shouldValidateSslCipherStrength;
        }

        internal static void ThrowIfNegotiationFault(Message message, EndpointAddress target)
        {
            if (message.IsFault)
            {
                MessageFault fault = MessageFault.CreateFault(message, 0x4000);
                Exception innerException = new FaultException(fault, message.Headers.Action);
                if (((fault.Code != null) && fault.Code.IsReceiverFault) && (fault.Code.SubCode != null))
                {
                    FaultCode subCode = fault.Code.SubCode;
                    if ((subCode.Name == "ServerTooBusy") && (subCode.Namespace == "http://schemas.microsoft.com/ws/2006/05/security"))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ServerTooBusyException(System.ServiceModel.SR.GetString("SecurityServerTooBusy", new object[] { target }), innerException));
                    }
                    if ((subCode.Name == "EndpointUnavailable") && (subCode.Namespace == message.Version.Addressing.Namespace))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("SecurityEndpointNotFound", new object[] { target }), innerException));
                    }
                }
                throw TraceUtility.ThrowHelperError(innerException, message);
            }
        }

        private static bool TryCreateIdentity(ClaimSet claimSet, string claimType, out EndpointIdentity identity)
        {
            identity = null;
            foreach (Claim claim in claimSet.FindClaims(claimType, null))
            {
                identity = EndpointIdentity.CreateIdentity(claim);
                return true;
            }
            return false;
        }

        public static bool TryCreateKeyFromIntrinsicKeyClause(SecurityKeyIdentifierClause keyIdentifierClause, SecurityTokenResolver resolver, out SecurityKey key)
        {
            key = null;
            if (keyIdentifierClause.CanCreateKey)
            {
                key = keyIdentifierClause.CreateKey();
                return true;
            }
            if (keyIdentifierClause is EncryptedKeyIdentifierClause)
            {
                EncryptedKeyIdentifierClause clause = (EncryptedKeyIdentifierClause) keyIdentifierClause;
                for (int i = 0; i < clause.EncryptingKeyIdentifier.Count; i++)
                {
                    SecurityKey key2 = null;
                    if (resolver.TryResolveSecurityKey(clause.EncryptingKeyIdentifier[i], out key2))
                    {
                        byte[] encryptedKey = clause.GetEncryptedKey();
                        string encryptionMethod = clause.EncryptionMethod;
                        byte[] symmetricKey = key2.DecryptKey(encryptionMethod, encryptedKey);
                        key = new InMemorySymmetricSecurityKey(symmetricKey, false);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TryCreateX509CertificateFromRawData(byte[] rawData, out X509Certificate2 certificate)
        {
            certificate = ((rawData == null) || (rawData.Length == 0)) ? null : new X509Certificate2(rawData);
            return ((certificate != null) && (certificate.Handle != IntPtr.Zero));
        }

        internal static bool TryGetCertificateFromStore(StoreName storeName, StoreLocation storeLocation, X509FindType findType, object findValue, EndpointAddress target, out X509Certificate2 certificate)
        {
            certificate = GetCertificateFromStoreCore(storeName, storeLocation, findType, findValue, target, false);
            return (certificate != null);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, ControlPrincipal=true, UnmanagedCode=true)]
        private static WindowsIdentity UnsafeCreateWindowsIdentityFromToken(IntPtr token, string authType)
        {
            if (authType != null)
            {
                return new WindowsIdentity(token, authType);
            }
            return new WindowsIdentity(token);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        private static string UnsafeGetCurrentUserSidAsString()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                return identity.User.Value;
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        private static IntPtr UnsafeGetWindowsIdentityToken(WindowsIdentity wid)
        {
            return wid.Token;
        }

        public static void ValidateAnonymityConstraint(WindowsIdentity identity, bool allowUnauthenticatedCallers)
        {
            if (!allowUnauthenticatedCallers && identity.User.IsWellKnown(WellKnownSidType.AnonymousSid))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityTokenValidationException(System.ServiceModel.SR.GetString("AnonymousLogonsAreNotAllowed")));
            }
        }

        public static void ValidateSslCipherStrength(int keySizeInBits)
        {
            if (ShouldValidateSslCipherStrength() && (keySizeInBits < MinimumSslCipherStrength))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("SslCipherKeyTooSmall", new object[] { keySizeInBits, MinimumSslCipherStrength })));
            }
        }

        public static SecurityIdentifier AdministratorsSid
        {
            get
            {
                if (administratorsSid == null)
                {
                    administratorsSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                }
                return administratorsSid;
            }
        }

        internal static IIdentity AnonymousIdentity
        {
            get
            {
                if (anonymousIdentity == null)
                {
                    anonymousIdentity = CreateIdentity(string.Empty);
                }
                return anonymousIdentity;
            }
        }

        internal static byte[] CombinedHashLabel
        {
            get
            {
                if (combinedHashLabel == null)
                {
                    combinedHashLabel = Encoding.UTF8.GetBytes("AUTH-HASH");
                }
                return combinedHashLabel;
            }
        }

        internal static bool IsChannelBindingDisabled
        {
            [SecuritySafeCritical]
            get
            {
                return ((GetSuppressChannelBindingValue() & 1) != 0);
            }
        }

        public static DateTime MaxUtcDateTime
        {
            get
            {
                return new DateTime(DateTime.MaxValue.Ticks - 0xc92a69c000L, DateTimeKind.Utc);
            }
        }

        public static DateTime MinUtcDateTime
        {
            get
            {
                return new DateTime(DateTime.MinValue.Ticks + 0xc92a69c000L, DateTimeKind.Utc);
            }
        }

        internal static X509SecurityTokenAuthenticator NonValidatingX509Authenticator
        {
            get
            {
                if (nonValidatingX509Authenticator == null)
                {
                    nonValidatingX509Authenticator = new X509SecurityTokenAuthenticator(X509CertificateValidator.None);
                }
                return nonValidatingX509Authenticator;
            }
        }

        internal static bool RequiresFipsCompliance
        {
            [SecuritySafeCritical]
            get
            {
                if (fipsAlgorithmPolicy == -1)
                {
                    if (OSEnvironmentHelper.IsVistaOrGreater)
                    {
                        bool flag;
                        if ((0 == System.ServiceModel.Channels.UnsafeNativeMethods.BCryptGetFipsAlgorithmMode(out flag)) && flag)
                        {
                            fipsAlgorithmPolicy = 1;
                        }
                        else
                        {
                            fipsAlgorithmPolicy = 0;
                        }
                    }
                    else
                    {
                        fipsAlgorithmPolicy = GetFipsAlgorithmPolicyKeyFromRegistry();
                        if (fipsAlgorithmPolicy != 1)
                        {
                            fipsAlgorithmPolicy = 0;
                        }
                    }
                }
                return (fipsAlgorithmPolicy == 1);
            }
        }

        private class CloseCommunicationObjectAsyncResult : AsyncResult
        {
            private ICommunicationObject communicationObject;
            private static AsyncCallback onClose;

            public CloseCommunicationObjectAsyncResult(object obj, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = obj as ICommunicationObject;
                bool flag = false;
                if (this.communicationObject == null)
                {
                    IDisposable disposable = obj as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                    flag = true;
                }
                else
                {
                    if (onClose == null)
                    {
                        onClose = Fx.ThunkCallback(new AsyncCallback(System.ServiceModel.Security.SecurityUtils.CloseCommunicationObjectAsyncResult.OnClose));
                    }
                    IAsyncResult result = this.communicationObject.BeginClose(timeout, onClose, this);
                    if (result.CompletedSynchronously)
                    {
                        this.communicationObject.EndClose(result);
                        flag = true;
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<System.ServiceModel.Security.SecurityUtils.CloseCommunicationObjectAsyncResult>(result);
            }

            private static void OnClose(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseCommunicationObjectAsyncResult asyncState = (System.ServiceModel.Security.SecurityUtils.CloseCommunicationObjectAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.communicationObject.EndClose(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }
        }

        private static class NetworkCredentialHelper
        {
            [SecuritySafeCritical]
            internal static bool IsDefault(NetworkCredential credential)
            {
                return UnsafeGetDefaultNetworkCredentials().Equals(credential);
            }

            [SecuritySafeCritical]
            internal static bool IsNullOrEmpty(NetworkCredential credential)
            {
                return ((credential == null) || ((string.IsNullOrEmpty(UnsafeGetUsername(credential)) && string.IsNullOrEmpty(UnsafeGetDomain(credential))) && string.IsNullOrEmpty(UnsafeGetPassword(credential))));
            }

            [SecurityCritical, EnvironmentPermission(SecurityAction.Assert, Read="USERNAME")]
            private static NetworkCredential UnsafeGetDefaultNetworkCredentials()
            {
                return CredentialCache.DefaultNetworkCredentials;
            }

            [SecurityCritical, EnvironmentPermission(SecurityAction.Assert, Read="USERDOMAIN")]
            internal static string UnsafeGetDomain(NetworkCredential credential)
            {
                return credential.Domain;
            }

            [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
            internal static string UnsafeGetPassword(NetworkCredential credential)
            {
                return credential.Password;
            }

            [SecurityCritical, EnvironmentPermission(SecurityAction.Assert, Read="USERNAME")]
            internal static string UnsafeGetUsername(NetworkCredential credential)
            {
                return credential.UserName;
            }
        }

        private class OpenCommunicationObjectAsyncResult : AsyncResult
        {
            private ICommunicationObject communicationObject;
            private static AsyncCallback onOpen;

            public OpenCommunicationObjectAsyncResult(object obj, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = obj as ICommunicationObject;
                bool flag = false;
                if (this.communicationObject == null)
                {
                    flag = true;
                }
                else
                {
                    if (onOpen == null)
                    {
                        onOpen = Fx.ThunkCallback(new AsyncCallback(System.ServiceModel.Security.SecurityUtils.OpenCommunicationObjectAsyncResult.OnOpen));
                    }
                    IAsyncResult result = this.communicationObject.BeginOpen(timeout, onOpen, this);
                    if (result.CompletedSynchronously)
                    {
                        this.communicationObject.EndOpen(result);
                        flag = true;
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<System.ServiceModel.Security.SecurityUtils.OpenCommunicationObjectAsyncResult>(result);
            }

            private static void OnOpen(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    System.ServiceModel.Security.SecurityUtils.OpenCommunicationObjectAsyncResult asyncState = (System.ServiceModel.Security.SecurityUtils.OpenCommunicationObjectAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.communicationObject.EndOpen(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }
        }
    }
}

