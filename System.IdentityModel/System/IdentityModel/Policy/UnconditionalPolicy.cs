namespace System.IdentityModel.Policy
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.Security.Principal;

    internal class UnconditionalPolicy : IAuthorizationPolicy, IAuthorizationComponent, IDisposable
    {
        private bool disposable;
        private bool disposed;
        private DateTime expirationTime;
        private SecurityUniqueId id;
        private ClaimSet issuance;
        private ReadOnlyCollection<ClaimSet> issuances;
        private ClaimSet issuer;
        private IIdentity primaryIdentity;

        public UnconditionalPolicy(ClaimSet issuance) : this(issuance, System.IdentityModel.SecurityUtils.MaxUtcDateTime)
        {
        }

        private UnconditionalPolicy(UnconditionalPolicy from)
        {
            this.disposable = from.disposable;
            this.primaryIdentity = from.disposable ? System.IdentityModel.SecurityUtils.CloneIdentityIfNecessary(from.primaryIdentity) : from.primaryIdentity;
            if (from.issuance != null)
            {
                this.issuance = from.disposable ? System.IdentityModel.SecurityUtils.CloneClaimSetIfNecessary(from.issuance) : from.issuance;
            }
            else
            {
                this.issuances = from.disposable ? System.IdentityModel.SecurityUtils.CloneClaimSetsIfNecessary(from.issuances) : from.issuances;
            }
            this.issuer = from.issuer;
            this.expirationTime = from.expirationTime;
        }

        public UnconditionalPolicy(ClaimSet issuance, DateTime expirationTime)
        {
            if (issuance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuance");
            }
            this.Initialize(ClaimSet.System, issuance, null, expirationTime);
        }

        internal UnconditionalPolicy(IIdentity primaryIdentity, ClaimSet issuance) : this(issuance)
        {
            this.primaryIdentity = primaryIdentity;
        }

        public UnconditionalPolicy(ReadOnlyCollection<ClaimSet> issuances, DateTime expirationTime)
        {
            if (issuances == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuances");
            }
            this.Initialize(ClaimSet.System, null, issuances, expirationTime);
        }

        internal UnconditionalPolicy(IIdentity primaryIdentity, ReadOnlyCollection<ClaimSet> issuances, DateTime expirationTime) : this(issuances, expirationTime)
        {
            this.primaryIdentity = primaryIdentity;
        }

        internal UnconditionalPolicy(IIdentity primaryIdentity, ClaimSet issuance, DateTime expirationTime) : this(issuance, expirationTime)
        {
            this.primaryIdentity = primaryIdentity;
        }

        internal UnconditionalPolicy Clone()
        {
            this.ThrowIfDisposed();
            if (!this.disposable)
            {
                return this;
            }
            return new UnconditionalPolicy(this);
        }

        public virtual void Dispose()
        {
            if (this.disposable && !this.disposed)
            {
                this.disposed = true;
                System.IdentityModel.SecurityUtils.DisposeIfNecessary(this.primaryIdentity as WindowsIdentity);
                System.IdentityModel.SecurityUtils.DisposeClaimSetIfNecessary(this.issuance);
                System.IdentityModel.SecurityUtils.DisposeClaimSetsIfNecessary(this.issuances);
            }
        }

        public virtual bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            this.ThrowIfDisposed();
            if (this.issuance != null)
            {
                evaluationContext.AddClaimSet(this, this.issuance);
            }
            else
            {
                for (int i = 0; i < this.issuances.Count; i++)
                {
                    if (this.issuances[i] != null)
                    {
                        evaluationContext.AddClaimSet(this, this.issuances[i]);
                    }
                }
            }
            if ((this.PrimaryIdentity != null) && (this.PrimaryIdentity != System.IdentityModel.SecurityUtils.AnonymousIdentity))
            {
                IList<IIdentity> list;
                object obj2;
                if (!evaluationContext.Properties.TryGetValue("Identities", out obj2))
                {
                    list = new List<IIdentity>(1);
                    evaluationContext.Properties.Add("Identities", list);
                }
                else
                {
                    list = obj2 as IList<IIdentity>;
                }
                if (list != null)
                {
                    list.Add(this.PrimaryIdentity);
                }
            }
            evaluationContext.RecordExpirationTime(this.expirationTime);
            return true;
        }

        private void Initialize(ClaimSet issuer, ClaimSet issuance, ReadOnlyCollection<ClaimSet> issuances, DateTime expirationTime)
        {
            this.issuer = issuer;
            this.issuance = issuance;
            this.issuances = issuances;
            this.expirationTime = expirationTime;
            if (issuance != null)
            {
                this.disposable = issuance is WindowsClaimSet;
            }
            else
            {
                for (int i = 0; i < issuances.Count; i++)
                {
                    if (issuances[i] is WindowsClaimSet)
                    {
                        this.disposable = true;
                        return;
                    }
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
        }

        public DateTime ExpirationTime
        {
            get
            {
                return this.expirationTime;
            }
        }

        public string Id
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

        internal bool IsDisposable
        {
            get
            {
                return this.disposable;
            }
        }

        internal ReadOnlyCollection<ClaimSet> Issuances
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.issuances == null)
                {
                    this.issuances = new List<ClaimSet>(1) { this.issuance }.AsReadOnly();
                }
                return this.issuances;
            }
        }

        public ClaimSet Issuer
        {
            get
            {
                return this.issuer;
            }
        }

        internal IIdentity PrimaryIdentity
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.primaryIdentity == null)
                {
                    IIdentity identity = null;
                    if (this.issuance != null)
                    {
                        if (this.issuance is IIdentityInfo)
                        {
                            identity = ((IIdentityInfo) this.issuance).Identity;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.issuances.Count; i++)
                        {
                            ClaimSet set = this.issuances[i];
                            if (set is IIdentityInfo)
                            {
                                identity = ((IIdentityInfo) set).Identity;
                                if ((identity != null) && (identity != System.IdentityModel.SecurityUtils.AnonymousIdentity))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    this.primaryIdentity = identity ?? System.IdentityModel.SecurityUtils.AnonymousIdentity;
                }
                return this.primaryIdentity;
            }
        }
    }
}

