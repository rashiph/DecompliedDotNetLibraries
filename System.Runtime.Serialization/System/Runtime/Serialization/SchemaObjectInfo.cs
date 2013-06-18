namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Xml.Schema;

    internal class SchemaObjectInfo
    {
        internal XmlSchemaElement element;
        internal List<XmlSchemaType> knownTypes;
        internal XmlSchema schema;
        internal XmlSchemaType type;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SchemaObjectInfo(XmlSchemaType type, XmlSchemaElement element, XmlSchema schema, List<XmlSchemaType> knownTypes)
        {
            this.type = type;
            this.element = element;
            this.schema = schema;
            this.knownTypes = knownTypes;
        }
    }
}

