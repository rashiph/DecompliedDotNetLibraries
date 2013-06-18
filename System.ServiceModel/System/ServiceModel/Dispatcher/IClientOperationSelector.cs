namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection;

    public interface IClientOperationSelector
    {
        string SelectOperation(MethodBase method, object[] parameters);

        bool AreParametersRequiredForSelection { get; }
    }
}

