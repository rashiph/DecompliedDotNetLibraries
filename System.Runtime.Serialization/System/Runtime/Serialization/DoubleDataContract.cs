namespace System.Runtime.Serialization
{
    using System;

    internal class DoubleDataContract : PrimitiveDataContract
    {
        internal DoubleDataContract() : base(typeof(double), DictionaryGlobals.DoubleLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsDouble(), context);
            }
            return reader.ReadElementContentAsDouble();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteDouble((double) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsDouble";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteDouble";
            }
        }
    }
}

