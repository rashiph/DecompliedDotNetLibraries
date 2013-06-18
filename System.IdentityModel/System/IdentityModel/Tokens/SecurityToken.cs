namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;

    public abstract class SecurityToken
    {
        protected SecurityToken()
        {
        }

        public virtual bool CanCreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            return ((typeof(T) == typeof(LocalIdKeyIdentifierClause)) && this.CanCreateLocalKeyIdentifierClause());
        }

        private bool CanCreateLocalKeyIdentifierClause()
        {
            return (this.Id != null);
        }

        public virtual T CreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            if ((typeof(T) != typeof(LocalIdKeyIdentifierClause)) || !this.CanCreateLocalKeyIdentifierClause())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("TokenDoesNotSupportKeyIdentifierClauseCreation", new object[] { base.GetType().Name, typeof(T).Name })));
            }
            return (new LocalIdKeyIdentifierClause(this.Id, base.GetType()) as T);
        }

        public virtual bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            LocalIdKeyIdentifierClause clause = keyIdentifierClause as LocalIdKeyIdentifierClause;
            return ((clause != null) && clause.Matches(this.Id, base.GetType()));
        }

        public virtual SecurityKey ResolveKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if ((this.SecurityKeys.Count != 0) && this.MatchesKeyIdentifierClause(keyIdentifierClause))
            {
                return this.SecurityKeys[0];
            }
            return null;
        }

        public abstract string Id { get; }

        public abstract ReadOnlyCollection<SecurityKey> SecurityKeys { get; }

        public abstract DateTime ValidFrom { get; }

        public abstract DateTime ValidTo { get; }
    }
}

