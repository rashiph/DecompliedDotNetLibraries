namespace Microsoft.Transactions.Wsat.Clusters
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    internal class SafeHResource : SafeClusterHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("clusapi.dll")]
        private static extern bool CloseClusterResource([In] IntPtr hResource);
        protected override bool ReleaseHandle()
        {
            return CloseClusterResource(base.handle);
        }
    }
}

