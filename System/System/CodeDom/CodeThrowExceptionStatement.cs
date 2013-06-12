namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeThrowExceptionStatement : CodeStatement
    {
        private CodeExpression toThrow;

        public CodeThrowExceptionStatement()
        {
        }

        public CodeThrowExceptionStatement(CodeExpression toThrow)
        {
            this.ToThrow = toThrow;
        }

        public CodeExpression ToThrow
        {
            get
            {
                return this.toThrow;
            }
            set
            {
                this.toThrow = value;
            }
        }
    }
}

