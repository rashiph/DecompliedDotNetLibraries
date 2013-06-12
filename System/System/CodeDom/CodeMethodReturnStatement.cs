namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeMethodReturnStatement : CodeStatement
    {
        private CodeExpression expression;

        public CodeMethodReturnStatement()
        {
        }

        public CodeMethodReturnStatement(CodeExpression expression)
        {
            this.Expression = expression;
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

