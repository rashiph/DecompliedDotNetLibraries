namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal abstract class DictionaryHeader : MessageHeader
    {
        protected DictionaryHeader()
        {
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteStartElement(this.DictionaryName, this.DictionaryNamespace);
            base.WriteHeaderAttributes(writer, messageVersion);
        }

        public abstract XmlDictionaryString DictionaryName { get; }

        public abstract XmlDictionaryString DictionaryNamespace { get; }

        public override string Name
        {
            get
            {
                return this.DictionaryName.Value;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.DictionaryNamespace.Value;
            }
        }
    }
}

