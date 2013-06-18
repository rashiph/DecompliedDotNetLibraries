namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Web.Services.Description;
    using System.Xml.Schema;

    public class XmlSerializerMessageContractImporter : IWsdlImportExtension
    {
        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext contractContext)
        {
            if (contractContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contractContext"));
            }
            MessageContractImporter.ImportMessageContract(importer, contractContext, MessageContractImporter.XmlSerializerSchemaImporter.Get(importer));
        }

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext endpointContext)
        {
            if (endpointContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endpointContext"));
            }
            MessageContractImporter.ImportMessageBinding(importer, endpointContext, typeof(MessageContractImporter.XmlSerializerSchemaImporter));
        }
    }
}

