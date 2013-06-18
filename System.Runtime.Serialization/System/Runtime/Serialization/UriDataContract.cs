namespace System.Runtime.Serialization
{
    using System;

    internal class UriDataContract : PrimitiveDataContract
    {
        internal UriDataContract() : base(typeof(Uri), DictionaryGlobals.UriLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsUri(), context);
            }
            if (!base.TryReadNullAtTopLevel(reader))
            {
                return reader.ReadElementContentAsUri();
            }
            return null;
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteUri((Uri) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsUri";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteUri";
            }
        }
    }
}

