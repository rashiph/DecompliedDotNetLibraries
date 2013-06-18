namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.Xml;

    internal class PeerDictionaryHeader : DictionaryHeader
    {
        private XmlDictionaryString name;
        private XmlDictionaryString nameSpace;
        private string value;

        public PeerDictionaryHeader(XmlDictionaryString name, XmlDictionaryString nameSpace, string value)
        {
            this.name = name;
            this.nameSpace = nameSpace;
            this.value = value;
        }

        public PeerDictionaryHeader(XmlDictionaryString name, XmlDictionaryString nameSpace, XmlDictionaryString value)
        {
            this.name = name;
            this.nameSpace = nameSpace;
            this.value = value.Value;
        }

        internal static PeerDictionaryHeader CreateFloodRole()
        {
            return new PeerDictionaryHeader(XD.PeerWireStringsDictionary.FloodAction, XD.PeerWireStringsDictionary.Namespace, XD.PeerWireStringsDictionary.Demuxer);
        }

        internal static PeerDictionaryHeader CreateHopCountHeader(ulong hopcount)
        {
            return new PeerDictionaryHeader(XD.PeerWireStringsDictionary.HopCount, XD.PeerWireStringsDictionary.HopCountNamespace, hopcount.ToString(CultureInfo.InvariantCulture));
        }

        internal static PeerDictionaryHeader CreateMessageIdHeader(UniqueId messageId)
        {
            return new PeerDictionaryHeader(XD.AddressingDictionary.MessageId, XD.PeerWireStringsDictionary.Namespace, messageId.ToString());
        }

        internal static PeerDictionaryHeader CreateToHeader(Uri to)
        {
            return new PeerDictionaryHeader(XD.PeerWireStringsDictionary.PeerTo, XD.PeerWireStringsDictionary.Namespace, to.ToString());
        }

        internal static PeerDictionaryHeader CreateViaHeader(Uri via)
        {
            return new PeerDictionaryHeader(XD.PeerWireStringsDictionary.PeerVia, XD.PeerWireStringsDictionary.Namespace, via.ToString());
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteString(this.value);
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return this.name;
            }
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get
            {
                return this.nameSpace;
            }
        }
    }
}

