namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeDirectionExpression : CodeExpression
    {
        private FieldDirection direction;
        private CodeExpression expression;

        public CodeDirectionExpression()
        {
        }

        public CodeDirectionExpression(FieldDirection direction, CodeExpression expression)
        {
            this.expression = expression;
            this.direction = direction;
        }

        public FieldDirection Direction
        {
            get
            {
                return this.direction;
            }
            set
            {
                this.direction = value;
            }
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

