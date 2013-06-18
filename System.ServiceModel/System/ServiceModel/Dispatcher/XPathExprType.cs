namespace System.ServiceModel.Dispatcher
{
    using System;

    internal enum XPathExprType : byte
    {
        And = 2,
        Filter = 14,
        Function = 11,
        LocationPath = 5,
        Math = 13,
        Number = 10,
        Or = 1,
        Path = 15,
        PathStep = 7,
        Relational = 3,
        RelativePath = 6,
        String = 9,
        Union = 4,
        Unknown = 0,
        XsltFunction = 12,
        XsltVariable = 8
    }
}

