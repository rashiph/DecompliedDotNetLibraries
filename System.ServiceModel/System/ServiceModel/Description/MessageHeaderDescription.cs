namespace System.ServiceModel.Description
{
    using System;
    using System.ComponentModel;
    using System.Xml;

    public class MessageHeaderDescription : MessagePartDescription
    {
        private string actor;
        private bool isUnknownHeader;
        private bool mustUnderstand;
        private bool relay;
        private bool typedHeader;

        internal MessageHeaderDescription(MessageHeaderDescription other) : base(other)
        {
            this.MustUnderstand = other.MustUnderstand;
            this.Relay = other.Relay;
            this.Actor = other.Actor;
            this.TypedHeader = other.TypedHeader;
            this.IsUnknownHeaderCollection = other.IsUnknownHeaderCollection;
        }

        public MessageHeaderDescription(string name, string ns) : base(name, ns)
        {
        }

        internal override MessagePartDescription Clone()
        {
            return new MessageHeaderDescription(this);
        }

        [DefaultValue((string) null)]
        public string Actor
        {
            get
            {
                return this.actor;
            }
            set
            {
                this.actor = value;
            }
        }

        internal bool IsUnknownHeaderCollection
        {
            get
            {
                return (this.isUnknownHeader || (base.Multiple && (base.Type == typeof(XmlElement))));
            }
            set
            {
                this.isUnknownHeader = value;
            }
        }

        [DefaultValue(false)]
        public bool MustUnderstand
        {
            get
            {
                return this.mustUnderstand;
            }
            set
            {
                this.mustUnderstand = value;
            }
        }

        [DefaultValue(false)]
        public bool Relay
        {
            get
            {
                return this.relay;
            }
            set
            {
                this.relay = value;
            }
        }

        [DefaultValue(false)]
        public bool TypedHeader
        {
            get
            {
                return this.typedHeader;
            }
            set
            {
                this.typedHeader = value;
            }
        }
    }
}

