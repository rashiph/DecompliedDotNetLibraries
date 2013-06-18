namespace System.ServiceModel
{
    using System;

    internal class WSAtomicTransaction11Protocol : TransactionProtocol
    {
        private static TransactionProtocol instance = new WSAtomicTransaction11Protocol();

        internal static TransactionProtocol Instance
        {
            get
            {
                return instance;
            }
        }

        internal override string Name
        {
            get
            {
                return "WSAtomicTransaction11";
            }
        }
    }
}

