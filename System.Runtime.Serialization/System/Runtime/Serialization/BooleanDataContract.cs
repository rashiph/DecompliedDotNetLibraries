namespace System.Runtime.Serialization
{
    using System;

    internal class BooleanDataContract : PrimitiveDataContract
    {
        internal BooleanDataContract() : base(typeof(bool), DictionaryGlobals.BooleanLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsBoolean(), context);
            }
            return reader.ReadElementContentAsBoolean();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteBoolean((bool) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsBoolean";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteBoolean";
            }
        }
    }
}

