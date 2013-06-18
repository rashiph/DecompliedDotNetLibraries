namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Xml;

    internal sealed class UnrecognizedPolicyAssertionElement : BindingElementExtensionElement
    {
        private ICollection<XmlElement> bindingAsserions;
        private IDictionary<MessageDescription, ICollection<XmlElement>> messageAssertions;
        private IDictionary<OperationDescription, ICollection<XmlElement>> operationAssertions;
        private XmlQualifiedName wsdlBinding;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            UnrecognizedPolicyAssertionElement element = (UnrecognizedPolicyAssertionElement) from;
            this.wsdlBinding = element.wsdlBinding;
            this.bindingAsserions = element.bindingAsserions;
            this.operationAssertions = element.operationAssertions;
            this.messageAssertions = element.messageAssertions;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            return new UnrecognizedAssertionsBindingElement(XmlQualifiedName.Empty, null);
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            UnrecognizedAssertionsBindingElement element = (UnrecognizedAssertionsBindingElement) bindingElement;
            this.wsdlBinding = element.WsdlBinding;
            this.bindingAsserions = element.BindingAsserions;
            this.operationAssertions = element.OperationAssertions;
            this.messageAssertions = element.MessageAssertions;
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            XmlDocument document = new XmlDocument();
            if (((writer == null) || (this.bindingAsserions == null)) || (this.bindingAsserions.Count <= 0))
            {
                return false;
            }
            int indent = 1;
            XmlWriterSettings settings = this.WriterSettings(writer);
            this.WriteComment(System.ServiceModel.SR.GetString("UnrecognizedBindingAssertions1", new object[] { this.wsdlBinding.Namespace }), indent, writer, settings);
            this.WriteComment(string.Format(CultureInfo.InvariantCulture, "<wsdl:binding name='{0}'>", new object[] { this.wsdlBinding.Name }), indent, writer, settings);
            indent++;
            foreach (XmlElement element in this.bindingAsserions)
            {
                this.WriteComment(this.ToString(element, document), indent, writer, settings);
            }
            if ((this.operationAssertions != null) && (this.operationAssertions.Count != 0))
            {
                foreach (OperationDescription description in this.operationAssertions.Keys)
                {
                    this.WriteComment(string.Format(CultureInfo.InvariantCulture, "<wsdl:operation name='{0}'>", new object[] { description.Name }), indent, writer, settings);
                    indent++;
                    foreach (XmlElement element2 in this.operationAssertions[description])
                    {
                        this.WriteComment(this.ToString(element2, document), indent, writer, settings);
                    }
                    if ((this.messageAssertions == null) || (this.messageAssertions.Count == 0))
                    {
                        return true;
                    }
                    foreach (MessageDescription description2 in description.Messages)
                    {
                        ICollection<XmlElement> is2;
                        if (this.messageAssertions.TryGetValue(description2, out is2))
                        {
                            if (description2.Direction == MessageDirection.Input)
                            {
                                this.WriteComment("<wsdl:input>", indent, writer, settings);
                            }
                            else if (description2.Direction == MessageDirection.Output)
                            {
                                this.WriteComment("<wsdl:output>", indent, writer, settings);
                            }
                            foreach (XmlElement element3 in is2)
                            {
                                this.WriteComment(this.ToString(element3, document), indent + 1, writer, settings);
                            }
                        }
                    }
                }
            }
            return true;
        }

        private string ToString(XmlElement e, XmlDocument document)
        {
            XmlElement element = document.CreateElement(e.Prefix, e.LocalName, e.NamespaceURI);
            element.InsertBefore(document.CreateTextNode(".."), null);
            return element.OuterXml;
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            if (sourceElement is UnrecognizedPolicyAssertionElement)
            {
                this.wsdlBinding = ((UnrecognizedPolicyAssertionElement) sourceElement).wsdlBinding;
                this.bindingAsserions = ((UnrecognizedPolicyAssertionElement) sourceElement).bindingAsserions;
                this.operationAssertions = ((UnrecognizedPolicyAssertionElement) sourceElement).operationAssertions;
                this.messageAssertions = ((UnrecognizedPolicyAssertionElement) sourceElement).messageAssertions;
            }
            base.Unmerge(sourceElement, parentElement, saveMode);
        }

        private void WriteComment(string text, int indent, XmlWriter writer, XmlWriterSettings settings)
        {
            if (settings.Indent)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < indent; i++)
                {
                    builder.Append(settings.IndentChars);
                }
                builder.Append(text);
                builder.Append(settings.IndentChars);
                text = builder.ToString();
            }
            writer.WriteComment(text);
        }

        private XmlWriterSettings WriterSettings(XmlWriter writer)
        {
            if (writer.Settings != null)
            {
                return writer.Settings;
            }
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlTextWriter writer2 = writer as XmlTextWriter;
            if (writer2 != null)
            {
                settings.Indent = writer2.Formatting == Formatting.Indented;
                if (!settings.Indent || (writer2.Indentation <= 0))
                {
                    return settings;
                }
                StringBuilder builder = new StringBuilder(writer2.Indentation);
                for (int i = 0; i < writer2.Indentation; i++)
                {
                    builder.Append(writer2.IndentChar);
                }
                settings.IndentChars = builder.ToString();
            }
            return settings;
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(UnrecognizedAssertionsBindingElement);
            }
        }
    }
}

