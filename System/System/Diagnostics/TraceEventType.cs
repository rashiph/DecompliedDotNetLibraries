namespace System.Diagnostics
{
    using System;
    using System.ComponentModel;

    public enum TraceEventType
    {
        Critical = 1,
        Error = 2,
        Information = 8,
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Resume = 0x800,
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Start = 0x100,
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Stop = 0x200,
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Suspend = 0x400,
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Transfer = 0x1000,
        Verbose = 0x10,
        Warning = 4
    }
}

