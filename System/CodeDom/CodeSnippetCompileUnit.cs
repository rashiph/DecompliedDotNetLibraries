namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeSnippetCompileUnit : CodeCompileUnit
    {
        private CodeLinePragma linePragma;
        private string value;

        public CodeSnippetCompileUnit()
        {
        }

        public CodeSnippetCompileUnit(string value)
        {
            this.Value = value;
        }

        public CodeLinePragma LinePragma
        {
            get
            {
                return this.linePragma;
            }
            set
            {
                this.linePragma = value;
            }
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

