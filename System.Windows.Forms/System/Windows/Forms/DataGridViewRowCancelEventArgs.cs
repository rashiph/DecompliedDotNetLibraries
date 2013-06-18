namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class DataGridViewRowCancelEventArgs : CancelEventArgs
    {
        private DataGridViewRow dataGridViewRow;

        public DataGridViewRowCancelEventArgs(DataGridViewRow dataGridViewRow)
        {
            this.dataGridViewRow = dataGridViewRow;
        }

        public DataGridViewRow Row
        {
            get
            {
                return this.dataGridViewRow;
            }
        }
    }
}

