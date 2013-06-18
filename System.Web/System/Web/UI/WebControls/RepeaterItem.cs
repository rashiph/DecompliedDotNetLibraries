namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    [ToolboxItem(false)]
    public class RepeaterItem : Control, IDataItemContainer, INamingContainer
    {
        private object dataItem;
        private int itemIndex;
        private ListItemType itemType;

        public RepeaterItem(int itemIndex, ListItemType itemType)
        {
            this.itemIndex = itemIndex;
            this.itemType = itemType;
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            if (e is CommandEventArgs)
            {
                RepeaterCommandEventArgs args = new RepeaterCommandEventArgs(this, source, (CommandEventArgs) e);
                base.RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }

        public virtual object DataItem
        {
            get
            {
                return this.dataItem;
            }
            set
            {
                this.dataItem = value;
            }
        }

        public virtual int ItemIndex
        {
            get
            {
                return this.itemIndex;
            }
        }

        public virtual ListItemType ItemType
        {
            get
            {
                return this.itemType;
            }
        }

        int IDataItemContainer.DataItemIndex
        {
            get
            {
                return this.ItemIndex;
            }
        }

        int IDataItemContainer.DisplayIndex
        {
            get
            {
                return this.ItemIndex;
            }
        }
    }
}

