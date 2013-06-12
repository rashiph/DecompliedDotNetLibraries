namespace System.Web.UI.WebControls
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Web.UI;

    public class FormViewRow : TableRow
    {
        private int _itemIndex;
        private DataControlRowState _rowState;
        private DataControlRowType _rowType;

        public FormViewRow(int itemIndex, DataControlRowType rowType, DataControlRowState rowState)
        {
            this._itemIndex = itemIndex;
            this._rowType = rowType;
            this._rowState = rowState;
            this.RenderTemplateContainer = true;
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            if (e is CommandEventArgs)
            {
                FormViewCommandEventArgs args = new FormViewCommandEventArgs(source, (CommandEventArgs) e);
                base.RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.RenderTemplateContainer)
            {
                base.Render(writer);
            }
            else
            {
                foreach (TableCell cell in this.Cells)
                {
                    cell.RenderContents(writer);
                }
            }
        }

        public virtual int ItemIndex
        {
            get
            {
                return this._itemIndex;
            }
        }

        internal bool RenderTemplateContainer { get; set; }

        public virtual DataControlRowState RowState
        {
            get
            {
                return this._rowState;
            }
        }

        public virtual DataControlRowType RowType
        {
            get
            {
                return this._rowType;
            }
        }
    }
}

