namespace System.IdentityModel.Claims
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.IdentityModel.Policy;
    using System.Net.Mail;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.Threading;

    public class X509CertificateClaimSet : ClaimSet, IIdentityInfo, IDisposable
    {
        private X509Certificate2 certificate;
        private IList<Claim> claims;
        private bool disposed;
        private X509ChainElementCollection elements;
        private DateTime expirationTime;
        private X509Identity identity;
        private int index;
        private ClaimSet issuer;

        private X509CertificateClaimSet(X509CertificateClaimSet from) : this(from.X509Certificate, true)
        {
        }

        public X509CertificateClaimSet(X509Certificate2 certificate) : this(certificate, true)
        {
        }

        internal X509CertificateClaimSet(X509Certificate2 certificate, bool clone)
        {
            this.expirationTime = System.IdentityModel.SecurityUtils.MinUtcDateTime;
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            this.certificate = clone ? new X509Certificate2(certificate) : certificate;
        }

        private X509CertificateClaimSet(X509ChainElementCollection elements, int index)
        {
            this.expirationTime = System.IdentityModel.SecurityUtils.MinUtcDateTime;
            this.elements = elements;
            this.index = index;
            this.certificate = elements[index].Certificate;
        }

        internal X509CertificateClaimSet Clone()
        {
            this.ThrowIfDisposed();
            return new X509CertificateClaimSet(this);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                System.IdentityModel.SecurityUtils.DisposeIfNecessary(this.identity);
                if ((this.issuer != null) && (this.issuer != this))
                {
                    System.IdentityModel.SecurityUtils.DisposeIfNecessary(this.issuer as IDisposable);
                }
                if (this.elements != null)
                {
                    for (int i = this.index + 1; i < this.elements.Count; i++)
                    {
                        this.elements[i].Certificate.Reset();
                    }
                }
                this.certificate.Reset();
            }
        }

        private void EnsureClaims()
        {
            if (this.claims == null)
            {
                this.claims = this.InitializeClaimsCore();
            }
        }

        public override IEnumerable<Claim> FindClaims(string claimType, string right)
        {
            this.ThrowIfDisposed();
            if (SupportedClaimType(claimType) && ClaimSet.SupportedRight(right))
            {
                if ((this.claims != null) || !ClaimTypes.Thumbprint.Equals(claimType))
                {
                    if ((this.claims == null) && ClaimTypes.Dns.Equals(claimType))
                    {
                        if ((right == null) || Rights.PossessProperty.Equals(right))
                        {
                            string nameInfo = this.certificate.GetNameInfo(X509NameType.DnsName, false);
                            if (!string.IsNullOrEmpty(nameInfo))
                            {
                                yield return Claim.CreateDnsClaim(nameInfo);
                            }
                        }
                    }
                    else
                    {
                        this.EnsureClaims();
                        bool iteratorVariable1 = claimType == null;
                        bool iteratorVariable2 = right == null;
                        for (int i = 0; i < this.claims.Count; i++)
                        {
                            Claim iteratorVariable4 = this.claims[i];
                            if (((iteratorVariable4 != null) && (iteratorVariable1 || claimType.Equals(iteratorVariable4.ClaimType))) && (iteratorVariable2 || right.Equals(iteratorVariable4.Right)))
                            {
                                yield return iteratorVariable4;
                            }
                        }
                    }
                }
                else
                {
                    if ((right == null) || Rights.Identity.Equals(right))
                    {
                        yield return new Claim(ClaimTypes.Thumbprint, this.certificate.GetCertHash(), Rights.Identity);
                    }
                    if ((right == null) || Rights.PossessProperty.Equals(right))
                    {
                        yield return new Claim(ClaimTypes.Thumbprint, this.certificate.GetCertHash(), Rights.PossessProperty);
                    }
                }
            }
        }

        public override IEnumerator<Claim> GetEnumerator()
        {
            this.ThrowIfDisposed();
            this.EnsureClaims();
            return this.claims.GetEnumerator();
        }

        private IList<Claim> InitializeClaimsCore()
        {
            List<Claim> list = new List<Claim>();
            byte[] certHash = this.certificate.GetCertHash();
            list.Add(new Claim(ClaimTypes.Thumbprint, certHash, Rights.Identity));
            list.Add(new Claim(ClaimTypes.Thumbprint, certHash, Rights.PossessProperty));
            if (!string.IsNullOrEmpty(this.certificate.SubjectName.Name))
            {
                list.Add(Claim.CreateX500DistinguishedNameClaim(this.certificate.SubjectName));
            }
            string nameInfo = this.certificate.GetNameInfo(X509NameType.DnsName, false);
            if (!string.IsNullOrEmpty(nameInfo))
            {
                list.Add(Claim.CreateDnsClaim(nameInfo));
            }
            nameInfo = this.certificate.GetNameInfo(X509NameType.SimpleName, false);
            if (!string.IsNullOrEmpty(nameInfo))
            {
                list.Add(Claim.CreateNameClaim(nameInfo));
            }
            nameInfo = this.certificate.GetNameInfo(X509NameType.EmailName, false);
            if (!string.IsNullOrEmpty(nameInfo))
            {
                list.Add(Claim.CreateMailAddressClaim(new MailAddress(nameInfo)));
            }
            nameInfo = this.certificate.GetNameInfo(X509NameType.UpnName, false);
            if (!string.IsNullOrEmpty(nameInfo))
            {
                list.Add(Claim.CreateUpnClaim(nameInfo));
            }
            nameInfo = this.certificate.GetNameInfo(X509NameType.UrlName, false);
            if (!string.IsNullOrEmpty(nameInfo))
            {
                list.Add(Claim.CreateUriClaim(new Uri(nameInfo)));
            }
            RSA key = this.certificate.PublicKey.Key as RSA;
            if (key != null)
            {
                list.Add(Claim.CreateRsaClaim(key));
            }
            return list;
        }

        private static bool SupportedClaimType(string claimType)
        {
            if ((((claimType != null) && !ClaimTypes.Thumbprint.Equals(claimType)) && (!ClaimTypes.X500DistinguishedName.Equals(claimType) && !ClaimTypes.Dns.Equals(claimType))) && ((!ClaimTypes.Name.Equals(claimType) && !ClaimTypes.Email.Equals(claimType)) && (!ClaimTypes.Upn.Equals(claimType) && !ClaimTypes.Uri.Equals(claimType))))
            {
                return ClaimTypes.Rsa.Equals(claimType);
            }
            return true;
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
        }

        public override string ToString()
        {
            if (!this.disposed)
            {
                return System.IdentityModel.SecurityUtils.ClaimSetToString(this);
            }
            return base.ToString();
        }

        public override int Count
        {
            get
            {
                this.ThrowIfDisposed();
                this.EnsureClaims();
                return this.claims.Count;
            }
        }

        public DateTime ExpirationTime
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.expirationTime == System.IdentityModel.SecurityUtils.MinUtcDateTime)
                {
                    this.expirationTime = this.certificate.NotAfter.ToUniversalTime();
                }
                return this.expirationTime;
            }
        }

        public override ClaimSet Issuer
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.issuer == null)
                {
                    if (this.elements == null)
                    {
                        X509Chain chain = new X509Chain {
                            ChainPolicy = { RevocationMode = X509RevocationMode.NoCheck }
                        };
                        chain.Build(this.certificate);
                        this.index = 0;
                        this.elements = chain.ChainElements;
                    }
                    if ((this.index + 1) < this.elements.Count)
                    {
                        this.issuer = new X509CertificateClaimSet(this.elements, this.index + 1);
                        this.elements = null;
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Equals(this.certificate.SubjectName.Name, this.certificate.IssuerName.Name))
                    {
                        this.issuer = this;
                    }
                    else
                    {
                        this.issuer = new X500DistinguishedNameClaimSet(this.certificate.IssuerName);
                    }
                }
                return this.issuer;
            }
        }

        public override Claim this[int index]
        {
            get
            {
                this.ThrowIfDisposed();
                this.EnsureClaims();
                return this.claims[index];
            }
        }

        IIdentity IIdentityInfo.Identity
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.identity == null)
                {
                    this.identity = new X509Identity(this.certificate, false, false);
                }
                return this.identity;
            }
        }

        public X509Certificate2 X509Certificate
        {
            get
            {
                this.ThrowIfDisposed();
                return this.certificate;
            }
        }


        private class X500DistinguishedNameClaimSet : DefaultClaimSet, IIdentityInfo
        {
            private IIdentity identity;

            public X500DistinguishedNameClaimSet(X500DistinguishedName x500DistinguishedName) : base(new Claim[0])
            {
                if (x500DistinguishedName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("x500DistinguishedName");
                }
                this.identity = new X509Identity(x500DistinguishedName);
                List<Claim> claims = new List<Claim>(2) {
                    new Claim(ClaimTypes.X500DistinguishedName, x500DistinguishedName, Rights.Identity),
                    Claim.CreateX500DistinguishedNameClaim(x500DistinguishedName)
                };
                base.Initialize(ClaimSet.Anonymous, claims);
            }

            public IIdentity Identity
            {
                get
                {
                    return this.identity;
                }
            }
        }
    }
}

