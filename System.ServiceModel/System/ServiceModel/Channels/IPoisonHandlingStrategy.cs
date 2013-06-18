namespace System.ServiceModel.Channels
{
    using System;

    internal interface IPoisonHandlingStrategy : IDisposable
    {
        bool CheckAndHandlePoisonMessage(MsmqMessageProperty messageProperty);
        void FinalDisposition(MsmqMessageProperty messageProperty);
        void Open();
    }
}

