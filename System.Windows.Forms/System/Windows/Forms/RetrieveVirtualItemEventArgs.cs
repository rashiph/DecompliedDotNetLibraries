namespace System.Windows.Forms
{
    using System;

    public class RetrieveVirtualItemEventArgs : EventArgs
    {
        private ListViewItem item;
        private int itemIndex;

        public RetrieveVirtualItemEventArgs(int itemIndex)
        {
            this.itemIndex = itemIndex;
        }

        public ListViewItem Item
        {
            get
            {
                return this.item;
            }
            set
            {
                this.item = value;
            }
        }

        public int ItemIndex
        {
            get
            {
                return this.itemIndex;
            }
        }
    }
}

