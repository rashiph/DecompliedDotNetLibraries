namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationPinDeployment
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Size;
        [MarshalAs(UnmanagedType.U4)]
        public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)]
        public System.Deployment.Internal.Isolation.IDefinitionAppId Application;
        [MarshalAs(UnmanagedType.I8)]
        public long ExpirationTime;
        public IntPtr Reference;
        [SecuritySafeCritical]
        public StoreOperationPinDeployment(System.Deployment.Internal.Isolation.IDefinitionAppId AppId, System.Deployment.Internal.Isolation.StoreApplicationReference Ref)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(System.Deployment.Internal.Isolation.StoreOperationPinDeployment));
            this.Flags = OpFlags.NeverExpires;
            this.Application = AppId;
            this.Reference = Ref.ToIntPtr();
            this.ExpirationTime = 0L;
        }

        public StoreOperationPinDeployment(System.Deployment.Internal.Isolation.IDefinitionAppId AppId, DateTime Expiry, System.Deployment.Internal.Isolation.StoreApplicationReference Ref) : this(AppId, Ref)
        {
            this.Flags |= OpFlags.NeverExpires;
        }

        [SecurityCritical]
        public void Destroy()
        {
            System.Deployment.Internal.Isolation.StoreApplicationReference.Destroy(this.Reference);
        }
        public enum Disposition
        {
            Failed,
            Pinned
        }

        [Flags]
        public enum OpFlags
        {
            Nothing,
            NeverExpires
        }
    }
}

