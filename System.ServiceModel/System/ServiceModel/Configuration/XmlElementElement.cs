namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security;
    using System.ServiceModel;
    using System.Xml;

    public sealed class XmlElementElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public XmlElementElement()
        {
        }

        public XmlElementElement(System.Xml.XmlElement element) : this()
        {
            this.XmlElement = element;
        }

        public void Copy(XmlElementElement source)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            if (source.XmlElement != null)
            {
                this.XmlElement = (System.Xml.XmlElement) source.XmlElement.Clone();
            }
        }

        [SecuritySafeCritical]
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            this.SetIsPresent();
            this.DeserializeElementCore(reader);
        }

        private void DeserializeElementCore(XmlReader reader)
        {
            XmlDocument document = new XmlDocument();
            this.XmlElement = (System.Xml.XmlElement) document.ReadNode(reader);
        }

        protected override void PostDeserialize()
        {
            this.Validate();
            base.PostDeserialize();
        }

        internal void ResetInternal(XmlElementElement element)
        {
            this.Reset(element);
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            bool flag = this.XmlElement != null;
            if (flag && (writer != null))
            {
                if (!string.Equals(elementName, "xmlElement", StringComparison.Ordinal))
                {
                    writer.WriteStartElement(elementName);
                }
                using (XmlNodeReader reader = new XmlNodeReader(this.XmlElement))
                {
                    writer.WriteNode(reader, false);
                }
                if (!string.Equals(elementName, "xmlElement", StringComparison.Ordinal))
                {
                    writer.WriteEndElement();
                }
            }
            return flag;
        }

        [SecurityCritical]
        private void SetIsPresent()
        {
            ConfigurationHelpers.SetIsPresent(this);
        }

        private void Validate()
        {
            if (this.XmlElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigXmlElementMustBeSet"), base.ElementInformation.Source, base.ElementInformation.LineNumber));
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("xmlElement", typeof(System.Xml.XmlElement), null, null, null, ConfigurationPropertyOptions.IsKey));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("xmlElement", DefaultValue=null, Options=ConfigurationPropertyOptions.IsKey)]
        public System.Xml.XmlElement XmlElement
        {
            get
            {
                return (System.Xml.XmlElement) base["xmlElement"];
            }
            set
            {
                base["xmlElement"] = value;
            }
        }
    }
}

