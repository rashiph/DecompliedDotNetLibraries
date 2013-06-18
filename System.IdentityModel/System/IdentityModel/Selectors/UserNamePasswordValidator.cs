namespace System.IdentityModel.Selectors
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Web.Security;

    public abstract class UserNamePasswordValidator
    {
        private static UserNamePasswordValidator none;

        protected UserNamePasswordValidator()
        {
        }

        public static UserNamePasswordValidator CreateMembershipProviderValidator(MembershipProvider provider)
        {
            if (provider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("provider");
            }
            return new MembershipProviderValidator(provider);
        }

        public abstract void Validate(string userName, string password);

        public static UserNamePasswordValidator None
        {
            get
            {
                if (none == null)
                {
                    none = new NoneUserNamePasswordValidator();
                }
                return none;
            }
        }

        private class MembershipProviderValidator : UserNamePasswordValidator
        {
            private MembershipProvider provider;

            public MembershipProviderValidator(MembershipProvider provider)
            {
                this.provider = provider;
            }

            public override void Validate(string userName, string password)
            {
                if (!this.provider.ValidateUser(userName, password))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(System.IdentityModel.SR.GetString("UserNameAuthenticationFailed", new object[] { this.provider.GetType().Name })));
                }
            }
        }

        private class NoneUserNamePasswordValidator : UserNamePasswordValidator
        {
            public override void Validate(string userName, string password)
            {
            }
        }
    }
}

