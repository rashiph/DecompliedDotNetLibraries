namespace System.Web.UI.WebControls
{
    using System;

    public class BulletedListEventArgs : EventArgs
    {
        private int _index;

        public BulletedListEventArgs(int index)
        {
            this._index = index;
        }

        public int Index
        {
            get
            {
                return this._index;
            }
        }
    }
}

