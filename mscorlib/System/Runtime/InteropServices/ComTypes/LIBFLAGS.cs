namespace System.Runtime.InteropServices.ComTypes
{
    using System;

    [Serializable, Flags]
    public enum LIBFLAGS : short
    {
        LIBFLAG_FCONTROL = 2,
        LIBFLAG_FHASDISKIMAGE = 8,
        LIBFLAG_FHIDDEN = 4,
        LIBFLAG_FRESTRICTED = 1
    }
}

