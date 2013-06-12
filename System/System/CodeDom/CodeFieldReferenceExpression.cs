namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeFieldReferenceExpression : CodeExpression
    {
        private string fieldName;
        private CodeExpression targetObject;

        public CodeFieldReferenceExpression()
        {
        }

        public CodeFieldReferenceExpression(CodeExpression targetObject, string fieldName)
        {
            this.TargetObject = targetObject;
            this.FieldName = fieldName;
        }

        public string FieldName
        {
            get
            {
                if (this.fieldName != null)
                {
                    return this.fieldName;
                }
                return string.Empty;
            }
            set
            {
                this.fieldName = value;
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

