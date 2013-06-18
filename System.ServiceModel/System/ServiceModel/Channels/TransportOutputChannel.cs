namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class TransportOutputChannel : OutputChannel
    {
        private bool anyHeadersToAdd;
        private bool manualAddressing;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private EndpointAddress to;
        private ToHeader toHeader;
        private Uri via;

        protected TransportOutputChannel(ChannelManagerBase channelManager, EndpointAddress to, Uri via, bool manualAddressing, System.ServiceModel.Channels.MessageVersion messageVersion) : base(channelManager)
        {
            this.manualAddressing = manualAddressing;
            this.messageVersion = messageVersion;
            this.to = to;
            this.via = via;
            if (!manualAddressing && (to != null))
            {
                Uri anonymousUri;
                if (to.IsAnonymous)
                {
                    anonymousUri = this.messageVersion.Addressing.AnonymousUri;
                }
                else if (to.IsNone)
                {
                    anonymousUri = this.messageVersion.Addressing.NoneUri;
                }
                else
                {
                    anonymousUri = to.Uri;
                }
                XmlDictionaryString dictionaryTo = new ToDictionary(anonymousUri.AbsoluteUri).To;
                this.toHeader = ToHeader.Create(anonymousUri, dictionaryTo, messageVersion.Addressing);
                this.anyHeadersToAdd = to.Headers.Count > 0;
            }
        }

        protected override void AddHeadersTo(Message message)
        {
            base.AddHeadersTo(message);
            if (this.toHeader != null)
            {
                message.Headers.SetToHeader(this.toHeader);
                if (this.anyHeadersToAdd)
                {
                    this.to.Headers.AddHeadersTo(message);
                }
            }
        }

        protected bool ManualAddressing
        {
            get
            {
                return this.manualAddressing;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public override EndpointAddress RemoteAddress
        {
            get
            {
                return this.to;
            }
        }

        public override Uri Via
        {
            get
            {
                return this.via;
            }
        }

        private class ToDictionary : IXmlDictionary
        {
            private XmlDictionaryString to;

            public ToDictionary(string to)
            {
                this.to = new XmlDictionaryString(this, to, 0);
            }

            public bool TryLookup(int key, out XmlDictionaryString result)
            {
                if (key == 0)
                {
                    result = this.to;
                    return true;
                }
                result = null;
                return false;
            }

            public bool TryLookup(string value, out XmlDictionaryString result)
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value == this.to.Value)
                {
                    result = this.to;
                    return true;
                }
                result = null;
                return false;
            }

            public bool TryLookup(XmlDictionaryString value, out XmlDictionaryString result)
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value == this.to)
                {
                    result = this.to;
                    return true;
                }
                result = null;
                return false;
            }

            public XmlDictionaryString To
            {
                get
                {
                    return this.to;
                }
            }
        }
    }
}

