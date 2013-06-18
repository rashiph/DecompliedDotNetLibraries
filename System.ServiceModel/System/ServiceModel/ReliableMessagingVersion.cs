namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Xml;

    [TypeConverter(typeof(ReliableMessagingVersionConverter))]
    public abstract class ReliableMessagingVersion
    {
        private XmlDictionaryString dictionaryNs;
        private string ns;

        internal ReliableMessagingVersion(string ns, XmlDictionaryString dictionaryNs)
        {
            this.ns = ns;
            this.dictionaryNs = dictionaryNs;
        }

        internal static bool IsDefined(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion != WSReliableMessaging11)
            {
                return (reliableMessagingVersion == WSReliableMessagingFebruary2005);
            }
            return true;
        }

        public static ReliableMessagingVersion Default
        {
            get
            {
                return ReliableSessionDefaults.ReliableMessagingVersion;
            }
        }

        internal XmlDictionaryString DictionaryNamespace
        {
            get
            {
                return this.dictionaryNs;
            }
        }

        internal string Namespace
        {
            get
            {
                return this.ns;
            }
        }

        public static ReliableMessagingVersion WSReliableMessaging11
        {
            get
            {
                return WSReliableMessaging11Version.Instance;
            }
        }

        public static ReliableMessagingVersion WSReliableMessagingFebruary2005
        {
            get
            {
                return WSReliableMessagingFebruary2005Version.Instance;
            }
        }
    }
}

