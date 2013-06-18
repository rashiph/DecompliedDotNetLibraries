namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    public interface IServiceChannel : IContextChannel, IChannel, ICommunicationObject, IExtensibleObject<IContextChannel>
    {
        Uri ListenUri { get; }
    }
}

