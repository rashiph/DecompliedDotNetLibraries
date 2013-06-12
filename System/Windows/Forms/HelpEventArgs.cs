namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class HelpEventArgs : EventArgs
    {
        private bool handled;
        private readonly Point mousePos;

        public HelpEventArgs(Point mousePos)
        {
            this.mousePos = mousePos;
        }

        public bool Handled
        {
            get
            {
                return this.handled;
            }
            set
            {
                this.handled = value;
            }
        }

        public Point MousePos
        {
            get
            {
                return this.mousePos;
            }
        }
    }
}

