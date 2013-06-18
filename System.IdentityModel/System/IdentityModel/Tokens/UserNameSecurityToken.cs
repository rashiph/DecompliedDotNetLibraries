namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;

    public class UserNameSecurityToken : SecurityToken
    {
        private DateTime effectiveTime;
        private string id;
        private string password;
        private string userName;

        public UserNameSecurityToken(string userName, string password) : this(userName, password, SecurityUniqueId.Create().Value)
        {
        }

        public UserNameSecurityToken(string userName, string password, string id)
        {
            if (userName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("userName");
            }
            if (userName == string.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("UserNameCannotBeEmpty"));
            }
            this.userName = userName;
            this.password = password;
            this.id = id;
            this.effectiveTime = DateTime.UtcNow;
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return EmptyReadOnlyCollection<SecurityKey>.Instance;
            }
        }

        public string UserName
        {
            get
            {
                return this.userName;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                return this.effectiveTime;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                return System.IdentityModel.SecurityUtils.MaxUtcDateTime;
            }
        }
    }
}

