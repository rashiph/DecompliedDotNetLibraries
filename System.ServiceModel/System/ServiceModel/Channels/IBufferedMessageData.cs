namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal interface IBufferedMessageData
    {
        void Close();
        void EnableMultipleUsers();
        XmlDictionaryReader GetMessageReader();
        void Open();
        void ReturnMessageState(RecycledMessageState messageState);
        RecycledMessageState TakeMessageState();

        ArraySegment<byte> Buffer { get; }

        System.ServiceModel.Channels.MessageEncoder MessageEncoder { get; }

        XmlDictionaryReaderQuotas Quotas { get; }
    }
}

