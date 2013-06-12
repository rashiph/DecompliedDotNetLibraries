namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeBinaryOperatorExpression : CodeExpression
    {
        private CodeExpression left;
        private CodeBinaryOperatorType op;
        private CodeExpression right;

        public CodeBinaryOperatorExpression()
        {
        }

        public CodeBinaryOperatorExpression(CodeExpression left, CodeBinaryOperatorType op, CodeExpression right)
        {
            this.Right = right;
            this.Operator = op;
            this.Left = left;
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

        public CodeBinaryOperatorType Operator
        {
            get
            {
                return this.op;
            }
            set
            {
                this.op = value;
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

