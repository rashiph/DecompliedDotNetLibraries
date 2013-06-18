namespace System.Drawing.Drawing2D
{
    using System;
    using System.Drawing;

    public sealed class ColorBlend
    {
        private Color[] colors;
        private float[] positions;

        public ColorBlend()
        {
            this.colors = new Color[1];
            this.positions = new float[1];
        }

        public ColorBlend(int count)
        {
            this.colors = new Color[count];
            this.positions = new float[count];
        }

        public Color[] Colors
        {
            get
            {
                return this.colors;
            }
            set
            {
                this.colors = value;
            }
        }

        public float[] Positions
        {
            get
            {
                return this.positions;
            }
            set
            {
                this.positions = value;
            }
        }
    }
}

