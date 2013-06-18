namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    public class GridViewSelectEventArgs : CancelEventArgs
    {
        private int _newSelectedIndex;

        public GridViewSelectEventArgs(int newSelectedIndex)
        {
            this._newSelectedIndex = newSelectedIndex;
        }

        public int NewSelectedIndex
        {
            get
            {
                return this._newSelectedIndex;
            }
            set
            {
                this._newSelectedIndex = value;
            }
        }
    }
}

