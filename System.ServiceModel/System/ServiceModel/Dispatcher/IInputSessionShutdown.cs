namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;

    public interface IInputSessionShutdown
    {
        void ChannelFaulted(IDuplexContextChannel channel);
        void DoneReceiving(IDuplexContextChannel channel);
    }
}

