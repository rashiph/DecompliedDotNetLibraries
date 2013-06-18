namespace System.ServiceModel
{
    using System;

    internal static class TransactionFlowOptionHelper
    {
        internal static bool AllowedOrRequired(TransactionFlowOption option)
        {
            if (option != TransactionFlowOption.Allowed)
            {
                return (option == TransactionFlowOption.Mandatory);
            }
            return true;
        }

        public static bool IsDefined(TransactionFlowOption option)
        {
            if ((option != TransactionFlowOption.NotAllowed) && (option != TransactionFlowOption.Allowed))
            {
                return (option == TransactionFlowOption.Mandatory);
            }
            return true;
        }
    }
}

