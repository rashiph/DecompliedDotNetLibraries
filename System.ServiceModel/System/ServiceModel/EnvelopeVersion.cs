namespace System.ServiceModel
{
    using System;
    using System.Xml;

    public sealed class EnvelopeVersion
    {
        private string actor;
        private XmlDictionaryString dictionaryActor;
        private XmlDictionaryString dictionaryNs;
        private string[] mustUnderstandActorValues;
        private string nextDestinationActorValue;
        private static EnvelopeVersion none = new EnvelopeVersion(null, null, "http://schemas.microsoft.com/ws/2005/05/envelope/none", XD.MessageDictionary.Namespace, null, null, "EnvelopeNoneToStringFormat", "Sender", "Receiver");
        private string ns;
        private string receiverFaultName;
        private string senderFaultName;
        private static EnvelopeVersion soap11 = new EnvelopeVersion("", "http://schemas.xmlsoap.org/soap/actor/next", "http://schemas.xmlsoap.org/soap/envelope/", XD.Message11Dictionary.Namespace, "actor", XD.Message11Dictionary.Actor, "Soap11ToStringFormat", "Client", "Server");
        private static EnvelopeVersion soap12 = new EnvelopeVersion("http://www.w3.org/2003/05/soap-envelope/role/ultimateReceiver", "http://www.w3.org/2003/05/soap-envelope/role/next", "http://www.w3.org/2003/05/soap-envelope", XD.Message12Dictionary.Namespace, "role", XD.Message12Dictionary.Role, "Soap12ToStringFormat", "Sender", "Receiver");
        private string toStringFormat;
        private string ultimateDestinationActor;
        private string[] ultimateDestinationActorValues;

        private EnvelopeVersion(string ultimateReceiverActor, string nextDestinationActorValue, string ns, XmlDictionaryString dictionaryNs, string actor, XmlDictionaryString dictionaryActor, string toStringFormat, string senderFaultName, string receiverFaultName)
        {
            this.toStringFormat = toStringFormat;
            this.ultimateDestinationActor = ultimateReceiverActor;
            this.nextDestinationActorValue = nextDestinationActorValue;
            this.ns = ns;
            this.dictionaryNs = dictionaryNs;
            this.actor = actor;
            this.dictionaryActor = dictionaryActor;
            this.senderFaultName = senderFaultName;
            this.receiverFaultName = receiverFaultName;
            if (ultimateReceiverActor != null)
            {
                if (ultimateReceiverActor.Length == 0)
                {
                    this.mustUnderstandActorValues = new string[] { "", nextDestinationActorValue };
                    this.ultimateDestinationActorValues = new string[] { "", nextDestinationActorValue };
                }
                else
                {
                    this.mustUnderstandActorValues = new string[] { "", ultimateReceiverActor, nextDestinationActorValue };
                    this.ultimateDestinationActorValues = new string[] { "", ultimateReceiverActor, nextDestinationActorValue };
                }
            }
        }

        public string[] GetUltimateDestinationActorValues()
        {
            return (string[]) this.ultimateDestinationActorValues.Clone();
        }

        internal bool IsUltimateDestinationActor(string actor)
        {
            if ((actor.Length != 0) && !(actor == this.ultimateDestinationActor))
            {
                return (actor == this.nextDestinationActorValue);
            }
            return true;
        }

        public override string ToString()
        {
            return System.ServiceModel.SR.GetString(this.toStringFormat, new object[] { this.Namespace });
        }

        internal string Actor
        {
            get
            {
                return this.actor;
            }
        }

        internal XmlDictionaryString DictionaryActor
        {
            get
            {
                return this.dictionaryActor;
            }
        }

        internal XmlDictionaryString DictionaryNamespace
        {
            get
            {
                return this.dictionaryNs;
            }
        }

        internal string[] MustUnderstandActorValues
        {
            get
            {
                return this.mustUnderstandActorValues;
            }
        }

        internal string Namespace
        {
            get
            {
                return this.ns;
            }
        }

        public string NextDestinationActorValue
        {
            get
            {
                return this.nextDestinationActorValue;
            }
        }

        public static EnvelopeVersion None
        {
            get
            {
                return none;
            }
        }

        internal string ReceiverFaultName
        {
            get
            {
                return this.receiverFaultName;
            }
        }

        internal string SenderFaultName
        {
            get
            {
                return this.senderFaultName;
            }
        }

        public static EnvelopeVersion Soap11
        {
            get
            {
                return soap11;
            }
        }

        public static EnvelopeVersion Soap12
        {
            get
            {
                return soap12;
            }
        }

        internal string UltimateDestinationActor
        {
            get
            {
                return this.ultimateDestinationActor;
            }
        }

        internal string[] UltimateDestinationActorValues
        {
            get
            {
                return this.ultimateDestinationActorValues;
            }
        }
    }
}

