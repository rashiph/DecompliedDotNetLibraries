namespace System.Web.Security
{
    using System;
    using System.Configuration.Provider;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Web;
    using System.Web.Util;

    [Serializable, TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class MembershipUser
    {
        private string _Comment;
        private DateTime _CreationDate;
        private string _Email;
        private bool _IsApproved;
        private bool _IsLockedOut;
        private DateTime _LastActivityDate;
        private DateTime _LastLockoutDate;
        private DateTime _LastLoginDate;
        private DateTime _LastPasswordChangedDate;
        private string _PasswordQuestion;
        private string _ProviderName;
        private object _ProviderUserKey;
        private string _UserName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected MembershipUser()
        {
        }

        public MembershipUser(string providerName, string name, object providerUserKey, string email, string passwordQuestion, string comment, bool isApproved, bool isLockedOut, DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate, DateTime lastPasswordChangedDate, DateTime lastLockoutDate)
        {
            if ((providerName == null) || (SystemWebProxy.Membership.Providers[providerName] == null))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ApplicationServicesStrings.Membership_provider_name_invalid, new object[0]), "providerName");
            }
            if (name != null)
            {
                name = name.Trim();
            }
            if (email != null)
            {
                email = email.Trim();
            }
            if (passwordQuestion != null)
            {
                passwordQuestion = passwordQuestion.Trim();
            }
            this._ProviderName = providerName;
            this._UserName = name;
            this._ProviderUserKey = providerUserKey;
            this._Email = email;
            this._PasswordQuestion = passwordQuestion;
            this._Comment = comment;
            this._IsApproved = isApproved;
            this._IsLockedOut = isLockedOut;
            this._CreationDate = creationDate.ToUniversalTime();
            this._LastLoginDate = lastLoginDate.ToUniversalTime();
            this._LastActivityDate = lastActivityDate.ToUniversalTime();
            this._LastPasswordChangedDate = lastPasswordChangedDate.ToUniversalTime();
            this._LastLockoutDate = lastLockoutDate.ToUniversalTime();
        }

        public virtual bool ChangePassword(string oldPassword, string newPassword)
        {
            SecurityServices.CheckPasswordParameter(oldPassword, "oldPassword");
            SecurityServices.CheckPasswordParameter(newPassword, "newPassword");
            if (!SystemWebProxy.Membership.Providers[this.ProviderName].ChangePassword(this.UserName, oldPassword, newPassword))
            {
                return false;
            }
            this.UpdateSelf();
            return true;
        }

        internal bool ChangePassword(string oldPassword, string newPassword, bool throwOnError)
        {
            bool flag = false;
            try
            {
                flag = this.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            catch (MembershipPasswordException)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            catch (ProviderException)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            return flag;
        }

        public virtual bool ChangePasswordQuestionAndAnswer(string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            SecurityServices.CheckPasswordParameter(password, "password");
            SecurityServices.CheckForEmptyOrWhiteSpaceParameter(ref newPasswordQuestion, "newPasswordQuestion");
            SecurityServices.CheckForEmptyOrWhiteSpaceParameter(ref newPasswordAnswer, "newPasswordAnswer");
            if (!SystemWebProxy.Membership.Providers[this.ProviderName].ChangePasswordQuestionAndAnswer(this.UserName, password, newPasswordQuestion, newPasswordAnswer))
            {
                return false;
            }
            this.UpdateSelf();
            return true;
        }

        public virtual string GetPassword()
        {
            return SystemWebProxy.Membership.Providers[this.ProviderName].GetPassword(this.UserName, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal string GetPassword(bool throwOnError)
        {
            return this.GetPassword(null, false, throwOnError);
        }

        public virtual string GetPassword(string passwordAnswer)
        {
            return SystemWebProxy.Membership.Providers[this.ProviderName].GetPassword(this.UserName, passwordAnswer);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal string GetPassword(string answer, bool throwOnError)
        {
            return this.GetPassword(answer, true, throwOnError);
        }

        private string GetPassword(string answer, bool useAnswer, bool throwOnError)
        {
            string password = null;
            try
            {
                if (useAnswer)
                {
                    return this.GetPassword(answer);
                }
                password = this.GetPassword();
            }
            catch (ArgumentException)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            catch (MembershipPasswordException)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            catch (ProviderException)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            return password;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual string ResetPassword()
        {
            return this.ResetPassword((string) null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal string ResetPassword(bool throwOnError)
        {
            return this.ResetPassword(null, false, throwOnError);
        }

        public virtual string ResetPassword(string passwordAnswer)
        {
            string str = SystemWebProxy.Membership.Providers[this.ProviderName].ResetPassword(this.UserName, passwordAnswer);
            if (!string.IsNullOrEmpty(str))
            {
                this.UpdateSelf();
            }
            return str;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal string ResetPassword(string passwordAnswer, bool throwOnError)
        {
            return this.ResetPassword(passwordAnswer, true, throwOnError);
        }

        private string ResetPassword(string passwordAnswer, bool useAnswer, bool throwOnError)
        {
            string str = null;
            try
            {
                if (useAnswer)
                {
                    return this.ResetPassword(passwordAnswer);
                }
                str = this.ResetPassword();
            }
            catch (ArgumentException)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            catch (MembershipPasswordException)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            catch (ProviderException)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            return str;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override string ToString()
        {
            return this.UserName;
        }

        public virtual bool UnlockUser()
        {
            if (SystemWebProxy.Membership.Providers[this.ProviderName].UnlockUser(this.UserName))
            {
                this.UpdateSelf();
                return !this.IsLockedOut;
            }
            return false;
        }

        internal virtual void Update()
        {
            SystemWebProxy.Membership.Providers[this.ProviderName].UpdateUser(this);
            this.UpdateSelf();
        }

        private void UpdateSelf()
        {
            MembershipUser user = SystemWebProxy.Membership.Providers[this.ProviderName].GetUser(this.UserName, false);
            if (user != null)
            {
                try
                {
                    this._LastPasswordChangedDate = user.LastPasswordChangedDate.ToUniversalTime();
                }
                catch (NotSupportedException)
                {
                }
                try
                {
                    this.LastActivityDate = user.LastActivityDate;
                }
                catch (NotSupportedException)
                {
                }
                try
                {
                    this.LastLoginDate = user.LastLoginDate;
                }
                catch (NotSupportedException)
                {
                }
                try
                {
                    this._CreationDate = user.CreationDate.ToUniversalTime();
                }
                catch (NotSupportedException)
                {
                }
                try
                {
                    this._LastLockoutDate = user.LastLockoutDate.ToUniversalTime();
                }
                catch (NotSupportedException)
                {
                }
                try
                {
                    this._IsLockedOut = user.IsLockedOut;
                }
                catch (NotSupportedException)
                {
                }
                try
                {
                    this.IsApproved = user.IsApproved;
                }
                catch (NotSupportedException)
                {
                }
                try
                {
                    this.Comment = user.Comment;
                }
                catch (NotSupportedException)
                {
                }
                try
                {
                    this._PasswordQuestion = user.PasswordQuestion;
                }
                catch (NotSupportedException)
                {
                }
                try
                {
                    this.Email = user.Email;
                }
                catch (NotSupportedException)
                {
                }
                try
                {
                    this._ProviderUserKey = user.ProviderUserKey;
                }
                catch (NotSupportedException)
                {
                }
            }
        }

        public virtual string Comment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._Comment;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._Comment = value;
            }
        }

        public virtual DateTime CreationDate
        {
            get
            {
                return this._CreationDate.ToLocalTime();
            }
        }

        public virtual string Email
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._Email;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._Email = value;
            }
        }

        public virtual bool IsApproved
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._IsApproved;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._IsApproved = value;
            }
        }

        public virtual bool IsLockedOut
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._IsLockedOut;
            }
        }

        public virtual bool IsOnline
        {
            get
            {
                TimeSpan span = new TimeSpan(0, SystemWebProxy.Membership.UserIsOnlineTimeWindow, 0);
                DateTime time = DateTime.UtcNow.Subtract(span);
                return (this.LastActivityDate.ToUniversalTime() > time);
            }
        }

        public virtual DateTime LastActivityDate
        {
            get
            {
                return this._LastActivityDate.ToLocalTime();
            }
            set
            {
                this._LastActivityDate = value.ToUniversalTime();
            }
        }

        public virtual DateTime LastLockoutDate
        {
            get
            {
                return this._LastLockoutDate.ToLocalTime();
            }
        }

        public virtual DateTime LastLoginDate
        {
            get
            {
                return this._LastLoginDate.ToLocalTime();
            }
            set
            {
                this._LastLoginDate = value.ToUniversalTime();
            }
        }

        public virtual DateTime LastPasswordChangedDate
        {
            get
            {
                return this._LastPasswordChangedDate.ToLocalTime();
            }
        }

        public virtual string PasswordQuestion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._PasswordQuestion;
            }
        }

        public virtual string ProviderName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._ProviderName;
            }
        }

        public virtual object ProviderUserKey
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._ProviderUserKey;
            }
        }

        public virtual string UserName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._UserName;
            }
        }
    }
}

