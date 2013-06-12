namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeSnippetExpression : CodeExpression
    {
        private string value;

        public CodeSnippetExpression()
        {
        }

        public CodeSnippetExpression(string value)
        {
            this.Value = value;
        }

        public string Value
        {
            get
            {
                if (this.value != null)
                {
                    return this.value;
                }
                return string.Empty;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

