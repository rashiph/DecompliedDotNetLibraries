namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class GridViewUpdateEventArgs : CancelEventArgs
    {
        private OrderedDictionary _keys;
        private OrderedDictionary _oldValues;
        private int _rowIndex;
        private OrderedDictionary _values;

        public GridViewUpdateEventArgs(int rowIndex) : base(false)
        {
            this._rowIndex = rowIndex;
        }

        public IOrderedDictionary Keys
        {
            get
            {
                if (this._keys == null)
                {
                    this._keys = new OrderedDictionary();
                }
                return this._keys;
            }
        }

        public IOrderedDictionary NewValues
        {
            get
            {
                if (this._values == null)
                {
                    this._values = new OrderedDictionary();
                }
                return this._values;
            }
        }

        public IOrderedDictionary OldValues
        {
            get
            {
                if (this._oldValues == null)
                {
                    this._oldValues = new OrderedDictionary();
                }
                return this._oldValues;
            }
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

