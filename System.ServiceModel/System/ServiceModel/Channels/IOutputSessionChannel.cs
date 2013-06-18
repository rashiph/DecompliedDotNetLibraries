namespace System.ServiceModel.Channels
{
    using System.ServiceModel;

    public interface IOutputSessionChannel : IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
    {
    }
}

