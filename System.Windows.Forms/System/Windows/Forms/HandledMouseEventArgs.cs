namespace System.Windows.Forms
{
    using System;

    public class HandledMouseEventArgs : MouseEventArgs
    {
        private bool handled;

        public HandledMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta) : this(button, clicks, x, y, delta, false)
        {
        }

        public HandledMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta, bool defaultHandledValue) : base(button, clicks, x, y, delta)
        {
            this.handled = defaultHandledValue;
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
    }
}

