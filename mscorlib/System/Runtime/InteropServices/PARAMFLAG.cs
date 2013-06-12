namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, Obsolete("Use System.Runtime.InteropServices.ComTypes.PARAMFLAG instead. http://go.microsoft.com/fwlink/?linkid=14202", false), Flags]
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

