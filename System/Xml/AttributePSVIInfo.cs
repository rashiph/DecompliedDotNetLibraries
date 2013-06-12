namespace System.Xml
{
    using System;
    using System.Xml.Schema;

    internal class AttributePSVIInfo
    {
        internal XmlSchemaInfo attributeSchemaInfo = new XmlSchemaInfo();
        internal string localName;
        internal string namespaceUri;
        internal object typedAttributeValue;

        internal AttributePSVIInfo()
        {
        }

        internal void Reset()
        {
            this.typedAttributeValue = null;
            this.localName = string.Empty;
            this.namespaceUri = string.Empty;
            this.attributeSchemaInfo.Clear();
        }
    }
}

