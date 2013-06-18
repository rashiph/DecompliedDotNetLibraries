namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ContextBindingElementImporter : IPolicyImportExtension, IWsdlImportExtension
    {
        public void BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
        }

        public void ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (context.Endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context.Endpoint");
            }
            if (context.Endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context.Endpoint.Binding");
            }
            CustomBinding binding = context.Endpoint.Binding as CustomBinding;
            if (binding != null)
            {
                System.ServiceModel.Channels.Binding binding2;
                UnrecognizedAssertionsBindingElement item = null;
                item = binding.Elements.Find<UnrecognizedAssertionsBindingElement>();
                HttpTransportBindingElement element2 = null;
                if (item != null)
                {
                    XmlElement httpUseCookieAssertion = null;
                    if (ContextBindingElementPolicy.TryGetHttpUseCookieAssertion(item.BindingAsserions, out httpUseCookieAssertion))
                    {
                        foreach (BindingElement element4 in binding.Elements)
                        {
                            element2 = element4 as HttpTransportBindingElement;
                            if (element2 != null)
                            {
                                element2.AllowCookies = true;
                                item.BindingAsserions.Remove(httpUseCookieAssertion);
                                if (item.BindingAsserions.Count == 0)
                                {
                                    binding.Elements.Remove(item);
                                }
                                break;
                            }
                        }
                    }
                }
                BindingElementCollection bindingElements = binding.CreateBindingElements();
                if (!WSHttpContextBinding.TryCreate(bindingElements, out binding2) && !NetTcpContextBinding.TryCreate(bindingElements, out binding2))
                {
                    if (element2 == null)
                    {
                        foreach (BindingElement element5 in bindingElements)
                        {
                            element2 = element5 as HttpTransportBindingElement;
                            if (element2 != null)
                            {
                                break;
                            }
                        }
                    }
                    if ((element2 != null) && element2.AllowCookies)
                    {
                        element2.AllowCookies = false;
                        if (BasicHttpBinding.TryCreate(bindingElements, out binding2))
                        {
                            ((BasicHttpBinding) binding2).AllowCookies = true;
                        }
                    }
                }
                if (binding2 != null)
                {
                    binding2.Name = context.Endpoint.Binding.Name;
                    binding2.Namespace = context.Endpoint.Binding.Namespace;
                    context.Endpoint.Binding = binding2;
                }
            }
        }

        public virtual void ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            ContextBindingElement element;
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (context.BindingElements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PolicyImportContextBindingElementCollectionIsNull")));
            }
            XmlElement httpUseCookieAssertion = null;
            if (ContextBindingElementPolicy.TryImportRequireContextAssertion(context.GetBindingAssertions(), out element))
            {
                context.BindingElements.Insert(0, element);
            }
            else if (ContextBindingElementPolicy.TryGetHttpUseCookieAssertion(context.GetBindingAssertions(), out httpUseCookieAssertion))
            {
                foreach (BindingElement element3 in context.BindingElements)
                {
                    HttpTransportBindingElement element4 = element3 as HttpTransportBindingElement;
                    if (element4 != null)
                    {
                        element4.AllowCookies = true;
                        context.GetBindingAssertions().Remove(httpUseCookieAssertion);
                        break;
                    }
                }
            }
        }
    }
}

