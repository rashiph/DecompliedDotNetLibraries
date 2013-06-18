namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    public interface IDispatchOperationSelector
    {
        string SelectOperation(ref Message message);
    }
}

