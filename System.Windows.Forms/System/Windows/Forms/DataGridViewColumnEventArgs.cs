namespace System.Windows.Forms
{
    using System;

    public class DataGridViewColumnEventArgs : EventArgs
    {
        private DataGridViewColumn dataGridViewColumn;

        public DataGridViewColumnEventArgs(DataGridViewColumn dataGridViewColumn)
        {
            if (dataGridViewColumn == null)
            {
                throw new ArgumentNullException("dataGridViewColumn");
            }
            this.dataGridViewColumn = dataGridViewColumn;
        }

        public DataGridViewColumn Column
        {
            get
            {
                return this.dataGridViewColumn;
            }
        }
    }
}

