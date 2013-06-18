namespace System.Windows.Forms
{
    using System;

    public class DataGridViewEditingControlShowingEventArgs : EventArgs
    {
        private DataGridViewCellStyle cellStyle;
        private System.Windows.Forms.Control control;

        public DataGridViewEditingControlShowingEventArgs(System.Windows.Forms.Control control, DataGridViewCellStyle cellStyle)
        {
            this.control = control;
            this.cellStyle = cellStyle;
        }

        public DataGridViewCellStyle CellStyle
        {
            get
            {
                return this.cellStyle;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.cellStyle = value;
            }
        }

        public System.Windows.Forms.Control Control
        {
            get
            {
                return this.control;
            }
        }
    }
}

