namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Description;

    internal interface ITransportPolicyImport
    {
        void ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext);
    }
}

