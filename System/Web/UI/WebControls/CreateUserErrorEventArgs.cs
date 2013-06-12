namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.Security;

    public class CreateUserErrorEventArgs : EventArgs
    {
        private MembershipCreateStatus _error;

        public CreateUserErrorEventArgs(MembershipCreateStatus s)
        {
            this._error = s;
        }

        public MembershipCreateStatus CreateUserError
        {
            get
            {
                return this._error;
            }
            set
            {
                this._error = value;
            }
        }
    }
}

