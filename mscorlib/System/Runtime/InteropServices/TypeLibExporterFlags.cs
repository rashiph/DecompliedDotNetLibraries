namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, Flags, ComVisible(true)]
    public enum TypeLibExporterFlags
    {
        CallerResolvedReferences = 2,
        ExportAs32Bit = 0x10,
        ExportAs64Bit = 0x20,
        None = 0,
        OldNames = 4,
        OnlyReferenceRegistered = 1
    }
}

