namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeTypeParameter : CodeObject
    {
        private CodeTypeReferenceCollection constraints;
        private CodeAttributeDeclarationCollection customAttributes;
        private bool hasConstructorConstraint;
        private string name;

        public CodeTypeParameter()
        {
        }

        public CodeTypeParameter(string name)
        {
            this.name = name;
        }

        public CodeTypeReferenceCollection Constraints
        {
            get
            {
                if (this.constraints == null)
                {
                    this.constraints = new CodeTypeReferenceCollection();
                }
                return this.constraints;
            }
        }

        public CodeAttributeDeclarationCollection CustomAttributes
        {
            get
            {
                if (this.customAttributes == null)
                {
                    this.customAttributes = new CodeAttributeDeclarationCollection();
                }
                return this.customAttributes;
            }
        }

        public bool HasConstructorConstraint
        {
            get
            {
                return this.hasConstructorConstraint;
            }
            set
            {
                this.hasConstructorConstraint = value;
            }
        }

        public string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            set
            {
                this.name = value;
            }
        }
    }
}

