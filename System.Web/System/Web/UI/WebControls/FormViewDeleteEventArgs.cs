namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class FormViewDeleteEventArgs : CancelEventArgs
    {
        private OrderedDictionary _keys;
        private int _rowIndex;
        private OrderedDictionary _values;

        public FormViewDeleteEventArgs(int rowIndex) : base(false)
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

        public int RowIndex
        {
            get
            {
                return this._rowIndex;
            }
        }

        public IOrderedDictionary Values
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
    }
}

