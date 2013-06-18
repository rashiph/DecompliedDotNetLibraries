namespace System.ServiceModel.Channels
{
    using System.ServiceModel;

    public interface IRequestSessionChannel : IRequestChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
    {
    }
}

