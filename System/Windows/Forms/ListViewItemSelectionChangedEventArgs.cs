namespace System.Windows.Forms
{
    using System;

    public class ListViewItemSelectionChangedEventArgs : EventArgs
    {
        private bool isSelected;
        private ListViewItem item;
        private int itemIndex;

        public ListViewItemSelectionChangedEventArgs(ListViewItem item, int itemIndex, bool isSelected)
        {
            this.item = item;
            this.itemIndex = itemIndex;
            this.isSelected = isSelected;
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
        }

        public ListViewItem Item
        {
            get
            {
                return this.item;
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

