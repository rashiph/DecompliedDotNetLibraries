namespace System.Web.UI.WebControls
{
    using System;

    public class LoginCancelEventArgs : EventArgs
    {
        private bool _cancel;

        public LoginCancelEventArgs() : this(false)
        {
        }

        public LoginCancelEventArgs(bool cancel)
        {
            this._cancel = cancel;
        }

        public bool Cancel
        {
            get
            {
                return this._cancel;
            }
            set
            {
                this._cancel = value;
            }
        }
    }
}

