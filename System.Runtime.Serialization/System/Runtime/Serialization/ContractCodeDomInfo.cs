namespace System.Runtime.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;

    internal class ContractCodeDomInfo
    {
        private string clrNamespace;
        internal System.CodeDom.CodeNamespace CodeNamespace;
        internal bool IsProcessed;
        private Dictionary<string, object> memberNames;
        internal bool ReferencedTypeExists;
        internal CodeTypeDeclaration TypeDeclaration;
        internal CodeTypeReference TypeReference;
        internal bool UsesWildcardNamespace;

        internal Dictionary<string, object> GetMemberNames()
        {
            if (this.ReferencedTypeExists)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("CannotSetMembersForReferencedType", new object[] { this.TypeReference.BaseType })));
            }
            if (this.memberNames == null)
            {
                this.memberNames = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            return this.memberNames;
        }

        internal string ClrNamespace
        {
            get
            {
                if (!this.ReferencedTypeExists)
                {
                    return this.clrNamespace;
                }
                return null;
            }
            set
            {
                if (this.ReferencedTypeExists)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("CannotSetNamespaceForReferencedType", new object[] { this.TypeReference.BaseType })));
                }
                this.clrNamespace = value;
            }
        }
    }
}

