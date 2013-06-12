namespace System.Drawing.Drawing2D
{
    using System;
    using System.Drawing;

    public sealed class PathData
    {
        private PointF[] points;
        private byte[] types;

        public PointF[] Points
        {
            get
            {
                return this.points;
            }
            set
            {
                this.points = value;
            }
        }

        public byte[] Types
        {
            get
            {
                return this.types;
            }
            set
            {
                this.types = value;
            }
        }
    }
}

