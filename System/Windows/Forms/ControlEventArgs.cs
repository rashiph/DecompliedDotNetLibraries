namespace System.Windows.Forms
{
    using System;

    public class ControlEventArgs : EventArgs
    {
        private System.Windows.Forms.Control control;

        public ControlEventArgs(System.Windows.Forms.Control control)
        {
            this.control = control;
        }

        public System.Windows.Forms.Control Control
        {
            get
            {
                return this.control;
            }
        }
    }
}

