namespace System.Web.UI.WebControls
{
    using System;
    using System.Runtime;
    using System.Web.UI;

    public class DataGridItem : TableRow, IDataItemContainer, INamingContainer
    {
        private object dataItem;
        private int dataSetIndex;
        private int itemIndex;
        private ListItemType itemType;

        public DataGridItem(int itemIndex, int dataSetIndex, ListItemType itemType)
        {
            this.itemIndex = itemIndex;
            this.dataSetIndex = dataSetIndex;
            this.itemType = itemType;
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            if (e is CommandEventArgs)
            {
                DataGridCommandEventArgs args = new DataGridCommandEventArgs(this, source, (CommandEventArgs) e);
                base.RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }

        protected internal virtual void SetItemType(ListItemType itemType)
        {
            this.itemType = itemType;
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

        public virtual int DataSetIndex
        {
            get
            {
                return this.dataSetIndex;
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
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.itemType;
            }
        }

        object IDataItemContainer.DataItem
        {
            get
            {
                return this.DataItem;
            }
        }

        int IDataItemContainer.DataItemIndex
        {
            get
            {
                return this.DataSetIndex;
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

