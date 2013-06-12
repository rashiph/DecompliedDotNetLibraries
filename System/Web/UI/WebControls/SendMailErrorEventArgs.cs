namespace System.Web.UI.WebControls
{
    using System;

    public class SendMailErrorEventArgs : EventArgs
    {
        private System.Exception _exception;
        private bool _handled;

        public SendMailErrorEventArgs(System.Exception e)
        {
            this._exception = e;
        }

        public System.Exception Exception
        {
            get
            {
                return this._exception;
            }
            set
            {
                this._exception = value;
            }
        }

        public bool Handled
        {
            get
            {
                return this._handled;
            }
            set
            {
                this._handled = value;
            }
        }
    }
}

