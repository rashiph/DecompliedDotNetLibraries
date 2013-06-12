namespace System.Windows.Forms
{
    using System;

    public class DataGridViewCellStateChangedEventArgs : EventArgs
    {
        private DataGridViewCell dataGridViewCell;
        private DataGridViewElementStates stateChanged;

        public DataGridViewCellStateChangedEventArgs(DataGridViewCell dataGridViewCell, DataGridViewElementStates stateChanged)
        {
            if (dataGridViewCell == null)
            {
                throw new ArgumentNullException("dataGridViewCell");
            }
            this.dataGridViewCell = dataGridViewCell;
            this.stateChanged = stateChanged;
        }

        public DataGridViewCell Cell
        {
            get
            {
                return this.dataGridViewCell;
            }
        }

        public DataGridViewElementStates StateChanged
        {
            get
            {
                return this.stateChanged;
            }
        }
    }
}

