namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class ActionHeader : AddressingHeader
    {
        private string action;
        private const bool mustUnderstandValue = true;

        private ActionHeader(string action, AddressingVersion version) : base(version)
        {
            this.action = action;
        }

        public static ActionHeader Create(string action, AddressingVersion addressingVersion)
        {
            if (action == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("action"));
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            return new ActionHeader(action, addressingVersion);
        }

        public static ActionHeader Create(XmlDictionaryString dictionaryAction, AddressingVersion addressingVersion)
        {
            if (dictionaryAction == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("action"));
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            return new DictionaryActionHeader(dictionaryAction, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteString(this.action);
        }

        public static ActionHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version, string actor, bool mustUnderstand, bool relay)
        {
            string action = ReadHeaderValue(reader, version);
            if (((actor.Length == 0) && mustUnderstand) && !relay)
            {
                return new ActionHeader(action, version);
            }
            return new FullActionHeader(action, actor, mustUnderstand, relay, version);
        }

        public static string ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion addressingVersion)
        {
            string s = reader.ReadElementContentAsString();
            if ((s.Length <= 0) || ((s[0] > ' ') && (s[s.Length - 1] > ' ')))
            {
                return s;
            }
            return XmlUtil.Trim(s);
        }

        public string Action
        {
            get
            {
                return this.action;
            }
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.AddressingDictionary.Action;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return true;
            }
        }

        private class DictionaryActionHeader : ActionHeader
        {
            private XmlDictionaryString dictionaryAction;

            public DictionaryActionHeader(XmlDictionaryString dictionaryAction, AddressingVersion version) : base(dictionaryAction.Value, version)
            {
                this.dictionaryAction = dictionaryAction;
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteString(this.dictionaryAction);
            }
        }

        private class FullActionHeader : ActionHeader
        {
            private string actor;
            private bool mustUnderstand;
            private bool relay;

            public FullActionHeader(string action, string actor, bool mustUnderstand, bool relay, AddressingVersion version) : base(action, version)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
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

            public override bool Relay
            {
                get
                {
                    return this.relay;
                }
            }
        }
    }
}

