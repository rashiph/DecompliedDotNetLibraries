namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;

    internal class UniqueCodeNamespaceScope : UniqueCodeIdentifierScope
    {
        private System.CodeDom.CodeNamespace codeNamespace;

        public UniqueCodeNamespaceScope(System.CodeDom.CodeNamespace codeNamespace)
        {
            this.codeNamespace = codeNamespace;
        }

        protected override void AddIdentifier(string identifier)
        {
        }

        public CodeTypeReference AddUnique(CodeTypeDeclaration codeType, string name, string defaultName)
        {
            codeType.Name = base.AddUnique(name, defaultName);
            this.codeNamespace.Types.Add(codeType);
            return ServiceContractGenerator.NamespaceHelper.GetCodeTypeReference(this.codeNamespace, codeType);
        }

        public override bool IsUnique(string identifier)
        {
            return !this.NamespaceContainsType(identifier);
        }

        private bool NamespaceContainsType(string typeName)
        {
            foreach (CodeTypeDeclaration declaration in this.codeNamespace.Types)
            {
                if (string.Compare(declaration.Name, typeName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public System.CodeDom.CodeNamespace CodeNamespace
        {
            get
            {
                return this.codeNamespace;
            }
        }
    }
}

