namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeNamespaceImport : CodeObject
    {
        private CodeLinePragma linePragma;
        private string nameSpace;

        public CodeNamespaceImport()
        {
        }

        public CodeNamespaceImport(string nameSpace)
        {
            this.Namespace = nameSpace;
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

        public string Namespace
        {
            get
            {
                if (this.nameSpace != null)
                {
                    return this.nameSpace;
                }
                return string.Empty;
            }
            set
            {
                this.nameSpace = value;
            }
        }
    }
}

