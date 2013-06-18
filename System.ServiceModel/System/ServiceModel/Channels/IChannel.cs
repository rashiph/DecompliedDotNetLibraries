namespace System.ServiceModel.Channels
{
    using System.ServiceModel;

    public interface IChannel : ICommunicationObject
    {
        T GetProperty<T>() where T: class;
    }
}

