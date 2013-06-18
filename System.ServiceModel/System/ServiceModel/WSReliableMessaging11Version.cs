namespace System.ServiceModel
{
    using System;

    internal class WSReliableMessaging11Version : ReliableMessagingVersion
    {
        private static ReliableMessagingVersion instance = new WSReliableMessaging11Version();

        private WSReliableMessaging11Version() : base("http://docs.oasis-open.org/ws-rx/wsrm/200702", DXD.Wsrm11Dictionary.Namespace)
        {
        }

        public override string ToString()
        {
            return "WSReliableMessaging11";
        }

        internal static ReliableMessagingVersion Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

