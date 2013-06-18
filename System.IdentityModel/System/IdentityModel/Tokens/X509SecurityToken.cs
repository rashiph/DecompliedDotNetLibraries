namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.Security.Cryptography.X509Certificates;

    public class X509SecurityToken : SecurityToken, IDisposable
    {
        private X509Certificate2 certificate;
        private bool disposable;
        private bool disposed;
        private DateTime effectiveTime;
        private DateTime expirationTime;
        private string id;
        private ReadOnlyCollection<SecurityKey> securityKeys;

        public X509SecurityToken(X509Certificate2 certificate) : this(certificate, SecurityUniqueId.Create().Value)
        {
        }

        internal X509SecurityToken(X509Certificate2 certificate, bool clone) : this(certificate, SecurityUniqueId.Create().Value, clone)
        {
        }

        public X509SecurityToken(X509Certificate2 certificate, string id) : this(certificate, id, true)
        {
        }

        internal X509SecurityToken(X509Certificate2 certificate, bool clone, bool disposable) : this(certificate, SecurityUniqueId.Create().Value, clone, disposable)
        {
        }

        internal X509SecurityToken(X509Certificate2 certificate, string id, bool clone) : this(certificate, id, clone, true)
        {
        }

        internal X509SecurityToken(X509Certificate2 certificate, string id, bool clone, bool disposable)
        {
            this.effectiveTime = System.IdentityModel.SecurityUtils.MaxUtcDateTime;
            this.expirationTime = System.IdentityModel.SecurityUtils.MinUtcDateTime;
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            }
            this.id = id;
            this.certificate = clone ? new X509Certificate2(certificate) : certificate;
            this.disposable = clone || disposable;
        }

        public override bool CanCreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            this.ThrowIfDisposed();
            if (typeof(T) == typeof(X509SubjectKeyIdentifierClause))
            {
                return X509SubjectKeyIdentifierClause.CanCreateFrom(this.certificate);
            }
            if ((!(typeof(T) == typeof(X509ThumbprintKeyIdentifierClause)) && !(typeof(T) == typeof(X509IssuerSerialKeyIdentifierClause))) && !(typeof(T) == typeof(X509RawDataKeyIdentifierClause)))
            {
                return base.CanCreateKeyIdentifierClause<T>();
            }
            return true;
        }

        public override T CreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            this.ThrowIfDisposed();
            if (typeof(T) == typeof(X509SubjectKeyIdentifierClause))
            {
                X509SubjectKeyIdentifierClause clause;
                if (X509SubjectKeyIdentifierClause.TryCreateFrom(this.certificate, out clause))
                {
                    return (clause as T);
                }
            }
            else
            {
                if (typeof(T) == typeof(X509ThumbprintKeyIdentifierClause))
                {
                    return (new X509ThumbprintKeyIdentifierClause(this.certificate) as T);
                }
                if (typeof(T) == typeof(X509IssuerSerialKeyIdentifierClause))
                {
                    return (new X509IssuerSerialKeyIdentifierClause(this.certificate) as T);
                }
                if (typeof(T) == typeof(X509RawDataKeyIdentifierClause))
                {
                    return (new X509RawDataKeyIdentifierClause(this.certificate) as T);
                }
            }
            return base.CreateKeyIdentifierClause<T>();
        }

        public virtual void Dispose()
        {
            if (this.disposable && !this.disposed)
            {
                this.disposed = true;
                this.certificate.Reset();
            }
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            this.ThrowIfDisposed();
            X509SubjectKeyIdentifierClause clause = keyIdentifierClause as X509SubjectKeyIdentifierClause;
            if (clause != null)
            {
                return clause.Matches(this.certificate);
            }
            X509ThumbprintKeyIdentifierClause clause2 = keyIdentifierClause as X509ThumbprintKeyIdentifierClause;
            if (clause2 != null)
            {
                return clause2.Matches(this.certificate);
            }
            X509IssuerSerialKeyIdentifierClause clause3 = keyIdentifierClause as X509IssuerSerialKeyIdentifierClause;
            if (clause3 != null)
            {
                return clause3.Matches(this.certificate);
            }
            X509RawDataKeyIdentifierClause clause4 = keyIdentifierClause as X509RawDataKeyIdentifierClause;
            if (clause4 != null)
            {
                return clause4.Matches(this.certificate);
            }
            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        protected void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
        }

        public X509Certificate2 Certificate
        {
            get
            {
                this.ThrowIfDisposed();
                return this.certificate;
            }
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.securityKeys == null)
                {
                    this.securityKeys = new List<SecurityKey>(1) { new X509AsymmetricSecurityKey(this.certificate) }.AsReadOnly();
                }
                return this.securityKeys;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.effectiveTime == System.IdentityModel.SecurityUtils.MaxUtcDateTime)
                {
                    this.effectiveTime = this.certificate.NotBefore.ToUniversalTime();
                }
                return this.effectiveTime;
            }
        }

        public override DateTime ValidTo
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
    }
}

