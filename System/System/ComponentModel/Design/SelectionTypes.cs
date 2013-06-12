namespace System.ComponentModel.Design
{
    using System;
    using System.Runtime.InteropServices;

    [Flags, ComVisible(true)]
    public enum SelectionTypes
    {
        Add = 0x40,
        Auto = 1,
        [Obsolete("This value has been deprecated. Use SelectionTypes.Primary instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        Click = 0x10,
        [Obsolete("This value has been deprecated.  It is no longer supported. http://go.microsoft.com/fwlink/?linkid=14202")]
        MouseDown = 4,
        [Obsolete("This value has been deprecated.  It is no longer supported. http://go.microsoft.com/fwlink/?linkid=14202")]
        MouseUp = 8,
        [Obsolete("This value has been deprecated. Use SelectionTypes.Auto instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        Normal = 1,
        Primary = 0x10,
        Remove = 0x80,
        Replace = 2,
        Toggle = 0x20,
        [Obsolete("This value has been deprecated. Use Enum class methods to determine valid values, or use a type converter. http://go.microsoft.com/fwlink/?linkid=14202")]
        Valid = 0x1f
    }
}

