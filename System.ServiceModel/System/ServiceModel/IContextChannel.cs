namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    public interface IContextChannel : IChannel, ICommunicationObject, IExtensibleObject<IContextChannel>
    {
        bool AllowOutputBatching { get; set; }

        IInputSession InputSession { get; }

        EndpointAddress LocalAddress { get; }

        TimeSpan OperationTimeout { get; set; }

        IOutputSession OutputSession { get; }

        EndpointAddress RemoteAddress { get; }

        string SessionId { get; }
    }
}

