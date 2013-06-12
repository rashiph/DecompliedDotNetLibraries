namespace System.Runtime.InteropServices.ComTypes
{
    using System;

    [Serializable, Flags]
    public enum TYPEFLAGS : short
    {
        TYPEFLAG_FAGGREGATABLE = 0x400,
        TYPEFLAG_FAPPOBJECT = 1,
        TYPEFLAG_FCANCREATE = 2,
        TYPEFLAG_FCONTROL = 0x20,
        TYPEFLAG_FDISPATCHABLE = 0x1000,
        TYPEFLAG_FDUAL = 0x40,
        TYPEFLAG_FHIDDEN = 0x10,
        TYPEFLAG_FLICENSED = 4,
        TYPEFLAG_FNONEXTENSIBLE = 0x80,
        TYPEFLAG_FOLEAUTOMATION = 0x100,
        TYPEFLAG_FPREDECLID = 8,
        TYPEFLAG_FPROXY = 0x4000,
        TYPEFLAG_FREPLACEABLE = 0x800,
        TYPEFLAG_FRESTRICTED = 0x200,
        TYPEFLAG_FREVERSEBIND = 0x2000
    }
}

