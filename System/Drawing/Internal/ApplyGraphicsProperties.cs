namespace System.Drawing.Internal
{
    using System;

    [Flags]
    internal enum ApplyGraphicsProperties
    {
        None,
        Clipping,
        TranslateTransform,
        All
    }
}

