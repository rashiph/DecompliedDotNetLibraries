namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [ToolboxItem(false)]
    public class DataListItem : WebControl, IDataItemContainer, INamingContainer
    {
        private object dataItem;
        private int itemIndex;
        private ListItemType itemType;

        public DataListItem(int itemIndex, ListItemType itemType)
        {
            this.itemIndex = itemIndex;
            this.itemType = itemType;
        }

        protected override Style CreateControlStyle()
        {
            return new TableItemStyle();
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            if (e is CommandEventArgs)
            {
                DataListCommandEventArgs args = new DataListCommandEventArgs(this, source, (CommandEventArgs) e);
                base.RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }

        public virtual void RenderItem(HtmlTextWriter writer, bool extractRows, bool tableLayout)
        {
            HttpContext context = this.Context;
            if ((context != null) && context.TraceIsEnabled)
            {
                int bufferedLength = context.Response.GetBufferedLength();
                this.RenderItemInternal(writer, extractRows, tableLayout);
                int num2 = context.Response.GetBufferedLength();
                context.Trace.AddControlSize(this.UniqueID, num2 - bufferedLength);
            }
            else
            {
                this.RenderItemInternal(writer, extractRows, tableLayout);
            }
        }

        private void RenderItemInternal(HtmlTextWriter writer, bool extractRows, bool tableLayout)
        {
            if (!extractRows)
            {
                if (tableLayout)
                {
                    this.RenderContents(writer);
                }
                else
                {
                    this.RenderControl(writer);
                }
            }
            else
            {
                IEnumerator enumerator = this.Controls.GetEnumerator();
                Table table = null;
                bool flag = false;
                while (enumerator.MoveNext())
                {
                    flag = true;
                    Control current = (Control) enumerator.Current;
                    if (current is Table)
                    {
                        table = (Table) current;
                        break;
                    }
                }
                if (table != null)
                {
                    IEnumerator enumerator2 = table.Rows.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        ((TableRow) enumerator2.Current).RenderControl(writer);
                    }
                }
                else if (flag)
                {
                    throw new HttpException(System.Web.SR.GetString("DataList_TemplateTableNotFound", new object[] { this.Parent.ID, this.itemType.ToString() }));
                }
            }
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

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
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

