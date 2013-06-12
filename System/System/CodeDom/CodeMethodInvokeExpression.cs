namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeMethodInvokeExpression : CodeExpression
    {
        private CodeMethodReferenceExpression method;
        private CodeExpressionCollection parameters;

        public CodeMethodInvokeExpression()
        {
            this.parameters = new CodeExpressionCollection();
        }

        public CodeMethodInvokeExpression(CodeMethodReferenceExpression method, params CodeExpression[] parameters)
        {
            this.parameters = new CodeExpressionCollection();
            this.method = method;
            this.Parameters.AddRange(parameters);
        }

        public CodeMethodInvokeExpression(CodeExpression targetObject, string methodName, params CodeExpression[] parameters)
        {
            this.parameters = new CodeExpressionCollection();
            this.method = new CodeMethodReferenceExpression(targetObject, methodName);
            this.Parameters.AddRange(parameters);
        }

        public CodeMethodReferenceExpression Method
        {
            get
            {
                if (this.method == null)
                {
                    this.method = new CodeMethodReferenceExpression();
                }
                return this.method;
            }
            set
            {
                this.method = value;
            }
        }

        public CodeExpressionCollection Parameters
        {
            get
            {
                return this.parameters;
            }
        }
    }
}

