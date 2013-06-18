namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    internal class ToolStripLocationCancelEventArgs : CancelEventArgs
    {
        private Point newLocation;

        public ToolStripLocationCancelEventArgs(Point newLocation, bool value) : base(value)
        {
            this.newLocation = newLocation;
        }

        public Point NewLocation
        {
            get
            {
                return this.newLocation;
            }
        }
    }
}

