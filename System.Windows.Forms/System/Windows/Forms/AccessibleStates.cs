namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum AccessibleStates
    {
        AlertHigh = 0x10000000,
        AlertLow = 0x4000000,
        AlertMedium = 0x8000000,
        Animated = 0x4000,
        Busy = 0x800,
        Checked = 0x10,
        Collapsed = 0x400,
        Default = 0x100,
        Expanded = 0x200,
        ExtSelectable = 0x2000000,
        Floating = 0x1000,
        Focusable = 0x100000,
        Focused = 4,
        HasPopup = 0x40000000,
        HotTracked = 0x80,
        Indeterminate = 0x20,
        Invisible = 0x8000,
        Linked = 0x400000,
        Marqueed = 0x2000,
        Mixed = 0x20,
        Moveable = 0x40000,
        MultiSelectable = 0x1000000,
        None = 0,
        Offscreen = 0x10000,
        Pressed = 8,
        Protected = 0x20000000,
        ReadOnly = 0x40,
        Selectable = 0x200000,
        Selected = 2,
        SelfVoicing = 0x80000,
        Sizeable = 0x20000,
        Traversed = 0x800000,
        Unavailable = 1,
        [Obsolete("This enumeration value has been deprecated. There is no replacement. http://go.microsoft.com/fwlink/?linkid=14202")]
        Valid = 0x3fffffff
    }
}

