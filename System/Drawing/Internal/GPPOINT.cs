namespace System.Drawing.Internal
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class GPPOINT
    {
        internal int X;
        internal int Y;
        internal GPPOINT()
        {
        }

        internal GPPOINT(PointF pt)
        {
            this.X = (int) pt.X;
            this.Y = (int) pt.Y;
        }

        internal GPPOINT(Point pt)
        {
            this.X = pt.X;
            this.Y = pt.Y;
        }

        internal PointF ToPoint()
        {
            return new PointF((float) this.X, (float) this.Y);
        }
    }
}

