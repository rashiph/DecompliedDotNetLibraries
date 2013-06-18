namespace System.ServiceModel
{
    using System;

    internal class WSReliableMessagingFebruary2005Version : ReliableMessagingVersion
    {
        private static ReliableMessagingVersion instance = new WSReliableMessagingFebruary2005Version();

        private WSReliableMessagingFebruary2005Version() : base("http://schemas.xmlsoap.org/ws/2005/02/rm", XD.WsrmFeb2005Dictionary.Namespace)
        {
        }

        public override string ToString()
        {
            return "WSReliableMessagingFebruary2005";
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

