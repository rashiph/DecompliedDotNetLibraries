namespace System.Net
{
    using System;
    using System.Diagnostics;
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

        [Conditional("TRAVE")]
        internal void DebugDump()
        {
        }
    }
}

