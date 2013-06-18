namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;

    internal static class WnodeFlags
    {
        internal const uint WnodeFlagLogWnode = 0x40000;
        internal const uint WnodeFlagTracedGuid = 0x20000;
        internal const uint WnodeFlagUseGuidPointer = 0x80000;
        internal const uint WnodeFlagUseMofPointer = 0x100000;
        internal const uint WnodeFlagUseNoHeader = 0x200000;
    }
}

