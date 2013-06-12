namespace System.Drawing.Internal
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class GPPOINTF
    {
        internal float X;
        internal float Y;
        internal GPPOINTF()
        {
        }

        internal GPPOINTF(PointF pt)
        {
            this.X = pt.X;
            this.Y = pt.Y;
        }

        internal GPPOINTF(Point pt)
        {
            this.X = pt.X;
            this.Y = pt.Y;
        }

        internal PointF ToPoint()
        {
            return new PointF(this.X, this.Y);
        }
    }
}

