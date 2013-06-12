namespace System.Runtime.InteropServices.ComTypes
{
    using System;

    [Flags]
    public enum ADVF
    {
        ADVF_DATAONSTOP = 0x40,
        ADVF_NODATA = 1,
        ADVF_ONLYONCE = 4,
        ADVF_PRIMEFIRST = 2,
        ADVFCACHE_FORCEBUILTIN = 0x10,
        ADVFCACHE_NOHANDLER = 8,
        ADVFCACHE_ONSAVE = 0x20
    }
}

