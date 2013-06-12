namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class ContentsResizedEventArgs : EventArgs
    {
        private readonly Rectangle newRectangle;

        public ContentsResizedEventArgs(Rectangle newRectangle)
        {
            this.newRectangle = newRectangle;
        }

        public Rectangle NewRectangle
        {
            get
            {
                return this.newRectangle;
            }
        }
    }
}

