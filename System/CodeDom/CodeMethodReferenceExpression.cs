namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeMethodReferenceExpression : CodeExpression
    {
        private string methodName;
        private CodeExpression targetObject;
        [OptionalField]
        private CodeTypeReferenceCollection typeArguments;

        public CodeMethodReferenceExpression()
        {
        }

        public CodeMethodReferenceExpression(CodeExpression targetObject, string methodName)
        {
            this.TargetObject = targetObject;
            this.MethodName = methodName;
        }

        public CodeMethodReferenceExpression(CodeExpression targetObject, string methodName, params CodeTypeReference[] typeParameters)
        {
            this.TargetObject = targetObject;
            this.MethodName = methodName;
            if ((typeParameters != null) && (typeParameters.Length > 0))
            {
                this.TypeArguments.AddRange(typeParameters);
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

        [ComVisible(false)]
        public CodeTypeReferenceCollection TypeArguments
        {
            get
            {
                if (this.typeArguments == null)
                {
                    this.typeArguments = new CodeTypeReferenceCollection();
                }
                return this.typeArguments;
            }
        }
    }
}

