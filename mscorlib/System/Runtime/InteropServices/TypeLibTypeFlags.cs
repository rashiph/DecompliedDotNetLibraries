namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, Flags, ComVisible(true)]
    public enum TypeLibTypeFlags
    {
        FAggregatable = 0x400,
        FAppObject = 1,
        FCanCreate = 2,
        FControl = 0x20,
        FDispatchable = 0x1000,
        FDual = 0x40,
        FHidden = 0x10,
        FLicensed = 4,
        FNonExtensible = 0x80,
        FOleAutomation = 0x100,
        FPreDeclId = 8,
        FReplaceable = 0x800,
        FRestricted = 0x200,
        FReverseBind = 0x2000
    }
}

