namespace System.Runtime.InteropServices.ComTypes
{
    using System;

    [Serializable]
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

