namespace System.Runtime.InteropServices.ComTypes
{
    using System;

    [Serializable, Flags]
    public enum FUNCFLAGS : short
    {
        FUNCFLAG_FBINDABLE = 4,
        FUNCFLAG_FDEFAULTBIND = 0x20,
        FUNCFLAG_FDEFAULTCOLLELEM = 0x100,
        FUNCFLAG_FDISPLAYBIND = 0x10,
        FUNCFLAG_FHIDDEN = 0x40,
        FUNCFLAG_FIMMEDIATEBIND = 0x1000,
        FUNCFLAG_FNONBROWSABLE = 0x400,
        FUNCFLAG_FREPLACEABLE = 0x800,
        FUNCFLAG_FREQUESTEDIT = 8,
        FUNCFLAG_FRESTRICTED = 1,
        FUNCFLAG_FSOURCE = 2,
        FUNCFLAG_FUIDEFAULT = 0x200,
        FUNCFLAG_FUSESGETLASTERROR = 0x80
    }
}

