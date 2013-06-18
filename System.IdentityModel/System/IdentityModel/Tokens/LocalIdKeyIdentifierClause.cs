namespace System.IdentityModel.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel;

    public class LocalIdKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private readonly string localId;
        private readonly Type[] ownerTypes;

        public LocalIdKeyIdentifierClause(string localId) : this(localId, (Type[]) null)
        {
        }

        public LocalIdKeyIdentifierClause(string localId, Type ownerType) : this(localId, (ownerType == null) ? null : new Type[] { ownerType })
        {
        }

        internal LocalIdKeyIdentifierClause(string localId, Type[] ownerTypes) : this(localId, null, 0, ownerTypes)
        {
        }

        public LocalIdKeyIdentifierClause(string localId, byte[] derivationNonce, int derivationLength, Type ownerType) : this(null, derivationNonce, derivationLength, (ownerType == null) ? null : new Type[] { ownerType })
        {
        }

        internal LocalIdKeyIdentifierClause(string localId, byte[] derivationNonce, int derivationLength, Type[] ownerTypes) : base(null, derivationNonce, derivationLength)
        {
            if (localId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localId");
            }
            if (localId == string.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("LocalIdCannotBeEmpty"));
            }
            this.localId = localId;
            this.ownerTypes = ownerTypes;
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            LocalIdKeyIdentifierClause objB = keyIdentifierClause as LocalIdKeyIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && objB.Matches(this.localId, this.OwnerType)));
        }

        public bool Matches(string localId, Type ownerType)
        {
            if (!string.IsNullOrEmpty(localId))
            {
                if (this.localId != localId)
                {
                    return false;
                }
                if ((this.ownerTypes == null) || (ownerType == null))
                {
                    return true;
                }
                for (int i = 0; i < this.ownerTypes.Length; i++)
                {
                    if ((this.ownerTypes[i] == null) || (this.ownerTypes[i] == ownerType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "LocalIdKeyIdentifierClause(LocalId = '{0}', Owner = '{1}')", new object[] { this.LocalId, this.OwnerType });
        }

        public string LocalId
        {
            get
            {
                return this.localId;
            }
        }

        public Type OwnerType
        {
            get
            {
                if ((this.ownerTypes != null) && (this.ownerTypes.Length != 0))
                {
                    return this.ownerTypes[0];
                }
                return null;
            }
        }
    }
}

