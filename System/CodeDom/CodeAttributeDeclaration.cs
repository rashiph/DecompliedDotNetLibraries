namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeAttributeDeclaration
    {
        private CodeAttributeArgumentCollection arguments;
        [OptionalField]
        private CodeTypeReference attributeType;
        private string name;

        public CodeAttributeDeclaration()
        {
            this.arguments = new CodeAttributeArgumentCollection();
        }

        public CodeAttributeDeclaration(CodeTypeReference attributeType) : this(attributeType, null)
        {
        }

        public CodeAttributeDeclaration(string name)
        {
            this.arguments = new CodeAttributeArgumentCollection();
            this.Name = name;
        }

        public CodeAttributeDeclaration(CodeTypeReference attributeType, params CodeAttributeArgument[] arguments)
        {
            this.arguments = new CodeAttributeArgumentCollection();
            this.attributeType = attributeType;
            if (attributeType != null)
            {
                this.name = attributeType.BaseType;
            }
            if (arguments != null)
            {
                this.Arguments.AddRange(arguments);
            }
        }

        public CodeAttributeDeclaration(string name, params CodeAttributeArgument[] arguments)
        {
            this.arguments = new CodeAttributeArgumentCollection();
            this.Name = name;
            this.Arguments.AddRange(arguments);
        }

        public CodeAttributeArgumentCollection Arguments
        {
            get
            {
                return this.arguments;
            }
        }

        public CodeTypeReference AttributeType
        {
            get
            {
                return this.attributeType;
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
                this.attributeType = new CodeTypeReference(this.name);
            }
        }
    }
}

