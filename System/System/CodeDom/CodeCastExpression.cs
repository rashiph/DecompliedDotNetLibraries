namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeCastExpression : CodeExpression
    {
        private CodeExpression expression;
        private CodeTypeReference targetType;

        public CodeCastExpression()
        {
        }

        public CodeCastExpression(CodeTypeReference targetType, CodeExpression expression)
        {
            this.TargetType = targetType;
            this.Expression = expression;
        }

        public CodeCastExpression(string targetType, CodeExpression expression)
        {
            this.TargetType = new CodeTypeReference(targetType);
            this.Expression = expression;
        }

        public CodeCastExpression(Type targetType, CodeExpression expression)
        {
            this.TargetType = new CodeTypeReference(targetType);
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

        public CodeTypeReference TargetType
        {
            get
            {
                if (this.targetType == null)
                {
                    this.targetType = new CodeTypeReference("");
                }
                return this.targetType;
            }
            set
            {
                this.targetType = value;
            }
        }
    }
}

