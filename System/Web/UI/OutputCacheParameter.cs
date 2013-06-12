namespace System.Web.UI
{
    using System;

    [Flags]
    internal enum OutputCacheParameter
    {
        CacheProfile = 1,
        Duration = 2,
        Enabled = 4,
        Location = 8,
        NoStore = 0x10,
        SqlDependency = 0x20,
        VaryByContentEncoding = 0x400,
        VaryByControl = 0x40,
        VaryByCustom = 0x80,
        VaryByHeader = 0x100,
        VaryByParam = 0x200
    }
}

