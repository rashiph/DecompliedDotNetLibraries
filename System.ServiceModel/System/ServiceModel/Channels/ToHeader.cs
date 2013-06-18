namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class ToHeader : AddressingHeader
    {
        private static ToHeader anonymousToHeader10;
        private static ToHeader anonymousToHeader200408;
        private const bool mustUnderstandValue = true;
        private Uri to;

        protected ToHeader(Uri to, AddressingVersion version) : base(version)
        {
            this.to = to;
        }

        public static ToHeader Create(Uri to, AddressingVersion addressingVersion)
        {
            if (to == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("to"));
            }
            if (to != addressingVersion.AnonymousUri)
            {
                return new ToHeader(to, addressingVersion);
            }
            if (addressingVersion == AddressingVersion.WSAddressing10)
            {
                return AnonymousTo10;
            }
            return AnonymousTo200408;
        }

        public static ToHeader Create(Uri toUri, XmlDictionaryString dictionaryTo, AddressingVersion addressingVersion)
        {
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            if (toUri != addressingVersion.AnonymousUri)
            {
                return new DictionaryToHeader(toUri, dictionaryTo, addressingVersion);
            }
            if (addressingVersion == AddressingVersion.WSAddressing10)
            {
                return AnonymousTo10;
            }
            return AnonymousTo200408;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteString(this.to.AbsoluteUri);
        }

        public static ToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version, UriCache uriCache, string actor, bool mustUnderstand, bool relay)
        {
            Uri to = ReadHeaderValue(reader, version, uriCache);
            if (((actor.Length != 0) || !mustUnderstand) || relay)
            {
                return new FullToHeader(to, actor, mustUnderstand, relay, version);
            }
            if (to != version.Anonymous)
            {
                return new ToHeader(to, version);
            }
            if (version == AddressingVersion.WSAddressing10)
            {
                return AnonymousTo10;
            }
            return AnonymousTo200408;
        }

        public static Uri ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            return ReadHeaderValue(reader, version, null);
        }

        public static Uri ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version, UriCache uriCache)
        {
            string uriString = reader.ReadElementContentAsString();
            if (uriString == version.Anonymous)
            {
                return version.AnonymousUri;
            }
            if (uriCache == null)
            {
                return new Uri(uriString);
            }
            return uriCache.CreateUri(uriString);
        }

        private static ToHeader AnonymousTo10
        {
            get
            {
                if (anonymousToHeader10 == null)
                {
                    anonymousToHeader10 = new AnonymousToHeader(AddressingVersion.WSAddressing10);
                }
                return anonymousToHeader10;
            }
        }

        private static ToHeader AnonymousTo200408
        {
            get
            {
                if (anonymousToHeader200408 == null)
                {
                    anonymousToHeader200408 = new AnonymousToHeader(AddressingVersion.WSAddressingAugust2004);
                }
                return anonymousToHeader200408;
            }
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.AddressingDictionary.To;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return true;
            }
        }

        public Uri To
        {
            get
            {
                return this.to;
            }
        }

        private class AnonymousToHeader : ToHeader
        {
            public AnonymousToHeader(AddressingVersion version) : base(version.AnonymousUri, version)
            {
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteString(base.Version.DictionaryAnonymous);
            }
        }

        private class DictionaryToHeader : ToHeader
        {
            private XmlDictionaryString dictionaryTo;

            public DictionaryToHeader(Uri to, XmlDictionaryString dictionaryTo, AddressingVersion version) : base(to, version)
            {
                this.dictionaryTo = dictionaryTo;
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteString(this.dictionaryTo);
            }
        }

        private class FullToHeader : ToHeader
        {
            private string actor;
            private bool mustUnderstand;
            private bool relay;

            public FullToHeader(Uri to, string actor, bool mustUnderstand, bool relay, AddressingVersion version) : base(to, version)
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

