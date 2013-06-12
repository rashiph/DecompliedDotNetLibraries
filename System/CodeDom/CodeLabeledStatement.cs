namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeLabeledStatement : CodeStatement
    {
        private string label;
        private CodeStatement statement;

        public CodeLabeledStatement()
        {
        }

        public CodeLabeledStatement(string label)
        {
            this.label = label;
        }

        public CodeLabeledStatement(string label, CodeStatement statement)
        {
            this.label = label;
            this.statement = statement;
        }

        public string Label
        {
            get
            {
                if (this.label != null)
                {
                    return this.label;
                }
                return string.Empty;
            }
            set
            {
                this.label = value;
            }
        }

        public CodeStatement Statement
        {
            get
            {
                return this.statement;
            }
            set
            {
                this.statement = value;
            }
        }
    }
}

