namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, Flags, Obsolete("Use System.Runtime.InteropServices.ComTypes.LIBFLAGS instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public enum LIBFLAGS : short
    {
        LIBFLAG_FCONTROL = 2,
        LIBFLAG_FHASDISKIMAGE = 8,
        LIBFLAG_FHIDDEN = 4,
        LIBFLAG_FRESTRICTED = 1
    }
}

