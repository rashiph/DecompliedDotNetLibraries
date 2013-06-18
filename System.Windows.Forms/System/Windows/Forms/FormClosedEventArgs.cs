namespace System.Windows.Forms
{
    using System;

    public class FormClosedEventArgs : EventArgs
    {
        private System.Windows.Forms.CloseReason closeReason;

        public FormClosedEventArgs(System.Windows.Forms.CloseReason closeReason)
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

