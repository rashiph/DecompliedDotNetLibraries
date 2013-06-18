namespace System.Web.Security
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Util;

    public class WindowsTokenRoleProvider : RoleProvider
    {
        private string _AppName;
        private static string _MachineName;

        private static string[] AddLocalGroupsWithoutDomainNames(string[] roles)
        {
            string machineName = GetMachineName();
            int length = machineName.Length;
            for (int i = 0; i < roles.Length; i++)
            {
                roles[i] = roles[i].Trim();
                if (roles[i].ToLower(CultureInfo.InvariantCulture).StartsWith(machineName, StringComparison.Ordinal))
                {
                    roles[i] = roles[i].Substring(length);
                }
            }
            return roles;
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new ProviderException(System.Web.SR.GetString("Windows_Token_API_not_supported"));
        }

        public override void CreateRole(string roleName)
        {
            throw new ProviderException(System.Web.SR.GetString("Windows_Token_API_not_supported"));
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new ProviderException(System.Web.SR.GetString("Windows_Token_API_not_supported"));
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new ProviderException(System.Web.SR.GetString("Windows_Token_API_not_supported"));
        }

        public override string[] GetAllRoles()
        {
            throw new ProviderException(System.Web.SR.GetString("Windows_Token_API_not_supported"));
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private IntPtr GetCurrentTokenAndCheckName(string userName)
        {
            return this.GetCurrentWindowsIdentityAndCheckName(userName).Token;
        }

        private WindowsIdentity GetCurrentWindowsIdentityAndCheckName(string userName)
        {
            if (HostingEnvironment.IsHosted)
            {
                HttpContext current = HttpContext.Current;
                if ((current == null) || (current.User == null))
                {
                    throw new ProviderException(System.Web.SR.GetString("API_supported_for_current_user_only"));
                }
                if (!(current.User.Identity is WindowsIdentity))
                {
                    throw new ProviderException(System.Web.SR.GetString("API_supported_for_current_user_only"));
                }
                if (!StringUtil.EqualsIgnoreCase(userName, current.User.Identity.Name))
                {
                    throw new ProviderException(System.Web.SR.GetString("API_supported_for_current_user_only"));
                }
                return (WindowsIdentity) current.User.Identity;
            }
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            if (((currentPrincipal == null) || (currentPrincipal.Identity == null)) || !(currentPrincipal.Identity is WindowsIdentity))
            {
                throw new ProviderException(System.Web.SR.GetString("API_supported_for_current_user_only"));
            }
            if (!StringUtil.EqualsIgnoreCase(userName, currentPrincipal.Identity.Name))
            {
                throw new ProviderException(System.Web.SR.GetString("API_supported_for_current_user_only"));
            }
            return (WindowsIdentity) currentPrincipal.Identity;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private static string GetMachineName()
        {
            if (_MachineName == null)
            {
                _MachineName = (Environment.MachineName + @"\").ToLower(CultureInfo.InvariantCulture);
            }
            return _MachineName;
        }

        public override string[] GetRolesForUser(string username)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "API_not_supported_at_this_level");
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            username = username.Trim();
            IntPtr currentTokenAndCheckName = this.GetCurrentTokenAndCheckName(username);
            if (username.Length < 1)
            {
                return new string[0];
            }
            StringBuilder allGroups = new StringBuilder(0x400);
            StringBuilder error = new StringBuilder(0x400);
            int num = System.Web.UnsafeNativeMethods.GetGroupsForUser(currentTokenAndCheckName, allGroups, 0x400, error, 0x400);
            if (num < 0)
            {
                allGroups = new StringBuilder(-num);
                num = System.Web.UnsafeNativeMethods.GetGroupsForUser(currentTokenAndCheckName, allGroups, -num, error, 0x400);
            }
            if (num <= 0)
            {
                throw new ProviderException(System.Web.SR.GetString("API_failed_due_to_error", new object[] { error.ToString() }));
            }
            return AddLocalGroupsWithoutDomainNames(allGroups.ToString().Split(new char[] { '\t' }));
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new ProviderException(System.Web.SR.GetString("Windows_Token_API_not_supported"));
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "WindowsTokenProvider";
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", System.Web.SR.GetString("RoleWindowsTokenProvider_description"));
            }
            base.Initialize(name, config);
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            this._AppName = config["applicationName"];
            if (string.IsNullOrEmpty(this._AppName))
            {
                this._AppName = SecUtility.GetDefaultAppName();
            }
            if (this._AppName.Length > 0x100)
            {
                throw new ProviderException(System.Web.SR.GetString("Provider_application_name_too_long"));
            }
            config.Remove("applicationName");
            if (config.Count > 0)
            {
                string key = config.GetKey(0);
                if (!string.IsNullOrEmpty(key))
                {
                    throw new ProviderException(System.Web.SR.GetString("Provider_unrecognized_attribute", new object[] { key }));
                }
            }
        }

        public bool IsUserInRole(string username, WindowsBuiltInRole role)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            username = username.Trim();
            WindowsIdentity currentWindowsIdentityAndCheckName = this.GetCurrentWindowsIdentityAndCheckName(username);
            if (username.Length < 1)
            {
                return false;
            }
            WindowsPrincipal principal = new WindowsPrincipal(currentWindowsIdentityAndCheckName);
            return principal.IsInRole(role);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            username = username.Trim();
            if (roleName == null)
            {
                throw new ArgumentNullException("roleName");
            }
            roleName = roleName.Trim();
            if (username.Length < 1)
            {
                return false;
            }
            StringBuilder error = new StringBuilder(0x400);
            switch (System.Web.UnsafeNativeMethods.IsUserInRole(this.GetCurrentTokenAndCheckName(username), roleName, error, 0x400))
            {
                case 0:
                    return false;

                case 1:
                    return true;
            }
            throw new ProviderException(System.Web.SR.GetString("API_failed_due_to_error", new object[] { error.ToString() }));
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new ProviderException(System.Web.SR.GetString("Windows_Token_API_not_supported"));
        }

        public override bool RoleExists(string roleName)
        {
            throw new ProviderException(System.Web.SR.GetString("Windows_Token_API_not_supported"));
        }

        public override string ApplicationName
        {
            get
            {
                return this._AppName;
            }
            set
            {
                this._AppName = value;
                if (this._AppName.Length > 0x100)
                {
                    throw new ProviderException(System.Web.SR.GetString("Provider_application_name_too_long"));
                }
            }
        }
    }
}

