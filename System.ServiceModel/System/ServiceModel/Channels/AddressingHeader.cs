namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class AddressingHeader : DictionaryHeader, IMessageHeaderWithSharedNamespace
    {
        private AddressingVersion version;

        protected AddressingHeader(AddressingVersion version)
        {
            this.version = version;
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get
            {
                return this.version.DictionaryNamespace;
            }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedNamespace
        {
            get
            {
                return this.version.DictionaryNamespace;
            }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedPrefix
        {
            get
            {
                return XD.AddressingDictionary.Prefix;
            }
        }

        internal AddressingVersion Version
        {
            get
            {
                return this.version;
            }
        }
    }
}

