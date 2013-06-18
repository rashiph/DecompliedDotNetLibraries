namespace System.Windows.Forms
{
    using System;

    internal class DataGridViewCellStyleChangedEventArgs : EventArgs
    {
        private bool changeAffectsPreferredSize;

        internal DataGridViewCellStyleChangedEventArgs()
        {
        }

        internal bool ChangeAffectsPreferredSize
        {
            get
            {
                return this.changeAffectsPreferredSize;
            }
            set
            {
                this.changeAffectsPreferredSize = value;
            }
        }
    }
}

