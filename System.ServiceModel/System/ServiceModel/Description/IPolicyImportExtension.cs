namespace System.ServiceModel.Description
{
    using System;

    public interface IPolicyImportExtension
    {
        void ImportPolicy(MetadataImporter importer, PolicyConversionContext context);
    }
}

