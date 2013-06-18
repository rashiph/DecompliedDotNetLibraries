namespace System.ServiceModel.Administration
{
    using System;

    internal interface IWmiInstanceProvider
    {
        void FillInstance(IWmiInstance wmiInstance);
        string GetInstanceType();
    }
}

