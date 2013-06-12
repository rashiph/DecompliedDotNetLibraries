namespace System.Runtime.InteropServices.ComTypes
{
    using System;

    [Serializable, Flags]
    public enum IDLFLAG : short
    {
        IDLFLAG_FIN = 1,
        IDLFLAG_FLCID = 4,
        IDLFLAG_FOUT = 2,
        IDLFLAG_FRETVAL = 8,
        IDLFLAG_NONE = 0
    }
}

