namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using System.Xml;

    public sealed class XPathMessageFilterElement : ConfigurationElement
    {
        private const int DefaultNodeQuota = 0x3e8;
        private ConfigurationPropertyCollection properties;

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            StringBuilder output = new StringBuilder();
            string str = string.Empty;
            XmlWriterSettings settings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = false
            };
            using (XmlWriter writer = XmlWriter.Create(output, settings))
            {
                writer.WriteStartElement(reader.Name);
                if (0 < reader.AttributeCount)
                {
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToAttribute(i);
                        if (reader.Name.Equals("nodeQuota", StringComparison.Ordinal))
                        {
                            str = reader.Value;
                        }
                        else if (reader.Name.Contains(":"))
                        {
                            string[] strArray = reader.Name.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            writer.WriteAttributeString(strArray[0], strArray[1], null, reader.Value);
                        }
                        else
                        {
                            writer.WriteAttributeString(reader.Name, reader.Value);
                        }
                    }
                    reader.MoveToElement();
                }
                string str2 = reader.ReadString().Trim();
                if (string.IsNullOrEmpty(str2))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ConfigXPathFilterMustNotBeEmpty")));
                }
                writer.WriteString(str2);
                writer.WriteEndElement();
            }
            XPathMessageFilter filter = null;
            using (StringReader reader2 = new StringReader(output.ToString()))
            {
                using (XmlReader reader3 = XmlReader.Create(reader2))
                {
                    filter = new XPathMessageFilter(reader3);
                }
            }
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    filter.NodeQuota = int.Parse(str, CultureInfo.CurrentCulture);
                }
                else
                {
                    filter.NodeQuota = 0x3e8;
                }
            }
            this.Filter = filter;
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            bool flag = this.Filter != null;
            if (flag && (writer != null))
            {
                writer.WriteStartElement(elementName);
                writer.WriteAttributeString("nodeQuota", this.Filter.NodeQuota.ToString(NumberFormatInfo.CurrentInfo));
                StringBuilder output = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings {
                    ConformanceLevel = ConformanceLevel.Fragment,
                    OmitXmlDeclaration = false
                };
                using (XmlWriter writer2 = XmlWriter.Create(output, settings))
                {
                    this.Filter.WriteXPathTo(writer2, null, elementName, null, true);
                }
                using (StringReader reader = new StringReader(output.ToString()))
                {
                    using (XmlReader reader2 = XmlReader.Create(reader))
                    {
                        if (reader2.Read())
                        {
                            if (0 < reader2.AttributeCount)
                            {
                                for (int i = 0; i < reader2.AttributeCount; i++)
                                {
                                    reader2.MoveToAttribute(i);
                                    writer.WriteAttributeString(reader2.Name, reader2.Value);
                                }
                                reader2.MoveToElement();
                            }
                            writer.WriteString(reader2.ReadString());
                        }
                    }
                }
                writer.WriteEndElement();
            }
            return flag;
        }

        [ConfigurationProperty("filter", DefaultValue=null, Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        public XPathMessageFilter Filter
        {
            get
            {
                return (XPathMessageFilter) base["filter"];
            }
            set
            {
                base["filter"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("filter", typeof(XPathMessageFilter), null, null, null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

