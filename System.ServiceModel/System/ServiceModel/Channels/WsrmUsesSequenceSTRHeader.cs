namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class WsrmUsesSequenceSTRHeader : WsrmMessageHeader
    {
        public WsrmUsesSequenceSTRHeader() : base(ReliableMessagingVersion.WSReliableMessaging11)
        {
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return DXD.Wsrm11Dictionary.UsesSequenceSTR;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return true;
            }
        }
    }
}

