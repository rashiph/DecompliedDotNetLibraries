namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    public class MessageEncodingBindingElementImporter : IWsdlImportExtension, IPolicyImportExtension
    {
        private static void ApplyAddressingVersion(MessageEncodingBindingElement encodingBindingElement, AddressingVersion addressingVersion)
        {
            EnvelopeVersion envelope = encodingBindingElement.MessageVersion.Envelope;
            if ((envelope == EnvelopeVersion.None) && (addressingVersion != AddressingVersion.None))
            {
                encodingBindingElement.MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap12, addressingVersion);
            }
            else
            {
                encodingBindingElement.MessageVersion = MessageVersion.CreateVersion(envelope, addressingVersion);
            }
        }

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

        private MessageEncodingBindingElement CreateEncodingBindingElement(ICollection<XmlElement> assertions, out XmlElement encodingAssertion)
        {
            encodingAssertion = null;
            foreach (XmlElement element in assertions)
            {
                string namespaceURI = element.NamespaceURI;
                if (namespaceURI == null)
                {
                    continue;
                }
                if (!(namespaceURI == "http://schemas.microsoft.com/ws/06/2004/mspolicy/netbinary1"))
                {
                    if (namespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/policy/optimizedmimeserialization")
                    {
                        goto Label_005F;
                    }
                    continue;
                }
                if (!(element.LocalName == "BinaryEncoding"))
                {
                    continue;
                }
                encodingAssertion = element;
                assertions.Remove(encodingAssertion);
                return new BinaryMessageEncodingBindingElement();
            Label_005F:
                if (element.LocalName == "OptimizedMimeSerialization")
                {
                    encodingAssertion = element;
                    assertions.Remove(encodingAssertion);
                    return new MtomMessageEncodingBindingElement();
                }
            }
            return new TextMessageEncodingBindingElement();
        }

        private static void EnsureMessageEncoding(WsdlEndpointConversionContext context, MessageEncodingBindingElement encodingBindingElement)
        {
            AddressingVersion none;
            EnvelopeVersion soapVersion = SoapHelper.GetSoapVersion(context.WsdlBinding);
            if (encodingBindingElement == null)
            {
                encodingBindingElement = new TextMessageEncodingBindingElement();
                ConvertToCustomBinding(context).Elements.Add(encodingBindingElement);
                none = AddressingVersion.None;
            }
            else if (soapVersion == EnvelopeVersion.None)
            {
                none = AddressingVersion.None;
            }
            else
            {
                none = encodingBindingElement.MessageVersion.Addressing;
            }
            MessageVersion messageVersion = MessageVersion.CreateVersion(soapVersion, none);
            if (!encodingBindingElement.MessageVersion.IsMatch(messageVersion))
            {
                ConvertToCustomBinding(context).Elements.Find<MessageEncodingBindingElement>().MessageVersion = MessageVersion.CreateVersion(soapVersion, none);
            }
        }

        private static BindingElementCollection GetBindingElements(WsdlEndpointConversionContext context)
        {
            System.ServiceModel.Channels.Binding binding = context.Endpoint.Binding;
            return ((binding is CustomBinding) ? ((CustomBinding) binding).Elements : binding.CreateBindingElements());
        }

        private static void ImportFaultSoapAction(WsdlContractConversionContext contractContext, FaultDescription fault, FaultBinding wsdlFaultBinding)
        {
            string str = SoapHelper.ReadSoapAction(wsdlFaultBinding.OperationBinding);
            if (((contractContext != null) && (WsdlImporter.WSAddressingHelper.FindWsaActionAttribute(contractContext.GetOperationFault(fault)) == null)) && (str != null))
            {
                fault.Action = str;
            }
        }

        private static void ImportMessageSoapAction(WsdlContractConversionContext contractContext, MessageDescription message, MessageBinding wsdlMessageBinding, bool isResponse)
        {
            string str = SoapHelper.ReadSoapAction(wsdlMessageBinding.OperationBinding);
            if (((contractContext != null) && (WsdlImporter.WSAddressingHelper.FindWsaActionAttribute(contractContext.GetOperationMessage(message)) == null)) && (str != null))
            {
                if (isResponse)
                {
                    message.Action = "*";
                }
                else
                {
                    message.Action = str;
                }
            }
        }

        private void ImportPolicyInternal(PolicyConversionContext context)
        {
            XmlElement element;
            context.GetBindingAssertions();
            MessageEncodingBindingElement encodingBindingElement = this.CreateEncodingBindingElement(context.GetBindingAssertions(), out element);
            AddressingVersion addressingVersion = WsdlImporter.WSAddressingHelper.FindAddressingVersion(context);
            ApplyAddressingVersion(encodingBindingElement, addressingVersion);
            context.BindingElements.Add(encodingBindingElement);
        }

        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (importer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("importer");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            this.ImportPolicyInternal(context);
        }

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
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
            MessageEncodingBindingElement encodingBindingElement = GetBindingElements(context).Find<MessageEncodingBindingElement>();
            if (encodingBindingElement != null)
            {
                System.Type type = encodingBindingElement.GetType();
                if (((type != typeof(TextMessageEncodingBindingElement)) && (type != typeof(BinaryMessageEncodingBindingElement))) && (type != typeof(MtomMessageEncodingBindingElement)))
                {
                    return;
                }
            }
            EnsureMessageEncoding(context, encodingBindingElement);
            foreach (OperationBinding binding in context.WsdlBinding.Operations)
            {
                OperationDescription operationDescription = context.GetOperationDescription(binding);
                for (int i = 0; i < operationDescription.Messages.Count; i++)
                {
                    MessageDescription message = operationDescription.Messages[i];
                    MessageBinding messageBinding = context.GetMessageBinding(message);
                    ImportMessageSoapAction(context.ContractConversionContext, message, messageBinding, i != 0);
                }
                foreach (FaultDescription description3 in operationDescription.Faults)
                {
                    FaultBinding faultBinding = context.GetFaultBinding(description3);
                    if (faultBinding != null)
                    {
                        ImportFaultSoapAction(context.ContractConversionContext, description3, faultBinding);
                    }
                }
            }
        }
    }
}

