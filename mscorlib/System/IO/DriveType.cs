namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum DriveType
    {
        Unknown,
        NoRootDirectory,
        Removable,
        Fixed,
        Network,
        CDRom,
        Ram
    }
}

