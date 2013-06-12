namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;

    public class ObjectDataSourceStatusEventArgs : EventArgs
    {
        private int _affectedRows;
        private System.Exception _exception;
        private bool _exceptionHandled;
        private IDictionary _outputParameters;
        private object _returnValue;

        public ObjectDataSourceStatusEventArgs(object returnValue, IDictionary outputParameters) : this(returnValue, outputParameters, null)
        {
        }

        public ObjectDataSourceStatusEventArgs(object returnValue, IDictionary outputParameters, System.Exception exception)
        {
            this._affectedRows = -1;
            this._returnValue = returnValue;
            this._outputParameters = outputParameters;
            this._exception = exception;
        }

        public int AffectedRows
        {
            get
            {
                return this._affectedRows;
            }
            set
            {
                this._affectedRows = value;
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

        public IDictionary OutputParameters
        {
            get
            {
                return this._outputParameters;
            }
        }

        public object ReturnValue
        {
            get
            {
                return this._returnValue;
            }
        }
    }
}

