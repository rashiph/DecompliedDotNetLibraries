namespace System.ServiceModel.Security
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.Xml;

    public class SecurityContextKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private readonly UniqueId contextId;
        private readonly UniqueId generation;

        public SecurityContextKeyIdentifierClause(UniqueId contextId) : this(contextId, null)
        {
        }

        public SecurityContextKeyIdentifierClause(UniqueId contextId, UniqueId generation) : this(contextId, generation, null, 0)
        {
        }

        public SecurityContextKeyIdentifierClause(UniqueId contextId, UniqueId generation, byte[] derivationNonce, int derivationLength) : base(null, derivationNonce, derivationLength)
        {
            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }
            this.contextId = contextId;
            this.generation = generation;
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SecurityContextKeyIdentifierClause objB = keyIdentifierClause as SecurityContextKeyIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && objB.Matches(this.contextId, this.generation)));
        }

        public bool Matches(UniqueId contextId, UniqueId generation)
        {
            return ((contextId == this.contextId) && (generation == this.generation));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "SecurityContextKeyIdentifierClause(ContextId = '{0}', Generation = '{1}')", new object[] { this.ContextId, this.Generation });
        }

        public UniqueId ContextId
        {
            get
            {
                return this.contextId;
            }
        }

        public UniqueId Generation
        {
            get
            {
                return this.generation;
            }
        }
    }
}

