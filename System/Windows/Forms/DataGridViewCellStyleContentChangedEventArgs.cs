namespace System.Windows.Forms
{
    using System;

    public class DataGridViewCellStyleContentChangedEventArgs : EventArgs
    {
        private bool changeAffectsPreferredSize;
        private DataGridViewCellStyle dataGridViewCellStyle;

        internal DataGridViewCellStyleContentChangedEventArgs(DataGridViewCellStyle dataGridViewCellStyle, bool changeAffectsPreferredSize)
        {
            this.dataGridViewCellStyle = dataGridViewCellStyle;
            this.changeAffectsPreferredSize = changeAffectsPreferredSize;
        }

        public DataGridViewCellStyle CellStyle
        {
            get
            {
                return this.dataGridViewCellStyle;
            }
        }

        public DataGridViewCellStyleScopes CellStyleScope
        {
            get
            {
                return this.dataGridViewCellStyle.Scope;
            }
        }

        internal bool ChangeAffectsPreferredSize
        {
            get
            {
                return this.changeAffectsPreferredSize;
            }
        }
    }
}

