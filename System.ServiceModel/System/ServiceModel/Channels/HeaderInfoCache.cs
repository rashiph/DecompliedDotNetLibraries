namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class HeaderInfoCache
    {
        private HeaderInfo[] headerInfos;
        private int index;
        private const int maxHeaderInfos = 4;

        public void ReturnHeaderInfo(MessageHeaderInfo headerInfo)
        {
            HeaderInfo info = headerInfo as HeaderInfo;
            if (info != null)
            {
                if (this.headerInfos == null)
                {
                    this.headerInfos = new HeaderInfo[4];
                }
                int index = this.index;
                do
                {
                    if (this.headerInfos[index] == null)
                    {
                        break;
                    }
                    index = (index + 1) % 4;
                }
                while (index != this.index);
                this.headerInfos[index] = info;
                this.index = (index + 1) % 4;
            }
        }

        public MessageHeaderInfo TakeHeaderInfo(XmlDictionaryReader reader, string actor, bool mustUnderstand, bool relay, bool isRefParam)
        {
            if (this.headerInfos != null)
            {
                int index = this.index;
                do
                {
                    HeaderInfo info = this.headerInfos[index];
                    if ((info != null) && info.Matches(reader, actor, mustUnderstand, relay, isRefParam))
                    {
                        this.headerInfos[index] = null;
                        this.index = (index + 1) % 4;
                        return info;
                    }
                    index = (index + 1) % 4;
                }
                while (index != this.index);
            }
            return new HeaderInfo(reader, actor, mustUnderstand, relay, isRefParam);
        }

        private class HeaderInfo : MessageHeaderInfo
        {
            private string actor;
            private bool isReferenceParameter;
            private bool mustUnderstand;
            private string name;
            private string ns;
            private bool relay;

            public HeaderInfo(XmlDictionaryReader reader, string actor, bool mustUnderstand, bool relay, bool isReferenceParameter)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
                this.isReferenceParameter = isReferenceParameter;
                reader.GetNonAtomizedNames(out this.name, out this.ns);
            }

            public bool Matches(XmlDictionaryReader reader, string actor, bool mustUnderstand, bool relay, bool isRefParam)
            {
                return (((reader.IsStartElement(this.name, this.ns) && (this.actor == actor)) && ((this.mustUnderstand == mustUnderstand) && (this.relay == relay))) && (this.isReferenceParameter == isRefParam));
            }

            public override string Actor
            {
                get
                {
                    return this.actor;
                }
            }

            public override bool IsReferenceParameter
            {
                get
                {
                    return this.isReferenceParameter;
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
}

