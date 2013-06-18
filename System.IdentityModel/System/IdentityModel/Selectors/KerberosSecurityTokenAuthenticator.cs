namespace System.IdentityModel.Selectors
{
    using System;
    using System.IdentityModel.Tokens;

    public class KerberosSecurityTokenAuthenticator : WindowsSecurityTokenAuthenticator
    {
        public KerberosSecurityTokenAuthenticator()
        {
        }

        public KerberosSecurityTokenAuthenticator(bool includeWindowsGroups) : base(includeWindowsGroups)
        {
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is KerberosReceiverSecurityToken);
        }
    }
}

