namespace System.ServiceModel.Description
{
    using System;

    internal interface IContractResolver
    {
        ContractDescription ResolveContract(string contractName);
    }
}

