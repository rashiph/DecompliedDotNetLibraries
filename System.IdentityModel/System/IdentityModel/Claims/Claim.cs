namespace System.IdentityModel.Claims
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel;
    using System.Net.Mail;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    [DataContract(Namespace="http://schemas.xmlsoap.org/ws/2005/05/identity")]
    public class Claim
    {
        [DataMember(Name="ClaimType")]
        private string claimType;
        private IEqualityComparer<Claim> comparer;
        [DataMember(Name="Resource")]
        private object resource;
        [DataMember(Name="Right")]
        private string right;
        private static Claim system;

        public Claim(string claimType, object resource, string right) : this(claimType, resource, right, null)
        {
        }

        private Claim(string claimType, object resource, string right, IEqualityComparer<Claim> comparer)
        {
            if (claimType == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimType");
            }
            if (claimType.Length <= 0)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("claimType", System.IdentityModel.SR.GetString("ArgumentCannotBeEmptyString"));
            }
            if (right == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("right");
            }
            if (right.Length <= 0)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("right", System.IdentityModel.SR.GetString("ArgumentCannotBeEmptyString"));
            }
            this.claimType = claimType;
            this.resource = resource;
            this.right = right;
            this.comparer = comparer;
        }

        public static Claim CreateDenyOnlyWindowsSidClaim(SecurityIdentifier sid)
        {
            if (sid == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sid");
            }
            return new Claim(ClaimTypes.DenyOnlySid, sid, Rights.PossessProperty);
        }

        public static Claim CreateDnsClaim(string dns)
        {
            if (dns == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dns");
            }
            return new Claim(ClaimTypes.Dns, dns, Rights.PossessProperty, ClaimComparer.Dns);
        }

        public static Claim CreateHashClaim(byte[] hash)
        {
            if (hash == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("hash");
            }
            return new Claim(ClaimTypes.Hash, System.IdentityModel.SecurityUtils.CloneBuffer(hash), Rights.PossessProperty, ClaimComparer.Hash);
        }

        public static Claim CreateMailAddressClaim(MailAddress mailAddress)
        {
            if (mailAddress == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("mailAddress");
            }
            return new Claim(ClaimTypes.Email, mailAddress, Rights.PossessProperty);
        }

        public static Claim CreateNameClaim(string name)
        {
            if (name == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            return new Claim(ClaimTypes.Name, name, Rights.PossessProperty);
        }

        public static Claim CreateRsaClaim(RSA rsa)
        {
            if (rsa == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rsa");
            }
            return new Claim(ClaimTypes.Rsa, rsa, Rights.PossessProperty, ClaimComparer.Rsa);
        }

        public static Claim CreateSpnClaim(string spn)
        {
            if (spn == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("spn");
            }
            return new Claim(ClaimTypes.Spn, spn, Rights.PossessProperty);
        }

        public static Claim CreateThumbprintClaim(byte[] thumbprint)
        {
            if (thumbprint == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("thumbprint");
            }
            return new Claim(ClaimTypes.Thumbprint, System.IdentityModel.SecurityUtils.CloneBuffer(thumbprint), Rights.PossessProperty, ClaimComparer.Thumbprint);
        }

        public static Claim CreateUpnClaim(string upn)
        {
            if (upn == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("upn");
            }
            return new Claim(ClaimTypes.Upn, upn, Rights.PossessProperty, ClaimComparer.Upn);
        }

        public static Claim CreateUriClaim(Uri uri)
        {
            if (uri == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
            }
            return new Claim(ClaimTypes.Uri, uri, Rights.PossessProperty);
        }

        public static Claim CreateWindowsSidClaim(SecurityIdentifier sid)
        {
            if (sid == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sid");
            }
            return new Claim(ClaimTypes.Sid, sid, Rights.PossessProperty);
        }

        public static Claim CreateX500DistinguishedNameClaim(X500DistinguishedName x500DistinguishedName)
        {
            if (x500DistinguishedName == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("x500DistinguishedName");
            }
            return new Claim(ClaimTypes.X500DistinguishedName, x500DistinguishedName, Rights.PossessProperty, ClaimComparer.X500DistinguishedName);
        }

        public override bool Equals(object obj)
        {
            if (this.comparer == null)
            {
                this.comparer = ClaimComparer.GetComparer(this.claimType);
            }
            return this.comparer.Equals(this, obj as Claim);
        }

        public override int GetHashCode()
        {
            if (this.comparer == null)
            {
                this.comparer = ClaimComparer.GetComparer(this.claimType);
            }
            return this.comparer.GetHashCode(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}: {1}", new object[] { this.right, this.claimType });
        }

        public string ClaimType
        {
            get
            {
                return this.claimType;
            }
        }

        public static IEqualityComparer<Claim> DefaultComparer
        {
            get
            {
                return EqualityComparer<Claim>.Default;
            }
        }

        public object Resource
        {
            get
            {
                return this.resource;
            }
        }

        public string Right
        {
            get
            {
                return this.right;
            }
        }

        public static Claim System
        {
            get
            {
                if (system == null)
                {
                    system = new Claim(ClaimTypes.System, "System", Rights.Identity);
                }
                return system;
            }
        }
    }
}

