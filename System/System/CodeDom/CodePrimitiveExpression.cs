namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodePrimitiveExpression : CodeExpression
    {
        private object value;

        public CodePrimitiveExpression()
        {
        }

        public CodePrimitiveExpression(object value)
        {
            this.Value = value;
        }

        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

