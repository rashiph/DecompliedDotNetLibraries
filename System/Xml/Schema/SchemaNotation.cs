namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal sealed class SchemaNotation
    {
        private XmlQualifiedName name;
        private string pubid;
        internal const int PUBLIC = 1;
        internal const int SYSTEM = 0;
        private string systemLiteral;

        internal SchemaNotation(XmlQualifiedName name)
        {
            this.name = name;
        }

        internal XmlQualifiedName Name
        {
            get
            {
                return this.name;
            }
        }

        internal string Pubid
        {
            get
            {
                return this.pubid;
            }
            set
            {
                this.pubid = value;
            }
        }

        internal string SystemLiteral
        {
            get
            {
                return this.systemLiteral;
            }
            set
            {
                this.systemLiteral = value;
            }
        }
    }
}

