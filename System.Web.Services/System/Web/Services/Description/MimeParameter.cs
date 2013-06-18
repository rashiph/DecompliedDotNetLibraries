namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;

    internal class MimeParameter
    {
        private CodeAttributeDeclarationCollection attrs;
        private string name;
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

        internal string Name
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

