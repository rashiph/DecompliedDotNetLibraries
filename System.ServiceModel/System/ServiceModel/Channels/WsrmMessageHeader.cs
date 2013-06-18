namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class WsrmMessageHeader : DictionaryHeader, IMessageHeaderWithSharedNamespace
    {
        private System.ServiceModel.ReliableMessagingVersion reliableMessagingVersion;

        protected WsrmMessageHeader(System.ServiceModel.ReliableMessagingVersion reliableMessagingVersion)
        {
            this.reliableMessagingVersion = reliableMessagingVersion;
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get
            {
                return WsrmIndex.GetNamespace(this.reliableMessagingVersion);
            }
        }

        public override string Namespace
        {
            get
            {
                return WsrmIndex.GetNamespaceString(this.reliableMessagingVersion);
            }
        }

        protected System.ServiceModel.ReliableMessagingVersion ReliableMessagingVersion
        {
            get
            {
                return this.reliableMessagingVersion;
            }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedNamespace
        {
            get
            {
                return WsrmIndex.GetNamespace(this.reliableMessagingVersion);
            }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedPrefix
        {
            get
            {
                return XD.WsrmFeb2005Dictionary.Prefix;
            }
        }
    }
}

