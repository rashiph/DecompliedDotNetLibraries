namespace System.Xml.Schema
{
    using System;

    internal class RedefineEntry
    {
        internal XmlSchemaRedefine redefine;
        internal XmlSchema schemaToUpdate;

        public RedefineEntry(XmlSchemaRedefine external, XmlSchema schema)
        {
            this.redefine = external;
            this.schemaToUpdate = schema;
        }
    }
}

