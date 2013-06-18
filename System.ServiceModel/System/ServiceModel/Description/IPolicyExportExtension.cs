namespace System.ServiceModel.Description
{
    using System;

    public interface IPolicyExportExtension
    {
        void ExportPolicy(MetadataExporter exporter, PolicyConversionContext context);
    }
}

