namespace System.Web.Security
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Globalization;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    public static class Roles
    {
        private static bool s_CacheRolesInCookie;
        private static string s_CookieName;
        private static string s_CookiePath;
        private static CookieProtection s_CookieProtection;
        private static bool s_CookieRequireSSL;
        private static bool s_CookieSlidingExpiration;
        private static int s_CookieTimeout;
        private static bool s_CreatePersistentCookie;
        private static string s_Domain;
        private static bool s_Enabled;
        private static bool s_EnabledSet;
        private static bool s_Initialized;
        private static bool s_InitializedDefaultProvider;
        private static Exception s_InitializeException = null;
        private static object s_lock = new object();
        private static int s_MaxCachedResults = 0x19;
        private static RoleProvider s_Provider;
        private static RoleProviderCollection s_Providers;

        public static void AddUsersToRole(string[] usernames, string roleName)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 0, "usernames");
            Provider.AddUsersToRoles(usernames, new string[] { roleName });
            try
            {
                RolePrincipal currentUser = GetCurrentUser() as RolePrincipal;
                if (((currentUser != null) && (currentUser.ProviderName == Provider.Name)) && currentUser.IsRoleListCached)
                {
                    foreach (string str in usernames)
                    {
                        if (System.Web.Util.StringUtil.EqualsIgnoreCase(currentUser.Identity.Name, str))
                        {
                            currentUser.SetDirty();
                            return;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public static void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            EnsureEnabled();
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 0, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 0, "usernames");
            Provider.AddUsersToRoles(usernames, roleNames);
            try
            {
                RolePrincipal currentUser = GetCurrentUser() as RolePrincipal;
                if (((currentUser != null) && (currentUser.ProviderName == Provider.Name)) && currentUser.IsRoleListCached)
                {
                    foreach (string str in usernames)
                    {
                        if (System.Web.Util.StringUtil.EqualsIgnoreCase(currentUser.Identity.Name, str))
                        {
                            currentUser.SetDirty();
                            return;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public static void AddUserToRole(string username, string roleName)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            SecUtility.CheckParameter(ref username, true, true, true, 0, "username");
            Provider.AddUsersToRoles(new string[] { username }, new string[] { roleName });
            try
            {
                RolePrincipal currentUser = GetCurrentUser() as RolePrincipal;
                if (((currentUser != null) && (currentUser.ProviderName == Provider.Name)) && (currentUser.IsRoleListCached && System.Web.Util.StringUtil.EqualsIgnoreCase(currentUser.Identity.Name, username)))
                {
                    currentUser.SetDirty();
                }
            }
            catch
            {
            }
        }

        public static void AddUserToRoles(string username, string[] roleNames)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref username, true, true, true, 0, "username");
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 0, "roleNames");
            Provider.AddUsersToRoles(new string[] { username }, roleNames);
            try
            {
                RolePrincipal currentUser = GetCurrentUser() as RolePrincipal;
                if (((currentUser != null) && (currentUser.ProviderName == Provider.Name)) && (currentUser.IsRoleListCached && System.Web.Util.StringUtil.EqualsIgnoreCase(currentUser.Identity.Name, username)))
                {
                    currentUser.SetDirty();
                }
            }
            catch
            {
            }
        }

        public static void CreateRole(string roleName)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            Provider.CreateRole(roleName);
        }

        public static void DeleteCookie()
        {
            EnsureEnabled();
            if ((CookieName != null) && (CookieName.Length >= 1))
            {
                HttpContext current = HttpContext.Current;
                if ((current != null) && current.Request.Browser.Cookies)
                {
                    string str = string.Empty;
                    if (current.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                    {
                        str = "NoCookie";
                    }
                    HttpCookie cookie = new HttpCookie(CookieName, str) {
                        HttpOnly = true,
                        Path = CookiePath,
                        Domain = Domain,
                        Expires = new DateTime(0x7cf, 10, 12),
                        Secure = CookieRequireSSL
                    };
                    current.Response.Cookies.RemoveCookie(CookieName);
                    current.Response.Cookies.Add(cookie);
                }
            }
        }

        public static bool DeleteRole(string roleName)
        {
            return DeleteRole(roleName, true);
        }

        public static bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            bool flag = Provider.DeleteRole(roleName, throwOnPopulatedRole);
            try
            {
                RolePrincipal currentUser = GetCurrentUser() as RolePrincipal;
                if (((currentUser != null) && (currentUser.ProviderName == Provider.Name)) && (currentUser.IsRoleListCached && currentUser.IsInRole(roleName)))
                {
                    currentUser.SetDirty();
                }
            }
            catch
            {
            }
            return flag;
        }

        private static void EnsureEnabled()
        {
            Initialize();
            if (!s_Enabled)
            {
                throw new ProviderException(System.Web.SR.GetString("Roles_feature_not_enabled"));
            }
        }

        public static string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0, "usernameToMatch");
            return Provider.FindUsersInRole(roleName, usernameToMatch);
        }

        public static string[] GetAllRoles()
        {
            EnsureEnabled();
            return Provider.GetAllRoles();
        }

        private static IPrincipal GetCurrentUser()
        {
            if (HostingEnvironment.IsHosted)
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    return current.User;
                }
            }
            return Thread.CurrentPrincipal;
        }

        private static string GetCurrentUserName()
        {
            IPrincipal currentUser = GetCurrentUser();
            if ((currentUser != null) && (currentUser.Identity != null))
            {
                return currentUser.Identity.Name;
            }
            return string.Empty;
        }

        public static string[] GetRolesForUser()
        {
            return GetRolesForUser(GetCurrentUserName());
        }

        public static string[] GetRolesForUser(string username)
        {
            string[] strArray2;
            if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(4, 8))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_BEGIN, HttpContext.Current.WorkerRequest);
            }
            EnsureEnabled();
            string[] roles = null;
            bool flag = false;
            try
            {
                SecUtility.CheckParameter(ref username, true, false, true, 0, "username");
                if (username.Length < 1)
                {
                    return new string[0];
                }
                IPrincipal currentUser = GetCurrentUser();
                if (((currentUser != null) && (currentUser is RolePrincipal)) && ((((RolePrincipal) currentUser).ProviderName == Provider.Name) && System.Web.Util.StringUtil.EqualsIgnoreCase(username, currentUser.Identity.Name)))
                {
                    roles = ((RolePrincipal) currentUser).GetRoles();
                    flag = true;
                }
                else
                {
                    roles = Provider.GetRolesForUser(username);
                }
                strArray2 = roles;
            }
            finally
            {
                if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(4, 8))
                {
                    if (EtwTrace.IsTraceEnabled(5, 8))
                    {
                        string str = null;
                        if ((roles != null) && (roles.Length > 0))
                        {
                            str = roles[0];
                        }
                        for (int i = 1; i < roles.Length; i++)
                        {
                            str = str + "," + roles[i];
                        }
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_GET_USER_ROLES, HttpContext.Current.WorkerRequest, flag ? "RolePrincipal" : Provider.GetType().FullName, username, str, null);
                    }
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_END, HttpContext.Current.WorkerRequest, flag ? "RolePrincipal" : Provider.GetType().FullName, username);
                }
            }
            return strArray2;
        }

        public static string[] GetUsersInRole(string roleName)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            return Provider.GetUsersInRole(roleName);
        }

        private static void Initialize()
        {
            if (s_Initialized)
            {
                if (s_InitializeException != null)
                {
                    throw s_InitializeException;
                }
                if (s_InitializedDefaultProvider)
                {
                    return;
                }
            }
            lock (s_lock)
            {
                if (s_Initialized)
                {
                    if (s_InitializeException != null)
                    {
                        throw s_InitializeException;
                    }
                    if (s_InitializedDefaultProvider)
                    {
                        return;
                    }
                }
                try
                {
                    if (HostingEnvironment.IsHosted)
                    {
                        HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
                    }
                    RoleManagerSection roleManager = RuntimeConfig.GetAppConfig().RoleManager;
                    if (!s_EnabledSet)
                    {
                        s_Enabled = roleManager.Enabled;
                    }
                    s_CookieName = roleManager.CookieName;
                    s_CacheRolesInCookie = roleManager.CacheRolesInCookie;
                    s_CookieTimeout = (int) roleManager.CookieTimeout.TotalMinutes;
                    s_CookiePath = roleManager.CookiePath;
                    s_CookieRequireSSL = roleManager.CookieRequireSSL;
                    s_CookieSlidingExpiration = roleManager.CookieSlidingExpiration;
                    s_CookieProtection = roleManager.CookieProtection;
                    s_Domain = roleManager.Domain;
                    s_CreatePersistentCookie = roleManager.CreatePersistentCookie;
                    s_MaxCachedResults = roleManager.MaxCachedResults;
                    if (s_Enabled)
                    {
                        if (s_MaxCachedResults < 0)
                        {
                            throw new ProviderException(System.Web.SR.GetString("Value_must_be_non_negative_integer", new object[] { "maxCachedResults" }));
                        }
                        InitializeSettings(roleManager);
                        InitializeDefaultProvider(roleManager);
                    }
                }
                catch (Exception exception)
                {
                    s_InitializeException = exception;
                }
                s_Initialized = true;
            }
            if (s_InitializeException != null)
            {
                throw s_InitializeException;
            }
        }

        private static void InitializeDefaultProvider(RoleManagerSection settings)
        {
            bool flag = !HostingEnvironment.IsHosted || (BuildManager.PreStartInitStage == PreStartInitStage.AfterPreStartInit);
            if (!s_InitializedDefaultProvider && flag)
            {
                s_Providers.SetReadOnly();
                if (settings.DefaultProvider == null)
                {
                    s_InitializeException = new ProviderException(System.Web.SR.GetString("Def_role_provider_not_specified"));
                }
                else
                {
                    try
                    {
                        s_Provider = s_Providers[settings.DefaultProvider];
                    }
                    catch
                    {
                    }
                }
                if (s_Provider == null)
                {
                    s_InitializeException = new ConfigurationErrorsException(System.Web.SR.GetString("Def_role_provider_not_found"), settings.ElementInformation.Properties["defaultProvider"].Source, settings.ElementInformation.Properties["defaultProvider"].LineNumber);
                }
                s_InitializedDefaultProvider = true;
            }
        }

        private static void InitializeSettings(RoleManagerSection settings)
        {
            if (!s_Initialized)
            {
                s_Providers = new RoleProviderCollection();
                if (HostingEnvironment.IsHosted)
                {
                    ProvidersHelper.InstantiateProviders(settings.Providers, s_Providers, typeof(RoleProvider));
                }
                else
                {
                    foreach (ProviderSettings settings2 in settings.Providers)
                    {
                        Type c = Type.GetType(settings2.Type, true, true);
                        if (!typeof(RoleProvider).IsAssignableFrom(c))
                        {
                            throw new ArgumentException(System.Web.SR.GetString("Provider_must_implement_type", new object[] { typeof(RoleProvider).ToString() }));
                        }
                        RoleProvider provider = (RoleProvider) Activator.CreateInstance(c);
                        NameValueCollection parameters = settings2.Parameters;
                        NameValueCollection config = new NameValueCollection(parameters.Count, StringComparer.Ordinal);
                        foreach (string str in parameters)
                        {
                            config[str] = parameters[str];
                        }
                        provider.Initialize(settings2.Name, config);
                        s_Providers.Add(provider);
                    }
                }
            }
        }

        public static bool IsUserInRole(string roleName)
        {
            return IsUserInRole(GetCurrentUserName(), roleName);
        }

        public static bool IsUserInRole(string username, string roleName)
        {
            bool flag3;
            if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(4, 8))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_BEGIN, HttpContext.Current.WorkerRequest);
            }
            EnsureEnabled();
            bool flag = false;
            bool flag2 = false;
            try
            {
                SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
                SecUtility.CheckParameter(ref username, true, false, true, 0, "username");
                if (username.Length < 1)
                {
                    return false;
                }
                IPrincipal currentUser = GetCurrentUser();
                if (((currentUser != null) && (currentUser is RolePrincipal)) && ((((RolePrincipal) currentUser).ProviderName == Provider.Name) && System.Web.Util.StringUtil.EqualsIgnoreCase(username, currentUser.Identity.Name)))
                {
                    flag = currentUser.IsInRole(roleName);
                }
                else
                {
                    flag = Provider.IsUserInRole(username, roleName);
                }
                flag3 = flag;
            }
            finally
            {
                if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(4, 8))
                {
                    if (EtwTrace.IsTraceEnabled(5, 8))
                    {
                        string str = System.Web.SR.Resources.GetString(flag ? "Etw_Success" : "Etw_Failure", CultureInfo.InstalledUICulture);
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_IS_USER_IN_ROLE, HttpContext.Current.WorkerRequest, flag2 ? "RolePrincipal" : Provider.GetType().FullName, username, roleName, str);
                    }
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_END, HttpContext.Current.WorkerRequest, flag2 ? "RolePrincipal" : Provider.GetType().FullName, username);
                }
            }
            return flag3;
        }

        public static void RemoveUserFromRole(string username, string roleName)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            SecUtility.CheckParameter(ref username, true, true, true, 0, "username");
            Provider.RemoveUsersFromRoles(new string[] { username }, new string[] { roleName });
            try
            {
                RolePrincipal currentUser = GetCurrentUser() as RolePrincipal;
                if (((currentUser != null) && (currentUser.ProviderName == Provider.Name)) && (currentUser.IsRoleListCached && System.Web.Util.StringUtil.EqualsIgnoreCase(currentUser.Identity.Name, username)))
                {
                    currentUser.SetDirty();
                }
            }
            catch
            {
            }
        }

        public static void RemoveUserFromRoles(string username, string[] roleNames)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref username, true, true, true, 0, "username");
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 0, "roleNames");
            Provider.RemoveUsersFromRoles(new string[] { username }, roleNames);
            try
            {
                RolePrincipal currentUser = GetCurrentUser() as RolePrincipal;
                if (((currentUser != null) && (currentUser.ProviderName == Provider.Name)) && (currentUser.IsRoleListCached && System.Web.Util.StringUtil.EqualsIgnoreCase(currentUser.Identity.Name, username)))
                {
                    currentUser.SetDirty();
                }
            }
            catch
            {
            }
        }

        public static void RemoveUsersFromRole(string[] usernames, string roleName)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 0, "usernames");
            Provider.RemoveUsersFromRoles(usernames, new string[] { roleName });
            try
            {
                RolePrincipal currentUser = GetCurrentUser() as RolePrincipal;
                if (((currentUser != null) && (currentUser.ProviderName == Provider.Name)) && currentUser.IsRoleListCached)
                {
                    foreach (string str in usernames)
                    {
                        if (System.Web.Util.StringUtil.EqualsIgnoreCase(currentUser.Identity.Name, str))
                        {
                            currentUser.SetDirty();
                            return;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public static void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            EnsureEnabled();
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 0, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 0, "usernames");
            Provider.RemoveUsersFromRoles(usernames, roleNames);
            try
            {
                RolePrincipal currentUser = GetCurrentUser() as RolePrincipal;
                if (((currentUser != null) && (currentUser.ProviderName == Provider.Name)) && currentUser.IsRoleListCached)
                {
                    foreach (string str in usernames)
                    {
                        if (System.Web.Util.StringUtil.EqualsIgnoreCase(currentUser.Identity.Name, str))
                        {
                            currentUser.SetDirty();
                            return;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public static bool RoleExists(string roleName)
        {
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            return Provider.RoleExists(roleName);
        }

        public static string ApplicationName
        {
            get
            {
                return Provider.ApplicationName;
            }
            set
            {
                Provider.ApplicationName = value;
            }
        }

        public static bool CacheRolesInCookie
        {
            get
            {
                Initialize();
                return s_CacheRolesInCookie;
            }
        }

        public static string CookieName
        {
            get
            {
                Initialize();
                return s_CookieName;
            }
        }

        public static string CookiePath
        {
            get
            {
                Initialize();
                return s_CookiePath;
            }
        }

        public static CookieProtection CookieProtectionValue
        {
            get
            {
                Initialize();
                return s_CookieProtection;
            }
        }

        public static bool CookieRequireSSL
        {
            get
            {
                Initialize();
                return s_CookieRequireSSL;
            }
        }

        public static bool CookieSlidingExpiration
        {
            get
            {
                Initialize();
                return s_CookieSlidingExpiration;
            }
        }

        public static int CookieTimeout
        {
            get
            {
                Initialize();
                return s_CookieTimeout;
            }
        }

        public static bool CreatePersistentCookie
        {
            get
            {
                Initialize();
                return s_CreatePersistentCookie;
            }
        }

        public static string Domain
        {
            get
            {
                Initialize();
                return s_Domain;
            }
        }

        public static bool Enabled
        {
            get
            {
                if (HostingEnvironment.IsHosted && !HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Low))
                {
                    return false;
                }
                if (!s_Initialized && !s_EnabledSet)
                {
                    s_Enabled = RuntimeConfig.GetAppConfig().RoleManager.Enabled;
                    s_EnabledSet = true;
                }
                return s_Enabled;
            }
            set
            {
                BuildManager.ThrowIfPreAppStartNotRunning();
                s_Enabled = value;
                s_EnabledSet = true;
            }
        }

        public static int MaxCachedResults
        {
            get
            {
                Initialize();
                return s_MaxCachedResults;
            }
        }

        public static RoleProvider Provider
        {
            get
            {
                EnsureEnabled();
                if (s_Provider == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Def_role_provider_not_found"));
                }
                return s_Provider;
            }
        }

        public static RoleProviderCollection Providers
        {
            get
            {
                EnsureEnabled();
                return s_Providers;
            }
        }
    }
}

