namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, Obsolete("Use System.Runtime.InteropServices.ComTypes.TYPEKIND instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public enum TYPEKIND
    {
        TKIND_ENUM,
        TKIND_RECORD,
        TKIND_MODULE,
        TKIND_INTERFACE,
        TKIND_DISPATCH,
        TKIND_COCLASS,
        TKIND_ALIAS,
        TKIND_UNION,
        TKIND_MAX
    }
}

