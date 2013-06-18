namespace System.ServiceModel.Dispatcher
{
    using System;

    internal interface IFunctionLibrary
    {
        QueryFunction Bind(string functionName, string functionNamespace, XPathExprList args);
    }
}

