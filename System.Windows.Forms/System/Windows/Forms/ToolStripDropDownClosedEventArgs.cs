namespace System.Windows.Forms
{
    using System;

    public class ToolStripDropDownClosedEventArgs : EventArgs
    {
        private ToolStripDropDownCloseReason closeReason;

        public ToolStripDropDownClosedEventArgs(ToolStripDropDownCloseReason reason)
        {
            this.closeReason = reason;
        }

        public ToolStripDropDownCloseReason CloseReason
        {
            get
            {
                return this.closeReason;
            }
        }
    }
}

