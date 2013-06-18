namespace System.Web.UI.WebControls
{
    using System;

    public class DataGridItemEventArgs : EventArgs
    {
        private DataGridItem item;

        public DataGridItemEventArgs(DataGridItem item)
        {
            this.item = item;
        }

        public DataGridItem Item
        {
            get
            {
                return this.item;
            }
        }
    }
}

