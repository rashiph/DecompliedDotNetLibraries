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
        public IDefinitionAppId Application;
        public IntPtr Reference;
        public StoreOperationInstallDeployment(IDefinitionAppId App, StoreApplicationReference reference) : this(App, true, reference)
        {
        }

        [SecuritySafeCritical]
        public StoreOperationInstallDeployment(IDefinitionAppId App, bool UninstallOthers, StoreApplicationReference reference)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(StoreOperationInstallDeployment));
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
            StoreApplicationReference.Destroy(this.Reference);
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

