namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal class XmlObjectSerializerHeader : MessageHeader
    {
        private string actor;
        private bool isNoneSupported;
        private bool isOneOneSupported;
        private bool isOneTwoSupported;
        private bool mustUnderstand;
        private string name;
        private string ns;
        private object objectToSerialize;
        private bool relay;
        private XmlObjectSerializer serializer;
        private object syncRoot;

        private XmlObjectSerializerHeader(XmlObjectSerializer serializer, bool mustUnderstand, string actor, bool relay)
        {
            this.syncRoot = new object();
            if (actor == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("actor");
            }
            this.mustUnderstand = mustUnderstand;
            this.relay = relay;
            this.serializer = serializer;
            this.actor = actor;
            if (actor == EnvelopeVersion.Soap12.UltimateDestinationActor)
            {
                this.isOneOneSupported = false;
                this.isOneTwoSupported = true;
            }
            else if (actor == EnvelopeVersion.Soap12.NextDestinationActorValue)
            {
                this.isOneOneSupported = false;
                this.isOneTwoSupported = true;
            }
            else if (actor == EnvelopeVersion.Soap11.NextDestinationActorValue)
            {
                this.isOneOneSupported = true;
                this.isOneTwoSupported = false;
            }
            else
            {
                this.isOneOneSupported = true;
                this.isOneTwoSupported = true;
                this.isNoneSupported = true;
            }
        }

        public XmlObjectSerializerHeader(string name, string ns, object objectToSerialize, XmlObjectSerializer serializer, bool mustUnderstand, string actor, bool relay) : this(serializer, mustUnderstand, actor, relay)
        {
            if (name == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            if (name.Length == 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFXHeaderNameCannotBeNullOrEmpty"), "name"));
            }
            if (ns == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            if (ns.Length > 0)
            {
                NamingHelper.CheckUriParameter(ns, "ns");
            }
            this.objectToSerialize = objectToSerialize;
            this.name = name;
            this.ns = ns;
        }

        public override bool IsMessageVersionSupported(MessageVersion messageVersion)
        {
            if (messageVersion == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");
            }
            if (messageVersion.Envelope == EnvelopeVersion.Soap12)
            {
                return this.isOneTwoSupported;
            }
            if (messageVersion.Envelope == EnvelopeVersion.Soap11)
            {
                return this.isOneOneSupported;
            }
            if (messageVersion.Envelope != EnvelopeVersion.None)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EnvelopeVersionUnknown", new object[] { messageVersion.Envelope.ToString() })));
            }
            return this.isNoneSupported;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            lock (this.syncRoot)
            {
                if (this.serializer == null)
                {
                    this.serializer = DataContractSerializerDefaults.CreateSerializer((this.objectToSerialize == null) ? typeof(object) : this.objectToSerialize.GetType(), this.Name, this.Namespace, 0x7fffffff);
                }
                this.serializer.WriteObjectContent(writer, this.objectToSerialize);
            }
        }

        public override string Actor
        {
            get
            {
                return this.actor;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return this.mustUnderstand;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.ns;
            }
        }

        public override bool Relay
        {
            get
            {
                return this.relay;
            }
        }
    }
}

