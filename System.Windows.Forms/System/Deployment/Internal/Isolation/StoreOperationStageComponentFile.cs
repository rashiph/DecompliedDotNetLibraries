namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationStageComponentFile
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
        public string ComponentRelativePath;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string SourceFilePath;
        public StoreOperationStageComponentFile(System.Deployment.Internal.Isolation.IDefinitionAppId App, string CompRelPath, string SrcFile) : this(App, null, CompRelPath, SrcFile)
        {
        }

        [SecuritySafeCritical]
        public StoreOperationStageComponentFile(System.Deployment.Internal.Isolation.IDefinitionAppId App, System.Deployment.Internal.Isolation.IDefinitionIdentity Component, string CompRelPath, string SrcFile)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(System.Deployment.Internal.Isolation.StoreOperationStageComponentFile));
            this.Flags = OpFlags.Nothing;
            this.Application = App;
            this.Component = Component;
            this.ComponentRelativePath = CompRelPath;
            this.SourceFilePath = SrcFile;
        }

        public void Destroy()
        {
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

