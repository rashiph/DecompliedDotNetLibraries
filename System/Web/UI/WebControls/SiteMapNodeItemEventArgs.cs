namespace System.Web.UI.WebControls
{
    using System;

    public class SiteMapNodeItemEventArgs : EventArgs
    {
        private SiteMapNodeItem _item;

        public SiteMapNodeItemEventArgs(SiteMapNodeItem item)
        {
            this._item = item;
        }

        public SiteMapNodeItem Item
        {
            get
            {
                return this._item;
            }
        }
    }
}

