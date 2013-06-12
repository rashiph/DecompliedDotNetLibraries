namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodePropertyReferenceExpression : CodeExpression
    {
        private CodeExpressionCollection parameters;
        private string propertyName;
        private CodeExpression targetObject;

        public CodePropertyReferenceExpression()
        {
            this.parameters = new CodeExpressionCollection();
        }

        public CodePropertyReferenceExpression(CodeExpression targetObject, string propertyName)
        {
            this.parameters = new CodeExpressionCollection();
            this.TargetObject = targetObject;
            this.PropertyName = propertyName;
        }

        public string PropertyName
        {
            get
            {
                if (this.propertyName != null)
                {
                    return this.propertyName;
                }
                return string.Empty;
            }
            set
            {
                this.propertyName = value;
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

