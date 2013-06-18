namespace System.Windows.Forms
{
    using System;

    public class DataGridViewAutoSizeColumnsModeEventArgs : EventArgs
    {
        private DataGridViewAutoSizeColumnMode[] previousModes;

        public DataGridViewAutoSizeColumnsModeEventArgs(DataGridViewAutoSizeColumnMode[] previousModes)
        {
            this.previousModes = previousModes;
        }

        public DataGridViewAutoSizeColumnMode[] PreviousModes
        {
            get
            {
                return this.previousModes;
            }
        }
    }
}

