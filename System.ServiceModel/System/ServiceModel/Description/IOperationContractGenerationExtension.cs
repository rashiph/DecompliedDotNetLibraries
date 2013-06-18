namespace System.ServiceModel.Description
{
    using System;

    public interface IOperationContractGenerationExtension
    {
        void GenerateOperation(OperationContractGenerationContext context);
    }
}

