namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Serialization;

    internal class SoapProtocolReflector : ProtocolReflector
    {
        private SoapExtensionReflector[] extensions;
        private ArrayList mappings = new ArrayList();
        private SoapReflectedMethod soapMethod;

        private void AllowExtensionsToReflectDescription()
        {
            if (this.extensions == null)
            {
                TypeElementCollection soapExtensionReflectorTypes = WebServicesSection.Current.SoapExtensionReflectorTypes;
                this.extensions = new SoapExtensionReflector[soapExtensionReflectorTypes.Count];
                for (int i = 0; i < this.extensions.Length; i++)
                {
                    SoapExtensionReflector reflector = (SoapExtensionReflector) Activator.CreateInstance(soapExtensionReflectorTypes[i].Type);
                    reflector.ReflectionContext = this;
                    this.extensions[i] = reflector;
                }
            }
            foreach (SoapExtensionReflector reflector2 in this.extensions)
            {
                reflector2.ReflectDescription();
            }
        }

        private void AllowExtensionsToReflectMethod()
        {
            if (this.extensions == null)
            {
                TypeElementCollection soapExtensionReflectorTypes = WebServicesSection.Current.SoapExtensionReflectorTypes;
                this.extensions = new SoapExtensionReflector[soapExtensionReflectorTypes.Count];
                for (int i = 0; i < this.extensions.Length; i++)
                {
                    SoapExtensionReflector reflector = (SoapExtensionReflector) Activator.CreateInstance(soapExtensionReflectorTypes[i].Type);
                    reflector.ReflectionContext = this;
                    this.extensions[i] = reflector;
                }
            }
            foreach (SoapExtensionReflector reflector2 in this.extensions)
            {
                reflector2.ReflectMethod();
            }
        }

        protected override void BeginClass()
        {
            if (base.Binding != null)
            {
                SoapBindingStyle rpc;
                if (SoapReflector.GetSoapServiceAttribute(base.ServiceType) is SoapRpcServiceAttribute)
                {
                    rpc = SoapBindingStyle.Rpc;
                }
                else
                {
                    rpc = SoapBindingStyle.Document;
                }
                base.Binding.Extensions.Add(this.CreateSoapBinding(rpc));
                SoapReflector.IncludeTypes(base.Methods, this.SoapImporter);
            }
            base.Port.Extensions.Add(this.CreateSoapAddressBinding(base.ServiceUrl));
        }

        private void CreateEncodedMessage(Message message, MessageBinding messageBinding, XmlMembersMapping members, bool wrapped)
        {
            this.SoapExporter.ExportMembersMapping(members, wrapped);
            if (wrapped)
            {
                MessagePart messagePart = new MessagePart {
                    Name = "parameters",
                    Type = new XmlQualifiedName(members.TypeName, members.TypeNamespace)
                };
                message.Parts.Add(messagePart);
            }
            else
            {
                for (int i = 0; i < members.Count; i++)
                {
                    XmlMemberMapping mapping = members[i];
                    MessagePart part2 = new MessagePart {
                        Name = mapping.XsdElementName,
                        Type = new XmlQualifiedName(mapping.TypeName, mapping.TypeNamespace)
                    };
                    message.Parts.Add(part2);
                }
            }
            messageBinding.Extensions.Add(this.CreateSoapBodyBinding(SoapBindingUse.Encoded, members.Namespace));
        }

        private void CreateHeaderMessages(string methodName, SoapBindingUse use, XmlMembersMapping inHeaderMappings, XmlMembersMapping outHeaderMappings, SoapReflectedHeader[] headers, bool rpc)
        {
            if (use == SoapBindingUse.Encoded)
            {
                this.SoapExporter.ExportMembersMapping(inHeaderMappings, false);
                if (outHeaderMappings != null)
                {
                    this.SoapExporter.ExportMembersMapping(outHeaderMappings, false);
                }
            }
            else
            {
                base.SchemaExporter.ExportMembersMapping(inHeaderMappings);
                if (outHeaderMappings != null)
                {
                    base.SchemaExporter.ExportMembersMapping(outHeaderMappings);
                }
            }
            CodeIdentifiers identifiers = new CodeIdentifiers();
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < headers.Length; i++)
            {
                SoapReflectedHeader header = headers[i];
                if (header.custom)
                {
                    XmlMemberMapping mapping;
                    Message message;
                    if ((header.direction & SoapHeaderDirection.In) != 0)
                    {
                        mapping = inHeaderMappings[num++];
                        if (header.direction != SoapHeaderDirection.In)
                        {
                            num2++;
                        }
                    }
                    else
                    {
                        mapping = outHeaderMappings[num2++];
                    }
                    MessagePart messagePart = new MessagePart {
                        Name = mapping.XsdElementName
                    };
                    if (use == SoapBindingUse.Encoded)
                    {
                        messagePart.Type = new XmlQualifiedName(mapping.TypeName, mapping.TypeNamespace);
                    }
                    else
                    {
                        messagePart.Element = new XmlQualifiedName(mapping.XsdElementName, mapping.Namespace);
                    }
                    message = new Message {
                        Name = identifiers.AddUnique(methodName + messagePart.Name, message)
                    };
                    message.Parts.Add(messagePart);
                    base.HeaderMessages.Add(message);
                    ServiceDescriptionFormatExtension extension = this.CreateSoapHeaderBinding(new XmlQualifiedName(message.Name, base.Binding.ServiceDescription.TargetNamespace), messagePart.Name, rpc ? mapping.Namespace : null, use);
                    if ((header.direction & SoapHeaderDirection.In) != 0)
                    {
                        base.OperationBinding.Input.Extensions.Add(extension);
                    }
                    if ((header.direction & SoapHeaderDirection.Out) != 0)
                    {
                        base.OperationBinding.Output.Extensions.Add(extension);
                    }
                    if ((header.direction & SoapHeaderDirection.Fault) != 0)
                    {
                        if (this.soapMethod.IsClaimsConformance)
                        {
                            throw new InvalidOperationException(System.Web.Services.Res.GetString("BPConformanceHeaderFault", new object[] { this.soapMethod.methodInfo.ToString(), this.soapMethod.methodInfo.DeclaringType.FullName, "Direction", typeof(SoapHeaderDirection).Name, SoapHeaderDirection.Fault.ToString() }));
                        }
                        base.OperationBinding.Output.Extensions.Add(extension);
                    }
                }
            }
        }

        private void CreateLiteralMessage(Message message, MessageBinding messageBinding, XmlMembersMapping members, bool wrapped, bool rpc)
        {
            if (((members.Count == 1) && members[0].Any) && ((members[0].ElementName.Length == 0) && !wrapped))
            {
                string name = base.SchemaExporter.ExportAnyType(members[0].Namespace);
                MessagePart messagePart = new MessagePart {
                    Name = members[0].MemberName,
                    Type = new XmlQualifiedName(name, members[0].Namespace)
                };
                message.Parts.Add(messagePart);
            }
            else
            {
                base.SchemaExporter.ExportMembersMapping(members, !rpc);
                if (wrapped)
                {
                    MessagePart part2 = new MessagePart {
                        Name = "parameters",
                        Element = new XmlQualifiedName(members.XsdElementName, members.Namespace)
                    };
                    message.Parts.Add(part2);
                }
                else
                {
                    for (int i = 0; i < members.Count; i++)
                    {
                        XmlMemberMapping mapping = members[i];
                        MessagePart part3 = new MessagePart();
                        if (rpc)
                        {
                            if ((mapping.TypeName == null) || (mapping.TypeName.Length == 0))
                            {
                                throw new InvalidOperationException(System.Web.Services.Res.GetString("WsdlGenRpcLitAnonimousType", new object[] { base.Method.DeclaringType.Name, base.Method.Name, mapping.MemberName }));
                            }
                            part3.Name = mapping.XsdElementName;
                            part3.Type = new XmlQualifiedName(mapping.TypeName, mapping.TypeNamespace);
                        }
                        else
                        {
                            part3.Name = XmlConvert.EncodeLocalName(mapping.MemberName);
                            part3.Element = new XmlQualifiedName(mapping.XsdElementName, mapping.Namespace);
                        }
                        message.Parts.Add(part3);
                    }
                }
            }
            messageBinding.Extensions.Add(this.CreateSoapBodyBinding(SoapBindingUse.Literal, rpc ? members.Namespace : null));
        }

        private void CreateMessage(bool rpc, SoapBindingUse use, SoapParameterStyle paramStyle, Message message, MessageBinding messageBinding, XmlMembersMapping members)
        {
            bool flag = paramStyle != SoapParameterStyle.Bare;
            if (use == SoapBindingUse.Encoded)
            {
                this.CreateEncodedMessage(message, messageBinding, members, flag && !rpc);
            }
            else
            {
                this.CreateLiteralMessage(message, messageBinding, members, flag && !rpc, rpc);
            }
        }

        protected virtual SoapAddressBinding CreateSoapAddressBinding(string serviceUrl)
        {
            return new SoapAddressBinding { Location = serviceUrl };
        }

        protected virtual SoapBinding CreateSoapBinding(SoapBindingStyle style)
        {
            return new SoapBinding { Transport = "http://schemas.xmlsoap.org/soap/http", Style = style };
        }

        protected virtual SoapBodyBinding CreateSoapBodyBinding(SoapBindingUse use, string ns)
        {
            SoapBodyBinding binding = new SoapBodyBinding {
                Use = use
            };
            if (use == SoapBindingUse.Encoded)
            {
                binding.Encoding = "http://schemas.xmlsoap.org/soap/encoding/";
            }
            binding.Namespace = ns;
            return binding;
        }

        protected virtual SoapHeaderBinding CreateSoapHeaderBinding(XmlQualifiedName message, string partName, SoapBindingUse use)
        {
            return this.CreateSoapHeaderBinding(message, partName, null, use);
        }

        protected virtual SoapHeaderBinding CreateSoapHeaderBinding(XmlQualifiedName message, string partName, string ns, SoapBindingUse use)
        {
            SoapHeaderBinding binding = new SoapHeaderBinding {
                Message = message,
                Part = partName,
                Use = use
            };
            if (use == SoapBindingUse.Encoded)
            {
                binding.Encoding = "http://schemas.xmlsoap.org/soap/encoding/";
                binding.Namespace = ns;
            }
            return binding;
        }

        protected virtual SoapOperationBinding CreateSoapOperationBinding(SoapBindingStyle style, string action)
        {
            return new SoapOperationBinding { SoapAction = action, Style = style };
        }

        private static string[] GetParameterOrder(LogicalMethodInfo methodInfo)
        {
            ParameterInfo[] parameters = methodInfo.Parameters;
            string[] strArray = new string[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                strArray[i] = parameters[i].Name;
            }
            return strArray;
        }

        protected override void ReflectDescription()
        {
            this.AllowExtensionsToReflectDescription();
        }

        protected override bool ReflectMethod()
        {
            this.soapMethod = base.ReflectionContext[base.Method] as SoapReflectedMethod;
            if (this.soapMethod == null)
            {
                this.soapMethod = SoapReflector.ReflectMethod(base.Method, false, base.ReflectionImporter, this.SoapImporter, base.DefaultNamespace);
                base.ReflectionContext[base.Method] = this.soapMethod;
                this.soapMethod.portType = (base.Binding != null) ? base.Binding.Type : null;
            }
            WebMethodAttribute methodAttribute = base.Method.MethodAttribute;
            base.OperationBinding.Extensions.Add(this.CreateSoapOperationBinding(this.soapMethod.rpc ? SoapBindingStyle.Rpc : SoapBindingStyle.Document, this.soapMethod.action));
            this.CreateMessage(this.soapMethod.rpc, this.soapMethod.use, this.soapMethod.paramStyle, base.InputMessage, base.OperationBinding.Input, this.soapMethod.requestMappings);
            if (!this.soapMethod.oneWay)
            {
                this.CreateMessage(this.soapMethod.rpc, this.soapMethod.use, this.soapMethod.paramStyle, base.OutputMessage, base.OperationBinding.Output, this.soapMethod.responseMappings);
            }
            this.CreateHeaderMessages(this.soapMethod.name, this.soapMethod.use, this.soapMethod.inHeaderMappings, this.soapMethod.outHeaderMappings, this.soapMethod.headers, this.soapMethod.rpc);
            if ((this.soapMethod.rpc && (this.soapMethod.use == SoapBindingUse.Encoded)) && (this.soapMethod.methodInfo.OutParameters.Length > 0))
            {
                base.Operation.ParameterOrder = GetParameterOrder(this.soapMethod.methodInfo);
            }
            this.AllowExtensionsToReflectMethod();
            return true;
        }

        protected override string ReflectMethodBinding()
        {
            return SoapReflector.GetSoapMethodBinding(base.Method);
        }

        internal override WsiProfiles ConformsTo
        {
            get
            {
                return WsiProfiles.BasicProfile1_1;
            }
        }

        public override string ProtocolName
        {
            get
            {
                return "Soap";
            }
        }

        internal SoapSchemaExporter SoapExporter
        {
            get
            {
                SoapSchemaExporter exporter = base.ReflectionContext[typeof(SoapSchemaExporter)] as SoapSchemaExporter;
                if (exporter == null)
                {
                    exporter = new SoapSchemaExporter(base.ServiceDescription.Types.Schemas);
                    base.ReflectionContext[typeof(SoapSchemaExporter)] = exporter;
                }
                return exporter;
            }
        }

        internal SoapReflectionImporter SoapImporter
        {
            get
            {
                SoapReflectionImporter importer = base.ReflectionContext[typeof(SoapReflectionImporter)] as SoapReflectionImporter;
                if (importer == null)
                {
                    importer = SoapReflector.CreateSoapImporter(base.DefaultNamespace, SoapReflector.ServiceDefaultIsEncoded(base.ServiceType));
                    base.ReflectionContext[typeof(SoapReflectionImporter)] = importer;
                }
                return importer;
            }
        }

        internal SoapReflectedMethod SoapMethod
        {
            get
            {
                return this.soapMethod;
            }
        }
    }
}

