namespace System.Deployment.Application
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ASSEMBLYMETADATA
    {
        public ushort usMajorVersion;
        public ushort usMinorVersion;
        public ushort usBuildNumber;
        public ushort usRevisionNumber;
        public IntPtr rpLocale;
        public uint cchLocale;
        public IntPtr rpProcessors;
        public uint cProcessors;
        public IntPtr rOses;
        public uint cOses;
    }
}

