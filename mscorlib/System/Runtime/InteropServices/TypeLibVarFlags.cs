namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, ComVisible(true), Flags]
    public enum TypeLibVarFlags
    {
        FBindable = 4,
        FDefaultBind = 0x20,
        FDefaultCollelem = 0x100,
        FDisplayBind = 0x10,
        FHidden = 0x40,
        FImmediateBind = 0x1000,
        FNonBrowsable = 0x400,
        FReadOnly = 1,
        FReplaceable = 0x800,
        FRequestEdit = 8,
        FRestricted = 0x80,
        FSource = 2,
        FUiDefault = 0x200
    }
}

