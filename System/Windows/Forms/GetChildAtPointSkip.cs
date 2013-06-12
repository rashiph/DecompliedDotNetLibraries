namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum GetChildAtPointSkip
    {
        Disabled = 2,
        Invisible = 1,
        None = 0,
        Transparent = 4
    }
}

