namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal abstract class ContentOnlyMessage : Message
    {
        private MessageHeaders headers = new MessageHeaders(MessageVersion.None);
        private MessageProperties properties;

        protected ContentOnlyMessage()
        {
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            this.OnWriteBodyContents(writer);
        }

        public override MessageHeaders Headers
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                return this.headers;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                if (this.properties == null)
                {
                    this.properties = new MessageProperties();
                }
                return this.properties;
            }
        }

        public override MessageVersion Version
        {
            get
            {
                return this.headers.MessageVersion;
            }
        }
    }
}

