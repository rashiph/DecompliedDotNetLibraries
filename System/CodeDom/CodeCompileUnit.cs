namespace System.CodeDom
{
    using System;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeCompileUnit : CodeObject
    {
        private StringCollection assemblies;
        private CodeAttributeDeclarationCollection attributes;
        [OptionalField]
        private CodeDirectiveCollection endDirectives;
        private CodeNamespaceCollection namespaces = new CodeNamespaceCollection();
        [OptionalField]
        private CodeDirectiveCollection startDirectives;

        public CodeAttributeDeclarationCollection AssemblyCustomAttributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new CodeAttributeDeclarationCollection();
                }
                return this.attributes;
            }
        }

        public CodeDirectiveCollection EndDirectives
        {
            get
            {
                if (this.endDirectives == null)
                {
                    this.endDirectives = new CodeDirectiveCollection();
                }
                return this.endDirectives;
            }
        }

        public CodeNamespaceCollection Namespaces
        {
            get
            {
                return this.namespaces;
            }
        }

        public StringCollection ReferencedAssemblies
        {
            get
            {
                if (this.assemblies == null)
                {
                    this.assemblies = new StringCollection();
                }
                return this.assemblies;
            }
        }

        public CodeDirectiveCollection StartDirectives
        {
            get
            {
                if (this.startDirectives == null)
                {
                    this.startDirectives = new CodeDirectiveCollection();
                }
                return this.startDirectives;
            }
        }
    }
}

