namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class SecurityBufferDescriptor
    {
        public readonly int Version = 0;
        public readonly int Count;
        public unsafe void* UnmanagedPointer;
        public unsafe SecurityBufferDescriptor(int count)
        {
            this.Count = count;
            this.UnmanagedPointer = null;
        }
    }
}

