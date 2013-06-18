namespace Microsoft.Transactions.Wsat.Clusters
{
    using System;

    [Flags]
    internal enum ClusterEnum : uint
    {
        Group = 8,
        InternalNetwork = 0x80000000,
        NetInterface = 0x20,
        Network = 0x10,
        Node = 1,
        Resource = 4,
        ResType = 2
    }
}

