namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeDelegateCreateExpression : CodeExpression
    {
        private CodeTypeReference delegateType;
        private string methodName;
        private CodeExpression targetObject;

        public CodeDelegateCreateExpression()
        {
        }

        public CodeDelegateCreateExpression(CodeTypeReference delegateType, CodeExpression targetObject, string methodName)
        {
            this.delegateType = delegateType;
            this.targetObject = targetObject;
            this.methodName = methodName;
        }

        public CodeTypeReference DelegateType
        {
            get
            {
                if (this.delegateType == null)
                {
                    this.delegateType = new CodeTypeReference("");
                }
                return this.delegateType;
            }
            set
            {
                this.delegateType = value;
            }
        }

        public string MethodName
        {
            get
            {
                if (this.methodName != null)
                {
                    return this.methodName;
                }
                return string.Empty;
            }
            set
            {
                this.methodName = value;
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

