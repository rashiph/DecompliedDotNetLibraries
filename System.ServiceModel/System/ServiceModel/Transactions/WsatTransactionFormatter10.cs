namespace System.ServiceModel.Transactions
{
    using System;
    using System.ServiceModel.Channels;

    internal class WsatTransactionFormatter10 : WsatTransactionFormatter
    {
        private static WsatTransactionHeader emptyTransactionHeader = new WsatTransactionHeader(null, ProtocolVersion.Version10);

        public WsatTransactionFormatter10() : base(ProtocolVersion.Version10)
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

