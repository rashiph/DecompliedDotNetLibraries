namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    public class DataContractSerializerMessageContractImporter : IWsdlImportExtension
    {
        private bool enabled = true;
        internal const string GenericMessageSchemaTypeName = "MessageBody";
        internal const string GenericMessageSchemaTypeNamespace = "http://schemas.microsoft.com/Message";
        internal static XmlQualifiedName GenericMessageTypeName = new XmlQualifiedName("MessageBody", "http://schemas.microsoft.com/Message");
        private const string StreamBodySchemaTypeName = "StreamBody";
        private const string StreamBodySchemaTypeNamespace = "http://schemas.microsoft.com/Message";
        internal static XmlQualifiedName StreamBodyTypeName = new XmlQualifiedName("StreamBody", "http://schemas.microsoft.com/Message");

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext contractContext)
        {
            if (contractContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contractContext"));
            }
            if (this.enabled)
            {
                MessageContractImporter.ImportMessageContract(importer, contractContext, MessageContractImporter.DataContractSerializerSchemaImporter.Get(importer));
            }
        }

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext endpointContext)
        {
            if (endpointContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endpointContext"));
            }
            if (this.enabled)
            {
                MessageContractImporter.ImportMessageBinding(importer, endpointContext, typeof(MessageContractImporter.DataContractSerializerSchemaImporter));
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }
    }
}

