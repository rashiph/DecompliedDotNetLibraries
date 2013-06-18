namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    public class X509SecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        private bool cloneHandle;
        private bool includeWindowsGroups;
        private bool mapToWindows;
        private X509CertificateValidator validator;

        public X509SecurityTokenAuthenticator() : this(X509CertificateValidator.ChainTrust)
        {
        }

        public X509SecurityTokenAuthenticator(X509CertificateValidator validator) : this(validator, false)
        {
        }

        public X509SecurityTokenAuthenticator(X509CertificateValidator validator, bool mapToWindows) : this(validator, mapToWindows, true)
        {
        }

        public X509SecurityTokenAuthenticator(X509CertificateValidator validator, bool mapToWindows, bool includeWindowsGroups) : this(validator, mapToWindows, includeWindowsGroups, true)
        {
        }

        internal X509SecurityTokenAuthenticator(X509CertificateValidator validator, bool mapToWindows, bool includeWindowsGroups, bool cloneHandle)
        {
            if (validator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("validator");
            }
            this.validator = validator;
            this.mapToWindows = mapToWindows;
            this.includeWindowsGroups = includeWindowsGroups;
            this.cloneHandle = cloneHandle;
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is X509SecurityToken);
        }

        private static unsafe WindowsIdentity KerberosCertificateLogon(X509Certificate2 certificate)
        {
            SafeHGlobalHandle handle = null;
            SafeHGlobalHandle handle2 = null;
            SafeHGlobalHandle handle3 = null;
            SafeLsaLogonProcessHandle lsaHandle = null;
            SafeLsaReturnBufferHandle profileBuffer = null;
            SafeCloseHandle token = null;
            WindowsIdentity identity;
            try
            {
                int num;
                uint num6;
                handle = SafeHGlobalHandle.AllocHGlobal((int) (System.IdentityModel.NativeMethods.LsaSourceName.Length + 1));
                Marshal.Copy(System.IdentityModel.NativeMethods.LsaSourceName, 0, handle.DangerousGetHandle(), System.IdentityModel.NativeMethods.LsaSourceName.Length);
                UNICODE_INTPTR_STRING logonProcessName = new UNICODE_INTPTR_STRING(System.IdentityModel.NativeMethods.LsaSourceName.Length, System.IdentityModel.NativeMethods.LsaSourceName.Length + 1, handle.DangerousGetHandle());
                System.IdentityModel.Privilege privilege = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    try
                    {
                        privilege = new System.IdentityModel.Privilege("SeTcbPrivilege");
                        privilege.Enable();
                    }
                    catch (PrivilegeNotHeldException exception)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                    }
                    IntPtr zero = IntPtr.Zero;
                    num = System.IdentityModel.NativeMethods.LsaRegisterLogonProcess(ref logonProcessName, out lsaHandle, out zero);
                    if (5 == System.IdentityModel.NativeMethods.LsaNtStatusToWinError(num))
                    {
                        num = System.IdentityModel.NativeMethods.LsaConnectUntrusted(out lsaHandle);
                    }
                    if (num < 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(System.IdentityModel.NativeMethods.LsaNtStatusToWinError(num)));
                    }
                }
                finally
                {
                    int error = -1;
                    string message = null;
                    try
                    {
                        error = privilege.Revert();
                        if (error != 0)
                        {
                            message = System.IdentityModel.SR.GetString("RevertingPrivilegeFailed", new object[] { new Win32Exception(error) });
                        }
                    }
                    finally
                    {
                        if (error != 0)
                        {
                            DiagnosticUtility.FailFast(message);
                        }
                    }
                }
                handle2 = SafeHGlobalHandle.AllocHGlobal((int) (System.IdentityModel.NativeMethods.LsaKerberosName.Length + 1));
                Marshal.Copy(System.IdentityModel.NativeMethods.LsaKerberosName, 0, handle2.DangerousGetHandle(), System.IdentityModel.NativeMethods.LsaKerberosName.Length);
                UNICODE_INTPTR_STRING packageName = new UNICODE_INTPTR_STRING(System.IdentityModel.NativeMethods.LsaKerberosName.Length, System.IdentityModel.NativeMethods.LsaKerberosName.Length + 1, handle2.DangerousGetHandle());
                uint authenticationPackage = 0;
                num = System.IdentityModel.NativeMethods.LsaLookupAuthenticationPackage(lsaHandle, ref packageName, out authenticationPackage);
                if (num < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(System.IdentityModel.NativeMethods.LsaNtStatusToWinError(num)));
                }
                TOKEN_SOURCE sourceContext = new TOKEN_SOURCE();
                if (!System.IdentityModel.NativeMethods.AllocateLocallyUniqueId(out sourceContext.SourceIdentifier))
                {
                    int num4 = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(num4));
                }
                sourceContext.Name = new char[8];
                sourceContext.Name[0] = 'W';
                sourceContext.Name[1] = 'C';
                sourceContext.Name[2] = 'F';
                byte[] rawData = certificate.RawData;
                int cb = KERB_CERTIFICATE_S4U_LOGON.Size + rawData.Length;
                handle3 = SafeHGlobalHandle.AllocHGlobal(cb);
                KERB_CERTIFICATE_S4U_LOGON* kerb_certificate_su_logonPtr = (KERB_CERTIFICATE_S4U_LOGON*) handle3.DangerousGetHandle().ToPointer();
                kerb_certificate_su_logonPtr->MessageType = KERB_LOGON_SUBMIT_TYPE.KerbCertificateS4ULogon;
                kerb_certificate_su_logonPtr->Flags = 2;
                kerb_certificate_su_logonPtr->UserPrincipalName = new UNICODE_INTPTR_STRING(0, 0, IntPtr.Zero);
                kerb_certificate_su_logonPtr->DomainName = new UNICODE_INTPTR_STRING(0, 0, IntPtr.Zero);
                kerb_certificate_su_logonPtr->CertificateLength = (uint) rawData.Length;
                kerb_certificate_su_logonPtr->Certificate = new IntPtr(handle3.DangerousGetHandle().ToInt64() + KERB_CERTIFICATE_S4U_LOGON.Size);
                Marshal.Copy(rawData, 0, kerb_certificate_su_logonPtr->Certificate, rawData.Length);
                QUOTA_LIMITS quotas = new QUOTA_LIMITS();
                LUID logonId = new LUID();
                int subStatus = 0;
                num = System.IdentityModel.NativeMethods.LsaLogonUser(lsaHandle, ref logonProcessName, System.IdentityModel.SecurityLogonType.Network, authenticationPackage, handle3.DangerousGetHandle(), (uint) cb, IntPtr.Zero, ref sourceContext, out profileBuffer, out num6, out logonId, out token, out quotas, out subStatus);
                if ((num == -1073741714) && (subStatus < 0))
                {
                    num = subStatus;
                }
                if (num < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(System.IdentityModel.NativeMethods.LsaNtStatusToWinError(num)));
                }
                if (subStatus < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(System.IdentityModel.NativeMethods.LsaNtStatusToWinError(subStatus)));
                }
                identity = new WindowsIdentity(token.DangerousGetHandle(), "SSL/PCT");
            }
            finally
            {
                if (token != null)
                {
                    token.Close();
                }
                if (handle3 != null)
                {
                    handle3.Close();
                }
                if (profileBuffer != null)
                {
                    profileBuffer.Close();
                }
                if (handle != null)
                {
                    handle.Close();
                }
                if (handle2 != null)
                {
                    handle2.Close();
                }
                if (lsaHandle != null)
                {
                    lsaHandle.Close();
                }
            }
            return identity;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            WindowsClaimSet set2;
            X509SecurityToken token2 = (X509SecurityToken) token;
            this.validator.Validate(token2.Certificate);
            X509CertificateClaimSet claimSet = new X509CertificateClaimSet(token2.Certificate, this.cloneHandle);
            if (!this.mapToWindows)
            {
                return System.IdentityModel.SecurityUtils.CreateAuthorizationPolicies(claimSet, token2.ValidTo);
            }
            if (token is X509WindowsSecurityToken)
            {
                set2 = new WindowsClaimSet(((X509WindowsSecurityToken) token).WindowsIdentity, "SSL/PCT", this.includeWindowsGroups, this.cloneHandle);
            }
            else
            {
                X509CertificateValidator.NTAuthChainTrust.Validate(token2.Certificate);
                WindowsIdentity windowsIdentity = null;
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    windowsIdentity = KerberosCertificateLogon(token2.Certificate);
                }
                else
                {
                    string nameInfo = token2.Certificate.GetNameInfo(X509NameType.UpnName, false);
                    if (string.IsNullOrEmpty(nameInfo))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(System.IdentityModel.SR.GetString("InvalidNtMapping", new object[] { System.IdentityModel.SecurityUtils.GetCertificateId(token2.Certificate) })));
                    }
                    using (WindowsIdentity identity2 = new WindowsIdentity(nameInfo, "SSL/PCT"))
                    {
                        windowsIdentity = new WindowsIdentity(identity2.Token, "SSL/PCT");
                    }
                }
                set2 = new WindowsClaimSet(windowsIdentity, "SSL/PCT", this.includeWindowsGroups, false);
            }
            List<ClaimSet> list = new List<ClaimSet>(2) {
                set2,
                claimSet
            };
            return new List<IAuthorizationPolicy>(1) { new UnconditionalPolicy(list.AsReadOnly(), token2.ValidTo) }.AsReadOnly();
        }

        public bool MapCertificateToWindowsAccount
        {
            get
            {
                return this.mapToWindows;
            }
        }
    }
}

