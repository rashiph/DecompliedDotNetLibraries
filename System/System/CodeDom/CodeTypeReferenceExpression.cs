namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeTypeReferenceExpression : CodeExpression
    {
        private CodeTypeReference type;

        public CodeTypeReferenceExpression()
        {
        }

        public CodeTypeReferenceExpression(CodeTypeReference type)
        {
            this.Type = type;
        }

        public CodeTypeReferenceExpression(string type)
        {
            this.Type = new CodeTypeReference(type);
        }

        public CodeTypeReferenceExpression(System.Type type)
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

