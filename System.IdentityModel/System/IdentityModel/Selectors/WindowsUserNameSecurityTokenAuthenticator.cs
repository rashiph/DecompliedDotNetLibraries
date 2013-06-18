namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    public class WindowsUserNameSecurityTokenAuthenticator : UserNameSecurityTokenAuthenticator
    {
        private bool includeWindowsGroups;

        public WindowsUserNameSecurityTokenAuthenticator() : this(true)
        {
        }

        public WindowsUserNameSecurityTokenAuthenticator(bool includeWindowsGroups)
        {
            this.includeWindowsGroups = includeWindowsGroups;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password)
        {
            string lpszDomain = null;
            ReadOnlyCollection<IAuthorizationPolicy> onlys;
            string[] strArray = userName.Split(new char[] { '\\' });
            if (strArray.Length != 1)
            {
                if ((strArray.Length != 2) || string.IsNullOrEmpty(strArray[0]))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("IncorrectUserNameFormat"));
                }
                userName = strArray[1];
                lpszDomain = strArray[0];
            }
            SafeCloseHandle phToken = null;
            try
            {
                if (!System.IdentityModel.NativeMethods.LogonUser(userName, lpszDomain, password, 8, 0, out phToken))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(System.IdentityModel.SR.GetString("FailLogonUser", new object[] { userName }), new Win32Exception(error)));
                }
                WindowsIdentity windowsIdentity = new WindowsIdentity(phToken.DangerousGetHandle(), "Basic");
                WindowsClaimSet claimSet = new WindowsClaimSet(windowsIdentity, "Basic", this.includeWindowsGroups, false);
                onlys = System.IdentityModel.SecurityUtils.CreateAuthorizationPolicies(claimSet, claimSet.ExpirationTime);
            }
            finally
            {
                if (phToken != null)
                {
                    phToken.Close();
                }
            }
            return onlys;
        }
    }
}

