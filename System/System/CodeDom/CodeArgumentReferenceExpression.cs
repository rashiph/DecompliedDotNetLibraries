namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeArgumentReferenceExpression : CodeExpression
    {
        private string parameterName;

        public CodeArgumentReferenceExpression()
        {
        }

        public CodeArgumentReferenceExpression(string parameterName)
        {
            this.parameterName = parameterName;
        }

        public string ParameterName
        {
            get
            {
                if (this.parameterName != null)
                {
                    return this.parameterName;
                }
                return string.Empty;
            }
            set
            {
                this.parameterName = value;
            }
        }
    }
}

