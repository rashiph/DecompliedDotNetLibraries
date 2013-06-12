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
        public IDefinitionAppId Application;
        [MarshalAs(UnmanagedType.I8)]
        public long ExpirationTime;
        public IntPtr Reference;
        [SecuritySafeCritical]
        public StoreOperationPinDeployment(IDefinitionAppId AppId, StoreApplicationReference Ref)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(StoreOperationPinDeployment));
            this.Flags = OpFlags.NeverExpires;
            this.Application = AppId;
            this.Reference = Ref.ToIntPtr();
            this.ExpirationTime = 0L;
        }

        public StoreOperationPinDeployment(IDefinitionAppId AppId, DateTime Expiry, StoreApplicationReference Ref) : this(AppId, Ref)
        {
            this.Flags |= OpFlags.NeverExpires;
        }

        [SecurityCritical]
        public void Destroy()
        {
            StoreApplicationReference.Destroy(this.Reference);
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

