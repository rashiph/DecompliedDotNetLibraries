namespace System.Runtime.InteropServices.ComTypes
{
    using System;

    [Serializable, Flags]
    public enum PARAMFLAG : short
    {
        PARAMFLAG_FHASCUSTDATA = 0x40,
        PARAMFLAG_FHASDEFAULT = 0x20,
        PARAMFLAG_FIN = 1,
        PARAMFLAG_FLCID = 4,
        PARAMFLAG_FOPT = 0x10,
        PARAMFLAG_FOUT = 2,
        PARAMFLAG_FRETVAL = 8,
        PARAMFLAG_NONE = 0
    }
}

