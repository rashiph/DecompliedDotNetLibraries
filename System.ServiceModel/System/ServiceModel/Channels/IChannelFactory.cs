namespace System.ServiceModel.Channels
{
    using System.ServiceModel;

    public interface IChannelFactory : ICommunicationObject
    {
        T GetProperty<T>() where T: class;
    }
}

