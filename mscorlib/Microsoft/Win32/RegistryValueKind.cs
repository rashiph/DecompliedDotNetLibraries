namespace Microsoft.Win32
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public enum RegistryValueKind
    {
        Binary = 3,
        DWord = 4,
        ExpandString = 2,
        MultiString = 7,
        [ComVisible(false)]
        None = -1,
        QWord = 11,
        String = 1,
        Unknown = 0
    }
}

