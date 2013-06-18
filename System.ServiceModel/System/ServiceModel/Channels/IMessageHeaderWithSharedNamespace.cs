namespace System.ServiceModel.Channels
{
    using System.Xml;

    internal interface IMessageHeaderWithSharedNamespace
    {
        XmlDictionaryString SharedNamespace { get; }

        XmlDictionaryString SharedPrefix { get; }
    }
}

