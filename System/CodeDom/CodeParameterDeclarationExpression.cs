namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeParameterDeclarationExpression : CodeExpression
    {
        private CodeAttributeDeclarationCollection customAttributes;
        private FieldDirection dir;
        private string name;
        private CodeTypeReference type;

        public CodeParameterDeclarationExpression()
        {
        }

        public CodeParameterDeclarationExpression(CodeTypeReference type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        public CodeParameterDeclarationExpression(string type, string name)
        {
            this.Type = new CodeTypeReference(type);
            this.Name = name;
        }

        public CodeParameterDeclarationExpression(System.Type type, string name)
        {
            this.Type = new CodeTypeReference(type);
            this.Name = name;
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
            set
            {
                this.customAttributes = value;
            }
        }

        public FieldDirection Direction
        {
            get
            {
                return this.dir;
            }
            set
            {
                this.dir = value;
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

        public CodeTypeReference Type
        {
            get
            {
                if (this.type == null)
                {
                    this.type = new CodeTypeReference("");
                }
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }
}

