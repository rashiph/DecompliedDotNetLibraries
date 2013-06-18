namespace System.Transactions.Oletx
{
    using System;

    [Serializable]
    internal class OletxRecoveryInformation
    {
        internal byte[] proxyRecoveryInformation;

        internal OletxRecoveryInformation(byte[] proxyRecoveryInformation)
        {
            this.proxyRecoveryInformation = proxyRecoveryInformation;
        }
    }
}

