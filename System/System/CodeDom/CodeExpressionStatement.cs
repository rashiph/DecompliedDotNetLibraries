namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeExpressionStatement : CodeStatement
    {
        private CodeExpression expression;

        public CodeExpressionStatement()
        {
        }

        public CodeExpressionStatement(CodeExpression expression)
        {
            this.expression = expression;
        }

        public CodeExpression Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                this.expression = value;
            }
        }
    }
}

