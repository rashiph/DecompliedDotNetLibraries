namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using System.Xml;

    public abstract class AddressHeader
    {
        private ParameterHeader header;

        protected AddressHeader()
        {
        }

        public static AddressHeader CreateAddressHeader(object value)
        {
            System.Type objectType = GetObjectType(value);
            return CreateAddressHeader(value, DataContractSerializerDefaults.CreateSerializer(objectType, 0x7fffffff));
        }

        public static AddressHeader CreateAddressHeader(object value, XmlObjectSerializer serializer)
        {
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            }
            return new XmlObjectSerializerAddressHeader(value, serializer);
        }

        public static AddressHeader CreateAddressHeader(string name, string ns, object value)
        {
            return CreateAddressHeader(name, ns, value, DataContractSerializerDefaults.CreateSerializer(GetObjectType(value), name, ns, 0x7fffffff));
        }

        internal static AddressHeader CreateAddressHeader(XmlDictionaryString name, XmlDictionaryString ns, object value)
        {
            return new DictionaryAddressHeader(name, ns, value);
        }

        public static AddressHeader CreateAddressHeader(string name, string ns, object value, XmlObjectSerializer serializer)
        {
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            }
            return new XmlObjectSerializerAddressHeader(name, ns, value, serializer);
        }

        public override bool Equals(object obj)
        {
            AddressHeader header = obj as AddressHeader;
            if (header == null)
            {
                return false;
            }
            StringBuilder builder = new StringBuilder();
            string comparableForm = this.GetComparableForm(builder);
            builder.Remove(0, builder.Length);
            string strB = header.GetComparableForm(builder);
            if (comparableForm.Length != strB.Length)
            {
                return false;
            }
            if (string.CompareOrdinal(comparableForm, strB) != 0)
            {
                return false;
            }
            return true;
        }

        public virtual XmlDictionaryReader GetAddressHeaderReader()
        {
            XmlBuffer buffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            this.WriteAddressHeader(writer);
            buffer.CloseSection();
            buffer.Close();
            return buffer.GetReader(0);
        }

        internal string GetComparableForm()
        {
            return this.GetComparableForm(new StringBuilder());
        }

        internal string GetComparableForm(StringBuilder builder)
        {
            return EndpointAddressProcessor.GetComparableForm(builder, this.GetComparableReader());
        }

        private XmlDictionaryReader GetComparableReader()
        {
            XmlBuffer buffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            ParameterHeader.WriteStartHeader(writer, this, AddressingVersion.WSAddressingAugust2004);
            ParameterHeader.WriteHeaderContents(writer, this);
            writer.WriteEndElement();
            buffer.CloseSection();
            buffer.Close();
            return buffer.GetReader(0);
        }

        public override int GetHashCode()
        {
            return this.GetComparableForm().GetHashCode();
        }

        private static System.Type GetObjectType(object value)
        {
            if (value != null)
            {
                return value.GetType();
            }
            return typeof(object);
        }

        public T GetValue<T>()
        {
            return this.GetValue<T>(DataContractSerializerDefaults.CreateSerializer(typeof(T), this.Name, this.Namespace, 0x7fffffff));
        }

        public T GetValue<T>(XmlObjectSerializer serializer)
        {
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            }
            using (XmlDictionaryReader reader = this.GetAddressHeaderReader())
            {
                if (!serializer.IsStartObject(reader))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ExpectedElementMissing", new object[] { this.Name, this.Namespace })));
                }
                return (T) serializer.ReadObject(reader);
            }
        }

        protected abstract void OnWriteAddressHeaderContents(XmlDictionaryWriter writer);
        protected virtual void OnWriteStartAddressHeader(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(this.Name, this.Namespace);
        }

        public MessageHeader ToMessageHeader()
        {
            if (this.header == null)
            {
                this.header = new ParameterHeader(this);
            }
            return this.header;
        }

        public void WriteAddressHeader(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            this.WriteStartAddressHeader(writer);
            this.WriteAddressHeaderContents(writer);
            writer.WriteEndElement();
        }

        public void WriteAddressHeader(XmlWriter writer)
        {
            this.WriteAddressHeader(XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteAddressHeaderContents(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            this.OnWriteAddressHeaderContents(writer);
        }

        public void WriteStartAddressHeader(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            this.OnWriteStartAddressHeader(writer);
        }

        internal bool IsReferenceProperty
        {
            get
            {
                BufferedAddressHeader header = this as BufferedAddressHeader;
                return ((header != null) && header.IsReferencePropertyHeader);
            }
        }

        public abstract string Name { get; }

        public abstract string Namespace { get; }

        private class DictionaryAddressHeader : AddressHeader.XmlObjectSerializerAddressHeader
        {
            private XmlDictionaryString name;
            private XmlDictionaryString ns;

            public DictionaryAddressHeader(XmlDictionaryString name, XmlDictionaryString ns, object value) : base(name.Value, ns.Value, value, DataContractSerializerDefaults.CreateSerializer(AddressHeader.GetObjectType(value), name, ns, 0x7fffffff))
            {
                this.name = name;
                this.ns = ns;
            }

            protected override void OnWriteStartAddressHeader(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement(this.name, this.ns);
            }
        }

        private class ParameterHeader : MessageHeader
        {
            private AddressHeader parameter;

            public ParameterHeader(AddressHeader parameter)
            {
                this.parameter = parameter;
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                WriteHeaderContents(writer, this.parameter);
            }

            protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                if (messageVersion == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageVersion"));
                }
                WriteStartHeader(writer, this.parameter, messageVersion.Addressing);
            }

            internal static void WriteHeaderContents(XmlDictionaryWriter writer, AddressHeader parameter)
            {
                parameter.WriteAddressHeaderContents(writer);
            }

            internal static void WriteStartHeader(XmlDictionaryWriter writer, AddressHeader parameter, AddressingVersion addressingVersion)
            {
                parameter.WriteStartAddressHeader(writer);
                if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    writer.WriteAttributeString(XD.AddressingDictionary.IsReferenceParameter, XD.Addressing10Dictionary.Namespace, "true");
                }
            }

            public override bool IsReferenceParameter
            {
                get
                {
                    return true;
                }
            }

            public override string Name
            {
                get
                {
                    return this.parameter.Name;
                }
            }

            public override string Namespace
            {
                get
                {
                    return this.parameter.Namespace;
                }
            }
        }

        private class XmlObjectSerializerAddressHeader : AddressHeader
        {
            private string name;
            private string ns;
            private object objectToSerialize;
            private XmlObjectSerializer serializer;

            public XmlObjectSerializerAddressHeader(object objectToSerialize, XmlObjectSerializer serializer)
            {
                this.serializer = serializer;
                this.objectToSerialize = objectToSerialize;
                System.Type type = (objectToSerialize == null) ? typeof(object) : objectToSerialize.GetType();
                XmlQualifiedName rootElementName = new XsdDataContractExporter().GetRootElementName(type);
                this.name = rootElementName.Name;
                this.ns = rootElementName.Namespace;
            }

            public XmlObjectSerializerAddressHeader(string name, string ns, object objectToSerialize, XmlObjectSerializer serializer)
            {
                if ((name == null) || (name.Length == 0))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
                }
                this.serializer = serializer;
                this.objectToSerialize = objectToSerialize;
                this.name = name;
                this.ns = ns;
            }

            protected override void OnWriteAddressHeaderContents(XmlDictionaryWriter writer)
            {
                lock (this.ThisLock)
                {
                    this.serializer.WriteObjectContent(writer, this.objectToSerialize);
                }
            }

            public override string Name
            {
                get
                {
                    return this.name;
                }
            }

            public override string Namespace
            {
                get
                {
                    return this.ns;
                }
            }

            private object ThisLock
            {
                get
                {
                    return this;
                }
            }
        }
    }
}

