namespace System.Web.Security
{
    using System;
    using System.Security.Principal;
    using System.Web;

    [Serializable]
    public class ActiveDirectoryMembershipUser : MembershipUser
    {
        internal bool commentModified;
        internal bool emailModified;
        internal bool isApprovedModified;
        [NonSerialized]
        private SecurityIdentifier sid;
        private byte[] sidBinaryForm;

        protected ActiveDirectoryMembershipUser()
        {
            this.emailModified = true;
            this.commentModified = true;
            this.isApprovedModified = true;
        }

        public ActiveDirectoryMembershipUser(string providerName, string name, object providerUserKey, string email, string passwordQuestion, string comment, bool isApproved, bool isLockedOut, DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate, DateTime lastPasswordChangedDate, DateTime lastLockoutDate) : base(providerName, name, null, email, passwordQuestion, comment, isApproved, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate)
        {
            this.emailModified = true;
            this.commentModified = true;
            this.isApprovedModified = true;
            if ((providerUserKey != null) && !(providerUserKey is SecurityIdentifier))
            {
                throw new ArgumentException(System.Web.SR.GetString("ADMembership_InvalidProviderUserKey"), "providerUserKey");
            }
            this.sid = (SecurityIdentifier) providerUserKey;
            if (this.sid != null)
            {
                this.sidBinaryForm = new byte[this.sid.BinaryLength];
                this.sid.GetBinaryForm(this.sidBinaryForm, 0);
            }
        }

        internal ActiveDirectoryMembershipUser(string providerName, string name, byte[] sidBinaryForm, object providerUserKey, string email, string passwordQuestion, string comment, bool isApproved, bool isLockedOut, DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate, DateTime lastPasswordChangedDate, DateTime lastLockoutDate, bool valuesAreUpdated) : base(providerName, name, null, email, passwordQuestion, comment, isApproved, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate)
        {
            this.emailModified = true;
            this.commentModified = true;
            this.isApprovedModified = true;
            if (valuesAreUpdated)
            {
                this.emailModified = false;
                this.commentModified = false;
                this.isApprovedModified = false;
            }
            this.sidBinaryForm = sidBinaryForm;
            this.sid = (SecurityIdentifier) providerUserKey;
        }

        public override string Comment
        {
            get
            {
                return base.Comment;
            }
            set
            {
                base.Comment = value;
                this.commentModified = true;
            }
        }

        public override string Email
        {
            get
            {
                return base.Email;
            }
            set
            {
                base.Email = value;
                this.emailModified = true;
            }
        }

        public override bool IsApproved
        {
            get
            {
                return base.IsApproved;
            }
            set
            {
                base.IsApproved = value;
                this.isApprovedModified = true;
            }
        }

        public override DateTime LastActivityDate
        {
            get
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_UserProperty_not_supported", new object[] { "LastActivityDate" }));
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_UserProperty_not_supported", new object[] { "LastActivityDate" }));
            }
        }

        public override DateTime LastLoginDate
        {
            get
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_UserProperty_not_supported", new object[] { "LastLoginDate" }));
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_UserProperty_not_supported", new object[] { "LastLoginDate" }));
            }
        }

        public override object ProviderUserKey
        {
            get
            {
                if ((this.sid == null) && (this.sidBinaryForm != null))
                {
                    this.sid = new SecurityIdentifier(this.sidBinaryForm, 0);
                }
                return this.sid;
            }
        }
    }
}

