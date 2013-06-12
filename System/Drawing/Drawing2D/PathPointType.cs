namespace System.Drawing.Drawing2D
{
    using System;

    public enum PathPointType
    {
        Bezier = 3,
        Bezier3 = 3,
        CloseSubpath = 0x80,
        DashMode = 0x10,
        Line = 1,
        PathMarker = 0x20,
        PathTypeMask = 7,
        Start = 0
    }
}

