namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, ComVisible(true)]
    public enum ImporterEventKind
    {
        NOTIF_TYPECONVERTED,
        NOTIF_CONVERTWARNING,
        ERROR_REFTOINVALIDTYPELIB
    }
}

