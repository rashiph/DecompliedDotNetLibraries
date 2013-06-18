namespace System.Web.Services.Description
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class ServiceDescriptionSerializationWriter : XmlSerializationWriter
    {
        protected override void InitCallbacks()
        {
        }

        private void Write10_XmlSchemaAppInfo(string n, string ns, XmlSchemaAppInfo o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaAppInfo)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaAppInfo", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("source", "", o.Source);
                XmlNode[] markup = o.Markup;
                if (markup != null)
                {
                    for (int i = 0; i < markup.Length; i++)
                    {
                        XmlNode node = markup[i];
                        if (node is XmlElement)
                        {
                            XmlElement element = (XmlElement) node;
                            if ((element == null) && (element != null))
                            {
                                throw base.CreateInvalidAnyTypeException(element);
                            }
                            base.WriteElementLiteral(element, "", null, false, true);
                        }
                        else if (node != null)
                        {
                            node.WriteTo(base.Writer);
                        }
                        else if (node != null)
                        {
                            throw base.CreateUnknownTypeException(node);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private string Write100_SoapBindingUse(SoapBindingUse v)
        {
            switch (v)
            {
                case SoapBindingUse.Encoded:
                    return "encoded";

                case SoapBindingUse.Literal:
                    return "literal";
            }
            long num = (long) v;
            throw base.CreateInvalidEnumValueException(num.ToString(CultureInfo.InvariantCulture), "System.Web.Services.Description.SoapBindingUse");
        }

        private void Write102_Soap12BodyBinding(string n, string ns, Soap12BodyBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Soap12BodyBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("Soap12BodyBinding", "http://schemas.xmlsoap.org/wsdl/soap12/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                if (o.Use != SoapBindingUse.Default)
                {
                    base.WriteAttribute("use", "", this.Write100_SoapBindingUse(o.Use));
                }
                if ((o.Namespace != null) && (o.Namespace.Length != 0))
                {
                    base.WriteAttribute("namespace", "", o.Namespace);
                }
                if ((o.Encoding != null) && (o.Encoding.Length != 0))
                {
                    base.WriteAttribute("encodingStyle", "", o.Encoding);
                }
                base.WriteAttribute("parts", "", o.PartsString);
                base.WriteEndElement(o);
            }
        }

        private void Write103_MimePart(string n, string ns, MimePart o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(MimePart)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("MimePart", "http://schemas.xmlsoap.org/wsdl/mime/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int i = 0; i < extensions.Count; i++)
                    {
                        object obj2 = extensions[i];
                        if (obj2 is Soap12BodyBinding)
                        {
                            this.Write102_Soap12BodyBinding("body", "http://schemas.xmlsoap.org/wsdl/soap12/", (Soap12BodyBinding) obj2, false, false);
                        }
                        else if (obj2 is SoapBodyBinding)
                        {
                            this.Write99_SoapBodyBinding("body", "http://schemas.xmlsoap.org/wsdl/soap/", (SoapBodyBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeContentBinding)
                        {
                            this.Write93_MimeContentBinding("content", "http://schemas.xmlsoap.org/wsdl/mime/", (MimeContentBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeXmlBinding)
                        {
                            this.Write94_MimeXmlBinding("mimeXml", "http://schemas.xmlsoap.org/wsdl/mime/", (MimeXmlBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeTextBinding)
                        {
                            this.Write97_MimeTextBinding("text", "http://microsoft.com/wsdl/mime/textMatching/", (MimeTextBinding) obj2, false, false);
                        }
                        else if (obj2 is XmlElement)
                        {
                            XmlElement element = (XmlElement) obj2;
                            if ((element == null) && (element != null))
                            {
                                throw base.CreateInvalidAnyTypeException(element);
                            }
                            base.WriteElementLiteral(element, "", null, false, true);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write104_MimeMultipartRelatedBinding(string n, string ns, MimeMultipartRelatedBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(MimeMultipartRelatedBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("MimeMultipartRelatedBinding", "http://schemas.xmlsoap.org/wsdl/mime/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                MimePartCollection parts = o.Parts;
                if (parts != null)
                {
                    for (int i = 0; i < parts.Count; i++)
                    {
                        this.Write103_MimePart("part", "http://schemas.xmlsoap.org/wsdl/mime/", parts[i], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write105_SoapHeaderFaultBinding(string n, string ns, SoapHeaderFaultBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(SoapHeaderFaultBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("SoapHeaderFaultBinding", "http://schemas.xmlsoap.org/wsdl/soap/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("message", "", base.FromXmlQualifiedName(o.Message));
                base.WriteAttribute("part", "", o.Part);
                if (o.Use != SoapBindingUse.Default)
                {
                    base.WriteAttribute("use", "", this.Write98_SoapBindingUse(o.Use));
                }
                if ((o.Encoding != null) && (o.Encoding.Length != 0))
                {
                    base.WriteAttribute("encodingStyle", "", o.Encoding);
                }
                if ((o.Namespace != null) && (o.Namespace.Length != 0))
                {
                    base.WriteAttribute("namespace", "", o.Namespace);
                }
                base.WriteEndElement(o);
            }
        }

        private void Write106_SoapHeaderBinding(string n, string ns, SoapHeaderBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(SoapHeaderBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("SoapHeaderBinding", "http://schemas.xmlsoap.org/wsdl/soap/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("message", "", base.FromXmlQualifiedName(o.Message));
                base.WriteAttribute("part", "", o.Part);
                if (o.Use != SoapBindingUse.Default)
                {
                    base.WriteAttribute("use", "", this.Write98_SoapBindingUse(o.Use));
                }
                if ((o.Encoding != null) && (o.Encoding.Length != 0))
                {
                    base.WriteAttribute("encodingStyle", "", o.Encoding);
                }
                if ((o.Namespace != null) && (o.Namespace.Length != 0))
                {
                    base.WriteAttribute("namespace", "", o.Namespace);
                }
                this.Write105_SoapHeaderFaultBinding("headerfault", "http://schemas.xmlsoap.org/wsdl/soap/", o.Fault, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write107_SoapHeaderFaultBinding(string n, string ns, SoapHeaderFaultBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(SoapHeaderFaultBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("SoapHeaderFaultBinding", "http://schemas.xmlsoap.org/wsdl/soap12/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("message", "", base.FromXmlQualifiedName(o.Message));
                base.WriteAttribute("part", "", o.Part);
                if (o.Use != SoapBindingUse.Default)
                {
                    base.WriteAttribute("use", "", this.Write100_SoapBindingUse(o.Use));
                }
                if ((o.Encoding != null) && (o.Encoding.Length != 0))
                {
                    base.WriteAttribute("encodingStyle", "", o.Encoding);
                }
                if ((o.Namespace != null) && (o.Namespace.Length != 0))
                {
                    base.WriteAttribute("namespace", "", o.Namespace);
                }
                base.WriteEndElement(o);
            }
        }

        private void Write109_Soap12HeaderBinding(string n, string ns, Soap12HeaderBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Soap12HeaderBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("Soap12HeaderBinding", "http://schemas.xmlsoap.org/wsdl/soap12/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("message", "", base.FromXmlQualifiedName(o.Message));
                base.WriteAttribute("part", "", o.Part);
                if (o.Use != SoapBindingUse.Default)
                {
                    base.WriteAttribute("use", "", this.Write100_SoapBindingUse(o.Use));
                }
                if ((o.Encoding != null) && (o.Encoding.Length != 0))
                {
                    base.WriteAttribute("encodingStyle", "", o.Encoding);
                }
                if ((o.Namespace != null) && (o.Namespace.Length != 0))
                {
                    base.WriteAttribute("namespace", "", o.Namespace);
                }
                this.Write107_SoapHeaderFaultBinding("headerfault", "http://schemas.xmlsoap.org/wsdl/soap12/", o.Fault, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write11_XmlSchemaAnnotation(string n, string ns, XmlSchemaAnnotation o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaAnnotation)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaAnnotation", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                XmlSchemaObjectCollection items = o.Items;
                if (items != null)
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        XmlSchemaObject obj2 = items[j];
                        if (obj2 is XmlSchemaAppInfo)
                        {
                            this.Write10_XmlSchemaAppInfo("appinfo", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAppInfo) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaDocumentation)
                        {
                            this.Write9_XmlSchemaDocumentation("documentation", "http://www.w3.org/2001/XMLSchema", (XmlSchemaDocumentation) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write110_InputBinding(string n, string ns, InputBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(InputBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("InputBinding", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        object obj2 = extensions[j];
                        if (obj2 is Soap12BodyBinding)
                        {
                            this.Write102_Soap12BodyBinding("body", "http://schemas.xmlsoap.org/wsdl/soap12/", (Soap12BodyBinding) obj2, false, false);
                        }
                        else if (obj2 is Soap12HeaderBinding)
                        {
                            this.Write109_Soap12HeaderBinding("header", "http://schemas.xmlsoap.org/wsdl/soap12/", (Soap12HeaderBinding) obj2, false, false);
                        }
                        else if (obj2 is SoapBodyBinding)
                        {
                            this.Write99_SoapBodyBinding("body", "http://schemas.xmlsoap.org/wsdl/soap/", (SoapBodyBinding) obj2, false, false);
                        }
                        else if (obj2 is SoapHeaderBinding)
                        {
                            this.Write106_SoapHeaderBinding("header", "http://schemas.xmlsoap.org/wsdl/soap/", (SoapHeaderBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeTextBinding)
                        {
                            this.Write97_MimeTextBinding("text", "http://microsoft.com/wsdl/mime/textMatching/", (MimeTextBinding) obj2, false, false);
                        }
                        else if (obj2 is HttpUrlReplacementBinding)
                        {
                            this.Write91_HttpUrlReplacementBinding("urlReplacement", "http://schemas.xmlsoap.org/wsdl/http/", (HttpUrlReplacementBinding) obj2, false, false);
                        }
                        else if (obj2 is HttpUrlEncodedBinding)
                        {
                            this.Write90_HttpUrlEncodedBinding("urlEncoded", "http://schemas.xmlsoap.org/wsdl/http/", (HttpUrlEncodedBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeContentBinding)
                        {
                            this.Write93_MimeContentBinding("content", "http://schemas.xmlsoap.org/wsdl/mime/", (MimeContentBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeMultipartRelatedBinding)
                        {
                            this.Write104_MimeMultipartRelatedBinding("multipartRelated", "http://schemas.xmlsoap.org/wsdl/mime/", (MimeMultipartRelatedBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeXmlBinding)
                        {
                            this.Write94_MimeXmlBinding("mimeXml", "http://schemas.xmlsoap.org/wsdl/mime/", (MimeXmlBinding) obj2, false, false);
                        }
                        else if (obj2 is XmlElement)
                        {
                            XmlElement element = (XmlElement) obj2;
                            if ((element == null) && (element != null))
                            {
                                throw base.CreateInvalidAnyTypeException(element);
                            }
                            base.WriteElementLiteral(element, "", null, false, true);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write111_OutputBinding(string n, string ns, OutputBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(OutputBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("OutputBinding", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        object obj2 = extensions[j];
                        if (obj2 is Soap12BodyBinding)
                        {
                            this.Write102_Soap12BodyBinding("body", "http://schemas.xmlsoap.org/wsdl/soap12/", (Soap12BodyBinding) obj2, false, false);
                        }
                        else if (obj2 is Soap12HeaderBinding)
                        {
                            this.Write109_Soap12HeaderBinding("header", "http://schemas.xmlsoap.org/wsdl/soap12/", (Soap12HeaderBinding) obj2, false, false);
                        }
                        else if (obj2 is SoapHeaderBinding)
                        {
                            this.Write106_SoapHeaderBinding("header", "http://schemas.xmlsoap.org/wsdl/soap/", (SoapHeaderBinding) obj2, false, false);
                        }
                        else if (obj2 is SoapBodyBinding)
                        {
                            this.Write99_SoapBodyBinding("body", "http://schemas.xmlsoap.org/wsdl/soap/", (SoapBodyBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeXmlBinding)
                        {
                            this.Write94_MimeXmlBinding("mimeXml", "http://schemas.xmlsoap.org/wsdl/mime/", (MimeXmlBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeContentBinding)
                        {
                            this.Write93_MimeContentBinding("content", "http://schemas.xmlsoap.org/wsdl/mime/", (MimeContentBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeTextBinding)
                        {
                            this.Write97_MimeTextBinding("text", "http://microsoft.com/wsdl/mime/textMatching/", (MimeTextBinding) obj2, false, false);
                        }
                        else if (obj2 is MimeMultipartRelatedBinding)
                        {
                            this.Write104_MimeMultipartRelatedBinding("multipartRelated", "http://schemas.xmlsoap.org/wsdl/mime/", (MimeMultipartRelatedBinding) obj2, false, false);
                        }
                        else if (obj2 is XmlElement)
                        {
                            XmlElement element = (XmlElement) obj2;
                            if ((element == null) && (element != null))
                            {
                                throw base.CreateInvalidAnyTypeException(element);
                            }
                            base.WriteElementLiteral(element, "", null, false, true);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write112_SoapFaultBinding(string n, string ns, SoapFaultBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(SoapFaultBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("SoapFaultBinding", "http://schemas.xmlsoap.org/wsdl/soap/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                if (o.Use != SoapBindingUse.Default)
                {
                    base.WriteAttribute("use", "", this.Write98_SoapBindingUse(o.Use));
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("namespace", "", o.Namespace);
                if ((o.Encoding != null) && (o.Encoding.Length != 0))
                {
                    base.WriteAttribute("encodingStyle", "", o.Encoding);
                }
                base.WriteEndElement(o);
            }
        }

        private void Write114_Soap12FaultBinding(string n, string ns, Soap12FaultBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Soap12FaultBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("Soap12FaultBinding", "http://schemas.xmlsoap.org/wsdl/soap12/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                if (o.Use != SoapBindingUse.Default)
                {
                    base.WriteAttribute("use", "", this.Write100_SoapBindingUse(o.Use));
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("namespace", "", o.Namespace);
                if ((o.Encoding != null) && (o.Encoding.Length != 0))
                {
                    base.WriteAttribute("encodingStyle", "", o.Encoding);
                }
                base.WriteEndElement(o);
            }
        }

        private void Write115_FaultBinding(string n, string ns, FaultBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(FaultBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("FaultBinding", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        object obj2 = extensions[j];
                        if (obj2 is Soap12FaultBinding)
                        {
                            this.Write114_Soap12FaultBinding("fault", "http://schemas.xmlsoap.org/wsdl/soap12/", (Soap12FaultBinding) obj2, false, false);
                        }
                        else if (obj2 is SoapFaultBinding)
                        {
                            this.Write112_SoapFaultBinding("fault", "http://schemas.xmlsoap.org/wsdl/soap/", (SoapFaultBinding) obj2, false, false);
                        }
                        else if (obj2 is XmlElement)
                        {
                            XmlElement element = (XmlElement) obj2;
                            if ((element == null) && (element != null))
                            {
                                throw base.CreateInvalidAnyTypeException(element);
                            }
                            base.WriteElementLiteral(element, "", null, false, true);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write116_OperationBinding(string n, string ns, OperationBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(OperationBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("OperationBinding", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        object obj2 = extensions[j];
                        if (obj2 is Soap12OperationBinding)
                        {
                            this.Write88_Soap12OperationBinding("operation", "http://schemas.xmlsoap.org/wsdl/soap12/", (Soap12OperationBinding) obj2, false, false);
                        }
                        else if (obj2 is HttpOperationBinding)
                        {
                            this.Write85_HttpOperationBinding("operation", "http://schemas.xmlsoap.org/wsdl/http/", (HttpOperationBinding) obj2, false, false);
                        }
                        else if (obj2 is SoapOperationBinding)
                        {
                            this.Write86_SoapOperationBinding("operation", "http://schemas.xmlsoap.org/wsdl/soap/", (SoapOperationBinding) obj2, false, false);
                        }
                        else if (obj2 is XmlElement)
                        {
                            XmlElement element = (XmlElement) obj2;
                            if ((element == null) && (element != null))
                            {
                                throw base.CreateInvalidAnyTypeException(element);
                            }
                            base.WriteElementLiteral(element, "", null, false, true);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                this.Write110_InputBinding("input", "http://schemas.xmlsoap.org/wsdl/", o.Input, false, false);
                this.Write111_OutputBinding("output", "http://schemas.xmlsoap.org/wsdl/", o.Output, false, false);
                FaultBindingCollection faults = o.Faults;
                if (faults != null)
                {
                    for (int k = 0; k < faults.Count; k++)
                    {
                        this.Write115_FaultBinding("fault", "http://schemas.xmlsoap.org/wsdl/", faults[k], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write117_Binding(string n, string ns, Binding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Binding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("Binding", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("type", "", base.FromXmlQualifiedName(o.Type));
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        object obj2 = extensions[j];
                        if (obj2 is Soap12Binding)
                        {
                            this.Write84_Soap12Binding("binding", "http://schemas.xmlsoap.org/wsdl/soap12/", (Soap12Binding) obj2, false, false);
                        }
                        else if (obj2 is HttpBinding)
                        {
                            this.Write77_HttpBinding("binding", "http://schemas.xmlsoap.org/wsdl/http/", (HttpBinding) obj2, false, false);
                        }
                        else if (obj2 is SoapBinding)
                        {
                            this.Write80_SoapBinding("binding", "http://schemas.xmlsoap.org/wsdl/soap/", (SoapBinding) obj2, false, false);
                        }
                        else if (obj2 is XmlElement)
                        {
                            XmlElement element = (XmlElement) obj2;
                            if ((element == null) && (element != null))
                            {
                                throw base.CreateInvalidAnyTypeException(element);
                            }
                            base.WriteElementLiteral(element, "", null, false, true);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                OperationBindingCollection operations = o.Operations;
                if (operations != null)
                {
                    for (int k = 0; k < operations.Count; k++)
                    {
                        this.Write116_OperationBinding("operation", "http://schemas.xmlsoap.org/wsdl/", operations[k], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write118_HttpAddressBinding(string n, string ns, HttpAddressBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(HttpAddressBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("HttpAddressBinding", "http://schemas.xmlsoap.org/wsdl/http/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("location", "", o.Location);
                base.WriteEndElement(o);
            }
        }

        private void Write119_SoapAddressBinding(string n, string ns, SoapAddressBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(SoapAddressBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("SoapAddressBinding", "http://schemas.xmlsoap.org/wsdl/soap/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("location", "", o.Location);
                base.WriteEndElement(o);
            }
        }

        private void Write12_XmlSchemaInclude(string n, string ns, XmlSchemaInclude o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaInclude)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaInclude", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("schemaLocation", "", o.SchemaLocation);
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write121_Soap12AddressBinding(string n, string ns, Soap12AddressBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Soap12AddressBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("Soap12AddressBinding", "http://schemas.xmlsoap.org/wsdl/soap12/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("location", "", o.Location);
                base.WriteEndElement(o);
            }
        }

        private void Write122_Port(string n, string ns, Port o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Port)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("Port", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("binding", "", base.FromXmlQualifiedName(o.Binding));
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        object obj2 = extensions[j];
                        if (obj2 is Soap12AddressBinding)
                        {
                            this.Write121_Soap12AddressBinding("address", "http://schemas.xmlsoap.org/wsdl/soap12/", (Soap12AddressBinding) obj2, false, false);
                        }
                        else if (obj2 is HttpAddressBinding)
                        {
                            this.Write118_HttpAddressBinding("address", "http://schemas.xmlsoap.org/wsdl/http/", (HttpAddressBinding) obj2, false, false);
                        }
                        else if (obj2 is SoapAddressBinding)
                        {
                            this.Write119_SoapAddressBinding("address", "http://schemas.xmlsoap.org/wsdl/soap/", (SoapAddressBinding) obj2, false, false);
                        }
                        else if (obj2 is XmlElement)
                        {
                            XmlElement element = (XmlElement) obj2;
                            if ((element == null) && (element != null))
                            {
                                throw base.CreateInvalidAnyTypeException(element);
                            }
                            base.WriteElementLiteral(element, "", null, false, true);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write123_Service(string n, string ns, Service o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Service)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("Service", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                PortCollection ports = o.Ports;
                if (ports != null)
                {
                    for (int k = 0; k < ports.Count; k++)
                    {
                        this.Write122_Port("port", "http://schemas.xmlsoap.org/wsdl/", ports[k], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write124_ServiceDescription(string n, string ns, ServiceDescription o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(ServiceDescription)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("ServiceDescription", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("targetNamespace", "", o.TargetNamespace);
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                ImportCollection imports = o.Imports;
                if (imports != null)
                {
                    for (int k = 0; k < imports.Count; k++)
                    {
                        this.Write4_Import("import", "http://schemas.xmlsoap.org/wsdl/", imports[k], false, false);
                    }
                }
                this.Write67_Types("types", "http://schemas.xmlsoap.org/wsdl/", o.Types, false, false);
                MessageCollection messages = o.Messages;
                if (messages != null)
                {
                    for (int m = 0; m < messages.Count; m++)
                    {
                        this.Write69_Message("message", "http://schemas.xmlsoap.org/wsdl/", messages[m], false, false);
                    }
                }
                PortTypeCollection portTypes = o.PortTypes;
                if (portTypes != null)
                {
                    for (int num5 = 0; num5 < portTypes.Count; num5++)
                    {
                        this.Write75_PortType("portType", "http://schemas.xmlsoap.org/wsdl/", portTypes[num5], false, false);
                    }
                }
                BindingCollection bindings = o.Bindings;
                if (bindings != null)
                {
                    for (int num6 = 0; num6 < bindings.Count; num6++)
                    {
                        this.Write117_Binding("binding", "http://schemas.xmlsoap.org/wsdl/", bindings[num6], false, false);
                    }
                }
                ServiceCollection services = o.Services;
                if (services != null)
                {
                    for (int num7 = 0; num7 < services.Count; num7++)
                    {
                        this.Write123_Service("service", "http://schemas.xmlsoap.org/wsdl/", services[num7], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        public void Write125_definitions(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("definitions", "http://schemas.xmlsoap.org/wsdl/");
            }
            else
            {
                base.TopLevelElement();
                this.Write124_ServiceDescription("definitions", "http://schemas.xmlsoap.org/wsdl/", (ServiceDescription) o, true, false);
            }
        }

        private void Write13_XmlSchemaImport(string n, string ns, XmlSchemaImport o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaImport)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaImport", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("schemaLocation", "", o.SchemaLocation);
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("namespace", "", o.Namespace);
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write17_XmlSchemaSimpleTypeList(string n, string ns, XmlSchemaSimpleTypeList o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaSimpleTypeList)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaSimpleTypeList", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("itemType", "", base.FromXmlQualifiedName(o.ItemTypeName));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                this.Write34_XmlSchemaSimpleType("simpleType", "http://www.w3.org/2001/XMLSchema", o.ItemType, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write20_XmlSchemaFractionDigitsFacet(string n, string ns, XmlSchemaFractionDigitsFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaFractionDigitsFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaFractionDigitsFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write21_XmlSchemaMinInclusiveFacet(string n, string ns, XmlSchemaMinInclusiveFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaMinInclusiveFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaMinInclusiveFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write22_XmlSchemaMaxLengthFacet(string n, string ns, XmlSchemaMaxLengthFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaMaxLengthFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaMaxLengthFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write23_XmlSchemaLengthFacet(string n, string ns, XmlSchemaLengthFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaLengthFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaLengthFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write24_XmlSchemaTotalDigitsFacet(string n, string ns, XmlSchemaTotalDigitsFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaTotalDigitsFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaTotalDigitsFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write25_XmlSchemaPatternFacet(string n, string ns, XmlSchemaPatternFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaPatternFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaPatternFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write26_XmlSchemaEnumerationFacet(string n, string ns, XmlSchemaEnumerationFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaEnumerationFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaEnumerationFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write27_XmlSchemaMaxInclusiveFacet(string n, string ns, XmlSchemaMaxInclusiveFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaMaxInclusiveFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaMaxInclusiveFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write28_XmlSchemaMaxExclusiveFacet(string n, string ns, XmlSchemaMaxExclusiveFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaMaxExclusiveFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaMaxExclusiveFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write29_XmlSchemaWhiteSpaceFacet(string n, string ns, XmlSchemaWhiteSpaceFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaWhiteSpaceFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaWhiteSpaceFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write30_XmlSchemaMinExclusiveFacet(string n, string ns, XmlSchemaMinExclusiveFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaMinExclusiveFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaMinExclusiveFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write31_XmlSchemaMinLengthFacet(string n, string ns, XmlSchemaMinLengthFacet o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaMinLengthFacet)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaMinLengthFacet", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    base.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write32_XmlSchemaSimpleTypeRestriction(string n, string ns, XmlSchemaSimpleTypeRestriction o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaSimpleTypeRestriction)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaSimpleTypeRestriction", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("base", "", base.FromXmlQualifiedName(o.BaseTypeName));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                this.Write34_XmlSchemaSimpleType("simpleType", "http://www.w3.org/2001/XMLSchema", o.BaseType, false, false);
                XmlSchemaObjectCollection facets = o.Facets;
                if (facets != null)
                {
                    for (int j = 0; j < facets.Count; j++)
                    {
                        XmlSchemaObject obj2 = facets[j];
                        if (obj2 is XmlSchemaLengthFacet)
                        {
                            this.Write23_XmlSchemaLengthFacet("length", "http://www.w3.org/2001/XMLSchema", (XmlSchemaLengthFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaTotalDigitsFacet)
                        {
                            this.Write24_XmlSchemaTotalDigitsFacet("totalDigits", "http://www.w3.org/2001/XMLSchema", (XmlSchemaTotalDigitsFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMaxLengthFacet)
                        {
                            this.Write22_XmlSchemaMaxLengthFacet("maxLength", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMaxLengthFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaFractionDigitsFacet)
                        {
                            this.Write20_XmlSchemaFractionDigitsFacet("fractionDigits", "http://www.w3.org/2001/XMLSchema", (XmlSchemaFractionDigitsFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMinLengthFacet)
                        {
                            this.Write31_XmlSchemaMinLengthFacet("minLength", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMinLengthFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMaxExclusiveFacet)
                        {
                            this.Write28_XmlSchemaMaxExclusiveFacet("maxExclusive", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMaxExclusiveFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaWhiteSpaceFacet)
                        {
                            this.Write29_XmlSchemaWhiteSpaceFacet("whiteSpace", "http://www.w3.org/2001/XMLSchema", (XmlSchemaWhiteSpaceFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMinExclusiveFacet)
                        {
                            this.Write30_XmlSchemaMinExclusiveFacet("minExclusive", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMinExclusiveFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaPatternFacet)
                        {
                            this.Write25_XmlSchemaPatternFacet("pattern", "http://www.w3.org/2001/XMLSchema", (XmlSchemaPatternFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMinInclusiveFacet)
                        {
                            this.Write21_XmlSchemaMinInclusiveFacet("minInclusive", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMinInclusiveFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMaxInclusiveFacet)
                        {
                            this.Write27_XmlSchemaMaxInclusiveFacet("maxInclusive", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMaxInclusiveFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaEnumerationFacet)
                        {
                            this.Write26_XmlSchemaEnumerationFacet("enumeration", "http://www.w3.org/2001/XMLSchema", (XmlSchemaEnumerationFacet) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write33_XmlSchemaSimpleTypeUnion(string n, string ns, XmlSchemaSimpleTypeUnion o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaSimpleTypeUnion)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaSimpleTypeUnion", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                XmlQualifiedName[] memberTypes = o.MemberTypes;
                if (memberTypes != null)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int j = 0; j < memberTypes.Length; j++)
                    {
                        XmlQualifiedName xmlQualifiedName = memberTypes[j];
                        if (j != 0)
                        {
                            builder.Append(" ");
                        }
                        builder.Append(base.FromXmlQualifiedName(xmlQualifiedName));
                    }
                    if (builder.Length != 0)
                    {
                        base.WriteAttribute("memberTypes", "", builder.ToString());
                    }
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                XmlSchemaObjectCollection baseTypes = o.BaseTypes;
                if (baseTypes != null)
                {
                    for (int k = 0; k < baseTypes.Count; k++)
                    {
                        this.Write34_XmlSchemaSimpleType("simpleType", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSimpleType) baseTypes[k], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write34_XmlSchemaSimpleType(string n, string ns, XmlSchemaSimpleType o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaSimpleType)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaSimpleType", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                if (o.Final != XmlSchemaDerivationMethod.None)
                {
                    base.WriteAttribute("final", "", this.Write7_XmlSchemaDerivationMethod(o.Final));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                if (o.Content is XmlSchemaSimpleTypeUnion)
                {
                    this.Write33_XmlSchemaSimpleTypeUnion("union", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSimpleTypeUnion) o.Content, false, false);
                }
                else if (o.Content is XmlSchemaSimpleTypeRestriction)
                {
                    this.Write32_XmlSchemaSimpleTypeRestriction("restriction", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSimpleTypeRestriction) o.Content, false, false);
                }
                else if (o.Content is XmlSchemaSimpleTypeList)
                {
                    this.Write17_XmlSchemaSimpleTypeList("list", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSimpleTypeList) o.Content, false, false);
                }
                else if (o.Content != null)
                {
                    throw base.CreateUnknownTypeException(o.Content);
                }
                base.WriteEndElement(o);
            }
        }

        private string Write35_XmlSchemaUse(XmlSchemaUse v)
        {
            switch (v)
            {
                case XmlSchemaUse.Optional:
                    return "optional";

                case XmlSchemaUse.Prohibited:
                    return "prohibited";

                case XmlSchemaUse.Required:
                    return "required";
            }
            long num = (long) v;
            throw base.CreateInvalidEnumValueException(num.ToString(CultureInfo.InvariantCulture), "System.Xml.Schema.XmlSchemaUse");
        }

        private void Write36_XmlSchemaAttribute(string n, string ns, XmlSchemaAttribute o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaAttribute)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaAttribute", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("default", "", o.DefaultValue);
                base.WriteAttribute("fixed", "", o.FixedValue);
                if (o.Form != XmlSchemaForm.None)
                {
                    base.WriteAttribute("form", "", this.Write6_XmlSchemaForm(o.Form));
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("ref", "", base.FromXmlQualifiedName(o.RefName));
                base.WriteAttribute("type", "", base.FromXmlQualifiedName(o.SchemaTypeName));
                if (o.Use != XmlSchemaUse.None)
                {
                    base.WriteAttribute("use", "", this.Write35_XmlSchemaUse(o.Use));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                this.Write34_XmlSchemaSimpleType("simpleType", "http://www.w3.org/2001/XMLSchema", o.SchemaType, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write37_XmlSchemaAttributeGroupRef(string n, string ns, XmlSchemaAttributeGroupRef o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaAttributeGroupRef)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaAttributeGroupRef", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("ref", "", base.FromXmlQualifiedName(o.RefName));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private string Write38_XmlSchemaContentProcessing(XmlSchemaContentProcessing v)
        {
            switch (v)
            {
                case XmlSchemaContentProcessing.Skip:
                    return "skip";

                case XmlSchemaContentProcessing.Lax:
                    return "lax";

                case XmlSchemaContentProcessing.Strict:
                    return "strict";
            }
            long num = (long) v;
            throw base.CreateInvalidEnumValueException(num.ToString(CultureInfo.InvariantCulture), "System.Xml.Schema.XmlSchemaContentProcessing");
        }

        private void Write39_XmlSchemaAnyAttribute(string n, string ns, XmlSchemaAnyAttribute o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaAnyAttribute)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaAnyAttribute", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("namespace", "", o.Namespace);
                if (o.ProcessContents != XmlSchemaContentProcessing.None)
                {
                    base.WriteAttribute("processContents", "", this.Write38_XmlSchemaContentProcessing(o.ProcessContents));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write4_Import(string n, string ns, Import o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Import)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("Import", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("namespace", "", o.Namespace);
                base.WriteAttribute("location", "", o.Location);
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write40_XmlSchemaAttributeGroup(string n, string ns, XmlSchemaAttributeGroup o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaAttributeGroup)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaAttributeGroup", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                XmlSchemaObjectCollection attributes = o.Attributes;
                if (attributes != null)
                {
                    for (int j = 0; j < attributes.Count; j++)
                    {
                        XmlSchemaObject obj2 = attributes[j];
                        if (obj2 is XmlSchemaAttributeGroupRef)
                        {
                            this.Write37_XmlSchemaAttributeGroupRef("attributeGroup", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttributeGroupRef) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaAttribute)
                        {
                            this.Write36_XmlSchemaAttribute("attribute", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttribute) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                this.Write39_XmlSchemaAnyAttribute("anyAttribute", "http://www.w3.org/2001/XMLSchema", o.AnyAttribute, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write44_XmlSchemaGroupRef(string n, string ns, XmlSchemaGroupRef o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaGroupRef)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaGroupRef", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("minOccurs", "", o.MinOccursString);
                base.WriteAttribute("maxOccurs", "", o.MaxOccursString);
                base.WriteAttribute("ref", "", base.FromXmlQualifiedName(o.RefName));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write46_XmlSchemaAny(string n, string ns, XmlSchemaAny o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaAny)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaAny", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("minOccurs", "", o.MinOccursString);
                base.WriteAttribute("maxOccurs", "", o.MaxOccursString);
                base.WriteAttribute("namespace", "", o.Namespace);
                if (o.ProcessContents != XmlSchemaContentProcessing.None)
                {
                    base.WriteAttribute("processContents", "", this.Write38_XmlSchemaContentProcessing(o.ProcessContents));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write47_XmlSchemaXPath(string n, string ns, XmlSchemaXPath o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaXPath)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaXPath", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                if ((o.XPath != null) && (o.XPath.Length != 0))
                {
                    base.WriteAttribute("xpath", "", o.XPath);
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write49_XmlSchemaKey(string n, string ns, XmlSchemaKey o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaKey)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaKey", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                this.Write47_XmlSchemaXPath("selector", "http://www.w3.org/2001/XMLSchema", o.Selector, false, false);
                XmlSchemaObjectCollection fields = o.Fields;
                if (fields != null)
                {
                    for (int j = 0; j < fields.Count; j++)
                    {
                        this.Write47_XmlSchemaXPath("field", "http://www.w3.org/2001/XMLSchema", (XmlSchemaXPath) fields[j], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write50_XmlSchemaUnique(string n, string ns, XmlSchemaUnique o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaUnique)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaUnique", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                this.Write47_XmlSchemaXPath("selector", "http://www.w3.org/2001/XMLSchema", o.Selector, false, false);
                XmlSchemaObjectCollection fields = o.Fields;
                if (fields != null)
                {
                    for (int j = 0; j < fields.Count; j++)
                    {
                        this.Write47_XmlSchemaXPath("field", "http://www.w3.org/2001/XMLSchema", (XmlSchemaXPath) fields[j], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write51_XmlSchemaKeyref(string n, string ns, XmlSchemaKeyref o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaKeyref)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaKeyref", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("refer", "", base.FromXmlQualifiedName(o.Refer));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                this.Write47_XmlSchemaXPath("selector", "http://www.w3.org/2001/XMLSchema", o.Selector, false, false);
                XmlSchemaObjectCollection fields = o.Fields;
                if (fields != null)
                {
                    for (int j = 0; j < fields.Count; j++)
                    {
                        this.Write47_XmlSchemaXPath("field", "http://www.w3.org/2001/XMLSchema", (XmlSchemaXPath) fields[j], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write52_XmlSchemaElement(string n, string ns, XmlSchemaElement o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaElement)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaElement", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("minOccurs", "", o.MinOccursString);
                base.WriteAttribute("maxOccurs", "", o.MaxOccursString);
                if (o.IsAbstract)
                {
                    base.WriteAttribute("abstract", "", XmlConvert.ToString(o.IsAbstract));
                }
                if (o.Block != XmlSchemaDerivationMethod.None)
                {
                    base.WriteAttribute("block", "", this.Write7_XmlSchemaDerivationMethod(o.Block));
                }
                base.WriteAttribute("default", "", o.DefaultValue);
                if (o.Final != XmlSchemaDerivationMethod.None)
                {
                    base.WriteAttribute("final", "", this.Write7_XmlSchemaDerivationMethod(o.Final));
                }
                base.WriteAttribute("fixed", "", o.FixedValue);
                if (o.Form != XmlSchemaForm.None)
                {
                    base.WriteAttribute("form", "", this.Write6_XmlSchemaForm(o.Form));
                }
                if ((o.Name != null) && (o.Name.Length != 0))
                {
                    base.WriteAttribute("name", "", o.Name);
                }
                if (o.IsNillable)
                {
                    base.WriteAttribute("nillable", "", XmlConvert.ToString(o.IsNillable));
                }
                base.WriteAttribute("ref", "", base.FromXmlQualifiedName(o.RefName));
                base.WriteAttribute("substitutionGroup", "", base.FromXmlQualifiedName(o.SubstitutionGroup));
                base.WriteAttribute("type", "", base.FromXmlQualifiedName(o.SchemaTypeName));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                if (o.SchemaType is XmlSchemaComplexType)
                {
                    this.Write62_XmlSchemaComplexType("complexType", "http://www.w3.org/2001/XMLSchema", (XmlSchemaComplexType) o.SchemaType, false, false);
                }
                else if (o.SchemaType is XmlSchemaSimpleType)
                {
                    this.Write34_XmlSchemaSimpleType("simpleType", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSimpleType) o.SchemaType, false, false);
                }
                else if (o.SchemaType != null)
                {
                    throw base.CreateUnknownTypeException(o.SchemaType);
                }
                XmlSchemaObjectCollection constraints = o.Constraints;
                if (constraints != null)
                {
                    for (int j = 0; j < constraints.Count; j++)
                    {
                        XmlSchemaObject obj2 = constraints[j];
                        if (obj2 is XmlSchemaKeyref)
                        {
                            this.Write51_XmlSchemaKeyref("keyref", "http://www.w3.org/2001/XMLSchema", (XmlSchemaKeyref) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaUnique)
                        {
                            this.Write50_XmlSchemaUnique("unique", "http://www.w3.org/2001/XMLSchema", (XmlSchemaUnique) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaKey)
                        {
                            this.Write49_XmlSchemaKey("key", "http://www.w3.org/2001/XMLSchema", (XmlSchemaKey) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write53_XmlSchemaSequence(string n, string ns, XmlSchemaSequence o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaSequence)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaSequence", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("minOccurs", "", o.MinOccursString);
                base.WriteAttribute("maxOccurs", "", o.MaxOccursString);
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                XmlSchemaObjectCollection items = o.Items;
                if (items != null)
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        XmlSchemaObject obj2 = items[j];
                        if (obj2 is XmlSchemaChoice)
                        {
                            this.Write54_XmlSchemaChoice("choice", "http://www.w3.org/2001/XMLSchema", (XmlSchemaChoice) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaSequence)
                        {
                            this.Write53_XmlSchemaSequence("sequence", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSequence) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaGroupRef)
                        {
                            this.Write44_XmlSchemaGroupRef("group", "http://www.w3.org/2001/XMLSchema", (XmlSchemaGroupRef) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaElement)
                        {
                            this.Write52_XmlSchemaElement("element", "http://www.w3.org/2001/XMLSchema", (XmlSchemaElement) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaAny)
                        {
                            this.Write46_XmlSchemaAny("any", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAny) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write54_XmlSchemaChoice(string n, string ns, XmlSchemaChoice o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaChoice)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaChoice", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("minOccurs", "", o.MinOccursString);
                base.WriteAttribute("maxOccurs", "", o.MaxOccursString);
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                XmlSchemaObjectCollection items = o.Items;
                if (items != null)
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        XmlSchemaObject obj2 = items[j];
                        if (obj2 is XmlSchemaSequence)
                        {
                            this.Write53_XmlSchemaSequence("sequence", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSequence) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaChoice)
                        {
                            this.Write54_XmlSchemaChoice("choice", "http://www.w3.org/2001/XMLSchema", (XmlSchemaChoice) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaGroupRef)
                        {
                            this.Write44_XmlSchemaGroupRef("group", "http://www.w3.org/2001/XMLSchema", (XmlSchemaGroupRef) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaElement)
                        {
                            this.Write52_XmlSchemaElement("element", "http://www.w3.org/2001/XMLSchema", (XmlSchemaElement) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaAny)
                        {
                            this.Write46_XmlSchemaAny("any", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAny) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write55_XmlSchemaAll(string n, string ns, XmlSchemaAll o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaAll)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaAll", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("minOccurs", "", o.MinOccursString);
                base.WriteAttribute("maxOccurs", "", o.MaxOccursString);
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                XmlSchemaObjectCollection items = o.Items;
                if (items != null)
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        this.Write52_XmlSchemaElement("element", "http://www.w3.org/2001/XMLSchema", (XmlSchemaElement) items[j], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write56_Item(string n, string ns, XmlSchemaComplexContentExtension o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaComplexContentExtension)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaComplexContentExtension", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("base", "", base.FromXmlQualifiedName(o.BaseTypeName));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                if (o.Particle is XmlSchemaAll)
                {
                    this.Write55_XmlSchemaAll("all", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAll) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaSequence)
                {
                    this.Write53_XmlSchemaSequence("sequence", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSequence) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaChoice)
                {
                    this.Write54_XmlSchemaChoice("choice", "http://www.w3.org/2001/XMLSchema", (XmlSchemaChoice) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaGroupRef)
                {
                    this.Write44_XmlSchemaGroupRef("group", "http://www.w3.org/2001/XMLSchema", (XmlSchemaGroupRef) o.Particle, false, false);
                }
                else if (o.Particle != null)
                {
                    throw base.CreateUnknownTypeException(o.Particle);
                }
                XmlSchemaObjectCollection attributes = o.Attributes;
                if (attributes != null)
                {
                    for (int j = 0; j < attributes.Count; j++)
                    {
                        XmlSchemaObject obj2 = attributes[j];
                        if (obj2 is XmlSchemaAttribute)
                        {
                            this.Write36_XmlSchemaAttribute("attribute", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttribute) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaAttributeGroupRef)
                        {
                            this.Write37_XmlSchemaAttributeGroupRef("attributeGroup", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttributeGroupRef) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                this.Write39_XmlSchemaAnyAttribute("anyAttribute", "http://www.w3.org/2001/XMLSchema", o.AnyAttribute, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write57_Item(string n, string ns, XmlSchemaComplexContentRestriction o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaComplexContentRestriction)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaComplexContentRestriction", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("base", "", base.FromXmlQualifiedName(o.BaseTypeName));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                if (o.Particle is XmlSchemaAll)
                {
                    this.Write55_XmlSchemaAll("all", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAll) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaSequence)
                {
                    this.Write53_XmlSchemaSequence("sequence", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSequence) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaChoice)
                {
                    this.Write54_XmlSchemaChoice("choice", "http://www.w3.org/2001/XMLSchema", (XmlSchemaChoice) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaGroupRef)
                {
                    this.Write44_XmlSchemaGroupRef("group", "http://www.w3.org/2001/XMLSchema", (XmlSchemaGroupRef) o.Particle, false, false);
                }
                else if (o.Particle != null)
                {
                    throw base.CreateUnknownTypeException(o.Particle);
                }
                XmlSchemaObjectCollection attributes = o.Attributes;
                if (attributes != null)
                {
                    for (int j = 0; j < attributes.Count; j++)
                    {
                        XmlSchemaObject obj2 = attributes[j];
                        if (obj2 is XmlSchemaAttribute)
                        {
                            this.Write36_XmlSchemaAttribute("attribute", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttribute) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaAttributeGroupRef)
                        {
                            this.Write37_XmlSchemaAttributeGroupRef("attributeGroup", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttributeGroupRef) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                this.Write39_XmlSchemaAnyAttribute("anyAttribute", "http://www.w3.org/2001/XMLSchema", o.AnyAttribute, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write58_XmlSchemaComplexContent(string n, string ns, XmlSchemaComplexContent o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaComplexContent)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaComplexContent", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("mixed", "", XmlConvert.ToString(o.IsMixed));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                if (o.Content is XmlSchemaComplexContentRestriction)
                {
                    this.Write57_Item("restriction", "http://www.w3.org/2001/XMLSchema", (XmlSchemaComplexContentRestriction) o.Content, false, false);
                }
                else if (o.Content is XmlSchemaComplexContentExtension)
                {
                    this.Write56_Item("extension", "http://www.w3.org/2001/XMLSchema", (XmlSchemaComplexContentExtension) o.Content, false, false);
                }
                else if (o.Content != null)
                {
                    throw base.CreateUnknownTypeException(o.Content);
                }
                base.WriteEndElement(o);
            }
        }

        private void Write59_Item(string n, string ns, XmlSchemaSimpleContentRestriction o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaSimpleContentRestriction)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaSimpleContentRestriction", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("base", "", base.FromXmlQualifiedName(o.BaseTypeName));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                this.Write34_XmlSchemaSimpleType("simpleType", "http://www.w3.org/2001/XMLSchema", o.BaseType, false, false);
                XmlSchemaObjectCollection facets = o.Facets;
                if (facets != null)
                {
                    for (int j = 0; j < facets.Count; j++)
                    {
                        XmlSchemaObject obj2 = facets[j];
                        if (obj2 is XmlSchemaMinLengthFacet)
                        {
                            this.Write31_XmlSchemaMinLengthFacet("minLength", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMinLengthFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMaxLengthFacet)
                        {
                            this.Write22_XmlSchemaMaxLengthFacet("maxLength", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMaxLengthFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaLengthFacet)
                        {
                            this.Write23_XmlSchemaLengthFacet("length", "http://www.w3.org/2001/XMLSchema", (XmlSchemaLengthFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaFractionDigitsFacet)
                        {
                            this.Write20_XmlSchemaFractionDigitsFacet("fractionDigits", "http://www.w3.org/2001/XMLSchema", (XmlSchemaFractionDigitsFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaTotalDigitsFacet)
                        {
                            this.Write24_XmlSchemaTotalDigitsFacet("totalDigits", "http://www.w3.org/2001/XMLSchema", (XmlSchemaTotalDigitsFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMinExclusiveFacet)
                        {
                            this.Write30_XmlSchemaMinExclusiveFacet("minExclusive", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMinExclusiveFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMaxInclusiveFacet)
                        {
                            this.Write27_XmlSchemaMaxInclusiveFacet("maxInclusive", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMaxInclusiveFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMaxExclusiveFacet)
                        {
                            this.Write28_XmlSchemaMaxExclusiveFacet("maxExclusive", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMaxExclusiveFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaMinInclusiveFacet)
                        {
                            this.Write21_XmlSchemaMinInclusiveFacet("minInclusive", "http://www.w3.org/2001/XMLSchema", (XmlSchemaMinInclusiveFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaWhiteSpaceFacet)
                        {
                            this.Write29_XmlSchemaWhiteSpaceFacet("whiteSpace", "http://www.w3.org/2001/XMLSchema", (XmlSchemaWhiteSpaceFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaEnumerationFacet)
                        {
                            this.Write26_XmlSchemaEnumerationFacet("enumeration", "http://www.w3.org/2001/XMLSchema", (XmlSchemaEnumerationFacet) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaPatternFacet)
                        {
                            this.Write25_XmlSchemaPatternFacet("pattern", "http://www.w3.org/2001/XMLSchema", (XmlSchemaPatternFacet) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                XmlSchemaObjectCollection attributes = o.Attributes;
                if (attributes != null)
                {
                    for (int k = 0; k < attributes.Count; k++)
                    {
                        XmlSchemaObject obj3 = attributes[k];
                        if (obj3 is XmlSchemaAttribute)
                        {
                            this.Write36_XmlSchemaAttribute("attribute", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttribute) obj3, false, false);
                        }
                        else if (obj3 is XmlSchemaAttributeGroupRef)
                        {
                            this.Write37_XmlSchemaAttributeGroupRef("attributeGroup", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttributeGroupRef) obj3, false, false);
                        }
                        else if (obj3 != null)
                        {
                            throw base.CreateUnknownTypeException(obj3);
                        }
                    }
                }
                this.Write39_XmlSchemaAnyAttribute("anyAttribute", "http://www.w3.org/2001/XMLSchema", o.AnyAttribute, false, false);
                base.WriteEndElement(o);
            }
        }

        private string Write6_XmlSchemaForm(XmlSchemaForm v)
        {
            switch (v)
            {
                case XmlSchemaForm.Qualified:
                    return "qualified";

                case XmlSchemaForm.Unqualified:
                    return "unqualified";
            }
            long num = (long) v;
            throw base.CreateInvalidEnumValueException(num.ToString(CultureInfo.InvariantCulture), "System.Xml.Schema.XmlSchemaForm");
        }

        private void Write60_Item(string n, string ns, XmlSchemaSimpleContentExtension o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaSimpleContentExtension)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaSimpleContentExtension", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("base", "", base.FromXmlQualifiedName(o.BaseTypeName));
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                XmlSchemaObjectCollection attributes = o.Attributes;
                if (attributes != null)
                {
                    for (int j = 0; j < attributes.Count; j++)
                    {
                        XmlSchemaObject obj2 = attributes[j];
                        if (obj2 is XmlSchemaAttribute)
                        {
                            this.Write36_XmlSchemaAttribute("attribute", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttribute) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaAttributeGroupRef)
                        {
                            this.Write37_XmlSchemaAttributeGroupRef("attributeGroup", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttributeGroupRef) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                this.Write39_XmlSchemaAnyAttribute("anyAttribute", "http://www.w3.org/2001/XMLSchema", o.AnyAttribute, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write61_XmlSchemaSimpleContent(string n, string ns, XmlSchemaSimpleContent o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaSimpleContent)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaSimpleContent", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                if (o.Content is XmlSchemaSimpleContentExtension)
                {
                    this.Write60_Item("extension", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSimpleContentExtension) o.Content, false, false);
                }
                else if (o.Content is XmlSchemaSimpleContentRestriction)
                {
                    this.Write59_Item("restriction", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSimpleContentRestriction) o.Content, false, false);
                }
                else if (o.Content != null)
                {
                    throw base.CreateUnknownTypeException(o.Content);
                }
                base.WriteEndElement(o);
            }
        }

        private void Write62_XmlSchemaComplexType(string n, string ns, XmlSchemaComplexType o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaComplexType)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaComplexType", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                if (o.Final != XmlSchemaDerivationMethod.None)
                {
                    base.WriteAttribute("final", "", this.Write7_XmlSchemaDerivationMethod(o.Final));
                }
                if (o.IsAbstract)
                {
                    base.WriteAttribute("abstract", "", XmlConvert.ToString(o.IsAbstract));
                }
                if (o.Block != XmlSchemaDerivationMethod.None)
                {
                    base.WriteAttribute("block", "", this.Write7_XmlSchemaDerivationMethod(o.Block));
                }
                if (o.IsMixed)
                {
                    base.WriteAttribute("mixed", "", XmlConvert.ToString(o.IsMixed));
                }
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                if (o.ContentModel is XmlSchemaSimpleContent)
                {
                    this.Write61_XmlSchemaSimpleContent("simpleContent", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSimpleContent) o.ContentModel, false, false);
                }
                else if (o.ContentModel is XmlSchemaComplexContent)
                {
                    this.Write58_XmlSchemaComplexContent("complexContent", "http://www.w3.org/2001/XMLSchema", (XmlSchemaComplexContent) o.ContentModel, false, false);
                }
                else if (o.ContentModel != null)
                {
                    throw base.CreateUnknownTypeException(o.ContentModel);
                }
                if (o.Particle is XmlSchemaChoice)
                {
                    this.Write54_XmlSchemaChoice("choice", "http://www.w3.org/2001/XMLSchema", (XmlSchemaChoice) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaAll)
                {
                    this.Write55_XmlSchemaAll("all", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAll) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaSequence)
                {
                    this.Write53_XmlSchemaSequence("sequence", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSequence) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaGroupRef)
                {
                    this.Write44_XmlSchemaGroupRef("group", "http://www.w3.org/2001/XMLSchema", (XmlSchemaGroupRef) o.Particle, false, false);
                }
                else if (o.Particle != null)
                {
                    throw base.CreateUnknownTypeException(o.Particle);
                }
                XmlSchemaObjectCollection attributes = o.Attributes;
                if (attributes != null)
                {
                    for (int j = 0; j < attributes.Count; j++)
                    {
                        XmlSchemaObject obj2 = attributes[j];
                        if (obj2 is XmlSchemaAttributeGroupRef)
                        {
                            this.Write37_XmlSchemaAttributeGroupRef("attributeGroup", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttributeGroupRef) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaAttribute)
                        {
                            this.Write36_XmlSchemaAttribute("attribute", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttribute) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                this.Write39_XmlSchemaAnyAttribute("anyAttribute", "http://www.w3.org/2001/XMLSchema", o.AnyAttribute, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write63_XmlSchemaGroup(string n, string ns, XmlSchemaGroup o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaGroup)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaGroup", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                if (o.Particle is XmlSchemaAll)
                {
                    this.Write55_XmlSchemaAll("all", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAll) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaChoice)
                {
                    this.Write54_XmlSchemaChoice("choice", "http://www.w3.org/2001/XMLSchema", (XmlSchemaChoice) o.Particle, false, false);
                }
                else if (o.Particle is XmlSchemaSequence)
                {
                    this.Write53_XmlSchemaSequence("sequence", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSequence) o.Particle, false, false);
                }
                else if (o.Particle != null)
                {
                    throw base.CreateUnknownTypeException(o.Particle);
                }
                base.WriteEndElement(o);
            }
        }

        private void Write64_XmlSchemaRedefine(string n, string ns, XmlSchemaRedefine o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaRedefine)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaRedefine", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("schemaLocation", "", o.SchemaLocation);
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                XmlSchemaObjectCollection items = o.Items;
                if (items != null)
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        XmlSchemaObject obj2 = items[j];
                        if (obj2 is XmlSchemaSimpleType)
                        {
                            this.Write34_XmlSchemaSimpleType("simpleType", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSimpleType) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaComplexType)
                        {
                            this.Write62_XmlSchemaComplexType("complexType", "http://www.w3.org/2001/XMLSchema", (XmlSchemaComplexType) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaGroup)
                        {
                            this.Write63_XmlSchemaGroup("group", "http://www.w3.org/2001/XMLSchema", (XmlSchemaGroup) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaAttributeGroup)
                        {
                            this.Write40_XmlSchemaAttributeGroup("attributeGroup", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttributeGroup) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaAnnotation)
                        {
                            this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAnnotation) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write65_XmlSchemaNotation(string n, string ns, XmlSchemaNotation o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaNotation)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaNotation", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("public", "", o.Public);
                base.WriteAttribute("system", "", o.System);
                this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", o.Annotation, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write66_XmlSchema(string n, string ns, XmlSchema o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchema)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchema", "http://www.w3.org/2001/XMLSchema");
                }
                if (o.AttributeFormDefault != XmlSchemaForm.None)
                {
                    base.WriteAttribute("attributeFormDefault", "", this.Write6_XmlSchemaForm(o.AttributeFormDefault));
                }
                if (o.BlockDefault != XmlSchemaDerivationMethod.None)
                {
                    base.WriteAttribute("blockDefault", "", this.Write7_XmlSchemaDerivationMethod(o.BlockDefault));
                }
                if (o.FinalDefault != XmlSchemaDerivationMethod.None)
                {
                    base.WriteAttribute("finalDefault", "", this.Write7_XmlSchemaDerivationMethod(o.FinalDefault));
                }
                if (o.ElementFormDefault != XmlSchemaForm.None)
                {
                    base.WriteAttribute("elementFormDefault", "", this.Write6_XmlSchemaForm(o.ElementFormDefault));
                }
                base.WriteAttribute("targetNamespace", "", o.TargetNamespace);
                base.WriteAttribute("version", "", o.Version);
                base.WriteAttribute("id", "", o.Id);
                XmlAttribute[] unhandledAttributes = o.UnhandledAttributes;
                if (unhandledAttributes != null)
                {
                    for (int i = 0; i < unhandledAttributes.Length; i++)
                    {
                        XmlAttribute node = unhandledAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                XmlSchemaObjectCollection includes = o.Includes;
                if (includes != null)
                {
                    for (int j = 0; j < includes.Count; j++)
                    {
                        XmlSchemaObject obj2 = includes[j];
                        if (obj2 is XmlSchemaRedefine)
                        {
                            this.Write64_XmlSchemaRedefine("redefine", "http://www.w3.org/2001/XMLSchema", (XmlSchemaRedefine) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaImport)
                        {
                            this.Write13_XmlSchemaImport("import", "http://www.w3.org/2001/XMLSchema", (XmlSchemaImport) obj2, false, false);
                        }
                        else if (obj2 is XmlSchemaInclude)
                        {
                            this.Write12_XmlSchemaInclude("include", "http://www.w3.org/2001/XMLSchema", (XmlSchemaInclude) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                XmlSchemaObjectCollection items = o.Items;
                if (items != null)
                {
                    for (int k = 0; k < items.Count; k++)
                    {
                        XmlSchemaObject obj3 = items[k];
                        if (obj3 is XmlSchemaElement)
                        {
                            this.Write52_XmlSchemaElement("element", "http://www.w3.org/2001/XMLSchema", (XmlSchemaElement) obj3, false, false);
                        }
                        else if (obj3 is XmlSchemaComplexType)
                        {
                            this.Write62_XmlSchemaComplexType("complexType", "http://www.w3.org/2001/XMLSchema", (XmlSchemaComplexType) obj3, false, false);
                        }
                        else if (obj3 is XmlSchemaSimpleType)
                        {
                            this.Write34_XmlSchemaSimpleType("simpleType", "http://www.w3.org/2001/XMLSchema", (XmlSchemaSimpleType) obj3, false, false);
                        }
                        else if (obj3 is XmlSchemaAttribute)
                        {
                            this.Write36_XmlSchemaAttribute("attribute", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttribute) obj3, false, false);
                        }
                        else if (obj3 is XmlSchemaAttributeGroup)
                        {
                            this.Write40_XmlSchemaAttributeGroup("attributeGroup", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAttributeGroup) obj3, false, false);
                        }
                        else if (obj3 is XmlSchemaNotation)
                        {
                            this.Write65_XmlSchemaNotation("notation", "http://www.w3.org/2001/XMLSchema", (XmlSchemaNotation) obj3, false, false);
                        }
                        else if (obj3 is XmlSchemaGroup)
                        {
                            this.Write63_XmlSchemaGroup("group", "http://www.w3.org/2001/XMLSchema", (XmlSchemaGroup) obj3, false, false);
                        }
                        else if (obj3 is XmlSchemaAnnotation)
                        {
                            this.Write11_XmlSchemaAnnotation("annotation", "http://www.w3.org/2001/XMLSchema", (XmlSchemaAnnotation) obj3, false, false);
                        }
                        else if (obj3 != null)
                        {
                            throw base.CreateUnknownTypeException(obj3);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write67_Types(string n, string ns, Types o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Types)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("Types", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                XmlSchemas schemas = o.Schemas;
                if (schemas != null)
                {
                    for (int k = 0; k < schemas.Count; k++)
                    {
                        this.Write66_XmlSchema("schema", "http://www.w3.org/2001/XMLSchema", schemas[k], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write68_MessagePart(string n, string ns, MessagePart o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(MessagePart)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("MessagePart", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("element", "", base.FromXmlQualifiedName(o.Element));
                base.WriteAttribute("type", "", base.FromXmlQualifiedName(o.Type));
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write69_Message(string n, string ns, Message o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Message)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("Message", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                MessagePartCollection parts = o.Parts;
                if (parts != null)
                {
                    for (int k = 0; k < parts.Count; k++)
                    {
                        this.Write68_MessagePart("part", "http://schemas.xmlsoap.org/wsdl/", parts[k], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private string Write7_XmlSchemaDerivationMethod(XmlSchemaDerivationMethod v)
        {
            switch (v)
            {
                case XmlSchemaDerivationMethod.Empty:
                    return "";

                case XmlSchemaDerivationMethod.Substitution:
                    return "substitution";

                case XmlSchemaDerivationMethod.Extension:
                    return "extension";

                case XmlSchemaDerivationMethod.Restriction:
                    return "restriction";

                case XmlSchemaDerivationMethod.List:
                    return "list";

                case XmlSchemaDerivationMethod.Union:
                    return "union";

                case XmlSchemaDerivationMethod.All:
                    return "#all";
            }
            return XmlSerializationWriter.FromEnum((long) v, new string[] { "", "substitution", "extension", "restriction", "list", "union", "#all" }, new long[] { 0L, 1L, 2L, 4L, 8L, 0x10L, 0xffL }, "System.Xml.Schema.XmlSchemaDerivationMethod");
        }

        private void Write71_OperationInput(string n, string ns, OperationInput o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(OperationInput)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("OperationInput", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("message", "", base.FromXmlQualifiedName(o.Message));
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write72_OperationOutput(string n, string ns, OperationOutput o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(OperationOutput)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("OperationOutput", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("message", "", base.FromXmlQualifiedName(o.Message));
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write73_OperationFault(string n, string ns, OperationFault o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(OperationFault)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("OperationFault", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("message", "", base.FromXmlQualifiedName(o.Message));
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write74_Operation(string n, string ns, Operation o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Operation)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("Operation", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                if ((o.ParameterOrderString != null) && (o.ParameterOrderString.Length != 0))
                {
                    base.WriteAttribute("parameterOrder", "", o.ParameterOrderString);
                }
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                OperationMessageCollection messages = o.Messages;
                if (messages != null)
                {
                    for (int k = 0; k < messages.Count; k++)
                    {
                        OperationMessage message = messages[k];
                        if (message is OperationOutput)
                        {
                            this.Write72_OperationOutput("output", "http://schemas.xmlsoap.org/wsdl/", (OperationOutput) message, false, false);
                        }
                        else if (message is OperationInput)
                        {
                            this.Write71_OperationInput("input", "http://schemas.xmlsoap.org/wsdl/", (OperationInput) message, false, false);
                        }
                        else if (message != null)
                        {
                            throw base.CreateUnknownTypeException(message);
                        }
                    }
                }
                OperationFaultCollection faults = o.Faults;
                if (faults != null)
                {
                    for (int m = 0; m < faults.Count; m++)
                    {
                        this.Write73_OperationFault("fault", "http://schemas.xmlsoap.org/wsdl/", faults[m], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write75_PortType(string n, string ns, PortType o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(PortType)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("PortType", "http://schemas.xmlsoap.org/wsdl/");
                }
                XmlAttribute[] extensibleAttributes = o.ExtensibleAttributes;
                if (extensibleAttributes != null)
                {
                    for (int i = 0; i < extensibleAttributes.Length; i++)
                    {
                        XmlAttribute node = extensibleAttributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("name", "", o.Name);
                if ((o.DocumentationElement == null) && (o.DocumentationElement != null))
                {
                    throw base.CreateInvalidAnyTypeException(o.DocumentationElement);
                }
                base.WriteElementLiteral(o.DocumentationElement, "documentation", "http://schemas.xmlsoap.org/wsdl/", false, true);
                ServiceDescriptionFormatExtensionCollection extensions = o.Extensions;
                if (extensions != null)
                {
                    for (int j = 0; j < extensions.Count; j++)
                    {
                        if (!(extensions[j] is XmlNode) && (extensions[j] != null))
                        {
                            throw base.CreateInvalidAnyTypeException(extensions[j]);
                        }
                        base.WriteElementLiteral((XmlNode) extensions[j], "", null, false, true);
                    }
                }
                OperationCollection operations = o.Operations;
                if (operations != null)
                {
                    for (int k = 0; k < operations.Count; k++)
                    {
                        this.Write74_Operation("operation", "http://schemas.xmlsoap.org/wsdl/", operations[k], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write77_HttpBinding(string n, string ns, HttpBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(HttpBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("HttpBinding", "http://schemas.xmlsoap.org/wsdl/http/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("verb", "", o.Verb);
                base.WriteEndElement(o);
            }
        }

        private string Write79_SoapBindingStyle(SoapBindingStyle v)
        {
            switch (v)
            {
                case SoapBindingStyle.Document:
                    return "document";

                case SoapBindingStyle.Rpc:
                    return "rpc";
            }
            long num = (long) v;
            throw base.CreateInvalidEnumValueException(num.ToString(CultureInfo.InvariantCulture), "System.Web.Services.Description.SoapBindingStyle");
        }

        private void Write80_SoapBinding(string n, string ns, SoapBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(SoapBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("SoapBinding", "http://schemas.xmlsoap.org/wsdl/soap/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("transport", "", o.Transport);
                if (o.Style != SoapBindingStyle.Document)
                {
                    base.WriteAttribute("style", "", this.Write79_SoapBindingStyle(o.Style));
                }
                base.WriteEndElement(o);
            }
        }

        private string Write82_SoapBindingStyle(SoapBindingStyle v)
        {
            switch (v)
            {
                case SoapBindingStyle.Document:
                    return "document";

                case SoapBindingStyle.Rpc:
                    return "rpc";
            }
            long num = (long) v;
            throw base.CreateInvalidEnumValueException(num.ToString(CultureInfo.InvariantCulture), "System.Web.Services.Description.SoapBindingStyle");
        }

        private void Write84_Soap12Binding(string n, string ns, Soap12Binding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Soap12Binding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("Soap12Binding", "http://schemas.xmlsoap.org/wsdl/soap12/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("transport", "", o.Transport);
                if (o.Style != SoapBindingStyle.Document)
                {
                    base.WriteAttribute("style", "", this.Write82_SoapBindingStyle(o.Style));
                }
                base.WriteEndElement(o);
            }
        }

        private void Write85_HttpOperationBinding(string n, string ns, HttpOperationBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(HttpOperationBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("HttpOperationBinding", "http://schemas.xmlsoap.org/wsdl/http/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("location", "", o.Location);
                base.WriteEndElement(o);
            }
        }

        private void Write86_SoapOperationBinding(string n, string ns, SoapOperationBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(SoapOperationBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("SoapOperationBinding", "http://schemas.xmlsoap.org/wsdl/soap/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("soapAction", "", o.SoapAction);
                if (o.Style != SoapBindingStyle.Default)
                {
                    base.WriteAttribute("style", "", this.Write79_SoapBindingStyle(o.Style));
                }
                base.WriteEndElement(o);
            }
        }

        private void Write88_Soap12OperationBinding(string n, string ns, Soap12OperationBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Soap12OperationBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("Soap12OperationBinding", "http://schemas.xmlsoap.org/wsdl/soap12/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("soapAction", "", o.SoapAction);
                if (o.Style != SoapBindingStyle.Default)
                {
                    base.WriteAttribute("style", "", this.Write82_SoapBindingStyle(o.Style));
                }
                if (o.SoapActionRequired)
                {
                    base.WriteAttribute("soapActionRequired", "", XmlConvert.ToString(o.SoapActionRequired));
                }
                base.WriteEndElement(o);
            }
        }

        private void Write9_XmlSchemaDocumentation(string n, string ns, XmlSchemaDocumentation o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(XmlSchemaDocumentation)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.EscapeName = false;
                base.WriteStartElement(n, ns, o, false, o.Namespaces);
                if (needType)
                {
                    base.WriteXsiType("XmlSchemaDocumentation", "http://www.w3.org/2001/XMLSchema");
                }
                base.WriteAttribute("source", "", o.Source);
                base.WriteAttribute("lang", "http://www.w3.org/XML/1998/namespace", o.Language);
                XmlNode[] markup = o.Markup;
                if (markup != null)
                {
                    for (int i = 0; i < markup.Length; i++)
                    {
                        XmlNode node = markup[i];
                        if (node is XmlElement)
                        {
                            XmlElement element = (XmlElement) node;
                            if ((element == null) && (element != null))
                            {
                                throw base.CreateInvalidAnyTypeException(element);
                            }
                            base.WriteElementLiteral(element, "", null, false, true);
                        }
                        else if (node != null)
                        {
                            node.WriteTo(base.Writer);
                        }
                        else if (node != null)
                        {
                            throw base.CreateUnknownTypeException(node);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write90_HttpUrlEncodedBinding(string n, string ns, HttpUrlEncodedBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(HttpUrlEncodedBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("HttpUrlEncodedBinding", "http://schemas.xmlsoap.org/wsdl/http/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteEndElement(o);
            }
        }

        private void Write91_HttpUrlReplacementBinding(string n, string ns, HttpUrlReplacementBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(HttpUrlReplacementBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("HttpUrlReplacementBinding", "http://schemas.xmlsoap.org/wsdl/http/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteEndElement(o);
            }
        }

        private void Write93_MimeContentBinding(string n, string ns, MimeContentBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(MimeContentBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("MimeContentBinding", "http://schemas.xmlsoap.org/wsdl/mime/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("part", "", o.Part);
                base.WriteAttribute("type", "", o.Type);
                base.WriteEndElement(o);
            }
        }

        private void Write94_MimeXmlBinding(string n, string ns, MimeXmlBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(MimeXmlBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("MimeXmlBinding", "http://schemas.xmlsoap.org/wsdl/mime/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                base.WriteAttribute("part", "", o.Part);
                base.WriteEndElement(o);
            }
        }

        private void Write96_MimeTextMatch(string n, string ns, MimeTextMatch o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(MimeTextMatch)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("MimeTextMatch", "http://microsoft.com/wsdl/mime/textMatching/");
                }
                base.WriteAttribute("name", "", o.Name);
                base.WriteAttribute("type", "", o.Type);
                if (o.Group != 1)
                {
                    base.WriteAttribute("group", "", XmlConvert.ToString(o.Group));
                }
                if (o.Capture != 0)
                {
                    base.WriteAttribute("capture", "", XmlConvert.ToString(o.Capture));
                }
                if (o.RepeatsString != "1")
                {
                    base.WriteAttribute("repeats", "", o.RepeatsString);
                }
                base.WriteAttribute("pattern", "", o.Pattern);
                base.WriteAttribute("ignoreCase", "", XmlConvert.ToString(o.IgnoreCase));
                MimeTextMatchCollection matches = o.Matches;
                if (matches != null)
                {
                    for (int i = 0; i < matches.Count; i++)
                    {
                        this.Write96_MimeTextMatch("match", "http://microsoft.com/wsdl/mime/textMatching/", matches[i], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write97_MimeTextBinding(string n, string ns, MimeTextBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(MimeTextBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("MimeTextBinding", "http://microsoft.com/wsdl/mime/textMatching/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                MimeTextMatchCollection matches = o.Matches;
                if (matches != null)
                {
                    for (int i = 0; i < matches.Count; i++)
                    {
                        this.Write96_MimeTextMatch("match", "http://microsoft.com/wsdl/mime/textMatching/", matches[i], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private string Write98_SoapBindingUse(SoapBindingUse v)
        {
            switch (v)
            {
                case SoapBindingUse.Encoded:
                    return "encoded";

                case SoapBindingUse.Literal:
                    return "literal";
            }
            long num = (long) v;
            throw base.CreateInvalidEnumValueException(num.ToString(CultureInfo.InvariantCulture), "System.Web.Services.Description.SoapBindingUse");
        }

        private void Write99_SoapBodyBinding(string n, string ns, SoapBodyBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(SoapBodyBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("SoapBodyBinding", "http://schemas.xmlsoap.org/wsdl/soap/");
                }
                if (o.Required)
                {
                    base.WriteAttribute("required", "http://schemas.xmlsoap.org/wsdl/", XmlConvert.ToString(o.Required));
                }
                if (o.Use != SoapBindingUse.Default)
                {
                    base.WriteAttribute("use", "", this.Write98_SoapBindingUse(o.Use));
                }
                if ((o.Namespace != null) && (o.Namespace.Length != 0))
                {
                    base.WriteAttribute("namespace", "", o.Namespace);
                }
                if ((o.Encoding != null) && (o.Encoding.Length != 0))
                {
                    base.WriteAttribute("encodingStyle", "", o.Encoding);
                }
                base.WriteAttribute("parts", "", o.PartsString);
                base.WriteEndElement(o);
            }
        }
    }
}

