namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Web.Services.Description;
    using System.Xml;

    internal static class SoapHelper
    {
        private static object SoapVersionStateKey = new object();
        private static XmlDocument xmlDocument;

        private static System.Xml.XmlAttribute CreateLocalAttribute(string name, string value)
        {
            System.Xml.XmlAttribute attribute = Document.CreateAttribute(name);
            attribute.Value = value;
            return attribute;
        }

        private static SoapAddressBinding CreateSoapAddressBinding(EnvelopeVersion version, Port wsdlPort)
        {
            SoapAddressBinding extension = null;
            if (version == EnvelopeVersion.Soap12)
            {
                extension = new Soap12AddressBinding();
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                extension = new SoapAddressBinding();
            }
            wsdlPort.Extensions.Add(extension);
            return extension;
        }

        private static SoapBinding CreateSoapBinding(EnvelopeVersion version, Binding wsdlBinding)
        {
            SoapBinding extension = null;
            if (version == EnvelopeVersion.Soap12)
            {
                extension = new Soap12Binding();
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                extension = new SoapBinding();
            }
            wsdlBinding.Extensions.Add(extension);
            return extension;
        }

        private static SoapBodyBinding CreateSoapBodyBinding(EnvelopeVersion version, MessageBinding wsdlMessageBinding)
        {
            SoapBodyBinding extension = null;
            if (version == EnvelopeVersion.Soap12)
            {
                extension = new Soap12BodyBinding();
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                extension = new SoapBodyBinding();
            }
            wsdlMessageBinding.Extensions.Add(extension);
            return extension;
        }

        private static XmlElement CreateSoapFaultBinding(EnvelopeVersion version)
        {
            string prefix = null;
            string namespaceURI = null;
            if (version == EnvelopeVersion.Soap12)
            {
                namespaceURI = "http://schemas.xmlsoap.org/wsdl/soap12/";
                prefix = "soap12";
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                namespaceURI = "http://schemas.xmlsoap.org/wsdl/soap/";
                prefix = "soap";
            }
            return Document.CreateElement(prefix, "fault", namespaceURI);
        }

        internal static void CreateSoapFaultBinding(string name, WsdlEndpointConversionContext endpointContext, FaultBinding wsdlFaultBinding, bool isEncoded)
        {
            XmlElement extension = CreateSoapFaultBinding(GetSoapVersion(endpointContext.WsdlBinding));
            extension.Attributes.Append(CreateLocalAttribute("name", name));
            extension.Attributes.Append(CreateLocalAttribute("use", isEncoded ? "encoded" : "literal"));
            wsdlFaultBinding.Extensions.Add(extension);
        }

        internal static SoapHeaderBinding CreateSoapHeaderBinding(WsdlEndpointConversionContext endpointContext, MessageBinding wsdlMessageBinding)
        {
            return CreateSoapHeaderBinding(GetSoapVersion(endpointContext.WsdlBinding), wsdlMessageBinding);
        }

        private static SoapHeaderBinding CreateSoapHeaderBinding(EnvelopeVersion version, MessageBinding wsdlMessageBinding)
        {
            SoapHeaderBinding extension = null;
            if (version == EnvelopeVersion.Soap12)
            {
                extension = new Soap12HeaderBinding();
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                extension = new SoapHeaderBinding();
            }
            wsdlMessageBinding.Extensions.Add(extension);
            return extension;
        }

        private static SoapOperationBinding CreateSoapOperationBinding(EnvelopeVersion version, OperationBinding wsdlOperationBinding)
        {
            SoapOperationBinding extension = null;
            if (version == EnvelopeVersion.Soap12)
            {
                extension = new Soap12OperationBinding();
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                extension = new SoapOperationBinding();
            }
            wsdlOperationBinding.Extensions.Add(extension);
            return extension;
        }

        internal static SoapAddressBinding GetOrCreateSoapAddressBinding(Binding wsdlBinding, Port wsdlPort, WsdlExporter exporter)
        {
            if (GetSoapVersionState(wsdlBinding, exporter) == EnvelopeVersion.None)
            {
                return null;
            }
            SoapAddressBinding soapAddressBinding = GetSoapAddressBinding(wsdlPort);
            EnvelopeVersion soapVersion = GetSoapVersion(wsdlBinding);
            if (soapAddressBinding != null)
            {
                return soapAddressBinding;
            }
            return CreateSoapAddressBinding(soapVersion, wsdlPort);
        }

        internal static SoapBinding GetOrCreateSoapBinding(WsdlEndpointConversionContext endpointContext, WsdlExporter exporter)
        {
            if (GetSoapVersionState(endpointContext.WsdlBinding, exporter) == EnvelopeVersion.None)
            {
                return null;
            }
            SoapBinding soapBinding = GetSoapBinding(endpointContext);
            if (soapBinding != null)
            {
                return soapBinding;
            }
            return CreateSoapBinding(GetSoapVersion(endpointContext.WsdlBinding), endpointContext.WsdlBinding);
        }

        internal static SoapBodyBinding GetOrCreateSoapBodyBinding(WsdlEndpointConversionContext endpointContext, MessageBinding wsdlMessageBinding, WsdlExporter exporter)
        {
            if (GetSoapVersionState(endpointContext.WsdlBinding, exporter) == EnvelopeVersion.None)
            {
                return null;
            }
            SoapBodyBinding soapBodyBinding = GetSoapBodyBinding(endpointContext, wsdlMessageBinding);
            EnvelopeVersion soapVersion = GetSoapVersion(endpointContext.WsdlBinding);
            if (soapBodyBinding != null)
            {
                return soapBodyBinding;
            }
            return CreateSoapBodyBinding(soapVersion, wsdlMessageBinding);
        }

        internal static SoapOperationBinding GetOrCreateSoapOperationBinding(WsdlEndpointConversionContext endpointContext, OperationDescription operation, WsdlExporter exporter)
        {
            if (GetSoapVersionState(endpointContext.WsdlBinding, exporter) == EnvelopeVersion.None)
            {
                return null;
            }
            SoapOperationBinding soapOperationBinding = GetSoapOperationBinding(endpointContext, operation);
            OperationBinding operationBinding = endpointContext.GetOperationBinding(operation);
            EnvelopeVersion soapVersion = GetSoapVersion(endpointContext.WsdlBinding);
            if (soapOperationBinding != null)
            {
                return soapOperationBinding;
            }
            return CreateSoapOperationBinding(soapVersion, operationBinding);
        }

        private static SoapAddressBinding GetSoapAddressBinding(Port wsdlPort)
        {
            foreach (object obj2 in wsdlPort.Extensions)
            {
                if (obj2 is SoapAddressBinding)
                {
                    return (SoapAddressBinding) obj2;
                }
            }
            return null;
        }

        private static SoapBinding GetSoapBinding(WsdlEndpointConversionContext endpointContext)
        {
            foreach (object obj2 in endpointContext.WsdlBinding.Extensions)
            {
                if (obj2 is SoapBinding)
                {
                    return (SoapBinding) obj2;
                }
            }
            return null;
        }

        private static SoapBodyBinding GetSoapBodyBinding(WsdlEndpointConversionContext endpointContext, MessageBinding wsdlMessageBinding)
        {
            foreach (object obj2 in wsdlMessageBinding.Extensions)
            {
                if (obj2 is SoapBodyBinding)
                {
                    return (SoapBodyBinding) obj2;
                }
            }
            return null;
        }

        private static SoapOperationBinding GetSoapOperationBinding(WsdlEndpointConversionContext endpointContext, OperationDescription operation)
        {
            foreach (object obj2 in endpointContext.GetOperationBinding(operation).Extensions)
            {
                if (obj2 is SoapOperationBinding)
                {
                    return (SoapOperationBinding) obj2;
                }
            }
            return null;
        }

        internal static EnvelopeVersion GetSoapVersion(Binding wsdlBinding)
        {
            foreach (object obj2 in wsdlBinding.Extensions)
            {
                if (obj2 is SoapBinding)
                {
                    return ((obj2 is Soap12Binding) ? EnvelopeVersion.Soap12 : EnvelopeVersion.Soap11);
                }
            }
            return EnvelopeVersion.Soap12;
        }

        private static EnvelopeVersion GetSoapVersionState(Binding wsdlBinding, WsdlExporter exporter)
        {
            object obj2 = null;
            if ((exporter.State.TryGetValue(SoapVersionStateKey, out obj2) && (obj2 != null)) && ((Dictionary<Binding, EnvelopeVersion>) obj2).ContainsKey(wsdlBinding))
            {
                return ((Dictionary<Binding, EnvelopeVersion>) obj2)[wsdlBinding];
            }
            return null;
        }

        internal static SoapBindingStyle GetStyle(Binding binding)
        {
            SoapBindingStyle style = SoapBindingStyle.Default;
            if (binding != null)
            {
                SoapBinding binding2 = binding.Extensions.Find(typeof(SoapBinding)) as SoapBinding;
                if (binding2 != null)
                {
                    style = binding2.Style;
                }
            }
            return style;
        }

        internal static SoapBindingStyle GetStyle(OperationBinding operationBinding, SoapBindingStyle defaultBindingStyle)
        {
            SoapBindingStyle style = defaultBindingStyle;
            if (operationBinding != null)
            {
                SoapOperationBinding binding = operationBinding.Extensions.Find(typeof(SoapOperationBinding)) as SoapOperationBinding;
                if ((binding != null) && (binding.Style != SoapBindingStyle.Default))
                {
                    style = binding.Style;
                }
            }
            return style;
        }

        internal static bool IsEncoded(XmlElement element)
        {
            System.Xml.XmlAttribute attributeNode = element.GetAttributeNode("use");
            if (attributeNode == null)
            {
                return false;
            }
            return (attributeNode.Value == "encoded");
        }

        internal static bool IsSoapFaultBinding(XmlElement element)
        {
            if ((element == null) || !(element.LocalName == "fault"))
            {
                return false;
            }
            if (!(element.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/soap12/"))
            {
                return (element.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/soap/");
            }
            return true;
        }

        internal static string ReadSoapAction(OperationBinding wsdlOperationBinding)
        {
            SoapOperationBinding binding = (SoapOperationBinding) wsdlOperationBinding.Extensions.Find(typeof(SoapOperationBinding));
            if (binding == null)
            {
                return null;
            }
            return binding.SoapAction;
        }

        internal static void SetSoapVersion(WsdlEndpointConversionContext endpointContext, WsdlExporter exporter, EnvelopeVersion version)
        {
            SetSoapVersionState(endpointContext.WsdlBinding, exporter, version);
            if (endpointContext.WsdlPort != null)
            {
                SoapConverter.ConvertExtensions(endpointContext.WsdlPort.Extensions, version, new SoapConverter.ConvertExtension(SoapConverter.ConvertSoapAddressBinding));
            }
            SoapConverter.ConvertExtensions(endpointContext.WsdlBinding.Extensions, version, new SoapConverter.ConvertExtension(SoapConverter.ConvertSoapBinding));
            foreach (OperationBinding binding in endpointContext.WsdlBinding.Operations)
            {
                SoapConverter.ConvertExtensions(binding.Extensions, version, new SoapConverter.ConvertExtension(SoapConverter.ConvertSoapOperationBinding));
                if (binding.Input != null)
                {
                    SoapConverter.ConvertExtensions(binding.Input.Extensions, version, new SoapConverter.ConvertExtension(SoapConverter.ConvertSoapMessageBinding));
                }
                if (binding.Output != null)
                {
                    SoapConverter.ConvertExtensions(binding.Output.Extensions, version, new SoapConverter.ConvertExtension(SoapConverter.ConvertSoapMessageBinding));
                }
                foreach (MessageBinding binding2 in binding.Faults)
                {
                    SoapConverter.ConvertExtensions(binding2.Extensions, version, new SoapConverter.ConvertExtension(SoapConverter.ConvertSoapMessageBinding));
                }
            }
        }

        private static void SetSoapVersionState(Binding wsdlBinding, WsdlExporter exporter, EnvelopeVersion version)
        {
            object obj2 = null;
            if (!exporter.State.TryGetValue(SoapVersionStateKey, out obj2))
            {
                obj2 = new Dictionary<Binding, EnvelopeVersion>();
                exporter.State[SoapVersionStateKey] = obj2;
            }
            ((Dictionary<Binding, EnvelopeVersion>) obj2)[wsdlBinding] = version;
        }

        private static XmlDocument Document
        {
            get
            {
                if (xmlDocument == null)
                {
                    xmlDocument = new XmlDocument();
                }
                return xmlDocument;
            }
        }

        private static class SoapConverter
        {
            internal static void ConvertExtensions(ServiceDescriptionFormatExtensionCollection extensions, EnvelopeVersion version, ConvertExtension conversionMethod)
            {
                bool flag = false;
                for (int i = extensions.Count - 1; i >= 0; i--)
                {
                    object src = extensions[i];
                    if (conversionMethod(ref src, version))
                    {
                        if (src == null)
                        {
                            extensions.Remove(extensions[i]);
                        }
                        else
                        {
                            extensions[i] = src;
                        }
                        flag = true;
                    }
                }
                if (!flag)
                {
                    object obj3 = null;
                    conversionMethod(ref obj3, version);
                    if (obj3 != null)
                    {
                        extensions.Add(obj3);
                    }
                }
            }

            internal static bool ConvertSoapAddressBinding(ref object src, EnvelopeVersion version)
            {
                SoapAddressBinding binding = src as SoapAddressBinding;
                if (src != null)
                {
                    if (binding == null)
                    {
                        return false;
                    }
                    if (GetBindingVersion<Soap12AddressBinding>(src) == version)
                    {
                        return true;
                    }
                }
                if (version == EnvelopeVersion.None)
                {
                    src = null;
                    return true;
                }
                SoapAddressBinding binding2 = (version == EnvelopeVersion.Soap12) ? new Soap12AddressBinding() : new SoapAddressBinding();
                if (binding != null)
                {
                    binding2.Required = binding.Required;
                    binding2.Location = binding.Location;
                }
                src = binding2;
                return true;
            }

            internal static bool ConvertSoapBinding(ref object src, EnvelopeVersion version)
            {
                SoapBinding binding = src as SoapBinding;
                if (src != null)
                {
                    if (binding == null)
                    {
                        return false;
                    }
                    if (GetBindingVersion<Soap12Binding>(src) == version)
                    {
                        return true;
                    }
                }
                if (version == EnvelopeVersion.None)
                {
                    src = null;
                    return true;
                }
                SoapBinding binding2 = (version == EnvelopeVersion.Soap12) ? new Soap12Binding() : new SoapBinding();
                if (binding != null)
                {
                    binding2.Required = binding.Required;
                    binding2.Style = binding.Style;
                    binding2.Transport = binding.Transport;
                }
                src = binding2;
                return true;
            }

            private static SoapBodyBinding ConvertSoapBodyBinding(SoapBodyBinding src, EnvelopeVersion version)
            {
                if (version == EnvelopeVersion.None)
                {
                    return null;
                }
                EnvelopeVersion bindingVersion = GetBindingVersion<Soap12BodyBinding>(src);
                if (bindingVersion == version)
                {
                    return src;
                }
                SoapBodyBinding binding = (version == EnvelopeVersion.Soap12) ? new Soap12BodyBinding() : new SoapBodyBinding();
                if (src != null)
                {
                    if (XmlSerializerOperationFormatter.GetEncoding(bindingVersion) == src.Encoding)
                    {
                        binding.Encoding = XmlSerializerOperationFormatter.GetEncoding(version);
                    }
                    binding.Encoding = XmlSerializerOperationFormatter.GetEncoding(version);
                    binding.Namespace = src.Namespace;
                    binding.Parts = src.Parts;
                    binding.PartsString = src.PartsString;
                    binding.Use = src.Use;
                    binding.Required = src.Required;
                }
                return binding;
            }

            private static SoapFaultBinding ConvertSoapFaultBinding(SoapFaultBinding src, EnvelopeVersion version)
            {
                if (version == EnvelopeVersion.None)
                {
                    return null;
                }
                if (GetBindingVersion<Soap12FaultBinding>(src) == version)
                {
                    return src;
                }
                SoapFaultBinding binding = (version == EnvelopeVersion.Soap12) ? new Soap12FaultBinding() : new SoapFaultBinding();
                if (src != null)
                {
                    binding.Encoding = src.Encoding;
                    binding.Name = src.Name;
                    binding.Namespace = src.Namespace;
                    binding.Use = src.Use;
                    binding.Required = src.Required;
                }
                return binding;
            }

            private static XmlElement ConvertSoapFaultBinding(XmlElement src, EnvelopeVersion version)
            {
                XmlElement element;
                if (src != null)
                {
                    if (version == EnvelopeVersion.Soap12)
                    {
                        if (src.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/soap12/")
                        {
                            return src;
                        }
                        goto Label_003F;
                    }
                    if (version == EnvelopeVersion.Soap11)
                    {
                        if (src.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/soap/")
                        {
                            return src;
                        }
                        goto Label_003F;
                    }
                }
                return null;
            Label_003F:
                element = SoapHelper.CreateSoapFaultBinding(version);
                if (src.HasAttributes)
                {
                    foreach (System.Xml.XmlAttribute attribute in src.Attributes)
                    {
                        element.SetAttribute(attribute.Name, attribute.Value);
                    }
                }
                return element;
            }

            private static SoapHeaderBinding ConvertSoapHeaderBinding(SoapHeaderBinding src, EnvelopeVersion version)
            {
                if (version == EnvelopeVersion.None)
                {
                    return null;
                }
                if (GetBindingVersion<Soap12HeaderBinding>(src) == version)
                {
                    return src;
                }
                SoapHeaderBinding binding = (version == EnvelopeVersion.Soap12) ? new Soap12HeaderBinding() : new SoapHeaderBinding();
                if (src != null)
                {
                    binding.Fault = src.Fault;
                    binding.MapToProperty = src.MapToProperty;
                    binding.Message = src.Message;
                    binding.Part = src.Part;
                    binding.Encoding = src.Encoding;
                    binding.Namespace = src.Namespace;
                    binding.Use = src.Use;
                    binding.Required = src.Required;
                }
                return binding;
            }

            internal static bool ConvertSoapMessageBinding(ref object src, EnvelopeVersion version)
            {
                SoapBodyBinding binding = src as SoapBodyBinding;
                if (binding != null)
                {
                    src = ConvertSoapBodyBinding(binding, version);
                    return true;
                }
                SoapHeaderBinding binding2 = src as SoapHeaderBinding;
                if (binding2 != null)
                {
                    src = ConvertSoapHeaderBinding(binding2, version);
                    return true;
                }
                SoapFaultBinding binding3 = src as SoapFaultBinding;
                if (binding3 != null)
                {
                    src = ConvertSoapFaultBinding(binding3, version);
                    return true;
                }
                XmlElement element = src as XmlElement;
                if ((element != null) && SoapHelper.IsSoapFaultBinding(element))
                {
                    src = ConvertSoapFaultBinding(element, version);
                    return true;
                }
                return (src == null);
            }

            internal static bool ConvertSoapOperationBinding(ref object src, EnvelopeVersion version)
            {
                SoapOperationBinding binding = src as SoapOperationBinding;
                if (src != null)
                {
                    if (binding == null)
                    {
                        return false;
                    }
                    if (GetBindingVersion<Soap12OperationBinding>(src) == version)
                    {
                        return true;
                    }
                }
                if (version == EnvelopeVersion.None)
                {
                    src = null;
                    return true;
                }
                SoapOperationBinding binding2 = (version == EnvelopeVersion.Soap12) ? new Soap12OperationBinding() : new SoapOperationBinding();
                if (src != null)
                {
                    binding2.Required = binding.Required;
                    binding2.Style = binding.Style;
                    binding2.SoapAction = binding.SoapAction;
                }
                src = binding2;
                return true;
            }

            internal static EnvelopeVersion GetBindingVersion<T12>(object src)
            {
                if (src is T12)
                {
                    return EnvelopeVersion.Soap12;
                }
                return EnvelopeVersion.Soap11;
            }

            internal delegate bool ConvertExtension(ref object src, EnvelopeVersion version);
        }
    }
}

