namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ToolboxItem(false)]
    public class SiteMapNodeItem : WebControl, IDataItemContainer, INamingContainer
    {
        private int _itemIndex;
        private SiteMapNodeItemType _itemType;
        private System.Web.SiteMapNode _siteMapNode;

        public SiteMapNodeItem(int itemIndex, SiteMapNodeItemType itemType)
        {
            this._itemIndex = itemIndex;
            this._itemType = itemType;
        }

        protected internal virtual void SetItemType(SiteMapNodeItemType itemType)
        {
            this._itemType = itemType;
        }

        public virtual int ItemIndex
        {
            get
            {
                return this._itemIndex;
            }
        }

        public virtual SiteMapNodeItemType ItemType
        {
            get
            {
                return this._itemType;
            }
        }

        public virtual System.Web.SiteMapNode SiteMapNode
        {
            get
            {
                return this._siteMapNode;
            }
            set
            {
                this._siteMapNode = value;
            }
        }

        object IDataItemContainer.DataItem
        {
            get
            {
                return this.SiteMapNode;
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

