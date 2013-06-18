namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;

    internal class QNameDataContract : PrimitiveDataContract
    {
        internal QNameDataContract() : base(typeof(XmlQualifiedName), DictionaryGlobals.QNameLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsQName(), context);
            }
            if (!base.TryReadNullAtTopLevel(reader))
            {
                return reader.ReadElementContentAsQName();
            }
            return null;
        }

        internal override void WriteRootElement(XmlWriterDelegator writer, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (object.ReferenceEquals(ns, DictionaryGlobals.SerializationNamespace))
            {
                writer.WriteStartElement("z", name, ns);
            }
            else if (((ns != null) && (ns.Value != null)) && (ns.Value.Length > 0))
            {
                writer.WriteStartElement("q", name, ns);
            }
            else
            {
                writer.WriteStartElement(name, ns);
            }
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteQName((XmlQualifiedName) obj);
        }

        internal override bool IsPrimitive
        {
            get
            {
                return false;
            }
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsQName";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteQName";
            }
        }
    }
}

