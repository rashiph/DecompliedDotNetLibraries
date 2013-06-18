namespace System.ServiceModel.Channels
{
    using System.Xml;

    public interface ITransportTokenAssertionProvider
    {
        XmlElement GetTransportTokenAssertion();
    }
}

