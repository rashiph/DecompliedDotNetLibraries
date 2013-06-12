namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeGotoStatement : CodeStatement
    {
        private string label;

        public CodeGotoStatement()
        {
        }

        public CodeGotoStatement(string label)
        {
            this.Label = label;
        }

        public string Label
        {
            get
            {
                return this.label;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                this.label = value;
            }
        }
    }
}

