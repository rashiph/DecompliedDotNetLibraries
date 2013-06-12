namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;

    public class FormViewUpdatedEventArgs : EventArgs
    {
        private int _affectedRows;
        private System.Exception _exception;
        private bool _exceptionHandled;
        private bool _keepInEditMode;
        private IOrderedDictionary _keys;
        private IOrderedDictionary _oldValues;
        private IOrderedDictionary _values;

        public FormViewUpdatedEventArgs(int affectedRows, System.Exception e)
        {
            this._affectedRows = affectedRows;
            this._exceptionHandled = false;
            this._exception = e;
            this._keepInEditMode = false;
        }

        internal void SetKeys(IOrderedDictionary keys)
        {
            this._keys = keys;
        }

        internal void SetNewValues(IOrderedDictionary newValues)
        {
            this._values = newValues;
        }

        internal void SetOldValues(IOrderedDictionary oldValues)
        {
            this._oldValues = oldValues;
        }

        public int AffectedRows
        {
            get
            {
                return this._affectedRows;
            }
        }

        public System.Exception Exception
        {
            get
            {
                return this._exception;
            }
        }

        public bool ExceptionHandled
        {
            get
            {
                return this._exceptionHandled;
            }
            set
            {
                this._exceptionHandled = value;
            }
        }

        public bool KeepInEditMode
        {
            get
            {
                return this._keepInEditMode;
            }
            set
            {
                this._keepInEditMode = value;
            }
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
    }
}

