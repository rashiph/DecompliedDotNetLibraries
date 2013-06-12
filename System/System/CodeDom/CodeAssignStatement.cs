namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeAssignStatement : CodeStatement
    {
        private CodeExpression left;
        private CodeExpression right;

        public CodeAssignStatement()
        {
        }

        public CodeAssignStatement(CodeExpression left, CodeExpression right)
        {
            this.Left = left;
            this.Right = right;
        }

        public CodeExpression Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.left = value;
            }
        }

        public CodeExpression Right
        {
            get
            {
                return this.right;
            }
            set
            {
                this.right = value;
            }
        }
    }
}

