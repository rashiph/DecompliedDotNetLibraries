namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeMemberMethod : CodeTypeMember
    {
        private CodeTypeReferenceCollection implementationTypes;
        private const int ImplTypesCollection = 4;
        private CodeParameterDeclarationExpressionCollection parameters = new CodeParameterDeclarationExpressionCollection();
        private const int ParametersCollection = 1;
        private int populated;
        private CodeTypeReference privateImplements;
        private CodeAttributeDeclarationCollection returnAttributes;
        private CodeTypeReference returnType;
        private CodeStatementCollection statements = new CodeStatementCollection();
        private const int StatementsCollection = 2;
        [OptionalField]
        private CodeTypeParameterCollection typeParameters;

        public event EventHandler PopulateImplementationTypes;

        public event EventHandler PopulateParameters;

        public event EventHandler PopulateStatements;

        public CodeTypeReferenceCollection ImplementationTypes
        {
            get
            {
                if (this.implementationTypes == null)
                {
                    this.implementationTypes = new CodeTypeReferenceCollection();
                }
                if ((this.populated & 4) == 0)
                {
                    this.populated |= 4;
                    if (this.PopulateImplementationTypes != null)
                    {
                        this.PopulateImplementationTypes(this, EventArgs.Empty);
                    }
                }
                return this.implementationTypes;
            }
        }

        public CodeParameterDeclarationExpressionCollection Parameters
        {
            get
            {
                if ((this.populated & 1) == 0)
                {
                    this.populated |= 1;
                    if (this.PopulateParameters != null)
                    {
                        this.PopulateParameters(this, EventArgs.Empty);
                    }
                }
                return this.parameters;
            }
        }

        public CodeTypeReference PrivateImplementationType
        {
            get
            {
                return this.privateImplements;
            }
            set
            {
                this.privateImplements = value;
            }
        }

        public CodeTypeReference ReturnType
        {
            get
            {
                if (this.returnType == null)
                {
                    this.returnType = new CodeTypeReference(typeof(void).FullName);
                }
                return this.returnType;
            }
            set
            {
                this.returnType = value;
            }
        }

        public CodeAttributeDeclarationCollection ReturnTypeCustomAttributes
        {
            get
            {
                if (this.returnAttributes == null)
                {
                    this.returnAttributes = new CodeAttributeDeclarationCollection();
                }
                return this.returnAttributes;
            }
        }

        public CodeStatementCollection Statements
        {
            get
            {
                if ((this.populated & 2) == 0)
                {
                    this.populated |= 2;
                    if (this.PopulateStatements != null)
                    {
                        this.PopulateStatements(this, EventArgs.Empty);
                    }
                }
                return this.statements;
            }
        }

        [ComVisible(false)]
        public CodeTypeParameterCollection TypeParameters
        {
            get
            {
                if (this.typeParameters == null)
                {
                    this.typeParameters = new CodeTypeParameterCollection();
                }
                return this.typeParameters;
            }
        }
    }
}

