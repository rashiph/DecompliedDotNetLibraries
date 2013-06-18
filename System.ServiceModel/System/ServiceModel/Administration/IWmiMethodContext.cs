namespace System.ServiceModel.Administration
{
    using System;

    internal interface IWmiMethodContext
    {
        object GetParameter(string name);
        void SetParameter(string name, object value);

        IWmiInstance Instance { get; }

        string MethodName { get; }

        object ReturnParameter { set; }
    }
}

