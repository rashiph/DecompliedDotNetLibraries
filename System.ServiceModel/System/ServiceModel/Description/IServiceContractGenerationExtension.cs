namespace System.ServiceModel.Description
{
    using System;

    public interface IServiceContractGenerationExtension
    {
        void GenerateContract(ServiceContractGenerationContext context);
    }
}

