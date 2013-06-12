namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeDelegateInvokeExpression : CodeExpression
    {
        private CodeExpressionCollection parameters;
        private CodeExpression targetObject;

        public CodeDelegateInvokeExpression()
        {
            this.parameters = new CodeExpressionCollection();
        }

        public CodeDelegateInvokeExpression(CodeExpression targetObject)
        {
            this.parameters = new CodeExpressionCollection();
            this.TargetObject = targetObject;
        }

        public CodeDelegateInvokeExpression(CodeExpression targetObject, params CodeExpression[] parameters)
        {
            this.parameters = new CodeExpressionCollection();
            this.TargetObject = targetObject;
            this.Parameters.AddRange(parameters);
        }

        public CodeExpressionCollection Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public CodeExpression TargetObject
        {
            get
            {
                return this.targetObject;
            }
            set
            {
                this.targetObject = value;
            }
        }
    }
}

