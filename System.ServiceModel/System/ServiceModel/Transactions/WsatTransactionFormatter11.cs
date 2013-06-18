namespace System.ServiceModel.Transactions
{
    using System;
    using System.ServiceModel.Channels;

    internal class WsatTransactionFormatter11 : WsatTransactionFormatter
    {
        private static WsatTransactionHeader emptyTransactionHeader = new WsatTransactionHeader(null, ProtocolVersion.Version11);

        public WsatTransactionFormatter11() : base(ProtocolVersion.Version11)
        {
        }

        public override MessageHeader EmptyTransactionHeader
        {
            get
            {
                return emptyTransactionHeader;
            }
        }
    }
}

