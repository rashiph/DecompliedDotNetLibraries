namespace System.ServiceModel.Channels
{
    using System.ServiceModel;

    public interface IInputSessionChannel : IInputChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
    {
    }
}

