namespace System.Web.Hosting
{
    using System;

    internal enum BufferType : byte
    {
        IISAllocatedRequestMemory = 2,
        Managed = 0,
        TransmitFile = 3,
        UnmanagedPool = 1
    }
}

