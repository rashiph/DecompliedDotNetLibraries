namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public interface IInstanceContextProvider
    {
        InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel);
        void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel);
        bool IsIdle(InstanceContext instanceContext);
        void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext);
    }
}

