namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;

    public class DetailsViewInsertedEventArgs : EventArgs
    {
        private int _affectedRows;
        private System.Exception _exception;
        private bool _exceptionHandled;
        private bool _keepInInsertMode;
        private IOrderedDictionary _values;

        public DetailsViewInsertedEventArgs(int affectedRows, System.Exception e)
        {
            this._affectedRows = affectedRows;
            this._exceptionHandled = false;
            this._exception = e;
            this._keepInInsertMode = false;
        }

        internal void SetValues(IOrderedDictionary values)
        {
            this._values = values;
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

        public bool KeepInInsertMode
        {
            get
            {
                return this._keepInInsertMode;
            }
            set
            {
                this._keepInInsertMode = value;
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

