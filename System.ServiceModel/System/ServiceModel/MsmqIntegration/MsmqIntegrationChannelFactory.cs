namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml.Serialization;

    internal sealed class MsmqIntegrationChannelFactory : MsmqChannelFactoryBase<IOutputChannel>
    {
        private System.ServiceModel.MsmqIntegration.ActiveXSerializer activeXSerializer;
        private System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter;
        private const int maxSerializerTableSize = 0x400;
        private MsmqMessageSerializationFormat serializationFormat;
        private HybridDictionary xmlSerializerTable;

        internal MsmqIntegrationChannelFactory(MsmqIntegrationBindingElement bindingElement, BindingContext context) : base(bindingElement, context, null)
        {
            this.serializationFormat = bindingElement.SerializationFormat;
        }

        private XmlSerializer GetXmlSerializerForType(System.Type serializedType)
        {
            if (this.xmlSerializerTable == null)
            {
                lock (base.ThisLock)
                {
                    if (this.xmlSerializerTable == null)
                    {
                        this.xmlSerializerTable = new HybridDictionary();
                    }
                }
            }
            XmlSerializer serializer = (XmlSerializer) this.xmlSerializerTable[serializedType];
            if (serializer != null)
            {
                return serializer;
            }
            lock (base.ThisLock)
            {
                if (this.xmlSerializerTable.Count >= 0x400)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("MsmqSerializationTableFull", new object[] { 0x400 })));
                }
                serializer = (XmlSerializer) this.xmlSerializerTable[serializedType];
                if (serializer == null)
                {
                    serializer = new XmlSerializer(serializedType);
                    this.xmlSerializerTable[serializedType] = serializer;
                }
                return serializer;
            }
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            base.ValidateScheme(via);
            return new MsmqIntegrationOutputChannel(this, to, via, base.ManualAddressing);
        }

        internal Stream Serialize(MsmqIntegrationMessageProperty property)
        {
            Stream stream;
            switch (this.SerializationFormat)
            {
                case MsmqMessageSerializationFormat.Xml:
                    stream = new MemoryStream();
                    this.GetXmlSerializerForType(property.Body.GetType()).Serialize(stream, property.Body);
                    return stream;

                case MsmqMessageSerializationFormat.Binary:
                    stream = new MemoryStream();
                    this.BinaryFormatter.Serialize(stream, property.Body);
                    property.BodyType = 0x300;
                    return stream;

                case MsmqMessageSerializationFormat.ActiveX:
                {
                    if (property.BodyType.HasValue)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MsmqCannotUseBodyTypeWithActiveXSerialization")));
                    }
                    stream = new MemoryStream();
                    int bodyType = 0;
                    this.ActiveXSerializer.Serialize(stream as MemoryStream, property.Body, ref bodyType);
                    property.BodyType = new int?(bodyType);
                    return stream;
                }
                case MsmqMessageSerializationFormat.ByteArray:
                {
                    byte[] body = property.Body as byte[];
                    if (body == null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqByteArrayBodyExpected")));
                    }
                    stream = new MemoryStream();
                    stream.Write(body, 0, body.Length);
                    return stream;
                }
                case MsmqMessageSerializationFormat.Stream:
                {
                    Stream stream2 = property.Body as Stream;
                    if (stream2 == null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqStreamBodyExpected")));
                    }
                    return stream2;
                }
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("MsmqUnsupportedSerializationFormat", new object[] { this.SerializationFormat })));
        }

        private System.ServiceModel.MsmqIntegration.ActiveXSerializer ActiveXSerializer
        {
            get
            {
                if (this.activeXSerializer == null)
                {
                    lock (base.ThisLock)
                    {
                        if (this.activeXSerializer == null)
                        {
                            this.activeXSerializer = new System.ServiceModel.MsmqIntegration.ActiveXSerializer();
                        }
                    }
                }
                return this.activeXSerializer;
            }
        }

        private System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BinaryFormatter
        {
            get
            {
                if (this.binaryFormatter == null)
                {
                    lock (base.ThisLock)
                    {
                        if (this.binaryFormatter == null)
                        {
                            this.binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        }
                    }
                }
                return this.binaryFormatter;
            }
        }

        public MsmqMessageSerializationFormat SerializationFormat
        {
            get
            {
                base.ThrowIfDisposed();
                return this.serializationFormat;
            }
        }
    }
}

