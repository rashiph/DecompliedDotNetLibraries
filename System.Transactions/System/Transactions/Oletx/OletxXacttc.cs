namespace System.Transactions.Oletx
{
    using System;

    [Flags]
    internal enum OletxXacttc
    {
        XACTTC_ASYNC = 4,
        XACTTC_ASYNC_PHASEONE = 4,
        XACTTC_NONE = 0,
        XACTTC_SYNC = 2,
        XACTTC_SYNC_PHASEONE = 1,
        XACTTC_SYNC_PHASETWO = 2
    }
}

