namespace Microsoft.Transactions.Wsat.Clusters
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal abstract class SafeClusterHandle : SafeHandle
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        internal SafeClusterHandle() : base(IntPtr.Zero, true)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        internal SafeClusterHandle(IntPtr hcluster) : base(IntPtr.Zero, true)
        {
            base.SetHandle(hcluster);
        }

        public override bool IsInvalid
        {
            get
            {
                if (!base.IsClosed)
                {
                    return (base.handle == IntPtr.Zero);
                }
                return true;
            }
        }
    }
}

