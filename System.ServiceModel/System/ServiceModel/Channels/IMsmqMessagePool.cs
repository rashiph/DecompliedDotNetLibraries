namespace System.ServiceModel.Channels
{
    using System;

    internal interface IMsmqMessagePool : IDisposable
    {
        void ReturnMessage(MsmqInputMessage message);
        MsmqInputMessage TakeMessage();
    }
}

