namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;

    internal class XmlObjectSerializerFault : MessageFault
    {
        private string actor;
        private FaultCode code;
        private object detail;
        private string node;
        private FaultReason reason;
        private XmlObjectSerializer serializer;

        public XmlObjectSerializerFault(FaultCode code, FaultReason reason, object detail, XmlObjectSerializer serializer, string actor, string node)
        {
            this.code = code;
            this.reason = reason;
            this.detail = detail;
            this.serializer = serializer;
            this.actor = actor;
            this.node = node;
        }

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            if (this.serializer != null)
            {
                lock (this.ThisLock)
                {
                    this.serializer.WriteObject(writer, this.detail);
                }
            }
        }

        public override string Actor
        {
            get
            {
                return this.actor;
            }
        }

        public override FaultCode Code
        {
            get
            {
                return this.code;
            }
        }

        public override bool HasDetail
        {
            get
            {
                return (this.serializer != null);
            }
        }

        public override string Node
        {
            get
            {
                return this.node;
            }
        }

        public override FaultReason Reason
        {
            get
            {
                return this.reason;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.code;
            }
        }
    }
}

