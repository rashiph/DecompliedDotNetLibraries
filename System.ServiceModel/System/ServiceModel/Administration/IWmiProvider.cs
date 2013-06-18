namespace System.ServiceModel.Administration
{
    using System;

    internal interface IWmiProvider
    {
        bool DeleteInstance(IWmiInstance instance);
        void EnumInstances(IWmiInstances instances);
        bool GetInstance(IWmiInstance instance);
        bool InvokeMethod(IWmiMethodContext method);
        bool PutInstance(IWmiInstance instance);
    }
}

