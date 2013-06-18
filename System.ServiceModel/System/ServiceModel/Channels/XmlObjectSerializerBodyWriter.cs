namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;

    internal class XmlObjectSerializerBodyWriter : BodyWriter
    {
        private object body;
        private XmlObjectSerializer serializer;

        public XmlObjectSerializerBodyWriter(object body, XmlObjectSerializer serializer) : base(true)
        {
            this.body = body;
            this.serializer = serializer;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            lock (this.ThisLock)
            {
                this.serializer.WriteObject(writer, this.body);
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }
    }
}

