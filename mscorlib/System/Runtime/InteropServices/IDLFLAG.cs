namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, Obsolete("Use System.Runtime.InteropServices.ComTypes.IDLFLAG instead. http://go.microsoft.com/fwlink/?linkid=14202", false), Flags]
    public enum IDLFLAG : short
    {
        IDLFLAG_FIN = 1,
        IDLFLAG_FLCID = 4,
        IDLFLAG_FOUT = 2,
        IDLFLAG_FRETVAL = 8,
        IDLFLAG_NONE = 0
    }
}

