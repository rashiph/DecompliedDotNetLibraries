namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AssemblyMetadata
    {
        public short MajorVersion;
        public short MinorVersion;
        public short uuildNumber;
        public short RevisionNumber;
        public IntPtr Locale;
        public IntPtr Processor;
        public int ulProcessor;
        public IntPtr rOS;
        public int ulOS;
    }
}

