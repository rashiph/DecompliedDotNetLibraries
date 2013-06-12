namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class ToolStripDropDownClosingEventArgs : CancelEventArgs
    {
        private ToolStripDropDownCloseReason closeReason;

        public ToolStripDropDownClosingEventArgs(ToolStripDropDownCloseReason reason)
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

