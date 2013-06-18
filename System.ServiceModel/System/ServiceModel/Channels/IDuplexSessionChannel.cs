namespace System.ServiceModel.Channels
{
    using System.ServiceModel;

    public interface IDuplexSessionChannel : IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
    {
    }
}

