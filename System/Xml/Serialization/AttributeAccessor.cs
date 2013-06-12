namespace System.Xml.Serialization
{
    using System;
    using System.Xml;
    using System.Xml.Schema;

    internal class AttributeAccessor : Accessor
    {
        private bool isList;
        private bool isSpecial;

        internal void CheckSpecial()
        {
            if (this.Name.LastIndexOf(':') >= 0)
            {
                if (!this.Name.StartsWith("xml:", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(Res.GetString("Xml_InvalidNameChars", new object[] { this.Name }));
                }
                this.Name = this.Name.Substring("xml:".Length);
                base.Namespace = "http://www.w3.org/XML/1998/namespace";
                this.isSpecial = true;
            }
            else if (base.Namespace == "http://www.w3.org/XML/1998/namespace")
            {
                this.isSpecial = true;
            }
            else
            {
                this.isSpecial = false;
            }
            if (this.isSpecial)
            {
                base.Form = XmlSchemaForm.Qualified;
            }
        }

        internal bool IsList
        {
            get
            {
                return this.isList;
            }
            set
            {
                this.isList = value;
            }
        }

        internal bool IsSpecialXmlNamespace
        {
            get
            {
                return this.isSpecial;
            }
        }
    }
}

