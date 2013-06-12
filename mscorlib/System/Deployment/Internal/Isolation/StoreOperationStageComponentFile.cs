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
        public IDefinitionAppId Application;
        [MarshalAs(UnmanagedType.Interface)]
        public IDefinitionIdentity Component;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ComponentRelativePath;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string SourceFilePath;
        public StoreOperationStageComponentFile(IDefinitionAppId App, string CompRelPath, string SrcFile) : this(App, null, CompRelPath, SrcFile)
        {
        }

        [SecuritySafeCritical]
        public StoreOperationStageComponentFile(IDefinitionAppId App, IDefinitionIdentity Component, string CompRelPath, string SrcFile)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(StoreOperationStageComponentFile));
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

