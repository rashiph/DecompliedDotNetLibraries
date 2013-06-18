namespace System.Windows.Forms.Internal
{
    using System;

    [Flags]
    internal enum WindowsPenStyle
    {
        Alternate = 8,
        Cosmetic = 0,
        Dash = 1,
        DashDot = 3,
        DashDotDot = 4,
        Default = 0,
        Dot = 2,
        EndcapFlat = 0x200,
        EndcapRound = 0,
        EndcapSquare = 0x100,
        Geometric = 0x10000,
        InsideFrame = 6,
        JoinBevel = 0x1000,
        JoinMiter = 0x2000,
        JoinRound = 0,
        Null = 5,
        Solid = 0,
        UserStyle = 7
    }
}

