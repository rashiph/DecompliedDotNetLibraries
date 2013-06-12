namespace System.Windows.Forms.Internal
{
    using System;

    [Flags]
    internal enum DeviceContextLayout
    {
        BitmapOrientationPreserved = 8,
        BottomToTop = 2,
        Normal = 0,
        RightToLeft = 1,
        VerticalBeforeHorizontal = 4
    }
}

