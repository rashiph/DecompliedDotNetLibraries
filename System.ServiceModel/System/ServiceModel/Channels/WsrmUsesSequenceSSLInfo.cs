namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal sealed class WsrmUsesSequenceSSLInfo : WsrmHeaderInfo
    {
        private WsrmUsesSequenceSSLInfo(MessageHeaderInfo header) : base(header)
        {
        }

        public static WsrmUsesSequenceSSLInfo ReadHeader(XmlDictionaryReader reader, MessageHeaderInfo header)
        {
            WsrmUtilities.ReadEmptyElement(reader);
            return new WsrmUsesSequenceSSLInfo(header);
        }
    }
}

