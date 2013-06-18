namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;

    internal class MimeReturn
    {
        private CodeAttributeDeclarationCollection attrs;
        private Type readerType;
        private string typeName;

        internal CodeAttributeDeclarationCollection Attributes
        {
            get
            {
                if (this.attrs == null)
                {
                    this.attrs = new CodeAttributeDeclarationCollection();
                }
                return this.attrs;
            }
        }

        internal Type ReaderType
        {
            get
            {
                return this.readerType;
            }
            set
            {
                this.readerType = value;
            }
        }

        internal string TypeName
        {
            get
            {
                if (this.typeName != null)
                {
                    return this.typeName;
                }
                return string.Empty;
            }
            set
            {
                this.typeName = value;
            }
        }
    }
}

