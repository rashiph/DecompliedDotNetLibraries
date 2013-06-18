namespace System.ServiceModel.Transactions
{
    using System;
    using System.Transactions;

    internal class WsatExtendedInformation
    {
        private string identifier;
        private uint timeout;
        public const string UuidScheme = "urn:uuid:";

        public WsatExtendedInformation(string identifier, uint timeout)
        {
            this.identifier = identifier;
            this.timeout = timeout;
        }

        public static string CreateNativeIdentifier(Guid transactionId)
        {
            return ("urn:uuid:" + transactionId.ToString("D"));
        }

        public static bool IsNativeIdentifier(string identifier, Guid transactionId)
        {
            return (string.Compare(identifier, CreateNativeIdentifier(transactionId), StringComparison.Ordinal) == 0);
        }

        public void TryCache(Transaction tx)
        {
            Guid distributedIdentifier = tx.TransactionInformation.DistributedIdentifier;
            string str = IsNativeIdentifier(this.identifier, distributedIdentifier) ? null : this.identifier;
            if (!string.IsNullOrEmpty(str) || (this.timeout != 0))
            {
                WsatExtendedInformationCache.Cache(tx, new WsatExtendedInformation(str, this.timeout));
            }
        }

        public string Identifier
        {
            get
            {
                return this.identifier;
            }
        }

        public uint Timeout
        {
            get
            {
                return this.timeout;
            }
        }
    }
}

