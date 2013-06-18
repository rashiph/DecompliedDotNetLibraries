namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    public sealed class AddressingVersion
    {
        private static AddressingVersion addressing10 = new AddressingVersion("http://www.w3.org/2005/08/addressing", XD.Addressing10Dictionary.Namespace, "Addressing10ToStringFormat", Addressing10SignedMessageParts, "http://www.w3.org/2005/08/addressing/anonymous", XD.Addressing10Dictionary.Anonymous, "http://www.w3.org/2005/08/addressing/none", "http://www.w3.org/2005/08/addressing/fault", "http://www.w3.org/2005/08/addressing/soap/fault");
        private static MessagePartSpecification addressing10SignedMessageParts;
        private static AddressingVersion addressing200408 = new AddressingVersion("http://schemas.xmlsoap.org/ws/2004/08/addressing", XD.Addressing200408Dictionary.Namespace, "Addressing200408ToStringFormat", Addressing200408SignedMessageParts, "http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous", XD.Addressing200408Dictionary.Anonymous, null, "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault");
        private static MessagePartSpecification addressing200408SignedMessageParts;
        private string anonymous;
        private Uri anonymousUri;
        private string defaultFaultAction;
        private XmlDictionaryString dictionaryAnonymous;
        private XmlDictionaryString dictionaryNs;
        private string faultAction;
        private static AddressingVersion none = new AddressingVersion("http://schemas.microsoft.com/ws/2005/05/addressing/none", XD.AddressingNoneDictionary.Namespace, "AddressingNoneToStringFormat", new MessagePartSpecification(), null, null, null, null, null);
        private Uri noneUri;
        private string ns;
        private MessagePartSpecification signedMessageParts;
        private string toStringFormat;

        private AddressingVersion(string ns, XmlDictionaryString dictionaryNs, string toStringFormat, MessagePartSpecification signedMessageParts, string anonymous, XmlDictionaryString dictionaryAnonymous, string none, string faultAction, string defaultFaultAction)
        {
            this.ns = ns;
            this.dictionaryNs = dictionaryNs;
            this.toStringFormat = toStringFormat;
            this.signedMessageParts = signedMessageParts;
            this.anonymous = anonymous;
            this.dictionaryAnonymous = dictionaryAnonymous;
            if (anonymous != null)
            {
                this.anonymousUri = new Uri(anonymous);
            }
            if (none != null)
            {
                this.noneUri = new Uri(none);
            }
            this.faultAction = faultAction;
            this.defaultFaultAction = defaultFaultAction;
        }

        public override string ToString()
        {
            return System.ServiceModel.SR.GetString(this.toStringFormat, new object[] { this.Namespace });
        }

        private static MessagePartSpecification Addressing10SignedMessageParts
        {
            get
            {
                if (addressing10SignedMessageParts == null)
                {
                    MessagePartSpecification specification = new MessagePartSpecification(new XmlQualifiedName[] { new XmlQualifiedName("To", "http://www.w3.org/2005/08/addressing"), new XmlQualifiedName("From", "http://www.w3.org/2005/08/addressing"), new XmlQualifiedName("FaultTo", "http://www.w3.org/2005/08/addressing"), new XmlQualifiedName("ReplyTo", "http://www.w3.org/2005/08/addressing"), new XmlQualifiedName("MessageID", "http://www.w3.org/2005/08/addressing"), new XmlQualifiedName("RelatesTo", "http://www.w3.org/2005/08/addressing"), new XmlQualifiedName("Action", "http://www.w3.org/2005/08/addressing") });
                    specification.MakeReadOnly();
                    addressing10SignedMessageParts = specification;
                }
                return addressing10SignedMessageParts;
            }
        }

        private static MessagePartSpecification Addressing200408SignedMessageParts
        {
            get
            {
                if (addressing200408SignedMessageParts == null)
                {
                    MessagePartSpecification specification = new MessagePartSpecification(new XmlQualifiedName[] { new XmlQualifiedName("To", "http://schemas.xmlsoap.org/ws/2004/08/addressing"), new XmlQualifiedName("From", "http://schemas.xmlsoap.org/ws/2004/08/addressing"), new XmlQualifiedName("FaultTo", "http://schemas.xmlsoap.org/ws/2004/08/addressing"), new XmlQualifiedName("ReplyTo", "http://schemas.xmlsoap.org/ws/2004/08/addressing"), new XmlQualifiedName("MessageID", "http://schemas.xmlsoap.org/ws/2004/08/addressing"), new XmlQualifiedName("RelatesTo", "http://schemas.xmlsoap.org/ws/2004/08/addressing"), new XmlQualifiedName("Action", "http://schemas.xmlsoap.org/ws/2004/08/addressing") });
                    specification.MakeReadOnly();
                    addressing200408SignedMessageParts = specification;
                }
                return addressing200408SignedMessageParts;
            }
        }

        internal string Anonymous
        {
            get
            {
                return this.anonymous;
            }
        }

        internal Uri AnonymousUri
        {
            get
            {
                return this.anonymousUri;
            }
        }

        internal string DefaultFaultAction
        {
            get
            {
                return this.defaultFaultAction;
            }
        }

        internal XmlDictionaryString DictionaryAnonymous
        {
            get
            {
                return this.dictionaryAnonymous;
            }
        }

        internal XmlDictionaryString DictionaryNamespace
        {
            get
            {
                return this.dictionaryNs;
            }
        }

        internal string FaultAction
        {
            get
            {
                return this.faultAction;
            }
        }

        internal string Namespace
        {
            get
            {
                return this.ns;
            }
        }

        public static AddressingVersion None
        {
            get
            {
                return none;
            }
        }

        internal Uri NoneUri
        {
            get
            {
                return this.noneUri;
            }
        }

        internal MessagePartSpecification SignedMessageParts
        {
            get
            {
                return this.signedMessageParts;
            }
        }

        public static AddressingVersion WSAddressing10
        {
            get
            {
                return addressing10;
            }
        }

        public static AddressingVersion WSAddressingAugust2004
        {
            get
            {
                return addressing200408;
            }
        }
    }
}

