namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Serialization;

    internal class XmlSerializerObjectSerializer : XmlObjectSerializer
    {
        private bool isSerializerSetExplicit;
        private string rootName;
        private string rootNamespace;
        private Type rootType;
        private XmlSerializer serializer;

        internal XmlSerializerObjectSerializer(Type type)
        {
            this.Initialize(type, null, null, null);
        }

        internal XmlSerializerObjectSerializer(Type type, XmlQualifiedName qualifiedName, XmlSerializer xmlSerializer)
        {
            if (qualifiedName == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("qualifiedName");
            }
            this.Initialize(type, qualifiedName.Name, qualifiedName.Namespace, xmlSerializer);
        }

        private void Initialize(Type type, string rootName, string rootNamespace, XmlSerializer xmlSerializer)
        {
            if (type == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("type");
            }
            this.rootType = type;
            this.rootName = rootName;
            this.rootNamespace = (rootNamespace == null) ? string.Empty : rootNamespace;
            this.serializer = xmlSerializer;
            if (this.serializer == null)
            {
                if (this.rootName == null)
                {
                    this.serializer = new XmlSerializer(type);
                }
                else
                {
                    XmlRootAttribute root = new XmlRootAttribute {
                        ElementName = this.rootName,
                        Namespace = this.rootNamespace
                    };
                    this.serializer = new XmlSerializer(type, root);
                }
            }
            else
            {
                this.isSerializerSetExplicit = true;
            }
            if (this.rootName == null)
            {
                XmlTypeMapping mapping = new XmlReflectionImporter().ImportTypeMapping(this.rootType);
                this.rootName = mapping.ElementName;
                this.rootNamespace = mapping.Namespace;
            }
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            reader.MoveToElement();
            if (this.rootName != null)
            {
                return reader.IsStartElement(this.rootName, this.rootNamespace);
            }
            return reader.IsStartElement();
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            if (!this.isSerializerSetExplicit)
            {
                return this.serializer.Deserialize(reader);
            }
            object[] objArray = (object[]) this.serializer.Deserialize(reader);
            if ((objArray != null) && (objArray.Length > 0))
            {
                return objArray[0];
            }
            return null;
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public override void WriteObject(XmlDictionaryWriter writer, object graph)
        {
            if (this.isSerializerSetExplicit)
            {
                this.serializer.Serialize((XmlWriter) writer, new object[] { graph });
            }
            else
            {
                this.serializer.Serialize((XmlWriter) writer, graph);
            }
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }
    }
}

