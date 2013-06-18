namespace Microsoft.Transactions.Wsat.Clusters
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    internal class SafeHCluster : SafeClusterHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("clusapi.dll")]
        private static extern bool CloseCluster([In] IntPtr hCluster);
        protected override bool ReleaseHandle()
        {
            return CloseCluster(base.handle);
        }
    }
}

