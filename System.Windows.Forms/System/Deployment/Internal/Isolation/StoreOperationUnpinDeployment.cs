namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationUnpinDeployment
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Size;
        [MarshalAs(UnmanagedType.U4)]
        public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)]
        public System.Deployment.Internal.Isolation.IDefinitionAppId Application;
        public IntPtr Reference;
        [SecuritySafeCritical]
        public StoreOperationUnpinDeployment(System.Deployment.Internal.Isolation.IDefinitionAppId app, System.Deployment.Internal.Isolation.StoreApplicationReference reference)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(System.Deployment.Internal.Isolation.StoreOperationUnpinDeployment));
            this.Flags = OpFlags.Nothing;
            this.Application = app;
            this.Reference = reference.ToIntPtr();
        }

        [SecurityCritical]
        public void Destroy()
        {
            System.Deployment.Internal.Isolation.StoreApplicationReference.Destroy(this.Reference);
        }
        public enum Disposition
        {
            Failed,
            Unpinned
        }

        [Flags]
        public enum OpFlags
        {
            Nothing
        }
    }
}

