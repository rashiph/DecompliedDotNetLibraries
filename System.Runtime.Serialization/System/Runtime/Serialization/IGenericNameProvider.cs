namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;

    internal interface IGenericNameProvider
    {
        string GetGenericTypeName();
        string GetNamespaces();
        IList<int> GetNestedParameterCounts();
        int GetParameterCount();
        string GetParameterName(int paramIndex);

        bool ParametersFromBuiltInNamespaces { get; }
    }
}

