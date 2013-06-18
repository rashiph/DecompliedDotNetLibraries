namespace System.Web.UI.WebControls
{
    using System;

    public class GridViewRowEventArgs : EventArgs
    {
        private GridViewRow _row;

        public GridViewRowEventArgs(GridViewRow row)
        {
            this._row = row;
        }

        public GridViewRow Row
        {
            get
            {
                return this._row;
            }
        }
    }
}

