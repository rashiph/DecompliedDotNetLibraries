namespace System.ServiceModel.Channels
{
    using System.ServiceModel;

    public interface IDuplexChannel : IInputChannel, IOutputChannel, IChannel, ICommunicationObject
    {
    }
}

