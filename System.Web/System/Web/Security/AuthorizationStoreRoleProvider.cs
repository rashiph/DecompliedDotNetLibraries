namespace System.Web.Security
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    public class AuthorizationStoreRoleProvider : RoleProvider
    {
        private string _AppName;
        private int _CacheRefreshInterval;
        private string _ConnectionString;
        private bool _InitAppDone;
        private DateTime _LastUpdateCacheDate;
        private bool _NewAuthInterface;
        private object _ObjAzApplication;
        private object _ObjAzAuthorizationStoreClass;
        private object _ObjAzScope;
        private string _ScopeName;
        private string _XmlFileName;

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, "API_not_supported_at_this_level");
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 0, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 0, "usernames");
            int num = 0;
            object[] args = new object[2];
            object[] objArray2 = new object[roleNames.Length];
            foreach (string str in roleNames)
            {
                objArray2[num++] = this.GetRole(str);
            }
            try
            {
                try
                {
                    foreach (object obj2 in objArray2)
                    {
                        foreach (string str2 in usernames)
                        {
                            args[0] = str2;
                            args[1] = null;
                            this.CallMethod(obj2, "AddMemberName", args);
                        }
                    }
                    foreach (object obj3 in objArray2)
                    {
                        args[0] = 0;
                        args[1] = null;
                        this.CallMethod(obj3, "Submit", args);
                    }
                }
                finally
                {
                    foreach (object obj4 in objArray2)
                    {
                        Marshal.FinalReleaseComObject(obj4);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private object CallMethod(object objectToCallOn, string methodName, object[] args)
        {
            object obj2;
            if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
            {
                InternalSecurityPermissions.Unrestricted.Assert();
            }
            try
            {
                using (new ApplicationImpersonationContext())
                {
                    obj2 = objectToCallOn.GetType().InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, objectToCallOn, args, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                throw;
            }
            return obj2;
        }

        private object CallProperty(object objectToCallOn, string propName, object[] args)
        {
            object obj2;
            if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
            {
                InternalSecurityPermissions.Unrestricted.Assert();
            }
            try
            {
                using (new ApplicationImpersonationContext())
                {
                    obj2 = objectToCallOn.GetType().InvokeMember(propName, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, objectToCallOn, args, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                throw;
            }
            return obj2;
        }

        public override void CreateRole(string roleName)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, "API_not_supported_at_this_level");
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            this.InitApp();
            object[] args = new object[] { roleName, null };
            object objectToCallOn = this.CallMethod((this._ObjAzScope != null) ? this._ObjAzScope : this._ObjAzApplication, "CreateRole", args);
            args[0] = 0;
            args[1] = null;
            try
            {
                try
                {
                    this.CallMethod(objectToCallOn, "Submit", args);
                }
                finally
                {
                    Marshal.FinalReleaseComObject(objectToCallOn);
                }
            }
            catch
            {
                throw;
            }
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, "API_not_supported_at_this_level");
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            this.InitApp();
            if (throwOnPopulatedRole)
            {
                string[] usersInRole;
                try
                {
                    usersInRole = this.GetUsersInRole(roleName);
                }
                catch
                {
                    return false;
                }
                if (usersInRole.Length != 0)
                {
                    throw new ProviderException(System.Web.SR.GetString("Role_is_not_empty"));
                }
            }
            object[] args = new object[] { roleName, null };
            this.CallMethod((this._ObjAzScope != null) ? this._ObjAzScope : this._ObjAzApplication, "DeleteRole", args);
            args[0] = 0;
            args[1] = null;
            this.CallMethod((this._ObjAzScope != null) ? this._ObjAzScope : this._ObjAzApplication, "Submit", args);
            return true;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            this.InitApp();
            object obj2 = this.CallProperty((this._ObjAzScope != null) ? this._ObjAzScope : this._ObjAzApplication, "Roles", null);
            StringCollection strings = new StringCollection();
            try
            {
                if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
                {
                    InternalSecurityPermissions.Unrestricted.Assert();
                }
                try
                {
                    IEnumerable enumerable = (IEnumerable) obj2;
                    foreach (object obj3 in enumerable)
                    {
                        string str = (string) this.CallProperty(obj3, "Name", null);
                        strings.Add(str);
                    }
                }
                finally
                {
                    if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            catch
            {
                throw;
            }
            string[] array = new string[strings.Count];
            strings.CopyTo(array, 0);
            return array;
        }

        private object GetClientContext(string userName)
        {
            this.InitApp();
            IntPtr windowsTokenWithAssert = this.GetWindowsTokenWithAssert(userName);
            if (windowsTokenWithAssert != IntPtr.Zero)
            {
                return this.GetClientContextFromToken(windowsTokenWithAssert);
            }
            return this.GetClientContextFromName(userName);
        }

        private object GetClientContextFromName(string userName)
        {
            string[] strArray = userName.Split(new char[] { '\\' });
            string str = null;
            if (strArray.Length > 1)
            {
                str = strArray[0];
                userName = strArray[1];
            }
            object[] args = new object[] { userName, str, null };
            return this.CallMethod(this._ObjAzApplication, "InitializeClientContextFromName", args);
        }

        private object GetClientContextFromToken(IntPtr token)
        {
            if (this._NewAuthInterface)
            {
                object[] objArray = new object[] { (uint) ((int) token), 0, null };
                return this.CallMethod(this._ObjAzApplication, "InitializeClientContextFromToken2", objArray);
            }
            object[] args = new object[] { (ulong) ((long) token), null };
            return this.CallMethod(this._ObjAzApplication, "InitializeClientContextFromToken", args);
        }

        private object GetRole(string roleName)
        {
            this.InitApp();
            object[] args = new object[] { roleName, null };
            return this.CallMethod((this._ObjAzScope != null) ? this._ObjAzScope : this._ObjAzApplication, "OpenRole", args);
        }

        public override string[] GetRolesForUser(string username)
        {
            SecUtility.CheckParameter(ref username, true, false, true, 0, "username");
            if (username.Length < 1)
            {
                return new string[0];
            }
            return this.GetRolesForUserCore(username);
        }

        private string[] GetRolesForUserCore(string username)
        {
            object clientContext = this.GetClientContext(username);
            if (clientContext == null)
            {
                return new string[0];
            }
            object obj3 = this.CallMethod(clientContext, "GetRoles", new object[] { this._ScopeName });
            if ((obj3 == null) || !(obj3 is IEnumerable))
            {
                return new string[0];
            }
            StringCollection strings = new StringCollection();
            try
            {
                if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
                {
                    InternalSecurityPermissions.Unrestricted.Assert();
                }
                try
                {
                    IEnumerable enumerable = (IEnumerable) obj3;
                    foreach (object obj4 in enumerable)
                    {
                        string str = (string) obj4;
                        if (str != null)
                        {
                            strings.Add(str);
                        }
                    }
                }
                finally
                {
                    if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            catch
            {
                throw;
            }
            string[] array = new string[strings.Count];
            strings.CopyTo(array, 0);
            return array;
        }

        public override string[] GetUsersInRole(string roleName)
        {
            object obj3;
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            object role = this.GetRole(roleName);
            try
            {
                try
                {
                    obj3 = this.CallProperty(role, "MembersName", null);
                }
                finally
                {
                    Marshal.FinalReleaseComObject(role);
                }
            }
            catch
            {
                throw;
            }
            StringCollection strings = new StringCollection();
            try
            {
                if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
                {
                    InternalSecurityPermissions.Unrestricted.Assert();
                }
                try
                {
                    IEnumerable enumerable = (IEnumerable) obj3;
                    foreach (object obj4 in enumerable)
                    {
                        strings.Add((string) obj4);
                    }
                }
                finally
                {
                    if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            catch
            {
                throw;
            }
            string[] array = new string[strings.Count];
            strings.CopyTo(array, 0);
            return array;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private IntPtr GetWindowsTokenWithAssert(string userName)
        {
            if (HostingEnvironment.IsHosted)
            {
                HttpContext current = HttpContext.Current;
                if ((((current != null) && (current.User != null)) && ((current.User.Identity != null) && (current.User.Identity is WindowsIdentity))) && System.Web.Util.StringUtil.EqualsIgnoreCase(userName, current.User.Identity.Name))
                {
                    return ((WindowsIdentity) current.User.Identity).Token;
                }
            }
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            if (((currentPrincipal != null) && (currentPrincipal.Identity != null)) && ((currentPrincipal.Identity is WindowsIdentity) && System.Web.Util.StringUtil.EqualsIgnoreCase(userName, currentPrincipal.Identity.Name)))
            {
                return ((WindowsIdentity) currentPrincipal.Identity).Token;
            }
            return IntPtr.Zero;
        }

        private void InitApp()
        {
            try
            {
                using (new ApplicationImpersonationContext())
                {
                    if (this._InitAppDone)
                    {
                        if (DateTime.Now > this._LastUpdateCacheDate.AddMinutes((double) this.CacheRefreshInterval))
                        {
                            this._LastUpdateCacheDate = DateTime.Now;
                            this.CallMethod(this._ObjAzAuthorizationStoreClass, "UpdateCache", null);
                        }
                    }
                    else
                    {
                        lock (this)
                        {
                            if (!this._InitAppDone)
                            {
                                if (this._ConnectionString.ToLower(CultureInfo.InvariantCulture).StartsWith("msxml://", StringComparison.Ordinal))
                                {
                                    if (this._ConnectionString.Contains("/~/"))
                                    {
                                        string newValue = null;
                                        if (HostingEnvironment.IsHosted)
                                        {
                                            newValue = HttpRuntime.AppDomainAppPath;
                                        }
                                        else
                                        {
                                            Process currentProcess = Process.GetCurrentProcess();
                                            ProcessModule module = (currentProcess != null) ? currentProcess.MainModule : null;
                                            string str2 = (module != null) ? module.FileName : null;
                                            if (str2 != null)
                                            {
                                                newValue = Path.GetDirectoryName(str2);
                                            }
                                            if ((newValue == null) || (newValue.Length < 1))
                                            {
                                                newValue = Environment.CurrentDirectory;
                                            }
                                        }
                                        newValue = newValue.Replace('\\', '/');
                                        this._ConnectionString = this._ConnectionString.Replace("~", newValue);
                                    }
                                    string path = this._ConnectionString.Substring("msxml://".Length).Replace('/', '\\');
                                    if (HostingEnvironment.IsHosted)
                                    {
                                        HttpRuntime.CheckFilePermission(path, false);
                                    }
                                    if (!System.Web.Util.FileUtil.FileExists(path))
                                    {
                                        throw new FileNotFoundException(System.Web.SR.GetString("AuthStore_policy_file_not_found", new object[] { HttpRuntime.GetSafePath(path) }));
                                    }
                                    this._XmlFileName = path;
                                }
                                Type type = null;
                                try
                                {
                                    this._NewAuthInterface = true;
                                    type = Type.GetType("Microsoft.Interop.Security.AzRoles.AzAuthorizationStoreClass, Microsoft.Interop.Security.AzRoles, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", false);
                                    if (type == null)
                                    {
                                        type = Type.GetType("Microsoft.Interop.Security.AzRoles.AzAuthorizationStoreClass, Microsoft.Interop.Security.AzRoles, Version=1.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", false);
                                    }
                                    if (type == null)
                                    {
                                        this._NewAuthInterface = false;
                                        type = Type.GetType("Microsoft.Interop.Security.AzRoles.AzAuthorizationStoreClass, Microsoft.Interop.Security.AzRoles, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", true);
                                    }
                                }
                                catch (FileNotFoundException exception)
                                {
                                    HttpContext current = HttpContext.Current;
                                    if (current == null)
                                    {
                                        throw new ProviderException(System.Web.SR.GetString("AuthStoreNotInstalled_Title"), exception);
                                    }
                                    current.Response.Clear();
                                    current.Response.StatusCode = 500;
                                    current.Response.Write(AuthStoreErrorFormatter.GetErrorText());
                                    current.Response.End();
                                }
                                if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
                                {
                                    InternalSecurityPermissions.Unrestricted.Assert();
                                }
                                this._ObjAzAuthorizationStoreClass = Activator.CreateInstance(type);
                                object[] objArray3 = new object[3];
                                objArray3[0] = 0;
                                objArray3[1] = this._ConnectionString;
                                object[] args = objArray3;
                                this.CallMethod(this._ObjAzAuthorizationStoreClass, "Initialize", args);
                                args = new object[] { this._AppName, null };
                                if (this._NewAuthInterface)
                                {
                                    this._ObjAzApplication = this.CallMethod(this._ObjAzAuthorizationStoreClass, "OpenApplication2", args);
                                }
                                else
                                {
                                    this._ObjAzApplication = this.CallMethod(this._ObjAzAuthorizationStoreClass, "OpenApplication", args);
                                }
                                if (this._ObjAzApplication == null)
                                {
                                    throw new ProviderException(System.Web.SR.GetString("AuthStore_Application_not_found"));
                                }
                                this._ObjAzScope = null;
                                if (!string.IsNullOrEmpty(this._ScopeName))
                                {
                                    args[0] = this._ScopeName;
                                    args[1] = null;
                                    this._ObjAzScope = this.CallMethod(this._ObjAzApplication, "OpenScope", args);
                                    if (this._ObjAzScope == null)
                                    {
                                        throw new ProviderException(System.Web.SR.GetString("AuthStore_Scope_not_found"));
                                    }
                                }
                                this._LastUpdateCacheDate = DateTime.Now;
                                this._InitAppDone = true;
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
            if (string.IsNullOrEmpty(name))
            {
                name = "AuthorizationStoreRoleProvider";
            }
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", System.Web.SR.GetString("RoleAuthStoreProvider_description"));
            }
            base.Initialize(name, config);
            this._CacheRefreshInterval = SecUtility.GetIntValue(config, "cacheRefreshInterval", 60, false, 0);
            this._ScopeName = config["scopeName"];
            if ((this._ScopeName != null) && (this._ScopeName.Length == 0))
            {
                this._ScopeName = null;
            }
            this._ConnectionString = config["connectionStringName"];
            if ((this._ConnectionString == null) || (this._ConnectionString.Length < 1))
            {
                throw new ProviderException(System.Web.SR.GetString("Connection_name_not_specified"));
            }
            ConnectionStringSettings settings = RuntimeConfig.GetAppConfig().ConnectionStrings.ConnectionStrings[this._ConnectionString];
            if (settings == null)
            {
                throw new ProviderException(System.Web.SR.GetString("Connection_string_not_found", new object[] { this._ConnectionString }));
            }
            if (string.IsNullOrEmpty(settings.ConnectionString))
            {
                throw new ProviderException(System.Web.SR.GetString("Connection_string_not_found", new object[] { this._ConnectionString }));
            }
            this._ConnectionString = settings.ConnectionString;
            this._AppName = config["applicationName"];
            if (string.IsNullOrEmpty(this._AppName))
            {
                this._AppName = SecUtility.GetDefaultAppName();
            }
            if (this._AppName.Length > 0x100)
            {
                throw new ProviderException(System.Web.SR.GetString("Provider_application_name_too_long"));
            }
            config.Remove("connectionStringName");
            config.Remove("cacheRefreshInterval");
            config.Remove("applicationName");
            config.Remove("scopeName");
            if (config.Count > 0)
            {
                string key = config.GetKey(0);
                if (!string.IsNullOrEmpty(key))
                {
                    throw new ProviderException(System.Web.SR.GetString("Provider_unrecognized_attribute", new object[] { key }));
                }
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            SecUtility.CheckParameter(ref username, true, false, true, 0, "username");
            if (username.Length < 1)
            {
                return false;
            }
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            return this.IsUserInRoleCore(username, roleName);
        }

        private bool IsUserInRoleCore(string username, string roleName)
        {
            bool flag;
            object clientContext = this.GetClientContext(username);
            if (clientContext == null)
            {
                return false;
            }
            object obj3 = this.CallMethod(clientContext, "GetRoles", new object[] { this._ScopeName });
            if ((obj3 == null) || !(obj3 is IEnumerable))
            {
                return false;
            }
            try
            {
                if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
                {
                    InternalSecurityPermissions.Unrestricted.Assert();
                }
                try
                {
                    IEnumerable enumerable = (IEnumerable) obj3;
                    foreach (object obj4 in enumerable)
                    {
                        string str = (string) obj4;
                        if ((str != null) && System.Web.Util.StringUtil.EqualsIgnoreCase(str, roleName))
                        {
                            return true;
                        }
                    }
                    flag = false;
                }
                finally
                {
                    if (HostingEnvironment.IsHosted && (this._XmlFileName != null))
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            catch
            {
                throw;
            }
            return flag;
        }

        public override void RemoveUsersFromRoles(string[] userNames, string[] roleNames)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, "API_not_supported_at_this_level");
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 0, "roleNames");
            SecUtility.CheckArrayParameter(ref userNames, true, true, true, 0, "userNames");
            int num = 0;
            object[] args = new object[2];
            object[] objArray2 = new object[roleNames.Length];
            foreach (string str in roleNames)
            {
                objArray2[num++] = this.GetRole(str);
            }
            try
            {
                try
                {
                    foreach (object obj2 in objArray2)
                    {
                        foreach (string str2 in userNames)
                        {
                            args[0] = str2;
                            args[1] = null;
                            this.CallMethod(obj2, "DeleteMemberName", args);
                        }
                    }
                    foreach (object obj3 in objArray2)
                    {
                        args[0] = 0;
                        args[1] = null;
                        this.CallMethod(obj3, "Submit", args);
                    }
                }
                finally
                {
                    foreach (object obj4 in objArray2)
                    {
                        Marshal.FinalReleaseComObject(obj4);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override bool RoleExists(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            bool flag = false;
            object o = null;
            try
            {
                o = this.GetRole(roleName);
                flag = o != null;
            }
            catch (TargetInvocationException exception)
            {
                COMException innerException = exception.InnerException as COMException;
                if ((innerException == null) || (innerException.ErrorCode != -2147023728))
                {
                    throw;
                }
                return false;
            }
            finally
            {
                if (o != null)
                {
                    Marshal.FinalReleaseComObject(o);
                }
            }
            return flag;
        }

        public override string ApplicationName
        {
            get
            {
                return this._AppName;
            }
            set
            {
                if (this._AppName != value)
                {
                    if (value.Length > 0x100)
                    {
                        throw new ProviderException(System.Web.SR.GetString("Provider_application_name_too_long"));
                    }
                    this._AppName = value;
                    this._InitAppDone = false;
                }
            }
        }

        public int CacheRefreshInterval
        {
            get
            {
                return this._CacheRefreshInterval;
            }
        }

        public string ScopeName
        {
            get
            {
                return this._ScopeName;
            }
            set
            {
                if (this._ScopeName != value)
                {
                    this._ScopeName = value;
                    this._InitAppDone = false;
                }
            }
        }
    }
}

