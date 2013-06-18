namespace System.Web.UI.WebControls
{
    using System;

    public class DetailsViewRow : TableRow
    {
        private int _rowIndex;
        private DataControlRowState _rowState;
        private DataControlRowType _rowType;

        public DetailsViewRow(int rowIndex, DataControlRowType rowType, DataControlRowState rowState)
        {
            this._rowIndex = rowIndex;
            this._rowType = rowType;
            this._rowState = rowState;
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            if (e is CommandEventArgs)
            {
                DetailsViewCommandEventArgs args = new DetailsViewCommandEventArgs(source, (CommandEventArgs) e);
                base.RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }

        public virtual int RowIndex
        {
            get
            {
                return this._rowIndex;
            }
        }

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

