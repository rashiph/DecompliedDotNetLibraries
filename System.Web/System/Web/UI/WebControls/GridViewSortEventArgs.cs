namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    public class GridViewSortEventArgs : CancelEventArgs
    {
        private System.Web.UI.WebControls.SortDirection _sortDirection;
        private string _sortExpression;

        public GridViewSortEventArgs(string sortExpression, System.Web.UI.WebControls.SortDirection sortDirection)
        {
            this._sortExpression = sortExpression;
            this._sortDirection = sortDirection;
        }

        public System.Web.UI.WebControls.SortDirection SortDirection
        {
            get
            {
                return this._sortDirection;
            }
            set
            {
                this._sortDirection = value;
            }
        }

        public string SortExpression
        {
            get
            {
                return this._sortExpression;
            }
            set
            {
                this._sortExpression = value;
            }
        }
    }
}

