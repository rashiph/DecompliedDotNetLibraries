namespace System.Runtime.Serialization
{
    using System;

    internal class FloatDataContract : PrimitiveDataContract
    {
        internal FloatDataContract() : base(typeof(float), DictionaryGlobals.FloatLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsFloat(), context);
            }
            return reader.ReadElementContentAsFloat();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteFloat((float) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsFloat";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteFloat";
            }
        }
    }
}

