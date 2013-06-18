namespace System.Windows.Forms
{
    using System;

    public class ListViewHitTestInfo
    {
        private ListViewItem item;
        private ListViewHitTestLocations loc;
        private ListViewItem.ListViewSubItem subItem;

        public ListViewHitTestInfo(ListViewItem hitItem, ListViewItem.ListViewSubItem hitSubItem, ListViewHitTestLocations hitLocation)
        {
            this.item = hitItem;
            this.subItem = hitSubItem;
            this.loc = hitLocation;
        }

        public ListViewItem Item
        {
            get
            {
                return this.item;
            }
        }

        public ListViewHitTestLocations Location
        {
            get
            {
                return this.loc;
            }
        }

        public ListViewItem.ListViewSubItem SubItem
        {
            get
            {
                return this.subItem;
            }
        }
    }
}

