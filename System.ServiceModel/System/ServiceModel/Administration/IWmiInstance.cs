namespace System.ServiceModel.Administration
{
    using System;

    internal interface IWmiInstance
    {
        object GetProperty(string name);
        IWmiInstance NewInstance(string className);
        void SetProperty(string name, object value);
    }
}

