namespace System.IdentityModel.Claims
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    internal class ClaimComparer : IEqualityComparer<Claim>
    {
        private static IEqualityComparer<Claim> defaultComparer;
        private static IEqualityComparer<Claim> dnsComparer;
        private static IEqualityComparer<Claim> hashComparer;
        private IEqualityComparer resourceComparer;
        private static IEqualityComparer<Claim> rsaComparer;
        private static IEqualityComparer<Claim> thumbprintComparer;
        private static IEqualityComparer<Claim> upnComparer;
        private static IEqualityComparer<Claim> x500DistinguishedNameComparer;

        private ClaimComparer(IEqualityComparer resourceComparer)
        {
            this.resourceComparer = resourceComparer;
        }

        public bool Equals(Claim claim1, Claim claim2)
        {
            if (object.ReferenceEquals(claim1, claim2))
            {
                return true;
            }
            if ((claim1 == null) || (claim2 == null))
            {
                return false;
            }
            return ((!(claim1.ClaimType != claim2.ClaimType) && !(claim1.Right != claim2.Right)) && this.resourceComparer.Equals(claim1.Resource, claim2.Resource));
        }

        public static IEqualityComparer<Claim> GetComparer(string claimType)
        {
            if (claimType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimType");
            }
            if (claimType == ClaimTypes.Dns)
            {
                return Dns;
            }
            if (claimType == ClaimTypes.Hash)
            {
                return Hash;
            }
            if (claimType == ClaimTypes.Rsa)
            {
                return Rsa;
            }
            if (claimType == ClaimTypes.Thumbprint)
            {
                return Thumbprint;
            }
            if (claimType == ClaimTypes.Upn)
            {
                return Upn;
            }
            if (claimType == ClaimTypes.X500DistinguishedName)
            {
                return X500DistinguishedName;
            }
            return Default;
        }

        public int GetHashCode(Claim claim)
        {
            if (claim == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            }
            return ((claim.ClaimType.GetHashCode() ^ claim.Right.GetHashCode()) ^ ((claim.Resource == null) ? 0 : this.resourceComparer.GetHashCode(claim.Resource)));
        }

        public static IEqualityComparer<Claim> Default
        {
            get
            {
                if (defaultComparer == null)
                {
                    defaultComparer = new ClaimComparer(new ObjectComparer());
                }
                return defaultComparer;
            }
        }

        public static IEqualityComparer<Claim> Dns
        {
            get
            {
                if (dnsComparer == null)
                {
                    dnsComparer = new ClaimComparer(StringComparer.OrdinalIgnoreCase);
                }
                return dnsComparer;
            }
        }

        public static IEqualityComparer<Claim> Hash
        {
            get
            {
                if (hashComparer == null)
                {
                    hashComparer = new ClaimComparer(new BinaryObjectComparer());
                }
                return hashComparer;
            }
        }

        public static IEqualityComparer<Claim> Rsa
        {
            get
            {
                if (rsaComparer == null)
                {
                    rsaComparer = new ClaimComparer(new RsaObjectComparer());
                }
                return rsaComparer;
            }
        }

        public static IEqualityComparer<Claim> Thumbprint
        {
            get
            {
                if (thumbprintComparer == null)
                {
                    thumbprintComparer = new ClaimComparer(new BinaryObjectComparer());
                }
                return thumbprintComparer;
            }
        }

        public static IEqualityComparer<Claim> Upn
        {
            get
            {
                if (upnComparer == null)
                {
                    upnComparer = new ClaimComparer(new UpnObjectComparer());
                }
                return upnComparer;
            }
        }

        public static IEqualityComparer<Claim> X500DistinguishedName
        {
            get
            {
                if (x500DistinguishedNameComparer == null)
                {
                    x500DistinguishedNameComparer = new ClaimComparer(new X500DistinguishedNameObjectComparer());
                }
                return x500DistinguishedNameComparer;
            }
        }

        private class BinaryObjectComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object obj1, object obj2)
            {
                if (!object.ReferenceEquals(obj1, obj2))
                {
                    byte[] buffer = obj1 as byte[];
                    byte[] buffer2 = obj2 as byte[];
                    if ((buffer == null) || (buffer2 == null))
                    {
                        return false;
                    }
                    if (buffer.Length != buffer2.Length)
                    {
                        return false;
                    }
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        if (buffer[i] != buffer2[i])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                byte[] buffer = obj as byte[];
                if (buffer == null)
                {
                    return 0;
                }
                int num = 0;
                for (int i = 0; (i < buffer.Length) && (i < 4); i++)
                {
                    num = (num << 8) | buffer[i];
                }
                return (num ^ buffer.Length);
            }
        }

        private class ObjectComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object obj1, object obj2)
            {
                return (((obj1 == null) && (obj2 == null)) || (((obj1 != null) && (obj2 != null)) && obj1.Equals(obj2)));
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return obj.GetHashCode();
            }
        }

        private class RsaObjectComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object obj1, object obj2)
            {
                if (!object.ReferenceEquals(obj1, obj2))
                {
                    RSA rsa = obj1 as RSA;
                    RSA rsa2 = obj2 as RSA;
                    if ((rsa == null) || (rsa2 == null))
                    {
                        return false;
                    }
                    RSAParameters parameters = rsa.ExportParameters(false);
                    RSAParameters parameters2 = rsa2.ExportParameters(false);
                    if ((parameters.Modulus.Length != parameters2.Modulus.Length) || (parameters.Exponent.Length != parameters2.Exponent.Length))
                    {
                        return false;
                    }
                    for (int i = 0; i < parameters.Modulus.Length; i++)
                    {
                        if (parameters.Modulus[i] != parameters2.Modulus[i])
                        {
                            return false;
                        }
                    }
                    for (int j = 0; j < parameters.Exponent.Length; j++)
                    {
                        if (parameters.Exponent[j] != parameters2.Exponent[j])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                RSA rsa = obj as RSA;
                if (rsa == null)
                {
                    return 0;
                }
                RSAParameters parameters = rsa.ExportParameters(false);
                return (parameters.Modulus.Length ^ parameters.Exponent.Length);
            }
        }

        private class UpnObjectComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object obj1, object obj2)
            {
                SecurityIdentifier identifier;
                SecurityIdentifier identifier2;
                if (StringComparer.OrdinalIgnoreCase.Equals(obj1, obj2))
                {
                    return true;
                }
                string upn = obj1 as string;
                string str2 = obj2 as string;
                if ((upn == null) || (str2 == null))
                {
                    return false;
                }
                if (!this.TryLookupSidFromName(upn, out identifier))
                {
                    return false;
                }
                if (!this.TryLookupSidFromName(str2, out identifier2))
                {
                    return false;
                }
                return (identifier == identifier2);
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                SecurityIdentifier identifier;
                string upn = obj as string;
                if (upn == null)
                {
                    return 0;
                }
                if (this.TryLookupSidFromName(upn, out identifier))
                {
                    return identifier.GetHashCode();
                }
                return StringComparer.OrdinalIgnoreCase.GetHashCode(upn);
            }

            private bool TryLookupSidFromName(string upn, out SecurityIdentifier sid)
            {
                sid = null;
                try
                {
                    NTAccount account = new NTAccount(upn);
                    sid = account.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
                }
                catch (IdentityNotMappedException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                return (sid != null);
            }
        }

        private class X500DistinguishedNameObjectComparer : IEqualityComparer
        {
            private IEqualityComparer binaryComparer = new ClaimComparer.BinaryObjectComparer();

            bool IEqualityComparer.Equals(object obj1, object obj2)
            {
                if (object.ReferenceEquals(obj1, obj2))
                {
                    return true;
                }
                X500DistinguishedName name = obj1 as X500DistinguishedName;
                X500DistinguishedName name2 = obj2 as X500DistinguishedName;
                if ((name == null) || (name2 == null))
                {
                    return false;
                }
                return (StringComparer.Ordinal.Equals(name.Name, name2.Name) || this.binaryComparer.Equals(name.RawData, name2.RawData));
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                X500DistinguishedName name = obj as X500DistinguishedName;
                if (name == null)
                {
                    return 0;
                }
                return this.binaryComparer.GetHashCode(name.RawData);
            }
        }
    }
}

