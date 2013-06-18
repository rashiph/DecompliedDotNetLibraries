namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class MouseEventArgs : EventArgs
    {
        private readonly MouseButtons button;
        private readonly int clicks;
        private readonly int delta;
        private readonly int x;
        private readonly int y;

        public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
        {
            this.button = button;
            this.clicks = clicks;
            this.x = x;
            this.y = y;
            this.delta = delta;
        }

        public MouseButtons Button
        {
            get
            {
                return this.button;
            }
        }

        public int Clicks
        {
            get
            {
                return this.clicks;
            }
        }

        public int Delta
        {
            get
            {
                return this.delta;
            }
        }

        public Point Location
        {
            get
            {
                return new Point(this.x, this.y);
            }
        }

        public int X
        {
            get
            {
                return this.x;
            }
        }

        public int Y
        {
            get
            {
                return this.y;
            }
        }
    }
}

