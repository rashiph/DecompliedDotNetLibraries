namespace System.Windows.Forms
{
    using System;

    internal class DataGridViewIntLinkedListElement
    {
        private int integer;
        private DataGridViewIntLinkedListElement next;

        public DataGridViewIntLinkedListElement(int integer)
        {
            this.integer = integer;
        }

        public int Int
        {
            get
            {
                return this.integer;
            }
            set
            {
                this.integer = value;
            }
        }

        public DataGridViewIntLinkedListElement Next
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

