namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    public class GridViewEditEventArgs : CancelEventArgs
    {
        private int _newEditIndex;

        public GridViewEditEventArgs(int newEditIndex)
        {
            this._newEditIndex = newEditIndex;
        }

        public int NewEditIndex
        {
            get
            {
                return this._newEditIndex;
            }
            set
            {
                this._newEditIndex = value;
            }
        }
    }
}

