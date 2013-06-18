namespace System.ServiceModel.Transactions
{
    using System;
    using System.ServiceModel;
    using System.Transactions;

    internal class OleTxTransactionInfo : TransactionInfo
    {
        private OleTxTransactionHeader header;

        public OleTxTransactionInfo(OleTxTransactionHeader header)
        {
            this.header = header;
        }

        public static Transaction UnmarshalPropagationToken(byte[] propToken)
        {
            Transaction transactionFromTransmitterPropagationToken;
            try
            {
                transactionFromTransmitterPropagationToken = TransactionInterop.GetTransactionFromTransmitterPropagationToken(propToken);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("InvalidPropagationToken"), exception));
            }
            return transactionFromTransmitterPropagationToken;
        }

        public override Transaction UnmarshalTransaction()
        {
            Transaction tx = UnmarshalPropagationToken(this.header.PropagationToken);
            if (this.header.WsatExtendedInformation != null)
            {
                this.header.WsatExtendedInformation.TryCache(tx);
            }
            return tx;
        }
    }
}

