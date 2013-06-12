namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeVariableReferenceExpression : CodeExpression
    {
        private string variableName;

        public CodeVariableReferenceExpression()
        {
        }

        public CodeVariableReferenceExpression(string variableName)
        {
            this.variableName = variableName;
        }

        public string VariableName
        {
            get
            {
                if (this.variableName != null)
                {
                    return this.variableName;
                }
                return string.Empty;
            }
            set
            {
                this.variableName = value;
            }
        }
    }
}

