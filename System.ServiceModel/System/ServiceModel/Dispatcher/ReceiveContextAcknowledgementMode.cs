namespace System.ServiceModel.Dispatcher
{
    using System;

    internal enum ReceiveContextAcknowledgementMode
    {
        AutoAcknowledgeOnReceive,
        AutoAcknowledgeOnRPCComplete,
        ManualAcknowledgement
    }
}

