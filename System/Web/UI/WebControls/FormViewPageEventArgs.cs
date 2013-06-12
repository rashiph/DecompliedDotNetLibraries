namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    public class FormViewPageEventArgs : CancelEventArgs
    {
        private int _newPageIndex;

        public FormViewPageEventArgs(int newPageIndex)
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
                this._newPageIndex = value;
            }
        }
    }
}

