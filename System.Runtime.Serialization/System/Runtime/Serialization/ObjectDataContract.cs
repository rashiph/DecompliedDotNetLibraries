namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;

    internal class ObjectDataContract : PrimitiveDataContract
    {
        internal ObjectDataContract() : base(typeof(object), DictionaryGlobals.ObjectLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            object obj2;
            if (reader.IsEmptyElement)
            {
                reader.Skip();
                obj2 = new object();
            }
            else
            {
                string localName = reader.LocalName;
                string namespaceURI = reader.NamespaceURI;
                reader.Read();
                try
                {
                    reader.ReadEndElement();
                    obj2 = new object();
                }
                catch (XmlException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("XmlForObjectCannotHaveContent", new object[] { localName, namespaceURI }), exception));
                }
            }
            if (context != null)
            {
                return base.HandleReadValue(obj2, context);
            }
            return obj2;
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
        }

        internal override bool CanContainReferences
        {
            get
            {
                return true;
            }
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
                return "ReadElementContentAsAnyType";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteAnyType";
            }
        }
    }
}

