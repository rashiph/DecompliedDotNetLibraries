namespace System.Runtime.InteropServices.ComTypes
{
    using System;

    [Serializable, Flags]
    public enum VARFLAGS : short
    {
        VARFLAG_FBINDABLE = 4,
        VARFLAG_FDEFAULTBIND = 0x20,
        VARFLAG_FDEFAULTCOLLELEM = 0x100,
        VARFLAG_FDISPLAYBIND = 0x10,
        VARFLAG_FHIDDEN = 0x40,
        VARFLAG_FIMMEDIATEBIND = 0x1000,
        VARFLAG_FNONBROWSABLE = 0x400,
        VARFLAG_FREADONLY = 1,
        VARFLAG_FREPLACEABLE = 0x800,
        VARFLAG_FREQUESTEDIT = 8,
        VARFLAG_FRESTRICTED = 0x80,
        VARFLAG_FSOURCE = 2,
        VARFLAG_FUIDEFAULT = 0x200
    }
}

