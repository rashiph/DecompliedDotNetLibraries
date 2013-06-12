namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeTypeOfExpression : CodeExpression
    {
        private CodeTypeReference type;

        public CodeTypeOfExpression()
        {
        }

        public CodeTypeOfExpression(CodeTypeReference type)
        {
            this.Type = type;
        }

        public CodeTypeOfExpression(string type)
        {
            this.Type = new CodeTypeReference(type);
        }

        public CodeTypeOfExpression(System.Type type)
        {
            this.Type = new CodeTypeReference(type);
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

