namespace System.Web.UI.WebControls
{
    using System;

    public class DataListItemEventArgs : EventArgs
    {
        private DataListItem item;

        public DataListItemEventArgs(DataListItem item)
        {
            this.item = item;
        }

        public DataListItem Item
        {
            get
            {
                return this.item;
            }
        }
    }
}

