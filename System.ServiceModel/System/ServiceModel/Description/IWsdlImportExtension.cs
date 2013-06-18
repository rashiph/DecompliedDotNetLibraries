namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Web.Services.Description;
    using System.Xml.Schema;

    public interface IWsdlImportExtension
    {
        void BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy);
        void ImportContract(WsdlImporter importer, WsdlContractConversionContext context);
        void ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context);
    }
}

