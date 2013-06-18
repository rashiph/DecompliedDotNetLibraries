namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Web.Services.Description;
    using System.Xml.Schema;

    public class StandardBindingImporter : IWsdlImportExtension
    {
        private void SetBinding(ServiceEndpoint endpoint, System.ServiceModel.Channels.Binding binding)
        {
            binding.Name = endpoint.Binding.Name;
            binding.Namespace = endpoint.Binding.Namespace;
            endpoint.Binding = binding;
        }

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
        }

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext endpointContext)
        {
            if (endpointContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointContext");
            }
            if (endpointContext.Endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointContext.Binding");
            }
            if (endpointContext.Endpoint.Binding is CustomBinding)
            {
                System.ServiceModel.Channels.Binding binding;
                BindingElementCollection elements = ((CustomBinding) endpointContext.Endpoint.Binding).Elements;
                TransportBindingElement element = elements.Find<TransportBindingElement>();
                if (element is HttpTransportBindingElement)
                {
                    if (WSHttpBindingBase.TryCreate(elements, out binding))
                    {
                        this.SetBinding(endpointContext.Endpoint, binding);
                    }
                    else if (WSDualHttpBinding.TryCreate(elements, out binding))
                    {
                        this.SetBinding(endpointContext.Endpoint, binding);
                    }
                    else if (BasicHttpBinding.TryCreate(elements, out binding))
                    {
                        this.SetBinding(endpointContext.Endpoint, binding);
                    }
                }
                else if ((element is MsmqTransportBindingElement) && NetMsmqBinding.TryCreate(elements, out binding))
                {
                    this.SetBinding(endpointContext.Endpoint, binding);
                }
                else if ((element is NamedPipeTransportBindingElement) && NetNamedPipeBinding.TryCreate(elements, out binding))
                {
                    this.SetBinding(endpointContext.Endpoint, binding);
                }
                else if ((element is PeerTransportBindingElement) && NetPeerTcpBinding.TryCreate(elements, out binding))
                {
                    this.SetBinding(endpointContext.Endpoint, binding);
                }
                else if ((element is TcpTransportBindingElement) && NetTcpBinding.TryCreate(elements, out binding))
                {
                    this.SetBinding(endpointContext.Endpoint, binding);
                }
            }
        }
    }
}

