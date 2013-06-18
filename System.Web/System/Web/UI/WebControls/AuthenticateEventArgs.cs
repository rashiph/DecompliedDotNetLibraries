namespace System.Web.UI.WebControls
{
    using System;

    public class AuthenticateEventArgs : EventArgs
    {
        private bool _authenticated;

        public AuthenticateEventArgs() : this(false)
        {
        }

        public AuthenticateEventArgs(bool authenticated)
        {
            this._authenticated = authenticated;
        }

        public bool Authenticated
        {
            get
            {
                return this._authenticated;
            }
            set
            {
                this._authenticated = value;
            }
        }
    }
}

