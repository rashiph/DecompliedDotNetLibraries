namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationInstallDeployment
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Size;
        [MarshalAs(UnmanagedType.U4)]
        public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)]
        public System.Deployment.Internal.Isolation.IDefinitionAppId Application;
        public IntPtr Reference;
        public StoreOperationInstallDeployment(System.Deployment.Internal.Isolation.IDefinitionAppId App, System.Deployment.Internal.Isolation.StoreApplicationReference reference) : this(App, true, reference)
        {
        }

        [SecuritySafeCritical]
        public StoreOperationInstallDeployment(System.Deployment.Internal.Isolation.IDefinitionAppId App, bool UninstallOthers, System.Deployment.Internal.Isolation.StoreApplicationReference reference)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(System.Deployment.Internal.Isolation.StoreOperationInstallDeployment));
            this.Flags = OpFlags.Nothing;
            this.Application = App;
            if (UninstallOthers)
            {
                this.Flags |= OpFlags.UninstallOthers;
            }
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
            AlreadyInstalled,
            Installed
        }

        [Flags]
        public enum OpFlags
        {
            Nothing,
            UninstallOthers
        }
    }
}

