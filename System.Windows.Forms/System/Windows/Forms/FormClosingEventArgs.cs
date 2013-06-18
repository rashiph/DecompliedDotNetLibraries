namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class FormClosingEventArgs : CancelEventArgs
    {
        private System.Windows.Forms.CloseReason closeReason;

        public FormClosingEventArgs(System.Windows.Forms.CloseReason closeReason, bool cancel) : base(cancel)
        {
            this.closeReason = closeReason;
        }

        public System.Windows.Forms.CloseReason CloseReason
        {
            get
            {
                return this.closeReason;
            }
        }
    }
}

