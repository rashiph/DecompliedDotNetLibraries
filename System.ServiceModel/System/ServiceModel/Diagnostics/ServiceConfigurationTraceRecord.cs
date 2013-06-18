namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Configuration;
    using System.Xml;

    internal class ServiceConfigurationTraceRecord : TraceRecord
    {
        private ServiceElement serviceElement;

        internal ServiceConfigurationTraceRecord(ServiceElement serviceElement)
        {
            this.serviceElement = serviceElement;
        }

        private void WriteElementString(string name, string value, XmlWriter xml)
        {
            if (!string.IsNullOrEmpty(value))
            {
                xml.WriteElementString(name, value);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            xml.WriteElementString("FoundServiceElement", (this.serviceElement != null).ToString(CultureInfo.InvariantCulture));
            if (this.serviceElement != null)
            {
                if (!string.IsNullOrEmpty(this.serviceElement.ElementInformation.Source))
                {
                    xml.WriteElementString("ConfigurationFileSource", this.serviceElement.ElementInformation.Source);
                    xml.WriteElementString("ConfigurationFileLineNumber", this.serviceElement.ElementInformation.LineNumber.ToString(CultureInfo.InvariantCulture));
                }
                xml.WriteStartElement("ServiceConfigurationInformation");
                this.WriteElementString("ServiceName", this.serviceElement.Name, xml);
                this.WriteElementString("BehaviorConfiguration", this.serviceElement.BehaviorConfiguration, xml);
                xml.WriteStartElement("Host");
                xml.WriteStartElement("Timeouts");
                xml.WriteElementString("OpenTimeout", this.serviceElement.Host.Timeouts.OpenTimeout.ToString());
                xml.WriteElementString("CloseTimeout", this.serviceElement.Host.Timeouts.CloseTimeout.ToString());
                xml.WriteEndElement();
                if (this.serviceElement.Host.BaseAddresses.Count > 0)
                {
                    xml.WriteStartElement("BaseAddresses");
                    foreach (BaseAddressElement element in this.serviceElement.Host.BaseAddresses)
                    {
                        this.WriteElementString("BaseAddress", element.BaseAddress, xml);
                    }
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();
                xml.WriteStartElement("Endpoints");
                foreach (ServiceEndpointElement element2 in this.serviceElement.Endpoints)
                {
                    xml.WriteStartElement("Endpoint");
                    if (element2.Address != null)
                    {
                        this.WriteElementString("Address", element2.Address.ToString(), xml);
                    }
                    this.WriteElementString("Binding", element2.Binding, xml);
                    this.WriteElementString("BindingConfiguration", element2.BindingConfiguration, xml);
                    this.WriteElementString("BindingName", element2.BindingName, xml);
                    this.WriteElementString("BindingNamespace", element2.BindingNamespace, xml);
                    this.WriteElementString("Contract", element2.Contract, xml);
                    if (element2.ListenUri != null)
                    {
                        xml.WriteElementString("ListenUri", element2.ListenUri.ToString());
                    }
                    xml.WriteElementString("ListenUriMode", element2.ListenUriMode.ToString());
                    this.WriteElementString("Name", element2.Name, xml);
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();
                xml.WriteEndElement();
            }
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("ServiceConfiguration");
            }
        }
    }
}

