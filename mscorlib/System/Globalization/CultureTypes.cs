namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum CultureTypes
    {
        AllCultures = 7,
        [Obsolete("This value has been deprecated.  Please use other values in CultureTypes.")]
        FrameworkCultures = 0x40,
        InstalledWin32Cultures = 4,
        NeutralCultures = 1,
        ReplacementCultures = 0x10,
        SpecificCultures = 2,
        UserCustomCulture = 8,
        [Obsolete("This value has been deprecated.  Please use other values in CultureTypes.")]
        WindowsOnlyCultures = 0x20
    }
}

