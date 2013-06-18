namespace System.Data
{
    using System;
    using System.ComponentModel;

    [Obsolete("PropertyAttributes has been deprecated.  http://go.microsoft.com/fwlink/?linkid=14202"), EditorBrowsable(EditorBrowsableState.Never), Flags]
    public enum PropertyAttributes
    {
        NotSupported = 0,
        Optional = 2,
        Read = 0x200,
        Required = 1,
        Write = 0x400
    }
}

