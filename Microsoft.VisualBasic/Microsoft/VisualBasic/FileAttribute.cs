namespace Microsoft.VisualBasic
{
    using System;

    [Flags]
    public enum FileAttribute
    {
        Archive = 0x20,
        Directory = 0x10,
        Hidden = 2,
        Normal = 0,
        ReadOnly = 1,
        System = 4,
        Volume = 8
    }
}

