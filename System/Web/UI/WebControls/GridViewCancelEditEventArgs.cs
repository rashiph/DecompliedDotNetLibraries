namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    public class GridViewCancelEditEventArgs : CancelEventArgs
    {
        private int _rowIndex;

        public GridViewCancelEditEventArgs(int rowIndex)
        {
            this._rowIndex = rowIndex;
        }

        public int RowIndex
        {
            get
            {
                return this._rowIndex;
            }
        }
    }
}

