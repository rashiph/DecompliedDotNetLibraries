namespace System.Windows.Forms
{
    using System;

    public class DataGridViewAutoSizeModeEventArgs : EventArgs
    {
        private bool previousModeAutoSized;

        public DataGridViewAutoSizeModeEventArgs(bool previousModeAutoSized)
        {
            this.previousModeAutoSized = previousModeAutoSized;
        }

        public bool PreviousModeAutoSized
        {
            get
            {
                return this.previousModeAutoSized;
            }
        }
    }
}

