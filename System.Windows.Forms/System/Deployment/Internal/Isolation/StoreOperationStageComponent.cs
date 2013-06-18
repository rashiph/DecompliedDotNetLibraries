namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationStageComponent
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Size;
        [MarshalAs(UnmanagedType.U4)]
        public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)]
        public System.Deployment.Internal.Isolation.IDefinitionAppId Application;
        [MarshalAs(UnmanagedType.Interface)]
        public System.Deployment.Internal.Isolation.IDefinitionIdentity Component;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ManifestPath;
        public void Destroy()
        {
        }

        public StoreOperationStageComponent(System.Deployment.Internal.Isolation.IDefinitionAppId app, string Manifest) : this(app, null, Manifest)
        {
        }

        [SecuritySafeCritical]
        public StoreOperationStageComponent(System.Deployment.Internal.Isolation.IDefinitionAppId app, System.Deployment.Internal.Isolation.IDefinitionIdentity comp, string Manifest)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(System.Deployment.Internal.Isolation.StoreOperationStageComponent));
            this.Flags = OpFlags.Nothing;
            this.Application = app;
            this.Component = comp;
            this.ManifestPath = Manifest;
        }
        public enum Disposition
        {
            Failed,
            Installed,
            Refreshed,
            AlreadyInstalled
        }

        [Flags]
        public enum OpFlags
        {
            Nothing
        }
    }
}

