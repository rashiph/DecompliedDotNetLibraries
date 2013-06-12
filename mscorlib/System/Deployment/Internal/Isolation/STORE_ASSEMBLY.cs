namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct STORE_ASSEMBLY
    {
        public uint Status;
        public IDefinitionIdentity DefinitionIdentity;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ManifestPath;
        public ulong AssemblySize;
        public ulong ChangeId;
    }
}

