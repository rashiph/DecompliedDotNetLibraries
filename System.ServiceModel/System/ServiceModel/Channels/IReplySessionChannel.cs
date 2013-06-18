namespace System.ServiceModel.Channels
{
    using System.ServiceModel;

    public interface IReplySessionChannel : IReplyChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
    {
    }
}

