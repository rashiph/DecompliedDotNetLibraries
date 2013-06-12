namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationUninstallDeployment
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Size;
        [MarshalAs(UnmanagedType.U4)]
        public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)]
        public System.Deployment.Internal.Isolation.IDefinitionAppId Application;
        public IntPtr Reference;
        [SecuritySafeCritical]
        public StoreOperationUninstallDeployment(System.Deployment.Internal.Isolation.IDefinitionAppId appid, System.Deployment.Internal.Isolation.StoreApplicationReference AppRef)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(System.Deployment.Internal.Isolation.StoreOperationUninstallDeployment));
            this.Flags = OpFlags.Nothing;
            this.Application = appid;
            this.Reference = AppRef.ToIntPtr();
        }

        [SecurityCritical]
        public void Destroy()
        {
            System.Deployment.Internal.Isolation.StoreApplicationReference.Destroy(this.Reference);
        }
        public enum Disposition
        {
            Failed,
            DidNotExist,
            Uninstalled
        }

        [Flags]
        public enum OpFlags
        {
            Nothing
        }
    }
}

