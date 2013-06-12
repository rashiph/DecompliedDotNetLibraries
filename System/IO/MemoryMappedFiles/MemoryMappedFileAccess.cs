namespace System.IO.MemoryMappedFiles
{
    using System;

    [Serializable]
    public enum MemoryMappedFileAccess
    {
        ReadWrite,
        Read,
        Write,
        CopyOnWrite,
        ReadExecute,
        ReadWriteExecute
    }
}

