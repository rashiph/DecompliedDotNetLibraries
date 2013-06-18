namespace System.Web.Security
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class ValidatePasswordEventArgs : EventArgs
    {
        private bool _cancel;
        private Exception _failureInformation;
        private bool _isNewUser;
        private string _password;
        private string _userName;

        public ValidatePasswordEventArgs(string userName, string password, bool isNewUser)
        {
            this._userName = userName;
            this._password = password;
            this._isNewUser = isNewUser;
            this._cancel = false;
        }

        public bool Cancel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._cancel;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._cancel = value;
            }
        }

        public Exception FailureInformation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._failureInformation;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._failureInformation = value;
            }
        }

        public bool IsNewUser
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._isNewUser;
            }
        }

        public string Password
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._password;
            }
        }

        public string UserName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._userName;
            }
        }
    }
}

