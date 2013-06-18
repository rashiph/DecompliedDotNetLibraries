namespace System.ServiceModel.Administration
{
    using System;

    internal interface IWmiInstances
    {
        void AddInstance(IWmiInstance inst);
        IWmiInstance NewInstance(string className);
    }
}

