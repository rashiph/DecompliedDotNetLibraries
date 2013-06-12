namespace System.IO.MemoryMappedFiles
{
    using System;

    [Serializable, Flags]
    public enum MemoryMappedFileOptions
    {
        DelayAllocatePages = 0x4000000,
        None = 0
    }
}

