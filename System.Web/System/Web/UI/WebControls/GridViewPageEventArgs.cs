namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    public class GridViewPageEventArgs : CancelEventArgs
    {
        private int _newPageIndex;

        public GridViewPageEventArgs(int newPageIndex)
        {
            this._newPageIndex = newPageIndex;
        }

        public int NewPageIndex
        {
            get
            {
                return this._newPageIndex;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._newPageIndex = value;
            }
        }
    }
}

