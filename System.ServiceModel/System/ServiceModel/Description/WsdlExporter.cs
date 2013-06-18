namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public class WsdlExporter : MetadataExporter
    {
        private Dictionary<BindingDictionaryKey, WsdlEndpointConversionContext> exportedBindings = new Dictionary<BindingDictionaryKey, WsdlEndpointConversionContext>();
        private Dictionary<ContractDescription, WsdlContractConversionContext> exportedContracts = new Dictionary<ContractDescription, WsdlContractConversionContext>();
        private Dictionary<EndpointDictionaryKey, ServiceEndpoint> exportedEndpoints = new Dictionary<EndpointDictionaryKey, ServiceEndpoint>();
        private bool isFaulted;
        private ServiceDescriptionCollection wsdlDocuments = new ServiceDescriptionCollection();
        private static XmlDocument xmlDocument;
        private XmlSchemaSet xmlSchemas = GetEmptySchemaSet();

        private void CallExportContract(WsdlContractConversionContext contractContext)
        {
            foreach (IWsdlExportExtension extension in contractContext.ExportExtensions)
            {
                this.CallExtension(contractContext, extension);
            }
        }

        private void CallExportEndpoint(WsdlEndpointConversionContext endpointContext)
        {
            foreach (IWsdlExportExtension extension in endpointContext.ExportExtensions)
            {
                this.CallExtension(endpointContext, extension);
            }
        }

        private void CallExtension(WsdlContractConversionContext contractContext, IWsdlExportExtension extension)
        {
            try
            {
                extension.ExportContract(this, contractContext);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ThrowExtensionException(contractContext.Contract, extension, exception));
            }
        }

        private void CallExtension(WsdlEndpointConversionContext endpointContext, IWsdlExportExtension extension)
        {
            try
            {
                extension.ExportEndpoint(this, endpointContext);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ThrowExtensionException(endpointContext.Endpoint, extension, exception));
            }
        }

        private System.Web.Services.Description.Binding CreateWsdlBindingAndPort(ServiceEndpoint endpoint, XmlQualifiedName wsdlServiceQName, out Port wsdlPort, out bool newBinding, out bool bindingNameWasUniquified)
        {
            System.Web.Services.Description.ServiceDescription orCreateWsdl;
            System.Web.Services.Description.Binding binding;
            WsdlEndpointConversionContext context;
            XmlQualifiedName name;
            XmlQualifiedName type;
            bool flag = IsWsdlExportable(endpoint.Binding);
            if (!this.exportedBindings.TryGetValue(new BindingDictionaryKey(endpoint.Contract, endpoint.Binding), out context))
            {
                name = WsdlNamingHelper.GetBindingQName(endpoint, this, out bindingNameWasUniquified);
                orCreateWsdl = this.GetOrCreateWsdl(name.Namespace);
                binding = new System.Web.Services.Description.Binding {
                    Name = name.Name
                };
                newBinding = true;
                PortType wsdlPortType = this.exportedContracts[endpoint.Contract].WsdlPortType;
                type = new XmlQualifiedName(wsdlPortType.Name, wsdlPortType.ServiceDescription.TargetNamespace);
                binding.Type = type;
                if (flag)
                {
                    orCreateWsdl.Bindings.Add(binding);
                }
                EnsureWsdlContainsImport(orCreateWsdl, type.Namespace);
            }
            else
            {
                name = new XmlQualifiedName(context.WsdlBinding.Name, context.WsdlBinding.ServiceDescription.TargetNamespace);
                bindingNameWasUniquified = false;
                orCreateWsdl = this.wsdlDocuments[name.Namespace];
                binding = orCreateWsdl.Bindings[name.Name];
                type = binding.Type;
                newBinding = false;
            }
            if (endpoint.Address != null)
            {
                Service orCreateWsdlService = this.GetOrCreateWsdlService(wsdlServiceQName);
                wsdlPort = new Port();
                string portName = WsdlNamingHelper.GetPortName(endpoint, orCreateWsdlService);
                wsdlPort.Name = portName;
                wsdlPort.Binding = name;
                SoapAddressBinding binding2 = SoapHelper.GetOrCreateSoapAddressBinding(binding, wsdlPort, this);
                if (binding2 != null)
                {
                    binding2.Location = endpoint.Address.Uri.AbsoluteUri;
                }
                EnsureWsdlContainsImport(orCreateWsdlService.ServiceDescription, name.Namespace);
                if (flag)
                {
                    orCreateWsdlService.Ports.Add(wsdlPort);
                }
                return binding;
            }
            wsdlPort = null;
            return binding;
        }

        private FaultBinding CreateWsdlFaultBinding(FaultDescription faultDescription, System.ServiceModel.Channels.Binding binding, OperationBinding wsdlOperationBinding)
        {
            FaultBinding bindingOperationFault = new FaultBinding();
            wsdlOperationBinding.Faults.Add(bindingOperationFault);
            if (faultDescription.Name != null)
            {
                bindingOperationFault.Name = faultDescription.Name;
            }
            return bindingOperationFault;
        }

        private MessageBinding CreateWsdlMessageBinding(MessageDescription messageDescription, System.ServiceModel.Channels.Binding binding, OperationBinding wsdlOperationBinding)
        {
            MessageBinding input;
            if (messageDescription.Direction == MessageDirection.Input)
            {
                wsdlOperationBinding.Input = new InputBinding();
                input = wsdlOperationBinding.Input;
            }
            else
            {
                wsdlOperationBinding.Output = new OutputBinding();
                input = wsdlOperationBinding.Output;
            }
            if (!System.ServiceModel.Description.XmlName.IsNullOrEmpty(messageDescription.MessageName))
            {
                input.Name = messageDescription.MessageName.EncodedName;
            }
            return input;
        }

        private Operation CreateWsdlOperation(OperationDescription operation, ContractDescription contract)
        {
            Operation wsdlOperation = new Operation {
                Name = WsdlNamingHelper.GetWsdlOperationName(operation, contract)
            };
            NetSessionHelper.AddInitiatingTerminatingAttributesIfNeeded(wsdlOperation, operation, contract);
            return wsdlOperation;
        }

        private OperationBinding CreateWsdlOperationBinding(ContractDescription contract, OperationDescription operation)
        {
            return new OperationBinding { Name = WsdlNamingHelper.GetWsdlOperationName(operation, contract) };
        }

        private OperationFault CreateWsdlOperationFault(FaultDescription fault)
        {
            OperationFault wsdlOperationMessage = new OperationFault {
                Name = fault.Name
            };
            WSAddressingHelper.AddActionAttribute(fault.Action, wsdlOperationMessage, base.PolicyVersion);
            return wsdlOperationMessage;
        }

        private OperationMessage CreateWsdlOperationMessage(MessageDescription message)
        {
            OperationMessage message2;
            if (message.Direction == MessageDirection.Input)
            {
                message2 = new OperationInput();
            }
            else
            {
                message2 = new OperationOutput();
            }
            if (!System.ServiceModel.Description.XmlName.IsNullOrEmpty(message.MessageName))
            {
                message2.Name = message.MessageName.EncodedName;
            }
            WSAddressingHelper.AddActionAttribute(message.Action, message2, base.PolicyVersion);
            return message2;
        }

        private PortType CreateWsdlPortType(ContractDescription contract)
        {
            XmlQualifiedName portTypeQName = WsdlNamingHelper.GetPortTypeQName(contract);
            System.Web.Services.Description.ServiceDescription orCreateWsdl = this.GetOrCreateWsdl(portTypeQName.Namespace);
            PortType wsdlPortType = new PortType {
                Name = portTypeQName.Name
            };
            if (orCreateWsdl.PortTypes[wsdlPortType.Name] != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("DuplicateContractQNameNameOnExport", new object[] { contract.Name, contract.Namespace })));
            }
            NetSessionHelper.AddUsingSessionAttributeIfNeeded(wsdlPortType, contract);
            orCreateWsdl.PortTypes.Add(wsdlPortType);
            return wsdlPortType;
        }

        private static void EnsureWsdlContainsImport(System.Web.Services.Description.ServiceDescription srcWsdl, string target)
        {
            if (srcWsdl.TargetNamespace != target)
            {
                foreach (Import import in srcWsdl.Imports)
                {
                    if (import.Namespace == target)
                    {
                        return;
                    }
                }
                Import import2 = new Import {
                    Location = null,
                    Namespace = target
                };
                srcWsdl.Imports.Add(import2);
                WsdlNamespaceHelper.FindOrCreatePrefix("i", target, new DocumentableItem[] { srcWsdl });
            }
        }

        public override void ExportContract(ContractDescription contract)
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlExporterIsFaulted")));
            }
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }
            if (!this.exportedContracts.ContainsKey(contract))
            {
                try
                {
                    PortType wsdlPortType = this.CreateWsdlPortType(contract);
                    WsdlContractConversionContext contractContext = new WsdlContractConversionContext(contract, wsdlPortType);
                    foreach (OperationDescription description in contract.Operations)
                    {
                        if (!OperationIsExportable(description))
                        {
                            string warningMessage = System.ServiceModel.SR.GetString("WarnSkippingOprtationWithWildcardAction", new object[] { contract.Name, contract.Namespace, description.Name });
                            this.LogExportWarning(warningMessage);
                        }
                        else
                        {
                            Operation operation = this.CreateWsdlOperation(description, contract);
                            wsdlPortType.Operations.Add(operation);
                            contractContext.AddOperation(description, operation);
                            foreach (MessageDescription description2 in description.Messages)
                            {
                                OperationMessage operationMessage = this.CreateWsdlOperationMessage(description2);
                                operation.Messages.Add(operationMessage);
                                contractContext.AddMessage(description2, operationMessage);
                            }
                            foreach (FaultDescription description3 in description.Faults)
                            {
                                OperationFault operationFaultMessage = this.CreateWsdlOperationFault(description3);
                                operation.Faults.Add(operationFaultMessage);
                                contractContext.AddFault(description3, operationFaultMessage);
                            }
                        }
                    }
                    this.CallExportContract(contractContext);
                    this.exportedContracts.Add(contract, contractContext);
                }
                catch
                {
                    this.isFaulted = true;
                    throw;
                }
            }
        }

        public override void ExportEndpoint(ServiceEndpoint endpoint)
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlExporterIsFaulted")));
            }
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            this.ExportEndpoint(endpoint, new XmlQualifiedName("service", "http://tempuri.org/"));
        }

        private void ExportEndpoint(ServiceEndpoint endpoint, XmlQualifiedName wsdlServiceQName)
        {
            if (endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("EndpointsMustHaveAValidBinding1", new object[] { endpoint.Name })));
            }
            EndpointDictionaryKey key = new EndpointDictionaryKey(endpoint, wsdlServiceQName);
            try
            {
                if (!this.exportedEndpoints.ContainsKey(key))
                {
                    bool flag;
                    bool flag2;
                    Port port;
                    this.ExportContract(endpoint.Contract);
                    WsdlContractConversionContext contractContext = this.exportedContracts[endpoint.Contract];
                    System.Web.Services.Description.Binding wsdlBinding = this.CreateWsdlBindingAndPort(endpoint, wsdlServiceQName, out port, out flag, out flag2);
                    if (flag || (port != null))
                    {
                        WsdlEndpointConversionContext context2;
                        if (flag)
                        {
                            context2 = new WsdlEndpointConversionContext(contractContext, endpoint, wsdlBinding, port);
                            foreach (OperationDescription description in endpoint.Contract.Operations)
                            {
                                if (OperationIsExportable(description))
                                {
                                    OperationBinding bindingOperation = this.CreateWsdlOperationBinding(endpoint.Contract, description);
                                    wsdlBinding.Operations.Add(bindingOperation);
                                    context2.AddOperationBinding(description, bindingOperation);
                                    foreach (MessageDescription description2 in description.Messages)
                                    {
                                        MessageBinding wsdlMessageBinding = this.CreateWsdlMessageBinding(description2, endpoint.Binding, bindingOperation);
                                        context2.AddMessageBinding(description2, wsdlMessageBinding);
                                    }
                                    foreach (FaultDescription description3 in description.Faults)
                                    {
                                        FaultBinding wsdlFaultBinding = this.CreateWsdlFaultBinding(description3, endpoint.Binding, bindingOperation);
                                        context2.AddFaultBinding(description3, wsdlFaultBinding);
                                    }
                                }
                            }
                            PolicyConversionContext policyContext = base.ExportPolicy(endpoint);
                            new WSPolicyAttachmentHelper(base.PolicyVersion).AttachPolicy(endpoint, context2, policyContext);
                            this.exportedBindings.Add(new BindingDictionaryKey(endpoint.Contract, endpoint.Binding), context2);
                        }
                        else
                        {
                            context2 = new WsdlEndpointConversionContext(this.exportedBindings[new BindingDictionaryKey(endpoint.Contract, endpoint.Binding)], endpoint, port);
                        }
                        this.CallExportEndpoint(context2);
                        this.exportedEndpoints.Add(key, endpoint);
                        if (flag2)
                        {
                            base.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("WarnDuplicateBindingQNameNameOnExport", new object[] { endpoint.Binding.Name, endpoint.Binding.Namespace, endpoint.Contract.Name }), true));
                        }
                    }
                }
            }
            catch
            {
                this.isFaulted = true;
                throw;
            }
        }

        public void ExportEndpoints(IEnumerable<ServiceEndpoint> endpoints, XmlQualifiedName wsdlServiceQName)
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlExporterIsFaulted")));
            }
            if (endpoints == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoints");
            }
            if (wsdlServiceQName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsdlServiceQName");
            }
            foreach (ServiceEndpoint endpoint in endpoints)
            {
                this.ExportEndpoint(endpoint, wsdlServiceQName);
            }
        }

        internal static XmlSchemaSet GetEmptySchemaSet()
        {
            return new XmlSchemaSet { XmlResolver = null };
        }

        public override MetadataSet GetGeneratedMetadata()
        {
            MetadataSet set = new MetadataSet();
            foreach (System.Web.Services.Description.ServiceDescription description in this.wsdlDocuments)
            {
                set.MetadataSections.Add(MetadataSection.CreateFromServiceDescription(description));
            }
            foreach (System.Xml.Schema.XmlSchema schema in this.xmlSchemas.Schemas())
            {
                set.MetadataSections.Add(MetadataSection.CreateFromSchema(schema));
            }
            return set;
        }

        internal System.Web.Services.Description.ServiceDescription GetOrCreateWsdl(string ns)
        {
            ServiceDescriptionCollection wsdlDocuments = this.wsdlDocuments;
            System.Web.Services.Description.ServiceDescription serviceDescription = wsdlDocuments[ns];
            if (serviceDescription == null)
            {
                serviceDescription = new System.Web.Services.Description.ServiceDescription {
                    TargetNamespace = ns
                };
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(new WsdlNamespaceHelper(base.PolicyVersion).SerializerNamespaces);
                if (!string.IsNullOrEmpty(serviceDescription.TargetNamespace))
                {
                    namespaces.Add("tns", serviceDescription.TargetNamespace);
                }
                serviceDescription.Namespaces = namespaces;
                wsdlDocuments.Add(serviceDescription);
            }
            return serviceDescription;
        }

        private Service GetOrCreateWsdlService(XmlQualifiedName wsdlServiceQName)
        {
            System.Web.Services.Description.ServiceDescription orCreateWsdl = this.GetOrCreateWsdl(wsdlServiceQName.Namespace);
            Service service = orCreateWsdl.Services[wsdlServiceQName.Name];
            if (service == null)
            {
                service = new Service {
                    Name = wsdlServiceQName.Name
                };
                if (string.IsNullOrEmpty(orCreateWsdl.Name))
                {
                    orCreateWsdl.Name = service.Name;
                }
                orCreateWsdl.Services.Add(service);
            }
            return service;
        }

        internal static bool IsBuiltInOperationBehavior(IWsdlExportExtension extension)
        {
            DataContractSerializerOperationBehavior behavior = extension as DataContractSerializerOperationBehavior;
            if (behavior != null)
            {
                return behavior.IsBuiltInOperationBehavior;
            }
            XmlSerializerOperationBehavior behavior2 = extension as XmlSerializerOperationBehavior;
            return ((behavior2 != null) && behavior2.IsBuiltInOperationBehavior);
        }

        private static bool IsWsdlExportable(System.ServiceModel.Channels.Binding binding)
        {
            BindingElementCollection elements = binding.CreateBindingElements();
            if (elements != null)
            {
                foreach (BindingElement element in elements)
                {
                    MessageEncodingBindingElement element2 = element as MessageEncodingBindingElement;
                    if ((element2 != null) && !element2.IsWsdlExportable)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void LogExportWarning(string warningMessage)
        {
            base.Errors.Add(new MetadataConversionError(warningMessage, true));
        }

        internal static bool OperationIsExportable(OperationDescription operation)
        {
            for (int i = 0; i < operation.Messages.Count; i++)
            {
                if (operation.Messages[i].Action == "*")
                {
                    return false;
                }
            }
            return true;
        }

        private Exception ThrowExtensionException(ContractDescription contract, IWsdlExportExtension exporter, Exception e)
        {
            string str = new XmlQualifiedName(contract.Name, contract.Namespace).ToString();
            return new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlExtensionContractExportError", new object[] { exporter.GetType(), str }), e);
        }

        private Exception ThrowExtensionException(ServiceEndpoint endpoint, IWsdlExportExtension exporter, Exception e)
        {
            string str;
            if ((endpoint.Address != null) && (endpoint.Address.Uri != null))
            {
                str = endpoint.Address.Uri.ToString();
            }
            else
            {
                str = string.Format(CultureInfo.InvariantCulture, "Contract={1}:{0} ,Binding={3}:{2}", new object[] { endpoint.Contract.Name, endpoint.Contract.Namespace, endpoint.Binding.Name, endpoint.Binding.Namespace });
            }
            return new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlExtensionEndpointExportError", new object[] { exporter.GetType(), str }), e);
        }

        public ServiceDescriptionCollection GeneratedWsdlDocuments
        {
            get
            {
                return this.wsdlDocuments;
            }
        }

        public XmlSchemaSet GeneratedXmlSchemas
        {
            get
            {
                return this.xmlSchemas;
            }
        }

        private static XmlDocument XmlDoc
        {
            get
            {
                if (xmlDocument == null)
                {
                    System.Xml.NameTable nt = new System.Xml.NameTable();
                    nt.Add("Policy");
                    nt.Add("All");
                    nt.Add("ExactlyOne");
                    nt.Add("PolicyURIs");
                    nt.Add("Id");
                    nt.Add("UsingAddressing");
                    nt.Add("UsingAddressing");
                    nt.Add("Addressing");
                    nt.Add("AnonymousResponses");
                    nt.Add("NonAnonymousResponses");
                    xmlDocument = new XmlDocument(nt);
                }
                return xmlDocument;
            }
        }

        private sealed class BindingDictionaryKey
        {
            public readonly System.ServiceModel.Channels.Binding Binding;
            public readonly ContractDescription Contract;

            public BindingDictionaryKey(ContractDescription contract, System.ServiceModel.Channels.Binding binding)
            {
                this.Contract = contract;
                this.Binding = binding;
            }

            public override bool Equals(object obj)
            {
                WsdlExporter.BindingDictionaryKey key = obj as WsdlExporter.BindingDictionaryKey;
                return (((key != null) && (key.Binding == this.Binding)) && (key.Contract == this.Contract));
            }

            public override int GetHashCode()
            {
                return (this.Contract.GetHashCode() ^ this.Binding.GetHashCode());
            }
        }

        private sealed class EndpointDictionaryKey
        {
            public readonly ServiceEndpoint Endpoint;
            public readonly XmlQualifiedName ServiceQName;

            public EndpointDictionaryKey(ServiceEndpoint endpoint, XmlQualifiedName serviceQName)
            {
                this.Endpoint = endpoint;
                this.ServiceQName = serviceQName;
            }

            public override bool Equals(object obj)
            {
                WsdlExporter.EndpointDictionaryKey key = obj as WsdlExporter.EndpointDictionaryKey;
                return (((key != null) && (key.Endpoint == this.Endpoint)) && (key.ServiceQName == this.ServiceQName));
            }

            public override int GetHashCode()
            {
                return (this.Endpoint.GetHashCode() ^ this.ServiceQName.GetHashCode());
            }
        }

        internal static class NetSessionHelper
        {
            internal const string False = "false";
            internal const string IsInitiating = "isInitiating";
            internal const string IsTerminating = "isTerminating";
            internal const string NamespaceUri = "http://schemas.microsoft.com/ws/2005/12/wsdl/contract";
            internal const string Prefix = "msc";
            internal const string True = "true";
            internal const string UsingSession = "usingSession";

            private static void AddInitiatingAttribute(Operation wsdlOperation, bool isInitiating)
            {
                wsdlOperation.ExtensibleAttributes = CloneAndAddToAttributes(wsdlOperation.ExtensibleAttributes, "msc", "isInitiating", "http://schemas.microsoft.com/ws/2005/12/wsdl/contract", ToValue(isInitiating));
            }

            internal static void AddInitiatingTerminatingAttributesIfNeeded(Operation wsdlOperation, OperationDescription operation, ContractDescription contract)
            {
                if (contract.SessionMode == SessionMode.Required)
                {
                    AddInitiatingAttribute(wsdlOperation, operation.IsInitiating);
                    AddTerminatingAttribute(wsdlOperation, operation.IsTerminating);
                }
            }

            private static void AddTerminatingAttribute(Operation wsdlOperation, bool isTerminating)
            {
                wsdlOperation.ExtensibleAttributes = CloneAndAddToAttributes(wsdlOperation.ExtensibleAttributes, "msc", "isTerminating", "http://schemas.microsoft.com/ws/2005/12/wsdl/contract", ToValue(isTerminating));
            }

            internal static void AddUsingSessionAttributeIfNeeded(PortType wsdlPortType, ContractDescription contract)
            {
                bool flag;
                if (contract.SessionMode == SessionMode.Required)
                {
                    flag = true;
                }
                else if (contract.SessionMode == SessionMode.NotAllowed)
                {
                    flag = false;
                }
                else
                {
                    return;
                }
                wsdlPortType.ExtensibleAttributes = CloneAndAddToAttributes(wsdlPortType.ExtensibleAttributes, "msc", "usingSession", "http://schemas.microsoft.com/ws/2005/12/wsdl/contract", ToValue(flag));
            }

            private static System.Xml.XmlAttribute[] CloneAndAddToAttributes(System.Xml.XmlAttribute[] originalAttributes, string prefix, string localName, string ns, string value)
            {
                System.Xml.XmlAttribute attribute = WsdlExporter.XmlDoc.CreateAttribute(prefix, localName, ns);
                attribute.Value = value;
                int length = 0;
                if (originalAttributes != null)
                {
                    length = originalAttributes.Length;
                }
                System.Xml.XmlAttribute[] array = new System.Xml.XmlAttribute[length + 1];
                if (originalAttributes != null)
                {
                    originalAttributes.CopyTo(array, 0);
                }
                array[array.Length - 1] = attribute;
                return array;
            }

            private static string ToValue(bool b)
            {
                if (!b)
                {
                    return "false";
                }
                return "true";
            }
        }

        internal static class WSAddressingHelper
        {
            internal static void AddActionAttribute(string actionUri, OperationMessage wsdlOperationMessage, PolicyVersion policyVersion)
            {
                System.Xml.XmlAttribute attribute;
                if (policyVersion == PolicyVersion.Policy12)
                {
                    attribute = WsdlExporter.XmlDoc.CreateAttribute("wsaw", "Action", "http://www.w3.org/2006/05/addressing/wsdl");
                }
                else
                {
                    attribute = WsdlExporter.XmlDoc.CreateAttribute("wsam", "Action", "http://www.w3.org/2007/05/addressing/metadata");
                }
                attribute.Value = actionUri;
                wsdlOperationMessage.ExtensibleAttributes = new System.Xml.XmlAttribute[] { attribute };
            }

            internal static void AddAddressToWsdlPort(Port wsdlPort, EndpointAddress addr, AddressingVersion addressing)
            {
                if (addressing != AddressingVersion.None)
                {
                    MemoryStream output = new MemoryStream();
                    XmlWriter writer = XmlWriter.Create(output);
                    writer.WriteStartElement("temp");
                    if (addressing == AddressingVersion.WSAddressing10)
                    {
                        writer.WriteAttributeString("xmlns", "wsa10", null, "http://www.w3.org/2005/08/addressing");
                    }
                    else
                    {
                        if (addressing != AddressingVersion.WSAddressingAugust2004)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressing })));
                        }
                        writer.WriteAttributeString("xmlns", "wsa", null, "http://schemas.xmlsoap.org/ws/2004/08/addressing");
                    }
                    addr.WriteTo(addressing, writer);
                    writer.WriteEndElement();
                    writer.Flush();
                    output.Seek(0L, SeekOrigin.Begin);
                    XmlReader reader = XmlReader.Create(output);
                    reader.MoveToContent();
                    XmlElement extension = (XmlElement) WsdlExporter.XmlDoc.ReadNode(reader).ChildNodes[0];
                    wsdlPort.Extensions.Add(extension);
                }
            }

            internal static void AddWSAddressingAssertion(MetadataExporter exporter, PolicyConversionContext context, AddressingVersion addressVersion)
            {
                XmlElement element;
                if (addressVersion == AddressingVersion.WSAddressingAugust2004)
                {
                    element = WsdlExporter.XmlDoc.CreateElement("wsap", "UsingAddressing", "http://schemas.xmlsoap.org/ws/2004/08/addressing/policy");
                }
                else if (addressVersion == AddressingVersion.WSAddressing10)
                {
                    if (exporter.PolicyVersion == PolicyVersion.Policy12)
                    {
                        element = WsdlExporter.XmlDoc.CreateElement("wsaw", "UsingAddressing", "http://www.w3.org/2006/05/addressing/wsdl");
                    }
                    else
                    {
                        element = WsdlExporter.XmlDoc.CreateElement("wsam", "Addressing", "http://www.w3.org/2007/05/addressing/metadata");
                        SupportedAddressingMode anonymous = SupportedAddressingMode.Anonymous;
                        string name = typeof(SupportedAddressingMode).Name;
                        if (exporter.State.ContainsKey(name) && (exporter.State[name] is SupportedAddressingMode))
                        {
                            anonymous = (SupportedAddressingMode) exporter.State[name];
                            if (!SupportedAddressingModeHelper.IsDefined(anonymous))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SupportedAddressingModeNotSupported", new object[] { anonymous })));
                            }
                        }
                        if (anonymous != SupportedAddressingMode.Mixed)
                        {
                            string str2;
                            if (anonymous == SupportedAddressingMode.Anonymous)
                            {
                                str2 = "AnonymousResponses";
                            }
                            else
                            {
                                str2 = "NonAnonymousResponses";
                            }
                            XmlElement newChild = WsdlExporter.XmlDoc.CreateElement("wsp", "Policy", "http://www.w3.org/ns/ws-policy");
                            XmlElement element3 = WsdlExporter.XmlDoc.CreateElement("wsam", str2, "http://www.w3.org/2007/05/addressing/metadata");
                            newChild.AppendChild(element3);
                            element.AppendChild(newChild);
                        }
                    }
                }
                else
                {
                    if (addressVersion != AddressingVersion.None)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressVersion })));
                    }
                    element = null;
                }
                if (element != null)
                {
                    context.GetBindingAssertions().Add(element);
                }
            }
        }

        private class WsdlNamespaceHelper
        {
            private PolicyVersion policyVersion;
            private XmlSerializerNamespaces xmlSerializerNamespaces;

            internal WsdlNamespaceHelper(PolicyVersion policyVersion)
            {
                this.policyVersion = policyVersion;
            }

            internal static string FindOrCreatePrefix(string prefixBase, string ns, params DocumentableItem[] scopes)
            {
                if (scopes.Length <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "You must pass at least one namespaceScope", new object[0])));
                }
                string prefix = null;
                if (string.IsNullOrEmpty(ns))
                {
                    prefix = string.Empty;
                }
                else
                {
                    for (int i = 0; i < scopes.Length; i++)
                    {
                        if (TryMatchNamespace(scopes[i].Namespaces.ToArray(), ns, out prefix))
                        {
                            return prefix;
                        }
                    }
                    int num2 = 0;
                    prefix = prefixBase + num2.ToString(CultureInfo.InvariantCulture);
                    while (PrefixExists(scopes[0].Namespaces.ToArray(), prefix))
                    {
                        prefix = prefixBase + ++num2.ToString(CultureInfo.InvariantCulture);
                    }
                }
                scopes[0].Namespaces.Add(prefix, ns);
                return prefix;
            }

            private static bool PrefixExists(XmlQualifiedName[] prefixDefinitions, string prefix)
            {
                return Array.Exists<XmlQualifiedName>(prefixDefinitions, prefixDef => prefixDef.Name == prefix);
            }

            private static bool TryMatchNamespace(XmlQualifiedName[] prefixDefinitions, string ns, out string prefix)
            {
                string foundPrefix = null;
                Array.Find<XmlQualifiedName>(prefixDefinitions, delegate (XmlQualifiedName prefixDef) {
                    if (prefixDef.Namespace == ns)
                    {
                        foundPrefix = prefixDef.Name;
                        return true;
                    }
                    return false;
                });
                prefix = foundPrefix;
                return (foundPrefix != null);
            }

            internal XmlSerializerNamespaces SerializerNamespaces
            {
                get
                {
                    if (this.xmlSerializerNamespaces == null)
                    {
                        XmlSerializerNamespaceWrapper wrapper = new XmlSerializerNamespaceWrapper();
                        wrapper.Add("wsdl", "http://schemas.xmlsoap.org/wsdl/");
                        wrapper.Add("xsd", "http://www.w3.org/2001/XMLSchema");
                        wrapper.Add("wsp", this.policyVersion.Namespace);
                        wrapper.Add("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                        wrapper.Add("wsa", "http://schemas.xmlsoap.org/ws/2004/08/addressing");
                        wrapper.Add("wsap", "http://schemas.xmlsoap.org/ws/2004/08/addressing/policy");
                        wrapper.Add("wsa10", "http://www.w3.org/2005/08/addressing");
                        wrapper.Add("wsaw", "http://www.w3.org/2006/05/addressing/wsdl");
                        wrapper.Add("wsam", "http://www.w3.org/2007/05/addressing/metadata");
                        wrapper.Add("wsx", "http://schemas.xmlsoap.org/ws/2004/09/mex");
                        wrapper.Add("msc", "http://schemas.microsoft.com/ws/2005/12/wsdl/contract");
                        wrapper.Add("soapenc", "http://schemas.xmlsoap.org/soap/encoding/");
                        wrapper.Add("soap12", "http://schemas.xmlsoap.org/wsdl/soap12/");
                        wrapper.Add("soap", "http://schemas.xmlsoap.org/wsdl/soap/");
                        this.xmlSerializerNamespaces = wrapper.GetNamespaces();
                    }
                    return this.xmlSerializerNamespaces;
                }
            }

            private class XmlSerializerNamespaceWrapper
            {
                private readonly Dictionary<string, string> lookup = new Dictionary<string, string>();
                private readonly XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

                internal void Add(string prefix, string namespaceUri)
                {
                    if (!this.lookup.ContainsKey(prefix))
                    {
                        this.namespaces.Add(prefix, namespaceUri);
                        this.lookup.Add(prefix, namespaceUri);
                    }
                }

                internal XmlSerializerNamespaces GetNamespaces()
                {
                    return this.namespaces;
                }
            }
        }

        internal static class WsdlNamingHelper
        {
            internal static XmlQualifiedName GetBindingQName(ServiceEndpoint endpoint, WsdlExporter exporter, out bool wasUniquified)
            {
                string name = endpoint.Name;
                string bindingWsdlNamespace = endpoint.Binding.Namespace;
                string str3 = NamingHelper.GetUniqueName(name, WsdlBindingQNameExists(exporter, bindingWsdlNamespace), null);
                wasUniquified = name != str3;
                return new XmlQualifiedName(str3, bindingWsdlNamespace);
            }

            internal static string GetPortName(ServiceEndpoint endpoint, Service wsdlService)
            {
                return NamingHelper.GetUniqueName(endpoint.Name, ServiceContainsPort(wsdlService), null);
            }

            internal static XmlQualifiedName GetPortTypeQName(ContractDescription contract)
            {
                return new XmlQualifiedName(contract.Name, contract.Namespace);
            }

            internal static string GetWsdlOperationName(OperationDescription operationDescription, ContractDescription parentContractDescription)
            {
                return operationDescription.Name;
            }

            private static NamingHelper.DoesNameExist ServiceContainsPort(Service service)
            {
                return delegate (string portName, object nameCollection) {
                    foreach (Port port in service.Ports)
                    {
                        if (port.Name == portName)
                        {
                            return true;
                        }
                    }
                    return false;
                };
            }

            private static NamingHelper.DoesNameExist WsdlBindingQNameExists(WsdlExporter exporter, string bindingWsdlNamespace)
            {
                return delegate (string localName, object nameCollection) {
                    new XmlQualifiedName(localName, bindingWsdlNamespace);
                    System.Web.Services.Description.ServiceDescription description = exporter.wsdlDocuments[bindingWsdlNamespace];
                    return ((description != null) && (description.Bindings[localName] != null));
                };
            }
        }

        private class WSPolicyAttachmentHelper
        {
            private PolicyVersion policyVersion;

            internal WSPolicyAttachmentHelper(PolicyVersion policyVersion)
            {
                this.policyVersion = policyVersion;
            }

            private void AttachItemPolicy(ICollection<XmlElement> assertions, string key, System.Web.Services.Description.ServiceDescription policyWsdl, DocumentableItem item)
            {
                string policyKey = this.InsertPolicy(key, policyWsdl, assertions);
                this.InsertPolicyReference(policyKey, item);
            }

            internal void AttachPolicy(ServiceEndpoint endpoint, WsdlEndpointConversionContext endpointContext, PolicyConversionContext policyContext)
            {
                string str;
                SortedList<string, string> policyKeys = new SortedList<string, string>();
                NamingHelper.DoesNameExist doesNameExist = (name, nameCollection) => policyKeys.ContainsKey(name);
                System.Web.Services.Description.ServiceDescription serviceDescription = endpointContext.WsdlBinding.ServiceDescription;
                ICollection<XmlElement> bindingAssertions = policyContext.GetBindingAssertions();
                System.Web.Services.Description.Binding wsdlBinding = endpointContext.WsdlBinding;
                if (bindingAssertions.Count > 0)
                {
                    str = NamingHelper.GetUniqueName(CreateBindingPolicyKey(wsdlBinding), doesNameExist, null);
                    policyKeys.Add(str, str);
                    this.AttachItemPolicy(bindingAssertions, str, serviceDescription, wsdlBinding);
                }
                foreach (OperationDescription description2 in endpoint.Contract.Operations)
                {
                    if (WsdlExporter.OperationIsExportable(description2))
                    {
                        bindingAssertions = policyContext.GetOperationBindingAssertions(description2);
                        if (bindingAssertions.Count > 0)
                        {
                            OperationBinding operationBinding = endpointContext.GetOperationBinding(description2);
                            str = NamingHelper.GetUniqueName(CreateOperationBindingPolicyKey(operationBinding), doesNameExist, null);
                            policyKeys.Add(str, str);
                            this.AttachItemPolicy(bindingAssertions, str, serviceDescription, operationBinding);
                        }
                        foreach (MessageDescription description3 in description2.Messages)
                        {
                            bindingAssertions = policyContext.GetMessageBindingAssertions(description3);
                            if (bindingAssertions.Count > 0)
                            {
                                MessageBinding messageBinding = endpointContext.GetMessageBinding(description3);
                                str = NamingHelper.GetUniqueName(CreateMessageBindingPolicyKey(messageBinding, description3.Direction), doesNameExist, null);
                                policyKeys.Add(str, str);
                                this.AttachItemPolicy(bindingAssertions, str, serviceDescription, messageBinding);
                            }
                        }
                        foreach (FaultDescription description4 in description2.Faults)
                        {
                            bindingAssertions = policyContext.GetFaultBindingAssertions(description4);
                            if (bindingAssertions.Count > 0)
                            {
                                FaultBinding faultBinding = endpointContext.GetFaultBinding(description4);
                                str = NamingHelper.GetUniqueName(CreateFaultBindingPolicyKey(faultBinding), doesNameExist, null);
                                policyKeys.Add(str, str);
                                this.AttachItemPolicy(bindingAssertions, str, serviceDescription, faultBinding);
                            }
                        }
                    }
                }
            }

            private static string CreateBindingPolicyKey(System.Web.Services.Description.Binding wsdlBinding)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}_policy", new object[] { wsdlBinding.Name });
            }

            private static string CreateFaultBindingPolicyKey(FaultBinding wsdlFaultBinding)
            {
                OperationBinding operationBinding = wsdlFaultBinding.OperationBinding;
                System.Web.Services.Description.Binding binding = operationBinding.Binding;
                if (string.IsNullOrEmpty(wsdlFaultBinding.Name))
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_Fault", new object[] { binding.Name, operationBinding.Name });
                }
                return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_Fault", new object[] { binding.Name, operationBinding.Name, wsdlFaultBinding.Name });
            }

            private static string CreateMessageBindingPolicyKey(MessageBinding wsdlMessageBinding, MessageDirection direction)
            {
                OperationBinding operationBinding = wsdlMessageBinding.OperationBinding;
                System.Web.Services.Description.Binding binding = operationBinding.Binding;
                if (direction == MessageDirection.Input)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_Input_policy", new object[] { binding.Name, operationBinding.Name });
                }
                return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_output_policy", new object[] { binding.Name, operationBinding.Name });
            }

            private static string CreateOperationBindingPolicyKey(OperationBinding wsdlOperationBinding)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_policy", new object[] { wsdlOperationBinding.Binding.Name, wsdlOperationBinding.Name });
            }

            private XmlElement CreatePolicyElement(ICollection<XmlElement> assertions)
            {
                XmlElement element = WsdlExporter.XmlDoc.CreateElement("wsp", "Policy", this.policyVersion.Namespace);
                XmlElement newChild = WsdlExporter.XmlDoc.CreateElement("wsp", "ExactlyOne", this.policyVersion.Namespace);
                element.AppendChild(newChild);
                XmlElement element3 = WsdlExporter.XmlDoc.CreateElement("wsp", "All", this.policyVersion.Namespace);
                newChild.AppendChild(element3);
                foreach (XmlElement element4 in assertions)
                {
                    System.Xml.XmlNode node = WsdlExporter.XmlDoc.ImportNode(element4, true);
                    element3.AppendChild(node);
                }
                return element;
            }

            private string InsertPolicy(string key, System.Web.Services.Description.ServiceDescription policyWsdl, ICollection<XmlElement> assertions)
            {
                XmlElement extension = this.CreatePolicyElement(assertions);
                System.Xml.XmlAttribute newAttr = WsdlExporter.XmlDoc.CreateAttribute("wsu", "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                newAttr.Value = key;
                extension.SetAttributeNode(newAttr);
                if (policyWsdl != null)
                {
                    policyWsdl.Extensions.Add(extension);
                }
                return string.Format(CultureInfo.InvariantCulture, "#{0}", new object[] { key });
            }

            private void InsertPolicyReference(string policyKey, DocumentableItem item)
            {
                XmlElement extension = WsdlExporter.XmlDoc.CreateElement("wsp", "PolicyReference", this.policyVersion.Namespace);
                System.Xml.XmlAttribute node = WsdlExporter.XmlDoc.CreateAttribute("URI");
                node.Value = policyKey;
                extension.Attributes.Append(node);
                item.Extensions.Add(extension);
            }
        }
    }
}

