namespace System.ServiceModel.Description
{
    using System;

    public interface IWsdlExportExtension
    {
        void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context);
        void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context);
    }
}

