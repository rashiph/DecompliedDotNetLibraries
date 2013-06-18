namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;

    public sealed class UserNamePasswordClientCredential
    {
        private bool isReadOnly;
        private string password;
        private string userName;

        internal UserNamePasswordClientCredential()
        {
        }

        internal UserNamePasswordClientCredential(UserNamePasswordClientCredential other)
        {
            this.userName = other.userName;
            this.password = other.password;
            this.isReadOnly = other.isReadOnly;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        private void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.ThrowIfImmutable();
                this.password = value;
            }
        }

        public string UserName
        {
            get
            {
                return this.userName;
            }
            set
            {
                this.ThrowIfImmutable();
                this.userName = value;
            }
        }
    }
}

