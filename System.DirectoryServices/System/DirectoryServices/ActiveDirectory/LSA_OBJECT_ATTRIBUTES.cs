namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class LSA_OBJECT_ATTRIBUTES
    {
        internal int Length = 0;
        private IntPtr RootDirectory = IntPtr.Zero;
        private IntPtr ObjectName = IntPtr.Zero;
        internal int Attributes = 0;
        private IntPtr SecurityDescriptor = IntPtr.Zero;
        private IntPtr SecurityQualityOfService = IntPtr.Zero;
    }
}

