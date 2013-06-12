namespace System.Xml.Schema
{
    using System;

    internal sealed class XmlSchemaCollectionNode
    {
        private string namespaceUri;
        private XmlSchema schema;
        private System.Xml.Schema.SchemaInfo schemaInfo;

        internal string NamespaceURI
        {
            get
            {
                return this.namespaceUri;
            }
            set
            {
                this.namespaceUri = value;
            }
        }

        internal XmlSchema Schema
        {
            get
            {
                return this.schema;
            }
            set
            {
                this.schema = value;
            }
        }

        internal System.Xml.Schema.SchemaInfo SchemaInfo
        {
            get
            {
                return this.schemaInfo;
            }
            set
            {
                this.schemaInfo = value;
            }
        }
    }
}

