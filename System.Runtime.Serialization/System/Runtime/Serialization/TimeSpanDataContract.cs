namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;

    internal class TimeSpanDataContract : PrimitiveDataContract
    {
        internal TimeSpanDataContract() : this(DictionaryGlobals.TimeSpanLocalName, DictionaryGlobals.SerializationNamespace)
        {
        }

        internal TimeSpanDataContract(XmlDictionaryString name, XmlDictionaryString ns) : base(typeof(TimeSpan), name, ns)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsTimeSpan(), context);
            }
            return reader.ReadElementContentAsTimeSpan();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteTimeSpan((TimeSpan) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsTimeSpan";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteTimeSpan";
            }
        }
    }
}

