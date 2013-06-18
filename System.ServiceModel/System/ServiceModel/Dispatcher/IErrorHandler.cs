namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    public interface IErrorHandler
    {
        bool HandleError(Exception error);
        void ProvideFault(Exception error, MessageVersion version, ref Message fault);
    }
}

