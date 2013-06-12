namespace System.Windows.Forms
{
    using System;

    public class DataGridViewRowEventArgs : EventArgs
    {
        private DataGridViewRow dataGridViewRow;

        public DataGridViewRowEventArgs(DataGridViewRow dataGridViewRow)
        {
            if (dataGridViewRow == null)
            {
                throw new ArgumentNullException("dataGridViewRow");
            }
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

