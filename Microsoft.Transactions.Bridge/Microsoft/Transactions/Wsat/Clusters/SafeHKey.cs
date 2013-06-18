namespace Microsoft.Transactions.Wsat.Clusters
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    internal class SafeHKey : SafeClusterHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("clusapi.dll")]
        private static extern int ClusterRegCloseKey([In] IntPtr hEnum);
        protected override bool ReleaseHandle()
        {
            return (ClusterRegCloseKey(base.handle) == 0L);
        }
    }
}

