namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    public class TransportBindingElementImporter : IWsdlImportExtension, IPolicyImportExtension
    {
        private static CustomBinding ConvertToCustomBinding(WsdlEndpointConversionContext context)
        {
            CustomBinding binding = context.Endpoint.Binding as CustomBinding;
            if (binding == null)
            {
                binding = new CustomBinding(context.Endpoint.Binding);
                context.Endpoint.Binding = binding;
            }
            return binding;
        }

        private static HttpsTransportBindingElement CreateHttpsFromHttp(HttpTransportBindingElement http)
        {
            if (http == null)
            {
                return new HttpsTransportBindingElement();
            }
            return HttpsTransportBindingElement.CreateFromHttpBindingElement(http);
        }

        private static void CreateLegacyTransportBindingElement(WsdlImporter importer, SoapBinding soapBinding, WsdlEndpointConversionContext context)
        {
            TransportBindingElement item = CreateTransportBindingElements(soapBinding.Transport, null);
            if (item != null)
            {
                ConvertToCustomBinding(context).Elements.Add(item);
                StateHelper.RegisterTransportBindingElement(importer, context);
            }
        }

        private static TransportBindingElement CreateTransportBindingElements(string transportUri, PolicyConversionContext policyContext)
        {
            TransportBindingElement element = null;
            string str = transportUri;
            if (str != null)
            {
                if (!(str == "http://schemas.xmlsoap.org/soap/http"))
                {
                    if (str == "http://schemas.microsoft.com/soap/tcp")
                    {
                        return new TcpTransportBindingElement();
                    }
                    if (str == "http://schemas.microsoft.com/soap/named-pipe")
                    {
                        return new NamedPipeTransportBindingElement();
                    }
                    if (str == "http://schemas.microsoft.com/soap/msmq")
                    {
                        return new MsmqTransportBindingElement();
                    }
                    if (str != "http://schemas.microsoft.com/soap/peer")
                    {
                        return element;
                    }
                    return new PeerTransportBindingElement();
                }
                if (policyContext != null)
                {
                    WSSecurityPolicy securityPolicy = null;
                    ICollection<XmlElement> bindingAssertions = policyContext.GetBindingAssertions();
                    if (WSSecurityPolicy.TryGetSecurityPolicyDriver(bindingAssertions, out securityPolicy) && securityPolicy.ContainsWsspHttpsTokenAssertion(bindingAssertions))
                    {
                        HttpsTransportBindingElement element2 = new HttpsTransportBindingElement {
                            MessageSecurityVersion = securityPolicy.GetSupportedMessageSecurityVersion(SecurityVersion.WSSecurity11)
                        };
                        element = element2;
                    }
                }
                if (element == null)
                {
                    element = new HttpTransportBindingElement();
                }
            }
            return element;
        }

        private static BindingElementCollection GetBindingElements(WsdlEndpointConversionContext context)
        {
            System.ServiceModel.Channels.Binding binding = context.Endpoint.Binding;
            return ((binding is CustomBinding) ? ((CustomBinding) binding).Elements : binding.CreateBindingElements());
        }

        private static void ImportAddress(WsdlEndpointConversionContext context, TransportBindingElement transportBindingElement)
        {
            EndpointAddress address = context.Endpoint.Address = WsdlImporter.WSAddressingHelper.ImportAddress(context.WsdlPort);
            if (address != null)
            {
                context.Endpoint.Address = address;
                if ((address.Uri.Scheme == Uri.UriSchemeHttps) && !(transportBindingElement is HttpsTransportBindingElement))
                {
                    BindingElementCollection elements = ConvertToCustomBinding(context).Elements;
                    elements.Remove(transportBindingElement);
                    elements.Add(CreateHttpsFromHttp(transportBindingElement as HttpTransportBindingElement));
                }
            }
        }

        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            XmlQualifiedName name;
            string transportUri = WsdlImporter.SoapInPolicyWorkaroundHelper.FindAdHocTransportPolicy(policyContext, out name);
            if ((transportUri != null) && !policyContext.BindingElements.Contains(typeof(TransportBindingElement)))
            {
                TransportBindingElement item = CreateTransportBindingElements(transportUri, policyContext);
                if (item != null)
                {
                    ITransportPolicyImport import = item as ITransportPolicyImport;
                    if (import != null)
                    {
                        import.ImportPolicy(importer, policyContext);
                    }
                    policyContext.BindingElements.Add(item);
                    StateHelper.RegisterTransportBindingElement(importer, name);
                }
            }
        }

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
            WsdlImporter.SoapInPolicyWorkaroundHelper.InsertAdHocTransportPolicy(wsdlDocuments);
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
        }

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (context.Endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context.Endpoint.Binding");
            }
            TransportBindingElement transportBindingElement = GetBindingElements(context).Find<TransportBindingElement>();
            if ((transportBindingElement == null) || StateHelper.IsRegisteredTransportBindingElement(importer, context))
            {
                SoapBinding soapBinding = (SoapBinding) context.WsdlBinding.Extensions.Find(typeof(SoapBinding));
                if ((soapBinding != null) && (transportBindingElement == null))
                {
                    CreateLegacyTransportBindingElement(importer, soapBinding, context);
                }
                if (context.WsdlPort != null)
                {
                    ImportAddress(context, transportBindingElement);
                }
            }
        }

        private static class StateHelper
        {
            private static readonly object StateBagKey = new object();

            private static Dictionary<XmlQualifiedName, XmlQualifiedName> GetGeneratedTransportBindingElements(MetadataImporter importer)
            {
                object obj2;
                if (!importer.State.TryGetValue(StateBagKey, out obj2))
                {
                    obj2 = new Dictionary<XmlQualifiedName, XmlQualifiedName>();
                    importer.State.Add(StateBagKey, obj2);
                }
                return (Dictionary<XmlQualifiedName, XmlQualifiedName>) obj2;
            }

            internal static bool IsRegisteredTransportBindingElement(WsdlImporter importer, WsdlEndpointConversionContext context)
            {
                XmlQualifiedName key = new XmlQualifiedName(context.WsdlBinding.Name, context.WsdlBinding.ServiceDescription.TargetNamespace);
                return GetGeneratedTransportBindingElements(importer).ContainsKey(key);
            }

            internal static void RegisterTransportBindingElement(MetadataImporter importer, WsdlEndpointConversionContext context)
            {
                XmlQualifiedName name = new XmlQualifiedName(context.WsdlBinding.Name, context.WsdlBinding.ServiceDescription.TargetNamespace);
                GetGeneratedTransportBindingElements(importer)[name] = name;
            }

            internal static void RegisterTransportBindingElement(MetadataImporter importer, XmlQualifiedName wsdlBindingQName)
            {
                GetGeneratedTransportBindingElements(importer)[wsdlBindingQName] = wsdlBindingQName;
            }
        }
    }
}

