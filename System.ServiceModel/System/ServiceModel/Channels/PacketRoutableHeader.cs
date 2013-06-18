namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal class PacketRoutableHeader : DictionaryHeader
    {
        private PacketRoutableHeader()
        {
        }

        public static void AddHeadersTo(Message message, MessageHeader header)
        {
            if (message.Headers.FindHeader("PacketRoutable", "http://schemas.microsoft.com/ws/2005/05/routing") == -1)
            {
                if (header == null)
                {
                    header = Create();
                }
                message.Headers.Add(header);
            }
        }

        public static PacketRoutableHeader Create()
        {
            return new PacketRoutableHeader();
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
        }

        public static bool TryValidateMessage(Message message)
        {
            return (message.Headers.FindHeader("PacketRoutable", "http://schemas.microsoft.com/ws/2005/05/routing") != -1);
        }

        public static void ValidateMessage(Message message)
        {
            if (!TryValidateMessage(message))
            {
                throw TraceUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("OneWayHeaderNotFound")), message);
            }
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.DotNetOneWayDictionary.HeaderName;
            }
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get
            {
                return XD.DotNetOneWayDictionary.Namespace;
            }
        }
    }
}

