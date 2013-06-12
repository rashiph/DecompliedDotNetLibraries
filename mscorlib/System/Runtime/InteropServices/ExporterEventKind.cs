namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, ComVisible(true)]
    public enum ExporterEventKind
    {
        NOTIF_TYPECONVERTED,
        NOTIF_CONVERTWARNING,
        ERROR_REFTOINVALIDASSEMBLY
    }
}

