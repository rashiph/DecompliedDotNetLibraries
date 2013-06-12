namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeAttributeArgument
    {
        private string name;
        private CodeExpression value;

        public CodeAttributeArgument()
        {
        }

        public CodeAttributeArgument(CodeExpression value)
        {
            this.Value = value;
        }

        public CodeAttributeArgument(string name, CodeExpression value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            set
            {
                this.name = value;
            }
        }

        public CodeExpression Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

