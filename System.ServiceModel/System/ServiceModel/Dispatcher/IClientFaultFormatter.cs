namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal interface IClientFaultFormatter
    {
        FaultException Deserialize(MessageFault messageFault, string action);
    }
}

