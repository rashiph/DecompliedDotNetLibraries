namespace System.Windows.Forms
{
    using System;

    public class ItemCheckedEventArgs : EventArgs
    {
        private ListViewItem lvi;

        public ItemCheckedEventArgs(ListViewItem item)
        {
            this.lvi = item;
        }

        public ListViewItem Item
        {
            get
            {
                return this.lvi;
            }
        }
    }
}

