namespace System.Windows.Forms
{
    using System;

    internal class DataGridViewCellLinkedListElement
    {
        private System.Windows.Forms.DataGridViewCell dataGridViewCell;
        private DataGridViewCellLinkedListElement next;

        public DataGridViewCellLinkedListElement(System.Windows.Forms.DataGridViewCell dataGridViewCell)
        {
            this.dataGridViewCell = dataGridViewCell;
        }

        public System.Windows.Forms.DataGridViewCell DataGridViewCell
        {
            get
            {
                return this.dataGridViewCell;
            }
        }

        public DataGridViewCellLinkedListElement Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }
    }
}

