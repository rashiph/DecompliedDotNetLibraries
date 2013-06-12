namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeDefaultValueExpression : CodeExpression
    {
        private CodeTypeReference type;

        public CodeDefaultValueExpression()
        {
        }

        public CodeDefaultValueExpression(CodeTypeReference type)
        {
            this.type = type;
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

