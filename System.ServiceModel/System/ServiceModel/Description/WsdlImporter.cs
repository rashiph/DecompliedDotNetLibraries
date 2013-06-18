namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Text;
    using System.Threading;
    using System.Web.Services.Configuration;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    public class WsdlImporter : MetadataImporter
    {
        private bool beforeImportCalled;
        private readonly Dictionary<XmlQualifiedName, WsdlEndpointConversionContext> importedBindings;
        private readonly Dictionary<Port, ServiceEndpoint> importedPorts;
        private readonly Dictionary<XmlQualifiedName, WsdlContractConversionContext> importedPortTypes;
        private readonly Dictionary<NamedItem, WsdlImportException> importErrors;
        private bool isFaulted;
        private readonly Dictionary<string, XmlElement> policyDocuments;
        private readonly Dictionary<string, string> warnings;
        private readonly ServiceDescriptionCollection wsdlDocuments;
        private readonly KeyedByTypeCollection<IWsdlImportExtension> wsdlExtensions;
        private WsdlPolicyReader wsdlPolicyReader;
        private readonly XmlSchemaSet xmlSchemas;
        private const string xPathDocumentFormatString = "//wsdl:definitions[@targetNamespace='{0}']";
        private const string xPathItemSubFormatString = "/wsdl:{0}";
        private const string xPathNamedItemSubFormatString = "/wsdl:{0}[@name='{1}']";

        public WsdlImporter(MetadataSet metadata) : this(metadata, null, null, MetadataImporterQuotas.Defaults)
        {
        }

        public WsdlImporter(MetadataSet metadata, IEnumerable<IPolicyImportExtension> policyImportExtensions, IEnumerable<IWsdlImportExtension> wsdlImportExtensions) : this(metadata, policyImportExtensions, wsdlImportExtensions, MetadataImporterQuotas.Defaults)
        {
        }

        public WsdlImporter(MetadataSet metadata, IEnumerable<IPolicyImportExtension> policyImportExtensions, IEnumerable<IWsdlImportExtension> wsdlImportExtensions, MetadataImporterQuotas quotas) : base(policyImportExtensions, quotas)
        {
            this.importErrors = new Dictionary<NamedItem, WsdlImportException>();
            this.importedPortTypes = new Dictionary<XmlQualifiedName, WsdlContractConversionContext>();
            this.importedBindings = new Dictionary<XmlQualifiedName, WsdlEndpointConversionContext>();
            this.importedPorts = new Dictionary<Port, ServiceEndpoint>();
            this.wsdlDocuments = new ServiceDescriptionCollection();
            this.xmlSchemas = WsdlExporter.GetEmptySchemaSet();
            this.policyDocuments = new Dictionary<string, XmlElement>();
            this.warnings = new Dictionary<string, string>();
            if (wsdlImportExtensions == null)
            {
                wsdlImportExtensions = LoadWsdlExtensionsFromConfig();
            }
            this.wsdlExtensions = new KeyedByTypeCollection<IWsdlImportExtension>(wsdlImportExtensions);
            if (metadata == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("metadata");
            }
            this.ProcessMetadataDocuments(metadata.MetadataSections);
        }

        private static void AddUnImportedPolicyString(StringBuilder stringBuilder, NamedItem item, IEnumerable<XmlElement> unimportdPolicy)
        {
            stringBuilder.AppendLine(System.ServiceModel.SR.GetString("UnImportedAssertionList", new object[] { CreateXPathString(item) }));
            Dictionary<XmlElement, XmlElement> dictionary = new Dictionary<XmlElement, XmlElement>();
            int num = 0;
            foreach (XmlElement element in unimportdPolicy)
            {
                if (!dictionary.ContainsKey(element))
                {
                    dictionary.Add(element, element);
                    num++;
                    if (num > 0x80)
                    {
                        stringBuilder.Append("..");
                        stringBuilder.AppendLine();
                        break;
                    }
                    WriteElement(element, stringBuilder);
                }
            }
        }

        private static void AppendUnImportedPolicyErrorMessage(ref StringBuilder unImportedPolicyMessage, WsdlEndpointConversionContext endpointContext, PolicyConversionContext policyContext)
        {
            if (unImportedPolicyMessage == null)
            {
                unImportedPolicyMessage = new StringBuilder(System.ServiceModel.SR.GetString("UnabletoImportPolicy"));
            }
            else
            {
                unImportedPolicyMessage.AppendLine();
            }
            if (policyContext.GetBindingAssertions().Count != 0)
            {
                AddUnImportedPolicyString(unImportedPolicyMessage, endpointContext.WsdlBinding, policyContext.GetBindingAssertions());
            }
            foreach (OperationDescription description in policyContext.Contract.Operations)
            {
                if (policyContext.GetOperationBindingAssertions(description).Count != 0)
                {
                    AddUnImportedPolicyString(unImportedPolicyMessage, endpointContext.GetOperationBinding(description), policyContext.GetOperationBindingAssertions(description));
                }
                foreach (MessageDescription description2 in description.Messages)
                {
                    if (policyContext.GetMessageBindingAssertions(description2).Count != 0)
                    {
                        AddUnImportedPolicyString(unImportedPolicyMessage, endpointContext.GetMessageBinding(description2), policyContext.GetMessageBindingAssertions(description2));
                    }
                }
            }
        }

        private void CallImportContract(WsdlContractConversionContext contractConversionContext)
        {
            foreach (IWsdlImportExtension extension in this.wsdlExtensions)
            {
                try
                {
                    extension.ImportContract(this, contractConversionContext);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateExtensionException(extension, exception));
                }
            }
        }

        private void CallImportEndpoint(WsdlEndpointConversionContext endpointConversionContext)
        {
            foreach (IWsdlImportExtension extension in this.wsdlExtensions)
            {
                try
                {
                    extension.ImportEndpoint(this, endpointConversionContext);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateExtensionException(extension, exception));
                }
            }
        }

        private static UnrecognizedAssertionsBindingElement CollectUnrecognizedAssertions(PolicyConversionContext policyContext, WsdlEndpointConversionContext endpointContext)
        {
            XmlQualifiedName wsdlBinding = new XmlQualifiedName(endpointContext.WsdlBinding.Name, endpointContext.WsdlBinding.ServiceDescription.TargetNamespace);
            UnrecognizedAssertionsBindingElement element = new UnrecognizedAssertionsBindingElement(wsdlBinding, policyContext.GetBindingAssertions());
            foreach (OperationDescription description in policyContext.Contract.Operations)
            {
                if (policyContext.GetOperationBindingAssertions(description).Count != 0)
                {
                    element.Add(description, policyContext.GetOperationBindingAssertions(description));
                }
                foreach (MessageDescription description2 in description.Messages)
                {
                    if (policyContext.GetMessageBindingAssertions(description2).Count != 0)
                    {
                        element.Add(description2, policyContext.GetMessageBindingAssertions(description2));
                    }
                }
            }
            return element;
        }

        private Exception CreateAlreadyFaultedException(NamedItem item)
        {
            return new AlreadyFaultedException(System.ServiceModel.SR.GetString("WsdlItemAlreadyFaulted", new object[] { GetElementName(item) }), this.importErrors[item]);
        }

        private static Exception CreateBeforeImportExtensionException(IWsdlImportExtension importer, Exception e)
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlExtensionBeforeImportError", new object[] { importer.GetType().AssemblyQualifiedName, e.Message }), e);
        }

        private System.ServiceModel.Channels.Binding CreateBinding(WsdlEndpointConversionContext endpointContext, XmlQualifiedName bindingQName)
        {
            System.ServiceModel.Channels.Binding binding2;
            try
            {
                CustomBinding binding = new CustomBinding(this.ImportPolicyFromWsdl(endpointContext)) {
                    Name = NamingHelper.CodeName(bindingQName.Name),
                    Namespace = bindingQName.Namespace
                };
                binding2 = binding;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsdlImportException.Create(endpointContext.WsdlBinding, exception));
            }
            return binding2;
        }

        private ContractDescription CreateContractDescription(PortType wsdlPortType, XmlQualifiedName wsdlPortTypeQName)
        {
            XmlQualifiedName contractName = WsdlNamingHelper.GetContractName(wsdlPortTypeQName);
            ContractDescription contractDescription = new ContractDescription(contractName.Name, contractName.Namespace);
            NetSessionHelper.SetSession(contractDescription, wsdlPortType);
            return contractDescription;
        }

        private static Exception CreateExtensionException(IWsdlImportExtension importer, Exception e)
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlExtensionImportError", new object[] { importer.GetType().FullName, e.Message }), e);
        }

        internal static IEnumerable<MetadataSection> CreateMetadataDocuments(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, IEnumerable<XmlElement> policyDocuments)
        {
            if (wsdlDocuments != null)
            {
                IEnumerator enumerator = wsdlDocuments.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    System.Web.Services.Description.ServiceDescription current = (System.Web.Services.Description.ServiceDescription) enumerator.Current;
                    yield return MetadataSection.CreateFromServiceDescription(current);
                }
            }
            if (xmlSchemas != null)
            {
                IEnumerator iteratorVariable5 = xmlSchemas.Schemas().GetEnumerator();
                while (iteratorVariable5.MoveNext())
                {
                    System.Xml.Schema.XmlSchema schema = (System.Xml.Schema.XmlSchema) iteratorVariable5.Current;
                    yield return MetadataSection.CreateFromSchema(schema);
                }
            }
            if (policyDocuments != null)
            {
                foreach (XmlElement iteratorVariable2 in policyDocuments)
                {
                    yield return MetadataSection.CreateFromPolicy(iteratorVariable2, null);
                }
            }
        }

        private OperationDescription CreateOperationDescription(PortType wsdlPortType, Operation wsdlOperation, ContractDescription contract)
        {
            OperationDescription operationDescription = new OperationDescription(WsdlNamingHelper.GetOperationName(wsdlOperation), contract);
            NetSessionHelper.SetInitiatingTerminating(operationDescription, wsdlOperation);
            contract.Operations.Add(operationDescription);
            return operationDescription;
        }

        private static string CreateXPathString(NamedItem item)
        {
            string str2;
            string str3;
            if (item == null)
            {
                return System.ServiceModel.SR.GetString("XPathUnavailable");
            }
            string name = item.Name;
            string rest = string.Empty;
            string str5 = string.Empty;
            GetXPathParameters(item, out str3, out str2, ref name, ref rest);
            string str6 = string.Format(CultureInfo.InvariantCulture, "//wsdl:definitions[@targetNamespace='{0}']", new object[] { str3 });
            if (str2 != null)
            {
                str5 = string.Format(CultureInfo.InvariantCulture, "/wsdl:{0}[@name='{1}']", new object[] { str2, name });
            }
            return (str6 + str5 + rest);
        }

        private void EnsureBeforeImportCalled()
        {
            if (!this.beforeImportCalled)
            {
                foreach (IWsdlImportExtension extension in this.wsdlExtensions)
                {
                    try
                    {
                        extension.BeforeImport(this.wsdlDocuments, this.xmlSchemas, this.policyDocuments.Values);
                    }
                    catch (Exception exception)
                    {
                        this.isFaulted = true;
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateBeforeImportExtensionException(extension, exception));
                    }
                }
                this.beforeImportCalled = true;
            }
        }

        private IEnumerable<System.Web.Services.Description.Binding> FindBindingsForContract(ContractDescription contract)
        {
            XmlQualifiedName portTypeQName = WsdlExporter.WsdlNamingHelper.GetPortTypeQName(contract);
            foreach (System.Web.Services.Description.Binding iteratorVariable1 in this.GetAllBindings())
            {
                if ((iteratorVariable1.Type.Name == portTypeQName.Name) && (iteratorVariable1.Type.Namespace == portTypeQName.Namespace))
                {
                    yield return iteratorVariable1;
                }
            }
        }

        private IEnumerable<System.Web.Services.Description.Binding> FindBindingsForPortType(PortType wsdlPortType)
        {
            foreach (System.Web.Services.Description.Binding iteratorVariable0 in this.GetAllBindings())
            {
                if ((iteratorVariable0.Type.Name == wsdlPortType.Name) && (iteratorVariable0.Type.Namespace == wsdlPortType.ServiceDescription.TargetNamespace))
                {
                    yield return iteratorVariable0;
                }
            }
        }

        private IEnumerable<Port> FindPortsForBinding(System.Web.Services.Description.Binding binding)
        {
            foreach (Port iteratorVariable0 in this.GetAllPorts())
            {
                if ((iteratorVariable0.Binding.Name == binding.Name) && (iteratorVariable0.Binding.Namespace == binding.ServiceDescription.TargetNamespace))
                {
                    yield return iteratorVariable0;
                }
            }
        }

        private IEnumerable<System.Web.Services.Description.Binding> GetAllBindings()
        {
            IEnumerator enumerator = this.WsdlDocuments.GetEnumerator();
            while (enumerator.MoveNext())
            {
                System.Web.Services.Description.ServiceDescription current = (System.Web.Services.Description.ServiceDescription) enumerator.Current;
                IEnumerator iteratorVariable4 = current.Bindings.GetEnumerator();
                while (iteratorVariable4.MoveNext())
                {
                    System.Web.Services.Description.Binding iteratorVariable1 = (System.Web.Services.Description.Binding) iteratorVariable4.Current;
                    yield return iteratorVariable1;
                }
            }
        }

        private IEnumerable<Port> GetAllPorts()
        {
            IEnumerator enumerator = this.WsdlDocuments.GetEnumerator();
            while (enumerator.MoveNext())
            {
                System.Web.Services.Description.ServiceDescription current = (System.Web.Services.Description.ServiceDescription) enumerator.Current;
                IEnumerator iteratorVariable5 = current.Services.GetEnumerator();
                while (iteratorVariable5.MoveNext())
                {
                    Service iteratorVariable1 = (Service) iteratorVariable5.Current;
                    IEnumerator iteratorVariable7 = iteratorVariable1.Ports.GetEnumerator();
                    while (iteratorVariable7.MoveNext())
                    {
                        Port iteratorVariable2 = (Port) iteratorVariable7.Current;
                        yield return iteratorVariable2;
                    }
                }
            }
        }

        private static string GetElementName(NamedItem item)
        {
            if (item is PortType)
            {
                return "wsdl:portType";
            }
            if (item is System.Web.Services.Description.Binding)
            {
                return "wsdl:binding";
            }
            if (item is System.Web.Services.Description.ServiceDescription)
            {
                return "wsdl:definitions";
            }
            if (item is Service)
            {
                return "wsdl:service";
            }
            if (item is System.Web.Services.Description.Message)
            {
                return "wsdl:message";
            }
            if (item is Operation)
            {
                return "wsdl:operation";
            }
            if (item is Port)
            {
                return "wsdl:port";
            }
            return null;
        }

        private ContractDescription GetOrImportContractDescription(XmlQualifiedName wsdlPortTypeQName, out bool wasExistingContractDescription)
        {
            ContractDescription description;
            if (!this.TryFindExistingContract(wsdlPortTypeQName, out description))
            {
                PortType portType = this.wsdlDocuments.GetPortType(wsdlPortTypeQName);
                description = this.ImportWsdlPortType(portType, WsdlPortTypeImportOptions.IgnoreExistingContracts, ErrorBehavior.RethrowExceptions);
                wasExistingContractDescription = false;
            }
            wasExistingContractDescription = true;
            return description;
        }

        private XmlQualifiedName GetUnhandledExtensionQName(object extension, NamedItem item)
        {
            XmlElement element = extension as XmlElement;
            if (element != null)
            {
                return new XmlQualifiedName(element.LocalName, element.NamespaceURI);
            }
            if (extension is ServiceDescriptionFormatExtension)
            {
                XmlFormatExtensionAttribute[] attributeArray = (XmlFormatExtensionAttribute[]) ServiceReflector.GetCustomAttributes(extension.GetType(), typeof(XmlFormatExtensionAttribute), false);
                if (attributeArray.Length > 0)
                {
                    return new XmlQualifiedName(attributeArray[0].ElementName, attributeArray[0].Namespace);
                }
            }
            WsdlImportException exception = WsdlImportException.Create(item, new InvalidOperationException(System.ServiceModel.SR.GetString("UnknownWSDLExtensionIgnored", new object[] { extension.GetType().AssemblyQualifiedName })));
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
        }

        private static void GetXPathParameters(NamedItem item, out string wsdlNs, out string localName, ref string nameValue, ref string rest)
        {
            if (item is System.Web.Services.Description.ServiceDescription)
            {
                localName = null;
                wsdlNs = ((System.Web.Services.Description.ServiceDescription) item).TargetNamespace ?? string.Empty;
            }
            if (item is PortType)
            {
                localName = "portType";
                wsdlNs = ((PortType) item).ServiceDescription.TargetNamespace ?? string.Empty;
            }
            else if (item is System.Web.Services.Description.Binding)
            {
                localName = "binding";
                wsdlNs = ((System.Web.Services.Description.Binding) item).ServiceDescription.TargetNamespace ?? string.Empty;
            }
            else if (item is System.Web.Services.Description.ServiceDescription)
            {
                localName = "definitions";
                wsdlNs = ((System.Web.Services.Description.ServiceDescription) item).TargetNamespace ?? string.Empty;
            }
            else if (item is Service)
            {
                localName = "service";
                wsdlNs = ((Service) item).ServiceDescription.TargetNamespace ?? string.Empty;
            }
            else if (item is System.Web.Services.Description.Message)
            {
                localName = "message";
                wsdlNs = ((System.Web.Services.Description.Message) item).ServiceDescription.TargetNamespace ?? string.Empty;
            }
            else if (item is Port)
            {
                Service service = ((Port) item).Service;
                localName = "service";
                nameValue = service.Name;
                wsdlNs = service.ServiceDescription.TargetNamespace ?? string.Empty;
                rest = string.Format(CultureInfo.InvariantCulture, "/wsdl:{0}[@name='{1}']", new object[] { "port", item.Name });
            }
            else if (item is Operation)
            {
                PortType portType = ((Operation) item).PortType;
                localName = "portType";
                nameValue = portType.Name;
                wsdlNs = portType.ServiceDescription.TargetNamespace ?? string.Empty;
                rest = string.Format(CultureInfo.InvariantCulture, "/wsdl:{0}[@name='{1}']", new object[] { "operation", item.Name });
            }
            else if (item is OperationBinding)
            {
                OperationBinding binding = (OperationBinding) item;
                localName = "binding";
                nameValue = binding.Binding.Name;
                wsdlNs = binding.Binding.ServiceDescription.TargetNamespace ?? string.Empty;
                rest = string.Format(CultureInfo.InvariantCulture, "/wsdl:{0}[@name='{1}']", new object[] { "operation", item.Name });
            }
            else if (item is MessageBinding)
            {
                localName = "binding";
                OperationBinding operationBinding = ((MessageBinding) item).OperationBinding;
                wsdlNs = operationBinding.Binding.ServiceDescription.TargetNamespace ?? string.Empty;
                nameValue = operationBinding.Binding.Name;
                string name = item.Name;
                string str2 = string.Empty;
                if (item is InputBinding)
                {
                    str2 = "input";
                }
                else if (item is OutputBinding)
                {
                    str2 = "output";
                }
                else if (item is FaultBinding)
                {
                    str2 = "fault";
                }
                rest = string.Format(CultureInfo.InvariantCulture, "/wsdl:{0}[@name='{1}']", new object[] { "operation", operationBinding.Name });
                if (string.IsNullOrEmpty(name))
                {
                    rest = rest + string.Format(CultureInfo.InvariantCulture, "/wsdl:{0}", new object[] { str2 });
                }
                else
                {
                    rest = rest + string.Format(CultureInfo.InvariantCulture, "/wsdl:{0}[@name='{1}']", new object[] { str2, name });
                }
            }
            else
            {
                localName = null;
                wsdlNs = null;
            }
        }

        public Collection<System.ServiceModel.Channels.Binding> ImportAllBindings()
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlImporterIsFaulted")));
            }
            this.EnsureBeforeImportCalled();
            Collection<System.ServiceModel.Channels.Binding> collection = new Collection<System.ServiceModel.Channels.Binding>();
            foreach (System.Web.Services.Description.Binding binding in this.GetAllBindings())
            {
                WsdlEndpointConversionContext context = null;
                if (!this.IsBlackListed(binding))
                {
                    context = this.ImportWsdlBinding(binding, ErrorBehavior.DoNotThrowExceptions);
                    if (context != null)
                    {
                        collection.Add(context.Endpoint.Binding);
                    }
                }
            }
            return collection;
        }

        public override Collection<ContractDescription> ImportAllContracts()
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlImporterIsFaulted")));
            }
            this.EnsureBeforeImportCalled();
            Collection<ContractDescription> collection = new Collection<ContractDescription>();
            foreach (System.Web.Services.Description.ServiceDescription description in this.wsdlDocuments)
            {
                foreach (PortType type in description.PortTypes)
                {
                    if (!this.IsBlackListed(type))
                    {
                        ContractDescription item = this.ImportWsdlPortType(type, WsdlPortTypeImportOptions.ReuseExistingContracts, ErrorBehavior.DoNotThrowExceptions);
                        if (item != null)
                        {
                            collection.Add(item);
                        }
                    }
                }
            }
            return collection;
        }

        public override ServiceEndpointCollection ImportAllEndpoints()
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlImporterIsFaulted")));
            }
            this.EnsureBeforeImportCalled();
            ServiceEndpointCollection endpoints = new ServiceEndpointCollection();
            foreach (Port port in this.GetAllPorts())
            {
                if (!this.IsBlackListed(port))
                {
                    ServiceEndpoint item = this.ImportWsdlPort(port, ErrorBehavior.DoNotThrowExceptions);
                    if (item != null)
                    {
                        endpoints.Add(item);
                    }
                }
            }
            return endpoints;
        }

        public System.ServiceModel.Channels.Binding ImportBinding(System.Web.Services.Description.Binding wsdlBinding)
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlImporterIsFaulted")));
            }
            if (wsdlBinding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsdlBinding");
            }
            return this.ImportWsdlBinding(wsdlBinding, ErrorBehavior.RethrowExceptions).Endpoint.Binding;
        }

        public ContractDescription ImportContract(PortType wsdlPortType)
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlImporterIsFaulted")));
            }
            if (wsdlPortType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsdlPortType");
            }
            return this.ImportWsdlPortType(wsdlPortType, WsdlPortTypeImportOptions.ReuseExistingContracts, ErrorBehavior.RethrowExceptions);
        }

        public ServiceEndpoint ImportEndpoint(Port wsdlPort)
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlImporterIsFaulted")));
            }
            if (wsdlPort == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsdlPort");
            }
            return this.ImportWsdlPort(wsdlPort, ErrorBehavior.RethrowExceptions);
        }

        internal ServiceEndpointCollection ImportEndpoints(ContractDescription contract)
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlImporterIsFaulted")));
            }
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }
            if (!base.KnownContracts.ContainsKey(WsdlExporter.WsdlNamingHelper.GetPortTypeQName(contract)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("WsdlImporterContractMustBeInKnownContracts")));
            }
            this.EnsureBeforeImportCalled();
            ServiceEndpointCollection endpoints = new ServiceEndpointCollection();
            foreach (System.Web.Services.Description.Binding binding in this.FindBindingsForContract(contract))
            {
                if (!this.IsBlackListed(binding))
                {
                    foreach (ServiceEndpoint endpoint in this.ImportEndpoints(binding))
                    {
                        endpoints.Add(endpoint);
                    }
                }
            }
            return endpoints;
        }

        public ServiceEndpointCollection ImportEndpoints(System.Web.Services.Description.Binding wsdlBinding)
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlImporterIsFaulted")));
            }
            if (wsdlBinding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsdlBinding");
            }
            if (this.IsBlackListed(wsdlBinding))
            {
                throw this.CreateAlreadyFaultedException(wsdlBinding);
            }
            this.ImportWsdlBinding(wsdlBinding, ErrorBehavior.RethrowExceptions);
            ServiceEndpointCollection endpoints = new ServiceEndpointCollection();
            foreach (Port port in this.FindPortsForBinding(wsdlBinding))
            {
                if (!this.IsBlackListed(port))
                {
                    ServiceEndpoint item = this.ImportWsdlPort(port, ErrorBehavior.DoNotThrowExceptions);
                    if (item != null)
                    {
                        endpoints.Add(item);
                    }
                }
            }
            return endpoints;
        }

        public ServiceEndpointCollection ImportEndpoints(PortType wsdlPortType)
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlImporterIsFaulted")));
            }
            if (wsdlPortType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsdlPortType");
            }
            if (this.IsBlackListed(wsdlPortType))
            {
                throw this.CreateAlreadyFaultedException(wsdlPortType);
            }
            this.ImportWsdlPortType(wsdlPortType, WsdlPortTypeImportOptions.ReuseExistingContracts, ErrorBehavior.RethrowExceptions);
            ServiceEndpointCollection endpoints = new ServiceEndpointCollection();
            foreach (System.Web.Services.Description.Binding binding in this.FindBindingsForPortType(wsdlPortType))
            {
                if (!this.IsBlackListed(binding))
                {
                    foreach (ServiceEndpoint endpoint in this.ImportEndpoints(binding))
                    {
                        endpoints.Add(endpoint);
                    }
                }
            }
            return endpoints;
        }

        public ServiceEndpointCollection ImportEndpoints(Service wsdlService)
        {
            if (this.isFaulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlImporterIsFaulted")));
            }
            if (wsdlService == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsdlService");
            }
            this.EnsureBeforeImportCalled();
            ServiceEndpointCollection endpoints = new ServiceEndpointCollection();
            foreach (Port port in wsdlService.Ports)
            {
                if (!this.IsBlackListed(port))
                {
                    ServiceEndpoint item = this.ImportWsdlPort(port, ErrorBehavior.DoNotThrowExceptions);
                    if (item != null)
                    {
                        endpoints.Add(item);
                    }
                }
            }
            return endpoints;
        }

        private BindingElementCollection ImportPolicyFromWsdl(WsdlEndpointConversionContext endpointContext)
        {
            MetadataImporter.PolicyAlternatives policyAlternatives = this.PolicyReader.GetPolicyAlternatives(endpointContext);
            IEnumerable<PolicyConversionContext> enumerable = MetadataImporter.GetPolicyConversionContextEnumerator(endpointContext.Endpoint, policyAlternatives, base.Quotas);
            PolicyConversionContext policyContext = null;
            StringBuilder unImportedPolicyMessage = null;
            int num = 0;
            foreach (PolicyConversionContext context2 in enumerable)
            {
                if (policyContext == null)
                {
                    policyContext = context2;
                }
                if (base.TryImportPolicy(context2))
                {
                    return context2.BindingElements;
                }
                AppendUnImportedPolicyErrorMessage(ref unImportedPolicyMessage, endpointContext, context2);
                if (++num >= base.Quotas.MaxPolicyConversionContexts)
                {
                    break;
                }
            }
            if (policyContext != null)
            {
                policyContext.BindingElements.Insert(0, CollectUnrecognizedAssertions(policyContext, endpointContext));
                this.LogImportWarning(unImportedPolicyMessage.ToString());
                return policyContext.BindingElements;
            }
            if (endpointContext.WsdlPort != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsdlImportException.Create(endpointContext.WsdlPort, new InvalidOperationException(System.ServiceModel.SR.GetString("NoUsablePolicyAssertions"))));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsdlImportException.Create(endpointContext.WsdlBinding, new InvalidOperationException(System.ServiceModel.SR.GetString("NoUsablePolicyAssertions"))));
        }

        private WsdlEndpointConversionContext ImportWsdlBinding(System.Web.Services.Description.Binding wsdlBinding, ErrorBehavior errorBehavior)
        {
            if (this.IsBlackListed(wsdlBinding))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateAlreadyFaultedException(wsdlBinding));
            }
            XmlQualifiedName key = new XmlQualifiedName(wsdlBinding.Name, wsdlBinding.ServiceDescription.TargetNamespace);
            WsdlEndpointConversionContext context = null;
            if (!this.importedBindings.TryGetValue(key, out context))
            {
                this.EnsureBeforeImportCalled();
                try
                {
                    bool flag;
                    ContractDescription orImportContractDescription = this.GetOrImportContractDescription(wsdlBinding.Type, out flag);
                    WsdlContractConversionContext context2 = null;
                    this.importedPortTypes.TryGetValue(wsdlBinding.Type, out context2);
                    ServiceEndpoint endpoint = new ServiceEndpoint(orImportContractDescription);
                    context = new WsdlEndpointConversionContext(context2, endpoint, wsdlBinding, null);
                    foreach (OperationBinding binding in wsdlBinding.Operations)
                    {
                        try
                        {
                            OperationDescription operationDescription = Binding2DescriptionHelper.FindOperationDescription(binding, this.wsdlDocuments, context);
                            context.AddOperationBinding(operationDescription, binding);
                            for (int i = 0; i < operationDescription.Messages.Count; i++)
                            {
                                MessageDescription message = operationDescription.Messages[i];
                                MessageBinding wsdlMessageBinding = Binding2DescriptionHelper.FindMessageBinding(binding, message);
                                context.AddMessageBinding(message, wsdlMessageBinding);
                            }
                            foreach (FaultDescription description4 in operationDescription.Faults)
                            {
                                FaultBinding wsdlFaultBinding = Binding2DescriptionHelper.FindFaultBinding(binding, description4);
                                if (wsdlFaultBinding != null)
                                {
                                    context.AddFaultBinding(description4, wsdlFaultBinding);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsdlImportException.Create(binding, exception));
                        }
                    }
                    XmlQualifiedName bindingName = WsdlNamingHelper.GetBindingName(wsdlBinding);
                    endpoint.Binding = this.CreateBinding(context, bindingName);
                    this.CallImportEndpoint(context);
                    this.VerifyImportedWsdlBinding(wsdlBinding);
                    this.importedBindings.Add(key, context);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    WsdlImportException wie = WsdlImportException.Create(wsdlBinding, exception2);
                    this.LogImportError(wsdlBinding, wie);
                    if (errorBehavior == ErrorBehavior.RethrowExceptions)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(wie);
                    }
                    return null;
                }
            }
            return context;
        }

        private ServiceEndpoint ImportWsdlPort(Port wsdlPort, ErrorBehavior errorBehavior)
        {
            if (this.IsBlackListed(wsdlPort))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateAlreadyFaultedException(wsdlPort));
            }
            ServiceEndpoint endpoint = null;
            if (!this.importedPorts.TryGetValue(wsdlPort, out endpoint))
            {
                this.EnsureBeforeImportCalled();
                try
                {
                    System.Web.Services.Description.Binding wsdlBinding = this.wsdlDocuments.GetBinding(wsdlPort.Binding);
                    WsdlEndpointConversionContext bindingContext = this.ImportWsdlBinding(wsdlBinding, ErrorBehavior.RethrowExceptions);
                    endpoint = new ServiceEndpoint(bindingContext.Endpoint.Contract) {
                        Name = WsdlNamingHelper.GetEndpointName(wsdlPort).EncodedName
                    };
                    WsdlEndpointConversionContext endpointContext = new WsdlEndpointConversionContext(bindingContext, endpoint, wsdlPort);
                    if (WsdlPolicyReader.HasPolicy(wsdlPort))
                    {
                        XmlQualifiedName bindingName = WsdlNamingHelper.GetBindingName(wsdlPort);
                        endpoint.Binding = this.CreateBinding(endpointContext, bindingName);
                    }
                    else
                    {
                        endpoint.Binding = bindingContext.Endpoint.Binding;
                    }
                    this.CallImportEndpoint(endpointContext);
                    this.VerifyImportedWsdlPort(wsdlPort);
                    this.importedPorts.Add(wsdlPort, endpoint);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    WsdlImportException wie = WsdlImportException.Create(wsdlPort, exception);
                    this.LogImportError(wsdlPort, wie);
                    if (errorBehavior == ErrorBehavior.RethrowExceptions)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(wie);
                    }
                    return null;
                }
            }
            return endpoint;
        }

        private ContractDescription ImportWsdlPortType(PortType wsdlPortType, WsdlPortTypeImportOptions importOptions, ErrorBehavior errorBehavior)
        {
            if (this.IsBlackListed(wsdlPortType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateAlreadyFaultedException(wsdlPortType));
            }
            XmlQualifiedName wsdlPortTypeQName = new XmlQualifiedName(wsdlPortType.Name, wsdlPortType.ServiceDescription.TargetNamespace);
            ContractDescription existingContract = null;
            if ((importOptions == WsdlPortTypeImportOptions.IgnoreExistingContracts) || !this.TryFindExistingContract(wsdlPortTypeQName, out existingContract))
            {
                this.EnsureBeforeImportCalled();
                try
                {
                    existingContract = this.CreateContractDescription(wsdlPortType, wsdlPortTypeQName);
                    WsdlContractConversionContext contractConversionContext = new WsdlContractConversionContext(existingContract, wsdlPortType);
                    foreach (Operation operation in wsdlPortType.Operations)
                    {
                        OperationDescription operationDescription = this.CreateOperationDescription(wsdlPortType, operation, existingContract);
                        contractConversionContext.AddOperation(operationDescription, operation);
                        foreach (OperationMessage message in operation.Messages)
                        {
                            MessageDescription description3;
                            if (TryCreateMessageDescription(message, operationDescription, out description3))
                            {
                                contractConversionContext.AddMessage(description3, message);
                            }
                        }
                        foreach (OperationFault fault in operation.Faults)
                        {
                            FaultDescription description4;
                            if (TryCreateFaultDescription(fault, operationDescription, out description4))
                            {
                                contractConversionContext.AddFault(description4, fault);
                            }
                        }
                    }
                    this.CallImportContract(contractConversionContext);
                    this.VerifyImportedWsdlPortType(wsdlPortType);
                    this.importedPortTypes.Add(wsdlPortTypeQName, contractConversionContext);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    WsdlImportException wie = WsdlImportException.Create(wsdlPortType, exception);
                    this.LogImportError(wsdlPortType, wie);
                    if (errorBehavior == ErrorBehavior.RethrowExceptions)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(wie);
                    }
                    return null;
                }
            }
            return existingContract;
        }

        private bool IsBlackListed(NamedItem item)
        {
            return this.importErrors.ContainsKey(item);
        }

        private static bool IsNonSoapWsdl11BindingExtension(object ext)
        {
            return ((((ext is HttpAddressBinding) || (ext is HttpBinding)) || ((ext is HttpOperationBinding) || (ext is HttpUrlEncodedBinding))) || ((((ext is HttpUrlReplacementBinding) || (ext is MimeContentBinding)) || ((ext is MimeMultipartRelatedBinding) || (ext is System.Web.Services.Description.MimePart))) || ((ext is MimeTextBinding) || (ext is MimeXmlBinding))));
        }

        [SecuritySafeCritical]
        private static Collection<IWsdlImportExtension> LoadWsdlExtensionsFromConfig()
        {
            return ClientSection.UnsafeGetSection().Metadata.LoadWsdlImportExtensions();
        }

        private void LogImportError(NamedItem item, WsdlImportException wie)
        {
            string str;
            if ((wie.InnerException != null) && (wie.InnerException is WsdlImportException))
            {
                WsdlImportException innerException = wie.InnerException as WsdlImportException;
                string str2 = System.ServiceModel.SR.GetString("WsdlImportErrorDependencyDetail", new object[] { GetElementName(innerException.SourceItem), GetElementName(item), CreateXPathString(innerException.SourceItem) });
                str = System.ServiceModel.SR.GetString("WsdlImportErrorMessageDetail", new object[] { GetElementName(item), CreateXPathString(wie.SourceItem), str2 });
            }
            else
            {
                str = System.ServiceModel.SR.GetString("WsdlImportErrorMessageDetail", new object[] { GetElementName(item), CreateXPathString(wie.SourceItem), wie.Message });
            }
            this.importErrors.Add(item, wie);
            base.Errors.Add(new MetadataConversionError(str, false));
        }

        private void LogImportWarning(string warningMessage)
        {
            if (!this.warnings.ContainsKey(warningMessage))
            {
                if (this.warnings.Count >= 0x400)
                {
                    this.warnings.Clear();
                }
                this.warnings.Add(warningMessage, warningMessage);
                base.Errors.Add(new MetadataConversionError(warningMessage, true));
            }
        }

        private void ProcessMetadataDocuments(IEnumerable<MetadataSection> metadataSections)
        {
            foreach (MetadataSection section in metadataSections)
            {
                try
                {
                    if (!(section.Metadata is MetadataReference) && !(section.Metadata is MetadataLocation))
                    {
                        if (section.Dialect == MetadataSection.ServiceDescriptionDialect)
                        {
                            this.wsdlDocuments.Add(this.TryConvert<System.Web.Services.Description.ServiceDescription>(section));
                        }
                        if (section.Dialect == MetadataSection.XmlSchemaDialect)
                        {
                            this.xmlSchemas.Add(this.TryConvert<System.Xml.Schema.XmlSchema>(section));
                        }
                        if (section.Dialect == MetadataSection.PolicyDialect)
                        {
                            if (string.IsNullOrEmpty(section.Identifier))
                            {
                                this.LogImportWarning(System.ServiceModel.SR.GetString("PolicyDocumentMustHaveIdentifier"));
                            }
                            else
                            {
                                this.policyDocuments.Add(section.Identifier, this.TryConvert<XmlElement>(section));
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(section.Identifier, exception));
                }
            }
        }

        internal override XmlElement ResolvePolicyReference(string policyReference, XmlElement contextAssertion)
        {
            return this.PolicyReader.ResolvePolicyReference(policyReference, contextAssertion);
        }

        private T TryConvert<T>(MetadataSection doc)
        {
            T metadata;
            try
            {
                metadata = (T) doc.Metadata;
            }
            catch (InvalidCastException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SFxBadMetadataDialect", new object[] { doc.Identifier, doc.Dialect, typeof(T).FullName, doc.GetType().FullName })));
            }
            return metadata;
        }

        private static bool TryCreateFaultDescription(OperationFault wsdlOperationFault, OperationDescription operationDescription, out FaultDescription faultDescription)
        {
            if (string.IsNullOrEmpty(wsdlOperationFault.Name))
            {
                faultDescription = null;
                return false;
            }
            string wsaActionUri = WSAddressingHelper.GetWsaActionUri(wsdlOperationFault);
            faultDescription = new FaultDescription(wsaActionUri);
            faultDescription.SetNameOnly(new System.ServiceModel.Description.XmlName(wsdlOperationFault.Name, true));
            operationDescription.Faults.Add(faultDescription);
            return true;
        }

        private static bool TryCreateMessageDescription(OperationMessage wsdlOperationMessage, OperationDescription operationDescription, out MessageDescription messageDescription)
        {
            MessageDirection input;
            string wsaActionUri = WSAddressingHelper.GetWsaActionUri(wsdlOperationMessage);
            if (wsdlOperationMessage is OperationInput)
            {
                input = MessageDirection.Input;
            }
            else if (wsdlOperationMessage is OperationOutput)
            {
                input = MessageDirection.Output;
            }
            else
            {
                messageDescription = null;
                return false;
            }
            messageDescription = new MessageDescription(wsaActionUri, input);
            messageDescription.MessageName = WsdlNamingHelper.GetOperationMessageName(wsdlOperationMessage);
            messageDescription.XsdTypeName = wsdlOperationMessage.Message;
            operationDescription.Messages.Add(messageDescription);
            return true;
        }

        private bool TryFindExistingContract(XmlQualifiedName wsdlPortTypeQName, out ContractDescription existingContract)
        {
            WsdlContractConversionContext context;
            XmlQualifiedName contractName = WsdlNamingHelper.GetContractName(wsdlPortTypeQName);
            if (base.KnownContracts.TryGetValue(contractName, out existingContract))
            {
                return true;
            }
            if (this.importedPortTypes.TryGetValue(wsdlPortTypeQName, out context))
            {
                existingContract = context.Contract;
                return true;
            }
            return false;
        }

        private void VerifyImportedExtensions(NamedItem item)
        {
            foreach (object obj2 in item.Extensions)
            {
                if (!item.Extensions.IsHandled(obj2))
                {
                    XmlQualifiedName unhandledExtensionQName = this.GetUnhandledExtensionQName(obj2, item);
                    if (item.Extensions.IsRequired(obj2) || IsNonSoapWsdl11BindingExtension(obj2))
                    {
                        string str = System.ServiceModel.SR.GetString("RequiredWSDLExtensionIgnored", new object[] { unhandledExtensionQName.Name, unhandledExtensionQName.Namespace });
                        WsdlImportException exception = WsdlImportException.Create(item, new InvalidOperationException(str));
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                    string str2 = CreateXPathString(item);
                    string message = System.ServiceModel.SR.GetString("OptionalWSDLExtensionIgnored", new object[] { unhandledExtensionQName.Name, unhandledExtensionQName.Namespace, str2 });
                    base.Errors.Add(new MetadataConversionError(message, true));
                }
            }
        }

        private void VerifyImportedWsdlBinding(System.Web.Services.Description.Binding wsdlBinding)
        {
            this.VerifyImportedExtensions(wsdlBinding);
            foreach (OperationBinding binding in wsdlBinding.Operations)
            {
                this.VerifyImportedExtensions(binding);
                if (binding.Input != null)
                {
                    this.VerifyImportedExtensions(binding.Input);
                }
                if (binding.Output != null)
                {
                    this.VerifyImportedExtensions(binding.Output);
                }
                foreach (MessageBinding binding2 in binding.Faults)
                {
                    this.VerifyImportedExtensions(binding2);
                }
            }
        }

        private void VerifyImportedWsdlPort(Port wsdlPort)
        {
            this.VerifyImportedExtensions(wsdlPort);
        }

        private void VerifyImportedWsdlPortType(PortType wsdlPortType)
        {
            this.VerifyImportedExtensions(wsdlPortType);
            foreach (Operation operation in wsdlPortType.Operations)
            {
                this.VerifyImportedExtensions(operation);
                foreach (OperationMessage message in operation.Messages)
                {
                    this.VerifyImportedExtensions(message);
                }
                foreach (OperationMessage message2 in operation.Faults)
                {
                    this.VerifyImportedExtensions(message2);
                }
            }
        }

        private static void WriteElement(XmlElement element, StringBuilder stringBuilder)
        {
            stringBuilder.Append("    <");
            stringBuilder.Append(element.Name);
            if (!string.IsNullOrEmpty(element.NamespaceURI))
            {
                stringBuilder.Append(' ');
                stringBuilder.Append("xmlns");
                if (!string.IsNullOrEmpty(element.Prefix))
                {
                    stringBuilder.Append(':');
                    stringBuilder.Append(element.Prefix);
                }
                stringBuilder.Append('=');
                stringBuilder.Append('\'');
                stringBuilder.Append(element.NamespaceURI);
                stringBuilder.Append('\'');
            }
            stringBuilder.Append(">..</");
            stringBuilder.Append(element.Name);
            stringBuilder.Append('>');
            stringBuilder.AppendLine();
        }

        private WsdlPolicyReader PolicyReader
        {
            get
            {
                if (this.wsdlPolicyReader == null)
                {
                    this.wsdlPolicyReader = new WsdlPolicyReader(this);
                }
                return this.wsdlPolicyReader;
            }
        }

        public ServiceDescriptionCollection WsdlDocuments
        {
            get
            {
                return this.wsdlDocuments;
            }
        }

        public KeyedByTypeCollection<IWsdlImportExtension> WsdlImportExtensions
        {
            get
            {
                return this.wsdlExtensions;
            }
        }

        public XmlSchemaSet XmlSchemas
        {
            get
            {
                return this.xmlSchemas;
            }
        }







        private class AlreadyFaultedException : InvalidOperationException
        {
            internal AlreadyFaultedException(string message, WsdlImporter.WsdlImportException innerException) : base(message, innerException)
            {
            }
        }

        internal static class Binding2DescriptionHelper
        {
            private static bool CompareOperations(OperationDescription operationDescription, ContractDescription parentContractDescription, OperationBinding wsdlOperationBinding)
            {
                if (WsdlExporter.WsdlNamingHelper.GetWsdlOperationName(operationDescription, parentContractDescription) != wsdlOperationBinding.Name)
                {
                    return false;
                }
                if (operationDescription.Messages.Count > 2)
                {
                    return false;
                }
                if (FindMessage(operationDescription.Messages, MessageDirection.Output) != (wsdlOperationBinding.Output != null))
                {
                    return false;
                }
                if (FindMessage(operationDescription.Messages, MessageDirection.Input) != (wsdlOperationBinding.Input != null))
                {
                    return false;
                }
                return true;
            }

            internal static FaultBinding FindFaultBinding(OperationBinding wsdlOperationBinding, FaultDescription fault)
            {
                foreach (FaultBinding binding in wsdlOperationBinding.Faults)
                {
                    if (binding.Name == fault.Name)
                    {
                        return binding;
                    }
                }
                return null;
            }

            private static bool FindMessage(MessageDescriptionCollection messageDescriptionCollection, MessageDirection transferDirection)
            {
                foreach (MessageDescription description in messageDescriptionCollection)
                {
                    if (description.Direction == transferDirection)
                    {
                        return true;
                    }
                }
                return false;
            }

            internal static MessageBinding FindMessageBinding(OperationBinding wsdlOperationBinding, MessageDescription message)
            {
                if (message.Direction == MessageDirection.Input)
                {
                    return wsdlOperationBinding.Input;
                }
                return wsdlOperationBinding.Output;
            }

            private static OperationDescription FindOperationDescription(ContractDescription contract, OperationBinding wsdlOperationBinding)
            {
                foreach (OperationDescription description in contract.Operations)
                {
                    if (CompareOperations(description, contract, wsdlOperationBinding))
                    {
                        return description;
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnableToLocateOperation2", new object[] { wsdlOperationBinding.Name, contract.Name })));
            }

            internal static OperationDescription FindOperationDescription(OperationBinding wsdlOperationBinding, ServiceDescriptionCollection wsdlDocuments, WsdlEndpointConversionContext endpointContext)
            {
                if (endpointContext.ContractConversionContext != null)
                {
                    Operation operation = FindWsdlOperation(wsdlOperationBinding, wsdlDocuments);
                    return endpointContext.ContractConversionContext.GetOperationDescription(operation);
                }
                return FindOperationDescription(endpointContext.Endpoint.Contract, wsdlOperationBinding);
            }

            private static Operation FindWsdlOperation(OperationBinding wsdlOperationBinding, ServiceDescriptionCollection wsdlDocuments)
            {
                PortType portType = wsdlDocuments.GetPortType(wsdlOperationBinding.Binding.Type);
                if (wsdlOperationBinding.Name == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidWsdlBindingOpNoName", new object[] { wsdlOperationBinding.Binding.Name })));
                }
                foreach (Operation operation in portType.Operations)
                {
                    if ((operation.Name == wsdlOperationBinding.Name) && IsOperationBoundBy(wsdlOperationBinding, operation))
                    {
                        return operation;
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidWsdlBindingOpMismatch2", new object[] { wsdlOperationBinding.Binding.Name, wsdlOperationBinding.Name })));
            }

            internal static bool IsOperationBoundBy(OperationBinding wsdlOperationBinding, Operation wsdlOperation)
            {
                foreach (OperationMessage message in wsdlOperation.Messages)
                {
                    MessageBinding input;
                    if (message is OperationInput)
                    {
                        input = wsdlOperationBinding.Input;
                    }
                    else
                    {
                        input = wsdlOperationBinding.Output;
                    }
                    if ((input == null) || (input.Name != message.Name))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private enum ErrorBehavior
        {
            RethrowExceptions,
            DoNotThrowExceptions
        }

        internal static class NetSessionHelper
        {
            private static System.Xml.XmlAttribute FindAttribute(System.Xml.XmlAttribute[] attributes, string localName, string ns)
            {
                if (attributes != null)
                {
                    foreach (System.Xml.XmlAttribute attribute in attributes)
                    {
                        if ((attribute.LocalName == localName) && (attribute.NamespaceURI == ns))
                        {
                            return attribute;
                        }
                    }
                }
                return null;
            }

            internal static void SetInitiatingTerminating(OperationDescription operationDescription, Operation wsdlOperation)
            {
                System.Xml.XmlAttribute attribute = FindAttribute(wsdlOperation.ExtensibleAttributes, "isInitiating", "http://schemas.microsoft.com/ws/2005/12/wsdl/contract");
                if (attribute != null)
                {
                    if (attribute.Value == "true")
                    {
                        operationDescription.IsInitiating = true;
                    }
                    if (attribute.Value == "false")
                    {
                        operationDescription.IsInitiating = false;
                    }
                }
                System.Xml.XmlAttribute attribute2 = FindAttribute(wsdlOperation.ExtensibleAttributes, "isTerminating", "http://schemas.microsoft.com/ws/2005/12/wsdl/contract");
                if (attribute2 != null)
                {
                    if (attribute2.Value == "true")
                    {
                        operationDescription.IsTerminating = true;
                    }
                    if (attribute2.Value == "false")
                    {
                        operationDescription.IsTerminating = false;
                    }
                }
            }

            internal static void SetSession(ContractDescription contractDescription, PortType wsdlPortType)
            {
                System.Xml.XmlAttribute attribute = FindAttribute(wsdlPortType.ExtensibleAttributes, "usingSession", "http://schemas.microsoft.com/ws/2005/12/wsdl/contract");
                if (attribute != null)
                {
                    if (attribute.Value == "true")
                    {
                        contractDescription.SessionMode = SessionMode.Required;
                    }
                    if (attribute.Value == "false")
                    {
                        contractDescription.SessionMode = SessionMode.NotAllowed;
                    }
                }
            }
        }

        internal static class SoapInPolicyWorkaroundHelper
        {
            private const string bindingAttrName = "bindingName";
            private const string bindingAttrNamespace = "bindingNamespace";
            private const string soapTransportUriKey = "TransportBindingElementImporter.TransportUri";
            private const string workaroundNS = "http://tempuri.org/temporaryworkaround";
            private static XmlDocument xmlDocument;

            private static string AddPolicyUri(System.Web.Services.Description.Binding wsdlBinding, string name)
            {
                string str = ReadPolicyUris(wsdlBinding);
                string str2 = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_BindingAdHocPolicy", new object[] { wsdlBinding.Name, name });
                string newValue = string.Format(CultureInfo.InvariantCulture, "#{0} {1}", new object[] { str2, str }).Trim();
                WritePolicyUris(wsdlBinding, newValue);
                return str2;
            }

            private static XmlElement CreatePolicyElement(string elementName, string value, XmlQualifiedName wsdlBindingQName)
            {
                XmlElement element = XmlDoc.CreateElement("wsp", "Policy", "http://schemas.xmlsoap.org/ws/2004/09/policy");
                XmlElement newChild = XmlDoc.CreateElement("wsp", "ExactlyOne", "http://schemas.xmlsoap.org/ws/2004/09/policy");
                element.AppendChild(newChild);
                XmlElement element3 = XmlDoc.CreateElement("wsp", "All", "http://schemas.xmlsoap.org/ws/2004/09/policy");
                newChild.AppendChild(element3);
                XmlElement element4 = xmlDocument.CreateElement(elementName, "http://tempuri.org/temporaryworkaround");
                element4.InnerText = value;
                System.Xml.XmlAttribute node = xmlDocument.CreateAttribute("bindingName");
                node.Value = wsdlBindingQName.Name;
                element4.Attributes.Append(node);
                System.Xml.XmlAttribute attribute2 = xmlDocument.CreateAttribute("bindingNamespace");
                attribute2.Value = wsdlBindingQName.Namespace;
                element4.Attributes.Append(attribute2);
                element4.Attributes.Append(attribute2);
                element3.AppendChild(element4);
                return element;
            }

            private static System.Xml.XmlAttribute CreatePolicyURIsAttribute(string value)
            {
                System.Xml.XmlAttribute attribute = XmlDoc.CreateAttribute("wsp", "PolicyURIs", "http://schemas.xmlsoap.org/ws/2004/09/policy");
                attribute.Value = value;
                return attribute;
            }

            private static string FindAdHocPolicy(PolicyConversionContext policyContext, string key, out XmlQualifiedName wsdlBindingQName)
            {
                if (policyContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");
                }
                XmlElement element = PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(), key, "http://tempuri.org/temporaryworkaround", true);
                if (element != null)
                {
                    wsdlBindingQName = new XmlQualifiedName(element.Attributes["bindingName"].Value, element.Attributes["bindingNamespace"].Value);
                    return element.InnerText;
                }
                wsdlBindingQName = null;
                return null;
            }

            public static string FindAdHocTransportPolicy(PolicyConversionContext policyContext, out XmlQualifiedName wsdlBindingQName)
            {
                return FindAdHocPolicy(policyContext, "TransportBindingElementImporter.TransportUri", out wsdlBindingQName);
            }

            private static void InsertAdHocPolicy(System.Web.Services.Description.Binding wsdlBinding, string value, string key)
            {
                XmlQualifiedName wsdlBindingQName = new XmlQualifiedName(wsdlBinding.Name, wsdlBinding.ServiceDescription.TargetNamespace);
                string id = AddPolicyUri(wsdlBinding, key);
                InsertPolicy(key, id, wsdlBinding.ServiceDescription, value, wsdlBindingQName);
            }

            internal static void InsertAdHocTransportPolicy(ServiceDescriptionCollection wsdlDocuments)
            {
                foreach (System.Web.Services.Description.ServiceDescription description in wsdlDocuments)
                {
                    if (description != null)
                    {
                        foreach (System.Web.Services.Description.Binding binding in description.Bindings)
                        {
                            if (WsdlImporter.WsdlPolicyReader.ContainsPolicy(binding))
                            {
                                SoapBinding binding2 = (SoapBinding) binding.Extensions.Find(typeof(SoapBinding));
                                if (binding2 != null)
                                {
                                    InsertAdHocPolicy(binding, binding2.Transport, "TransportBindingElementImporter.TransportUri");
                                }
                            }
                        }
                    }
                }
            }

            private static void InsertPolicy(string key, string id, System.Web.Services.Description.ServiceDescription policyWsdl, string value, XmlQualifiedName wsdlBindingQName)
            {
                XmlElement extension = CreatePolicyElement(key, value, wsdlBindingQName);
                System.Xml.XmlAttribute newAttr = XmlDoc.CreateAttribute("wsu", "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                newAttr.Value = id;
                extension.SetAttributeNode(newAttr);
                policyWsdl.Extensions.Add(extension);
            }

            private static string ReadPolicyUris(DocumentableItem item)
            {
                System.Xml.XmlAttribute[] extensibleAttributes = item.ExtensibleAttributes;
                if ((extensibleAttributes != null) && (extensibleAttributes.Length > 0))
                {
                    foreach (System.Xml.XmlAttribute attribute in extensibleAttributes)
                    {
                        if (MetadataImporter.PolicyHelper.IsPolicyURIs(attribute))
                        {
                            return attribute.Value;
                        }
                    }
                }
                return string.Empty;
            }

            private static void WritePolicyUris(DocumentableItem item, string newValue)
            {
                int length;
                System.Xml.XmlAttribute[] extensibleAttributes = item.ExtensibleAttributes;
                if ((extensibleAttributes != null) && (extensibleAttributes.Length > 0))
                {
                    foreach (System.Xml.XmlAttribute attribute in extensibleAttributes)
                    {
                        if (MetadataImporter.PolicyHelper.IsPolicyURIs(attribute))
                        {
                            attribute.Value = newValue;
                            return;
                        }
                    }
                    length = extensibleAttributes.Length;
                    Array.Resize<System.Xml.XmlAttribute>(ref extensibleAttributes, length + 1);
                }
                else
                {
                    length = 0;
                    extensibleAttributes = new System.Xml.XmlAttribute[1];
                }
                extensibleAttributes[length] = CreatePolicyURIsAttribute(newValue);
                item.ExtensibleAttributes = extensibleAttributes;
            }

            private static XmlDocument XmlDoc
            {
                get
                {
                    if (xmlDocument == null)
                    {
                        NameTable nt = new NameTable();
                        nt.Add("Policy");
                        nt.Add("All");
                        nt.Add("ExactlyOne");
                        nt.Add("PolicyURIs");
                        nt.Add("Id");
                        xmlDocument = new XmlDocument(nt);
                    }
                    return xmlDocument;
                }
            }
        }

        internal static class WSAddressingHelper
        {
            private static string CreateDefaultWsaActionUri(OperationMessage wsdlOperationMessage)
            {
                if (wsdlOperationMessage is OperationFault)
                {
                    return AddressingVersion.WSAddressing10.DefaultFaultAction;
                }
                string str = wsdlOperationMessage.Operation.PortType.ServiceDescription.TargetNamespace ?? string.Empty;
                string str2 = wsdlOperationMessage.Operation.PortType.Name;
                System.ServiceModel.Description.XmlName operationMessageName = WsdlImporter.WsdlNamingHelper.GetOperationMessageName(wsdlOperationMessage);
                string str3 = str.StartsWith("urn:", StringComparison.OrdinalIgnoreCase) ? ":" : "/";
                string str4 = str.EndsWith(str3, StringComparison.OrdinalIgnoreCase) ? str : (str + str3);
                return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}", new object[] { str4, str2, str3, operationMessageName.EncodedName });
            }

            internal static SupportedAddressingMode DetermineSupportedAddressingMode(MetadataImporter importer, PolicyConversionContext context)
            {
                XmlElement element = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(), "Addressing", "http://www.w3.org/2007/05/addressing/metadata", false);
                if (element != null)
                {
                    XmlElement element2 = null;
                    foreach (System.Xml.XmlNode node in element.ChildNodes)
                    {
                        if ((node is XmlElement) && MetadataSection.IsPolicyElement((XmlElement) node))
                        {
                            element2 = (XmlElement) node;
                            break;
                        }
                    }
                    if (element2 == null)
                    {
                        string message = System.ServiceModel.SR.GetString("ElementRequired", new object[] { "wsam", "Addressing", "wsp", "Policy" });
                        importer.Errors.Add(new MetadataConversionError(message, false));
                        return SupportedAddressingMode.Anonymous;
                    }
                    foreach (IEnumerable<XmlElement> enumerable2 in importer.NormalizePolicy(new XmlElement[] { element2 }))
                    {
                        foreach (XmlElement element3 in enumerable2)
                        {
                            if (element3.NamespaceURI == "http://www.w3.org/2007/05/addressing/metadata")
                            {
                                if (element3.LocalName == "NonAnonymousResponses")
                                {
                                    return SupportedAddressingMode.NonAnonymous;
                                }
                                if (element3.LocalName == "AnonymousResponses")
                                {
                                    return SupportedAddressingMode.Anonymous;
                                }
                            }
                        }
                    }
                }
                return SupportedAddressingMode.Anonymous;
            }

            internal static AddressingVersion FindAddressingVersion(PolicyConversionContext policyContext)
            {
                if (PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(), "UsingAddressing", "http://www.w3.org/2006/05/addressing/wsdl", true) != null)
                {
                    return AddressingVersion.WSAddressing10;
                }
                if (PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(), "Addressing", "http://www.w3.org/2007/05/addressing/metadata", true) != null)
                {
                    return AddressingVersion.WSAddressing10;
                }
                if (PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(), "UsingAddressing", "http://schemas.xmlsoap.org/ws/2004/08/addressing/policy", true) != null)
                {
                    return AddressingVersion.WSAddressingAugust2004;
                }
                return AddressingVersion.None;
            }

            internal static string FindWsaActionAttribute(OperationMessage wsdlOperationMessage)
            {
                System.Xml.XmlAttribute[] extensibleAttributes = wsdlOperationMessage.ExtensibleAttributes;
                if ((extensibleAttributes != null) && (extensibleAttributes.Length > 0))
                {
                    foreach (System.Xml.XmlAttribute attribute in extensibleAttributes)
                    {
                        if (((attribute.NamespaceURI == "http://www.w3.org/2006/05/addressing/wsdl") || (attribute.NamespaceURI == "http://www.w3.org/2007/05/addressing/metadata")) && (attribute.LocalName == "Action"))
                        {
                            return attribute.Value;
                        }
                    }
                }
                return null;
            }

            internal static string GetWsaActionUri(OperationMessage wsdlOperationMessage)
            {
                string str = FindWsaActionAttribute(wsdlOperationMessage);
                if (str != null)
                {
                    return str;
                }
                return CreateDefaultWsaActionUri(wsdlOperationMessage);
            }

            internal static EndpointAddress ImportAddress(Port wsdlPort)
            {
                if (wsdlPort != null)
                {
                    XmlElement node = wsdlPort.Extensions.Find("EndpointReference", "http://www.w3.org/2005/08/addressing");
                    XmlElement element2 = wsdlPort.Extensions.Find("EndpointReference", "http://schemas.xmlsoap.org/ws/2004/08/addressing");
                    SoapAddressBinding binding = (SoapAddressBinding) wsdlPort.Extensions.Find(typeof(SoapAddressBinding));
                    if (node != null)
                    {
                        return EndpointAddress.ReadFrom(AddressingVersion.WSAddressing10, new XmlNodeReader(node));
                    }
                    if (element2 != null)
                    {
                        return EndpointAddress.ReadFrom(AddressingVersion.WSAddressingAugust2004, new XmlNodeReader(element2));
                    }
                    if (binding != null)
                    {
                        return new EndpointAddress(binding.Location);
                    }
                }
                return null;
            }
        }

        private class WsdlImportException : Exception
        {
            private NamedItem sourceItem;
            private readonly string xPath;

            private WsdlImportException(NamedItem item, Exception innerException) : base(string.Empty, innerException)
            {
                this.xPath = WsdlImporter.CreateXPathString(item);
                this.sourceItem = item;
            }

            internal static WsdlImporter.WsdlImportException Create(NamedItem item, Exception innerException)
            {
                WsdlImporter.WsdlImportException exception = innerException as WsdlImporter.WsdlImportException;
                if ((exception != null) && exception.IsChildNodeOf(item))
                {
                    exception.sourceItem = item;
                    return exception;
                }
                WsdlImporter.AlreadyFaultedException exception2 = innerException as WsdlImporter.AlreadyFaultedException;
                if (exception2 != null)
                {
                    return new WsdlImporter.WsdlImportException(item, exception2.InnerException);
                }
                return new WsdlImporter.WsdlImportException(item, innerException);
            }

            internal bool IsChildNodeOf(NamedItem item)
            {
                return this.XPath.StartsWith(WsdlImporter.CreateXPathString(item), StringComparison.Ordinal);
            }

            public override string Message
            {
                get
                {
                    Exception innerException = base.InnerException;
                    while (innerException is WsdlImporter.WsdlImportException)
                    {
                        innerException = innerException.InnerException;
                    }
                    if (innerException == null)
                    {
                        return string.Empty;
                    }
                    return innerException.Message;
                }
            }

            internal NamedItem SourceItem
            {
                get
                {
                    return this.sourceItem;
                }
            }

            internal string XPath
            {
                get
                {
                    return this.xPath;
                }
            }
        }

        private static class WsdlNamingHelper
        {
            internal static XmlQualifiedName GetBindingName(System.Web.Services.Description.Binding wsdlBinding)
            {
                System.ServiceModel.Description.XmlName name = new System.ServiceModel.Description.XmlName(wsdlBinding.Name, true);
                return new XmlQualifiedName(name.EncodedName, wsdlBinding.ServiceDescription.TargetNamespace);
            }

            internal static XmlQualifiedName GetBindingName(Port wsdlPort)
            {
                System.ServiceModel.Description.XmlName name = new System.ServiceModel.Description.XmlName(string.Format(CultureInfo.InvariantCulture, "{0}_{1}", new object[] { wsdlPort.Service.Name, wsdlPort.Name }), true);
                return new XmlQualifiedName(name.EncodedName, wsdlPort.Service.ServiceDescription.TargetNamespace);
            }

            internal static XmlQualifiedName GetContractName(XmlQualifiedName wsdlPortTypeQName)
            {
                return wsdlPortTypeQName;
            }

            internal static System.ServiceModel.Description.XmlName GetEndpointName(Port wsdlPort)
            {
                return new System.ServiceModel.Description.XmlName(wsdlPort.Name, true);
            }

            internal static System.ServiceModel.Description.XmlName GetOperationMessageName(OperationMessage wsdlOperationMessage)
            {
                string name = null;
                if (!string.IsNullOrEmpty(wsdlOperationMessage.Name))
                {
                    name = wsdlOperationMessage.Name;
                }
                else if (wsdlOperationMessage.Operation.Messages.Count == 1)
                {
                    name = wsdlOperationMessage.Operation.Name;
                }
                else if (wsdlOperationMessage.Operation.Messages.IndexOf(wsdlOperationMessage) == 0)
                {
                    if (wsdlOperationMessage is OperationInput)
                    {
                        name = wsdlOperationMessage.Operation.Name + "Request";
                    }
                    else if (wsdlOperationMessage is OperationOutput)
                    {
                        name = wsdlOperationMessage.Operation.Name + "Solicit";
                    }
                }
                else if (wsdlOperationMessage.Operation.Messages.IndexOf(wsdlOperationMessage) == 1)
                {
                    name = wsdlOperationMessage.Operation.Name + "Response";
                }
                return new System.ServiceModel.Description.XmlName(name, true);
            }

            internal static string GetOperationName(Operation wsdlOperation)
            {
                return wsdlOperation.Name;
            }
        }

        internal class WsdlPolicyReader
        {
            private static readonly string[] EmptyStringArray = new string[0];
            private WsdlImporter importer;
            private WsdlPolicyDictionary policyDictionary;

            internal WsdlPolicyReader(WsdlImporter importer)
            {
                this.importer = importer;
                this.policyDictionary = new WsdlPolicyDictionary(importer);
                importer.PolicyWarningOccured += new MetadataImporter.PolicyWarningHandler(this.LogPolicyNormalizationWarning);
            }

            internal static bool ContainsPolicy(System.Web.Services.Description.Binding wsdlBinding)
            {
                if (HasPolicyAttached(wsdlBinding))
                {
                    return true;
                }
                foreach (OperationBinding binding in wsdlBinding.Operations)
                {
                    if (HasPolicyAttached(binding))
                    {
                        return true;
                    }
                    if ((binding.Input != null) && HasPolicyAttached(binding.Input))
                    {
                        return true;
                    }
                    if ((binding.Output != null) && HasPolicyAttached(binding.Output))
                    {
                        return true;
                    }
                    foreach (FaultBinding binding2 in binding.Faults)
                    {
                        if (HasPolicyAttached(binding2))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            private void CreateFaultBindingAlternatives(MetadataImporter.PolicyAlternatives policyAlternatives, System.Web.Services.Description.ServiceDescription bindingWsdl, FaultDescription fault, FaultBinding wsdlFaultBinding)
            {
                try
                {
                    IEnumerable<IEnumerable<XmlElement>> enumerable = this.GetPolicyAlternatives(wsdlFaultBinding, bindingWsdl);
                    policyAlternatives.FaultBindingAlternatives.Add(fault, enumerable);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsdlImporter.WsdlImportException.Create(wsdlFaultBinding, exception));
                }
            }

            private void CreateMessageBindingAlternatives(MetadataImporter.PolicyAlternatives policyAlternatives, System.Web.Services.Description.ServiceDescription bindingWsdl, MessageDescription message, MessageBinding wsdlMessageBinding)
            {
                try
                {
                    IEnumerable<IEnumerable<XmlElement>> enumerable = this.GetPolicyAlternatives(wsdlMessageBinding, bindingWsdl);
                    policyAlternatives.MessageBindingAlternatives.Add(message, enumerable);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsdlImporter.WsdlImportException.Create(wsdlMessageBinding, exception));
                }
            }

            internal static IEnumerable<XmlElement> GetEmbeddedPolicy(NamedItem item)
            {
                List<XmlElement> list = new List<XmlElement>();
                list.AddRange(item.Extensions.FindAll("Policy", "http://schemas.xmlsoap.org/ws/2004/09/policy"));
                list.AddRange(item.Extensions.FindAll("Policy", "http://www.w3.org/ns/ws-policy"));
                return list;
            }

            internal MetadataImporter.PolicyAlternatives GetPolicyAlternatives(WsdlEndpointConversionContext endpointContext)
            {
                MetadataImporter.PolicyAlternatives policyAlternatives = new MetadataImporter.PolicyAlternatives();
                System.Web.Services.Description.ServiceDescription serviceDescription = endpointContext.WsdlBinding.ServiceDescription;
                IEnumerable<IEnumerable<XmlElement>> xs = this.GetPolicyAlternatives(endpointContext.WsdlBinding, serviceDescription);
                if (endpointContext.WsdlPort != null)
                {
                    IEnumerable<IEnumerable<XmlElement>> ys = this.GetPolicyAlternatives(endpointContext.WsdlPort, endpointContext.WsdlPort.Service.ServiceDescription);
                    policyAlternatives.EndpointAlternatives = MetadataImporter.PolicyHelper.CrossProduct<XmlElement>(xs, ys, new MetadataImporter.YieldLimiter(this.importer.Quotas.MaxYields, this.importer));
                }
                else
                {
                    policyAlternatives.EndpointAlternatives = xs;
                }
                policyAlternatives.OperationBindingAlternatives = new Dictionary<OperationDescription, IEnumerable<IEnumerable<XmlElement>>>(endpointContext.Endpoint.Contract.Operations.Count);
                policyAlternatives.MessageBindingAlternatives = new Dictionary<MessageDescription, IEnumerable<IEnumerable<XmlElement>>>();
                policyAlternatives.FaultBindingAlternatives = new Dictionary<FaultDescription, IEnumerable<IEnumerable<XmlElement>>>();
                foreach (OperationDescription description2 in endpointContext.Endpoint.Contract.Operations)
                {
                    OperationBinding operationBinding = endpointContext.GetOperationBinding(description2);
                    try
                    {
                        IEnumerable<IEnumerable<XmlElement>> enumerable3 = this.GetPolicyAlternatives(operationBinding, serviceDescription);
                        policyAlternatives.OperationBindingAlternatives.Add(description2, enumerable3);
                        foreach (MessageDescription description3 in description2.Messages)
                        {
                            MessageBinding messageBinding = endpointContext.GetMessageBinding(description3);
                            this.CreateMessageBindingAlternatives(policyAlternatives, serviceDescription, description3, messageBinding);
                        }
                        foreach (FaultDescription description4 in description2.Faults)
                        {
                            FaultBinding faultBinding = endpointContext.GetFaultBinding(description4);
                            this.CreateFaultBindingAlternatives(policyAlternatives, serviceDescription, description4, faultBinding);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsdlImporter.WsdlImportException.Create(operationBinding, exception));
                    }
                }
                return policyAlternatives;
            }

            private IEnumerable<IEnumerable<XmlElement>> GetPolicyAlternatives(NamedItem item, System.Web.Services.Description.ServiceDescription wsdl)
            {
                Collection<XmlElement> policyAssertions = new Collection<XmlElement>();
                foreach (XmlElement element in this.GetReferencedPolicy(item, wsdl))
                {
                    policyAssertions.Add(element);
                }
                foreach (XmlElement element2 in GetEmbeddedPolicy(item))
                {
                    policyAssertions.Add(element2);
                    if (!this.policyDictionary.PolicySourceTable.ContainsKey(element2))
                    {
                        this.policyDictionary.PolicySourceTable.Add(element2, wsdl);
                    }
                }
                return this.importer.NormalizePolicy(policyAssertions);
            }

            private IEnumerable<string> GetPolicyReferenceUris(NamedItem item, string xPath)
            {
                foreach (string iteratorVariable0 in ReadPolicyUrisAttribute(item))
                {
                    yield return iteratorVariable0;
                }
                foreach (string iteratorVariable1 in this.ReadPolicyReferenceElements(item, xPath))
                {
                    yield return iteratorVariable1;
                }
            }

            private IEnumerable<XmlElement> GetReferencedPolicy(NamedItem item, System.Web.Services.Description.ServiceDescription wsdl)
            {
                string xPath = WsdlImporter.CreateXPathString(item);
                foreach (string iteratorVariable1 in this.GetPolicyReferenceUris(item, xPath))
                {
                    XmlElement iteratorVariable2 = this.policyDictionary.ResolvePolicyReference(iteratorVariable1, wsdl);
                    if (iteratorVariable2 == null)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.AppendLine(System.ServiceModel.SR.GetString("UnableToFindPolicyWithId", new object[] { iteratorVariable1 }));
                        builder.AppendLine(System.ServiceModel.SR.GetString("XPathPointer", new object[] { xPath }));
                        this.importer.LogImportWarning(builder.ToString());
                    }
                    else
                    {
                        yield return iteratorVariable2;
                    }
                }
            }

            internal static bool HasPolicy(Port wsdlPort)
            {
                return HasPolicyAttached(wsdlPort);
            }

            private static bool HasPolicyAttached(NamedItem item)
            {
                System.Xml.XmlAttribute[] extensibleAttributes = item.ExtensibleAttributes;
                if ((extensibleAttributes == null) || !Array.Exists<System.Xml.XmlAttribute>(extensibleAttributes, new Predicate<System.Xml.XmlAttribute>(MetadataImporter.PolicyHelper.IsPolicyURIs)))
                {
                    if ((item.Extensions.Find("PolicyReference", "http://schemas.xmlsoap.org/ws/2004/09/policy") != null) || (item.Extensions.Find("PolicyReference", "http://www.w3.org/ns/ws-policy") != null))
                    {
                        return true;
                    }
                    if ((item.Extensions.Find("Policy", "http://schemas.xmlsoap.org/ws/2004/09/policy") == null) && (item.Extensions.Find("Policy", "http://www.w3.org/ns/ws-policy") == null))
                    {
                        return false;
                    }
                }
                return true;
            }

            private void LogPolicyNormalizationWarning(XmlElement contextAssertion, string warningMessage)
            {
                string str = null;
                if (contextAssertion != null)
                {
                    str = this.policyDictionary.CreateIdXPath(contextAssertion);
                }
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(warningMessage);
                if (!string.IsNullOrEmpty(str))
                {
                    builder.AppendLine(System.ServiceModel.SR.GetString("XPathPointer", new object[] { str }));
                }
                else
                {
                    builder.AppendLine(System.ServiceModel.SR.GetString("XPathPointer", new object[] { System.ServiceModel.SR.GetString("XPathUnavailable") }));
                }
                this.importer.LogImportWarning(builder.ToString());
            }

            private IEnumerable<string> ReadPolicyReferenceElements(NamedItem item, string xPath)
            {
                List<XmlElement> iteratorVariable0 = new List<XmlElement>();
                iteratorVariable0.AddRange(item.Extensions.FindAll("PolicyReference", "http://schemas.xmlsoap.org/ws/2004/09/policy"));
                iteratorVariable0.AddRange(item.Extensions.FindAll("PolicyReference", "http://www.w3.org/ns/ws-policy"));
                foreach (XmlElement iteratorVariable1 in iteratorVariable0)
                {
                    string attribute = iteratorVariable1.GetAttribute("URI");
                    if (attribute == null)
                    {
                        string warningMessage = System.ServiceModel.SR.GetString("PolicyReferenceMissingURI", new object[] { "URI" });
                        this.importer.LogImportWarning(warningMessage);
                    }
                    else if (attribute == string.Empty)
                    {
                        string str2 = System.ServiceModel.SR.GetString("PolicyReferenceInvalidId");
                        this.importer.LogImportWarning(str2);
                    }
                    else
                    {
                        yield return attribute;
                    }
                }
            }

            private static string[] ReadPolicyUrisAttribute(NamedItem item)
            {
                System.Xml.XmlAttribute[] extensibleAttributes = item.ExtensibleAttributes;
                if ((extensibleAttributes != null) && (extensibleAttributes.Length > 0))
                {
                    foreach (System.Xml.XmlAttribute attribute in extensibleAttributes)
                    {
                        if (MetadataImporter.PolicyHelper.IsPolicyURIs(attribute))
                        {
                            return attribute.Value.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
                        }
                    }
                }
                return EmptyStringArray;
            }

            internal XmlElement ResolvePolicyReference(string policyReference, XmlElement contextPolicyAssertion)
            {
                return this.policyDictionary.ResolvePolicyReference(policyReference, contextPolicyAssertion);
            }




            private class WsdlPolicyDictionary
            {
                private readonly Dictionary<System.Web.Services.Description.ServiceDescription, Dictionary<string, XmlElement>> embeddedPolicyDictionary = new Dictionary<System.Web.Services.Description.ServiceDescription, Dictionary<string, XmlElement>>();
                private readonly Dictionary<string, XmlElement> externalPolicyDictionary = new Dictionary<string, XmlElement>();
                private readonly MetadataImporter importer;
                private readonly Dictionary<XmlElement, System.Web.Services.Description.ServiceDescription> policySourceTable = new Dictionary<XmlElement, System.Web.Services.Description.ServiceDescription>();
                private static readonly string wspPolicy = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[] { "wsp", "Policy" });
                private static readonly string wsuId = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[] { "wsu", "Id" });
                private static readonly string xmlId = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[] { "xml", "id" });

                internal WsdlPolicyDictionary(WsdlImporter importer)
                {
                    this.importer = importer;
                    foreach (System.Web.Services.Description.ServiceDescription description in importer.wsdlDocuments)
                    {
                        foreach (XmlElement element in WsdlImporter.WsdlPolicyReader.GetEmbeddedPolicy(description))
                        {
                            this.AddEmbeddedPolicy(importer, description, element);
                        }
                    }
                    foreach (KeyValuePair<string, XmlElement> pair in importer.policyDocuments)
                    {
                        this.AddExternalPolicy(importer, pair);
                    }
                }

                private void AddEmbeddedPolicy(WsdlImporter importer, System.Web.Services.Description.ServiceDescription wsdl, XmlElement element)
                {
                    string fragmentIdentifier = GetFragmentIdentifier(element);
                    if (string.IsNullOrEmpty(fragmentIdentifier))
                    {
                        string str2 = WsdlImporter.CreateXPathString(wsdl);
                        string warningMessage = System.ServiceModel.SR.GetString("PolicyInWsdlMustHaveFragmentId", new object[] { str2 });
                        importer.LogImportWarning(warningMessage);
                    }
                    else
                    {
                        Dictionary<string, XmlElement> dictionary;
                        if (!this.embeddedPolicyDictionary.TryGetValue(wsdl, out dictionary))
                        {
                            dictionary = new Dictionary<string, XmlElement>();
                            this.embeddedPolicyDictionary.Add(wsdl, dictionary);
                        }
                        else if (dictionary.ContainsKey(fragmentIdentifier))
                        {
                            string str4 = CreateIdXPath(wsdl, element, fragmentIdentifier);
                            string str5 = System.ServiceModel.SR.GetString("DuplicatePolicyInWsdlSkipped", new object[] { str4 });
                            importer.LogImportWarning(str5);
                            return;
                        }
                        dictionary.Add(fragmentIdentifier, element);
                        this.policySourceTable.Add(element, wsdl);
                    }
                }

                private void AddExternalPolicy(WsdlImporter importer, KeyValuePair<string, XmlElement> policyDocument)
                {
                    if ((policyDocument.Value.NamespaceURI != "http://schemas.xmlsoap.org/ws/2004/09/policy") && (policyDocument.Value.NamespaceURI != "http://www.w3.org/ns/ws-policy"))
                    {
                        string warningMessage = System.ServiceModel.SR.GetString("UnrecognizedPolicyDocumentNamespace", new object[] { policyDocument.Value.NamespaceURI });
                        importer.LogImportWarning(warningMessage);
                    }
                    else if (MetadataImporter.PolicyHelper.GetNodeType(policyDocument.Value) != MetadataImporter.PolicyHelper.NodeType.Policy)
                    {
                        string str2 = System.ServiceModel.SR.GetString("UnsupportedPolicyDocumentRoot", new object[] { policyDocument.Value.Name });
                        importer.LogImportWarning(str2);
                    }
                    else
                    {
                        string key = CreateKeyFromPolicy(policyDocument.Key, policyDocument.Value);
                        if (this.externalPolicyDictionary.ContainsKey(key))
                        {
                            string str4 = System.ServiceModel.SR.GetString("DuplicatePolicyDocumentSkipped", new object[] { key });
                            importer.LogImportWarning(str4);
                        }
                        else
                        {
                            this.externalPolicyDictionary.Add(key, policyDocument.Value);
                        }
                    }
                }

                internal string CreateIdXPath(XmlElement policyAssertion)
                {
                    System.Web.Services.Description.ServiceDescription description;
                    if (!this.policySourceTable.TryGetValue(policyAssertion, out description))
                    {
                        return null;
                    }
                    string fragmentIdentifier = GetFragmentIdentifier(policyAssertion);
                    if (string.IsNullOrEmpty(fragmentIdentifier))
                    {
                        return null;
                    }
                    return CreateIdXPath(description, policyAssertion, fragmentIdentifier);
                }

                internal static string CreateIdXPath(System.Web.Services.Description.ServiceDescription wsdl, XmlElement element, string key)
                {
                    string wsuId;
                    string str = WsdlImporter.CreateXPathString(wsdl);
                    if (element.HasAttribute("Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"))
                    {
                        wsuId = WsdlImporter.WsdlPolicyReader.WsdlPolicyDictionary.wsuId;
                    }
                    else if (element.HasAttribute("id", "http://www.w3.org/XML/1998/namespace"))
                    {
                        wsuId = xmlId;
                    }
                    else
                    {
                        return null;
                    }
                    return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/[@{2}='{3}']", new object[] { str, wspPolicy, wsuId, key });
                }

                private static string CreateKeyFromPolicy(string identifier, XmlElement policyElement)
                {
                    string fragmentIdentifier = GetFragmentIdentifier(policyElement);
                    return string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { identifier, fragmentIdentifier });
                }

                private static string GetFragmentIdentifier(XmlElement element)
                {
                    return MetadataImporter.PolicyHelper.GetFragmentIdentifier(element);
                }

                internal XmlElement ResolvePolicyReference(string policyReference, System.Web.Services.Description.ServiceDescription wsdlDocument)
                {
                    XmlElement element;
                    Dictionary<string, XmlElement> dictionary;
                    if (policyReference[0] != '#')
                    {
                        this.externalPolicyDictionary.TryGetValue(policyReference, out element);
                        return element;
                    }
                    if (!this.embeddedPolicyDictionary.TryGetValue(wsdlDocument, out dictionary))
                    {
                        return null;
                    }
                    dictionary.TryGetValue(policyReference, out element);
                    return element;
                }

                internal XmlElement ResolvePolicyReference(string policyReference, XmlElement contextPolicyAssertion)
                {
                    System.Web.Services.Description.ServiceDescription description;
                    if (policyReference[0] != '#')
                    {
                        XmlElement element;
                        this.externalPolicyDictionary.TryGetValue(policyReference, out element);
                        return element;
                    }
                    if (contextPolicyAssertion == null)
                    {
                        return null;
                    }
                    if (!this.policySourceTable.TryGetValue(contextPolicyAssertion, out description))
                    {
                        return null;
                    }
                    return this.ResolvePolicyReference(policyReference, description);
                }

                internal Dictionary<XmlElement, System.Web.Services.Description.ServiceDescription> PolicySourceTable
                {
                    get
                    {
                        return this.policySourceTable;
                    }
                }
            }
        }

        private enum WsdlPortTypeImportOptions
        {
            ReuseExistingContracts,
            IgnoreExistingContracts
        }
    }
}

