namespace System.Windows.Forms
{
    using System;

    public class DataGridViewCellErrorTextNeededEventArgs : DataGridViewCellEventArgs
    {
        private string errorText;

        internal DataGridViewCellErrorTextNeededEventArgs(int columnIndex, int rowIndex, string errorText) : base(columnIndex, rowIndex)
        {
            this.errorText = errorText;
        }

        public string ErrorText
        {
            get
            {
                return this.errorText;
            }
            set
            {
                this.errorText = value;
            }
        }
    }
}

