namespace System.IdentityModel
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    internal static class SecurityUtils
    {
        private static IIdentity anonymousIdentity;
        public const string AuthTypeAnonymous = "";
        public const string AuthTypeBasic = "Basic";
        public const string AuthTypeCertMap = "SSL/PCT";
        public const string AuthTypeKerberos = "Kerberos";
        public const string AuthTypeNegotiate = "Negotiate";
        public const string AuthTypeNTLM = "NTLM";
        private static int fipsAlgorithmPolicy = -1;
        private const string fipsPolicyRegistryKey = @"System\CurrentControlSet\Control\Lsa";
        public const string Identities = "Identities";
        public const int WindowsVistaMajorNumber = 6;

        internal static string ClaimSetToString(ClaimSet claimSet)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("ClaimSet [");
            for (int i = 0; i < claimSet.Count; i++)
            {
                Claim claim = claimSet[i];
                if (claim != null)
                {
                    builder.Append("  ");
                    builder.AppendLine(claim.ToString());
                }
            }
            string str = "] by ";
            ClaimSet issuer = claimSet;
            do
            {
                issuer = issuer.Issuer;
                builder.AppendFormat("{0}{1}", str, (issuer == claimSet) ? "Self" : ((issuer.Count <= 0) ? "Unknown" : issuer[0].ToString()));
                str = " -> ";
            }
            while (issuer.Issuer != issuer);
            return builder.ToString();
        }

        internal static ReadOnlyCollection<IAuthorizationPolicy> CloneAuthorizationPoliciesIfNecessary(ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if ((authorizationPolicies == null) || (authorizationPolicies.Count <= 0))
            {
                return authorizationPolicies;
            }
            bool flag = false;
            for (int i = 0; i < authorizationPolicies.Count; i++)
            {
                UnconditionalPolicy policy = authorizationPolicies[i] as UnconditionalPolicy;
                if ((policy != null) && policy.IsDisposable)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return authorizationPolicies;
            }
            List<IAuthorizationPolicy> list = new List<IAuthorizationPolicy>(authorizationPolicies.Count);
            for (int j = 0; j < authorizationPolicies.Count; j++)
            {
                UnconditionalPolicy policy2 = authorizationPolicies[j] as UnconditionalPolicy;
                if (policy2 != null)
                {
                    list.Add(policy2.Clone());
                }
                else
                {
                    list.Add(authorizationPolicies[j]);
                }
            }
            return list.AsReadOnly();
        }

        internal static byte[] CloneBuffer(byte[] buffer)
        {
            return CloneBuffer(buffer, 0, buffer.Length);
        }

        internal static byte[] CloneBuffer(byte[] buffer, int offset, int len)
        {
            byte[] dst = DiagnosticUtility.Utility.AllocateByteArray(len);
            Buffer.BlockCopy(buffer, offset, dst, 0, len);
            return dst;
        }

        internal static ClaimSet CloneClaimSetIfNecessary(ClaimSet claimSet)
        {
            if (claimSet != null)
            {
                WindowsClaimSet set = claimSet as WindowsClaimSet;
                if (set != null)
                {
                    return set.Clone();
                }
            }
            return claimSet;
        }

        internal static ReadOnlyCollection<ClaimSet> CloneClaimSetsIfNecessary(ReadOnlyCollection<ClaimSet> claimSets)
        {
            if (claimSets == null)
            {
                return claimSets;
            }
            bool flag = false;
            for (int i = 0; i < claimSets.Count; i++)
            {
                if (claimSets[i] is WindowsClaimSet)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return claimSets;
            }
            List<ClaimSet> list = new List<ClaimSet>(claimSets.Count);
            for (int j = 0; j < claimSets.Count; j++)
            {
                list.Add(CloneClaimSetIfNecessary(claimSets[j]));
            }
            return list.AsReadOnly();
        }

        internal static IIdentity CloneIdentityIfNecessary(IIdentity identity)
        {
            if (identity != null)
            {
                WindowsIdentity wid = identity as WindowsIdentity;
                if (wid != null)
                {
                    return CloneWindowsIdentityIfNecessary(wid);
                }
            }
            return identity;
        }

        [SecuritySafeCritical]
        internal static WindowsIdentity CloneWindowsIdentityIfNecessary(WindowsIdentity wid)
        {
            return CloneWindowsIdentityIfNecessary(wid, wid.AuthenticationType);
        }

        [SecuritySafeCritical]
        internal static WindowsIdentity CloneWindowsIdentityIfNecessary(WindowsIdentity wid, string authenticationType)
        {
            if (wid != null)
            {
                IntPtr token = UnsafeGetWindowsIdentityToken(wid);
                if (token != IntPtr.Zero)
                {
                    return UnsafeCreateWindowsIdentityFromToken(token, authenticationType);
                }
            }
            return wid;
        }

        private static int ConvertHexDigit(char val)
        {
            if ((val <= '9') && (val >= '0'))
            {
                return (val - '0');
            }
            if ((val >= 'a') && (val <= 'f'))
            {
                return ((val - 'a') + 10);
            }
            if ((val < 'A') || (val > 'F'))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.IdentityModel.SR.GetString("InvalidHexString")));
            }
            return ((val - 'A') + 10);
        }

        internal static ReadOnlyCollection<IAuthorizationPolicy> CreateAuthorizationPolicies(ClaimSet claimSet)
        {
            return CreateAuthorizationPolicies(claimSet, MaxUtcDateTime);
        }

        internal static ReadOnlyCollection<IAuthorizationPolicy> CreateAuthorizationPolicies(ClaimSet claimSet, DateTime expirationTime)
        {
            return new List<IAuthorizationPolicy>(1) { new UnconditionalPolicy(claimSet, expirationTime) }.AsReadOnly();
        }

        internal static AuthorizationContext CreateDefaultAuthorizationContext(IList<IAuthorizationPolicy> authorizationPolicies)
        {
            AuthorizationContext context;
            if (((authorizationPolicies != null) && (authorizationPolicies.Count == 1)) && (authorizationPolicies[0] is UnconditionalPolicy))
            {
                context = new SimpleAuthorizationContext(authorizationPolicies);
            }
            else
            {
                int generation;
                if ((authorizationPolicies == null) || (authorizationPolicies.Count <= 0))
                {
                    return DefaultAuthorizationContext.Empty;
                }
                DefaultEvaluationContext evaluationContext = new DefaultEvaluationContext();
                object[] objArray = new object[authorizationPolicies.Count];
                object obj2 = new object();
                do
                {
                    generation = evaluationContext.Generation;
                    for (int i = 0; i < authorizationPolicies.Count; i++)
                    {
                        if (objArray[i] != obj2)
                        {
                            IAuthorizationPolicy policy = authorizationPolicies[i];
                            if (policy == null)
                            {
                                objArray[i] = obj2;
                            }
                            else if (policy.Evaluate(evaluationContext, ref objArray[i]))
                            {
                                objArray[i] = obj2;
                                if (DiagnosticUtility.ShouldTraceVerbose)
                                {
                                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0xc0003, System.IdentityModel.SR.GetString("AuthorizationPolicyEvaluated", new object[] { policy.Id }));
                                }
                            }
                        }
                    }
                }
                while (generation < evaluationContext.Generation);
                context = new DefaultAuthorizationContext(evaluationContext);
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xc0002, System.IdentityModel.SR.GetString("AuthorizationContextCreated", new object[] { context.Id }));
            }
            return context;
        }

        internal static IIdentity CreateIdentity(string name)
        {
            return new GenericIdentity(name);
        }

        internal static IIdentity CreateIdentity(string name, string authenticationType)
        {
            return new GenericIdentity(name, authenticationType);
        }

        internal static byte[] DecodeHexString(string hexString)
        {
            byte[] buffer;
            hexString = hexString.Trim();
            bool flag = false;
            int num = 0;
            int length = hexString.Length;
            if (((length >= 2) && (hexString[0] == '0')) && ((hexString[1] == 'x') || (hexString[1] == 'X')))
            {
                length = hexString.Length - 2;
                num = 2;
            }
            if (length < 2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.IdentityModel.SR.GetString("InvalidHexString")));
            }
            if ((length >= 3) && (hexString[num + 2] == ' '))
            {
                if ((length % 3) != 2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.IdentityModel.SR.GetString("InvalidHexString")));
                }
                flag = true;
                buffer = DiagnosticUtility.Utility.AllocateByteArray((length / 3) + 1);
            }
            else
            {
                if ((length % 2) != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.IdentityModel.SR.GetString("InvalidHexString")));
                }
                flag = false;
                buffer = DiagnosticUtility.Utility.AllocateByteArray(length / 2);
            }
            for (int i = 0; num < hexString.Length; i++)
            {
                int num4 = ConvertHexDigit(hexString[num]);
                int num3 = ConvertHexDigit(hexString[num + 1]);
                buffer[i] = (byte) (num3 | (num4 << 4));
                if (flag)
                {
                    num++;
                }
                num += 2;
            }
            return buffer;
        }

        public static void DisposeAuthorizationPoliciesIfNecessary(ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if ((authorizationPolicies != null) && (authorizationPolicies.Count > 0))
            {
                for (int i = 0; i < authorizationPolicies.Count; i++)
                {
                    DisposeIfNecessary(authorizationPolicies[i] as UnconditionalPolicy);
                }
            }
        }

        internal static void DisposeClaimSetIfNecessary(ClaimSet claimSet)
        {
            if (claimSet != null)
            {
                DisposeIfNecessary(claimSet as WindowsClaimSet);
            }
        }

        internal static void DisposeClaimSetsIfNecessary(ReadOnlyCollection<ClaimSet> claimSets)
        {
            if (claimSets != null)
            {
                for (int i = 0; i < claimSets.Count; i++)
                {
                    DisposeIfNecessary(claimSets[i] as WindowsClaimSet);
                }
            }
        }

        public static void DisposeIfNecessary(IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
            }
        }

        internal static string GenerateId()
        {
            return SecurityUniqueId.Create().Value;
        }

        internal static string GetCertificateId(X509Certificate2 certificate)
        {
            string name = certificate.SubjectName.Name;
            if (string.IsNullOrEmpty(name))
            {
                name = certificate.Thumbprint;
            }
            return name;
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

        internal static bool IsCurrentlyTimeEffective(DateTime effectiveTime, DateTime expirationTime, TimeSpan maxClockSkew)
        {
            DateTime time = (effectiveTime < DateTime.MinValue.Add(maxClockSkew)) ? effectiveTime : effectiveTime.Subtract(maxClockSkew);
            DateTime time2 = (expirationTime > DateTime.MaxValue.Subtract(maxClockSkew)) ? expirationTime : expirationTime.Add(maxClockSkew);
            DateTime utcNow = DateTime.UtcNow;
            return ((time.ToUniversalTime() <= utcNow) && (utcNow < time2.ToUniversalTime()));
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

        internal static bool MatchesBuffer(byte[] src, byte[] dst)
        {
            return MatchesBuffer(src, 0, dst, 0);
        }

        internal static bool MatchesBuffer(byte[] src, int srcOffset, byte[] dst, int dstOffset)
        {
            if ((dstOffset < 0) || (srcOffset < 0))
            {
                return false;
            }
            if ((src == null) || (srcOffset >= src.Length))
            {
                return false;
            }
            if ((dst == null) || (dstOffset >= dst.Length))
            {
                return false;
            }
            if ((src.Length - srcOffset) != (dst.Length - dstOffset))
            {
                return false;
            }
            int index = srcOffset;
            for (int i = dstOffset; index < src.Length; i++)
            {
                if (src[index] != dst[i])
                {
                    return false;
                }
                index++;
            }
            return true;
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

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, ControlPrincipal=true, UnmanagedCode=true)]
        private static WindowsIdentity UnsafeCreateWindowsIdentityFromToken(IntPtr token, string authenticationType)
        {
            if (authenticationType != null)
            {
                return new WindowsIdentity(token, authenticationType);
            }
            return new WindowsIdentity(token);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        private static IntPtr UnsafeGetWindowsIdentityToken(WindowsIdentity wid)
        {
            return wid.Token;
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

        internal static bool RequiresFipsCompliance
        {
            [SecuritySafeCritical]
            get
            {
                if (fipsAlgorithmPolicy == -1)
                {
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        bool flag;
                        if ((0 == CAPI.BCryptGetFipsAlgorithmMode(out flag)) && flag)
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

        private class SimpleAuthorizationContext : AuthorizationContext
        {
            private SecurityUniqueId id;
            private UnconditionalPolicy policy;
            private IDictionary<string, object> properties;

            public SimpleAuthorizationContext(IList<IAuthorizationPolicy> authorizationPolicies)
            {
                this.policy = (UnconditionalPolicy) authorizationPolicies[0];
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                if ((this.policy.PrimaryIdentity != null) && (this.policy.PrimaryIdentity != System.IdentityModel.SecurityUtils.AnonymousIdentity))
                {
                    List<IIdentity> list = new List<IIdentity> {
                        this.policy.PrimaryIdentity
                    };
                    dictionary.Add("Identities", list);
                }
                this.properties = dictionary;
            }

            public override ReadOnlyCollection<ClaimSet> ClaimSets
            {
                get
                {
                    return this.policy.Issuances;
                }
            }

            public override DateTime ExpirationTime
            {
                get
                {
                    return this.policy.ExpirationTime;
                }
            }

            public override string Id
            {
                get
                {
                    if (this.id == null)
                    {
                        this.id = SecurityUniqueId.Create();
                    }
                    return this.id.Value;
                }
            }

            public override IDictionary<string, object> Properties
            {
                get
                {
                    return this.properties;
                }
            }
        }
    }
}

