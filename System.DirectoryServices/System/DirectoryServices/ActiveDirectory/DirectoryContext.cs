namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    [EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
    public class DirectoryContext
    {
        internal static LoadLibrarySafeHandle ADAMHandle;
        internal static LoadLibrarySafeHandle ADHandle;
        private bool contextIsValid;
        private DirectoryContextType contextType;
        private NetworkCredential credential;
        private static bool dnsgetdcSupported = true;
        private string name;
        internal bool passwordIsNull;
        private static bool platformSupported = true;
        private static bool serverBindSupported = true;
        internal string serverName;
        internal bool usernameIsNull;
        private bool validated;
        private static bool w2k = false;

        static DirectoryContext()
        {
            OperatingSystem oSVersion = Environment.OSVersion;
            if ((oSVersion.Platform == PlatformID.Win32NT) && (oSVersion.Version.Major >= 5))
            {
                if ((oSVersion.Version.Major == 5) && (oSVersion.Version.Minor == 0))
                {
                    w2k = true;
                    dnsgetdcSupported = false;
                    OSVersionInfoEx ver = new OSVersionInfoEx();
                    if (!System.DirectoryServices.ActiveDirectory.NativeMethods.GetVersionEx(ver))
                    {
                        int lastError = System.DirectoryServices.ActiveDirectory.NativeMethods.GetLastError();
                        throw new SystemException(Res.GetString("VersionFailure", new object[] { lastError }));
                    }
                    if (ver.servicePackMajor < 3)
                    {
                        serverBindSupported = false;
                    }
                }
                GetLibraryHandle();
            }
            else
            {
                platformSupported = false;
                serverBindSupported = false;
                dnsgetdcSupported = false;
            }
        }

        internal DirectoryContext(DirectoryContext context)
        {
            this.name = context.Name;
            this.contextType = context.ContextType;
            this.credential = context.Credential;
            this.usernameIsNull = context.usernameIsNull;
            this.passwordIsNull = context.passwordIsNull;
            if (context.ContextType != DirectoryContextType.ConfigurationSet)
            {
                this.serverName = context.serverName;
            }
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectoryContext(DirectoryContextType contextType)
        {
            if ((contextType != DirectoryContextType.Domain) && (contextType != DirectoryContextType.Forest))
            {
                throw new ArgumentException(Res.GetString("OnlyDomainOrForest"), "contextType");
            }
            this.InitializeDirectoryContext(contextType, null, null, null);
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectoryContext(DirectoryContextType contextType, string name)
        {
            if ((contextType < DirectoryContextType.Domain) || (contextType > DirectoryContextType.ApplicationPartition))
            {
                throw new InvalidEnumArgumentException("contextType", (int) contextType, typeof(DirectoryContextType));
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "name");
            }
            this.InitializeDirectoryContext(contextType, name, null, null);
        }

        internal DirectoryContext(DirectoryContextType contextType, string name, DirectoryContext context)
        {
            this.name = name;
            this.contextType = contextType;
            if (context != null)
            {
                this.credential = context.Credential;
                this.usernameIsNull = context.usernameIsNull;
                this.passwordIsNull = context.passwordIsNull;
            }
            else
            {
                this.credential = new NetworkCredential(null, "", null);
                this.usernameIsNull = true;
                this.passwordIsNull = true;
            }
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectoryContext(DirectoryContextType contextType, string username, string password)
        {
            if ((contextType != DirectoryContextType.Domain) && (contextType != DirectoryContextType.Forest))
            {
                throw new ArgumentException(Res.GetString("OnlyDomainOrForest"), "contextType");
            }
            this.InitializeDirectoryContext(contextType, null, username, password);
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectoryContext(DirectoryContextType contextType, string name, string username, string password)
        {
            if ((contextType < DirectoryContextType.Domain) || (contextType > DirectoryContextType.ApplicationPartition))
            {
                throw new InvalidEnumArgumentException("contextType", (int) contextType, typeof(DirectoryContextType));
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "name");
            }
            this.InitializeDirectoryContext(contextType, name, username, password);
        }

        internal static string GetDnsDomainName(string domainName)
        {
            DomainControllerInfo info;
            int errorCode = 0;
            errorCode = Locator.DsGetDcNameWrapper(null, domainName, null, 0x10L, out info);
            if (errorCode == 0x54b)
            {
                errorCode = Locator.DsGetDcNameWrapper(null, domainName, null, 0x11L, out info);
                switch (errorCode)
                {
                    case 0x54b:
                        return null;

                    case 0:
                        goto Label_0044;
                }
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
            }
            if (errorCode != 0)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
            }
        Label_0044:
            return info.DomainName;
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        private static void GetLibraryHandle()
        {
            IntPtr ptr = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LoadLibrary(Environment.SystemDirectory + @"\ntdsapi.dll");
            if (ptr == IntPtr.Zero)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            ADHandle = new LoadLibrarySafeHandle(ptr);
            ptr = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LoadLibrary(Environment.CurrentDirectory + @"\ntdsapi.dll");
            if (ptr == IntPtr.Zero)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Environment.SystemDirectory, 0, Environment.SystemDirectory.Length - 8);
                ptr = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LoadLibrary(builder.ToString() + @"ADAM\ntdsapi.dll");
                if (ptr == IntPtr.Zero)
                {
                    ADAMHandle = ADHandle;
                }
                else
                {
                    ADAMHandle = new LoadLibrarySafeHandle(ptr);
                }
            }
            else
            {
                ADAMHandle = new LoadLibrarySafeHandle(ptr);
            }
        }

        internal static string GetLoggedOnDomain()
        {
            string dnsDomainName = null;
            LsaLogonProcessSafeHandle handle;
            NegotiateCallerNameRequest structure = new NegotiateCallerNameRequest();
            int submitBufferLength = Marshal.SizeOf(structure);
            IntPtr zero = IntPtr.Zero;
            NegotiateCallerNameResponse response = new NegotiateCallerNameResponse();
            int status = System.DirectoryServices.ActiveDirectory.NativeMethods.LsaConnectUntrusted(out handle);
            switch (status)
            {
                case 0:
                    int num2;
                    int num3;
                    structure.messageType = 1;
                    status = System.DirectoryServices.ActiveDirectory.NativeMethods.LsaCallAuthenticationPackage(handle, 0, structure, submitBufferLength, out zero, out num2, out num3);
                    try
                    {
                        if ((status != 0) || (num3 != 0))
                        {
                            if (status == -1073741756)
                            {
                                throw new OutOfMemoryException();
                            }
                            if ((status != 0) || (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaNtStatusToWinError(num3) != 0x520))
                            {
                                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaNtStatusToWinError((status != 0) ? status : num3));
                            }
                            WindowsIdentity current = WindowsIdentity.GetCurrent();
                            int index = current.Name.IndexOf('\\');
                            dnsDomainName = current.Name.Substring(0, index);
                        }
                        else
                        {
                            Marshal.PtrToStructure(zero, response);
                            int length = response.callerName.IndexOf('\\');
                            dnsDomainName = response.callerName.Substring(0, length);
                        }
                        if ((dnsDomainName != null) && (Utils.Compare(dnsDomainName, Utils.GetNtAuthorityString()) == 0))
                        {
                            dnsDomainName = GetDnsDomainName(null);
                        }
                        else
                        {
                            dnsDomainName = GetDnsDomainName(dnsDomainName);
                        }
                        if (dnsDomainName == null)
                        {
                            throw new ActiveDirectoryOperationException(Res.GetString("ContextNotAssociatedWithDomain"));
                        }
                        return dnsDomainName;
                    }
                    finally
                    {
                        if (zero != IntPtr.Zero)
                        {
                            System.DirectoryServices.ActiveDirectory.NativeMethods.LsaFreeReturnBuffer(zero);
                        }
                    }
                    break;

                case -1073741756:
                    throw new OutOfMemoryException();
            }
            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaNtStatusToWinError(status));
        }

        internal string GetServerName()
        {
            if (this.serverName == null)
            {
                switch (this.contextType)
                {
                    case DirectoryContextType.Domain:
                    case DirectoryContextType.Forest:
                        goto Label_004A;

                    case DirectoryContextType.DirectoryServer:
                        this.serverName = this.name;
                        break;

                    case DirectoryContextType.ConfigurationSet:
                        using (AdamInstance instance = ConfigurationSet.FindAnyAdamInstance(this))
                        {
                            this.serverName = instance.Name;
                            break;
                        }
                        goto Label_004A;

                    case DirectoryContextType.ApplicationPartition:
                        this.serverName = this.name;
                        break;
                }
            }
            goto Label_009D;
        Label_004A:
            if ((this.name == null) || ((this.contextType == DirectoryContextType.Forest) && this.isCurrentForest()))
            {
                this.serverName = GetLoggedOnDomain();
            }
            else
            {
                this.serverName = GetDnsDomainName(this.name);
            }
        Label_009D:
            return this.serverName;
        }

        internal void InitializeDirectoryContext(DirectoryContextType contextType, string name, string username, string password)
        {
            if (!platformSupported)
            {
                throw new PlatformNotSupportedException(Res.GetString("SupportedPlatforms"));
            }
            this.name = name;
            this.contextType = contextType;
            this.credential = new NetworkCredential(username, password);
            if (username == null)
            {
                this.usernameIsNull = true;
            }
            if (password == null)
            {
                this.passwordIsNull = true;
            }
        }

        internal bool isADAMConfigSet()
        {
            if (this.contextType != DirectoryContextType.ConfigurationSet)
            {
                return false;
            }
            if (!this.validated)
            {
                this.contextIsValid = IsContextValid(this, DirectoryContextType.ConfigurationSet);
                this.validated = true;
            }
            return this.contextIsValid;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        internal static bool IsContextValid(DirectoryContext context, DirectoryContextType contextType)
        {
            bool flag = false;
            if ((contextType == DirectoryContextType.Domain) || ((contextType == DirectoryContextType.Forest) && (context.Name == null)))
            {
                DomainControllerInfo info;
                string name = context.Name;
                if (name == null)
                {
                    context.serverName = GetLoggedOnDomain();
                    return true;
                }
                int errorCode = 0;
                errorCode = Locator.DsGetDcNameWrapper(null, name, null, 0x10L, out info);
                switch (errorCode)
                {
                    case 0x54b:
                        errorCode = Locator.DsGetDcNameWrapper(null, name, null, 0x11L, out info);
                        if (errorCode == 0x54b)
                        {
                            return false;
                        }
                        if (errorCode != 0)
                        {
                            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
                        }
                        context.serverName = info.DomainName;
                        return true;

                    case 0x4bc:
                        return false;
                }
                if (errorCode != 0)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
                }
                context.serverName = info.DomainName;
                return true;
            }
            if (contextType == DirectoryContextType.Forest)
            {
                DomainControllerInfo info2;
                int num2 = 0;
                num2 = Locator.DsGetDcNameWrapper(null, context.Name, null, 80L, out info2);
                switch (num2)
                {
                    case 0x54b:
                        num2 = Locator.DsGetDcNameWrapper(null, context.Name, null, 0x51L, out info2);
                        if (num2 == 0x54b)
                        {
                            return false;
                        }
                        if (num2 != 0)
                        {
                            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(num2);
                        }
                        context.serverName = info2.DnsForestName;
                        return true;

                    case 0x4bc:
                        return false;
                }
                if (num2 != 0)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(num2);
                }
                context.serverName = info2.DnsForestName;
                return true;
            }
            if (contextType == DirectoryContextType.ApplicationPartition)
            {
                DomainControllerInfo info3;
                int num3 = 0;
                num3 = Locator.DsGetDcNameWrapper(null, context.Name, null, 0x8000L, out info3);
                switch (num3)
                {
                    case 0x54b:
                        num3 = Locator.DsGetDcNameWrapper(null, context.Name, null, 0x8001L, out info3);
                        if (num3 == 0x54b)
                        {
                            return false;
                        }
                        if (num3 != 0)
                        {
                            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(num3);
                        }
                        return true;

                    case 0x4bc:
                        return false;
                }
                if (num3 != 0)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(num3);
                }
                return true;
            }
            if (contextType == DirectoryContextType.DirectoryServer)
            {
                string str3;
                string str2 = null;
                str2 = Utils.SplitServerNameAndPortNumber(context.Name, out str3);
                using (DirectoryEntry entry = new DirectoryEntry("WinNT://" + str2 + ",computer", context.UserName, context.Password, Utils.DefaultAuthType))
                {
                    try
                    {
                        entry.Bind(true);
                        flag = true;
                    }
                    catch (COMException exception)
                    {
                        if (((exception.ErrorCode != -2147024843) && (exception.ErrorCode != -2147024845)) && (exception.ErrorCode != -2147463168))
                        {
                            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(context, exception);
                        }
                        return false;
                    }
                    return flag;
                }
            }
            return true;
        }

        internal bool isCurrentForest()
        {
            DomainControllerInfo info2;
            DomainControllerInfo info = Locator.GetDomainControllerInfo(null, this.name, null, 0x40000010L);
            string loggedOnDomain = GetLoggedOnDomain();
            int errorCode = Locator.DsGetDcNameWrapper(null, loggedOnDomain, null, 0x40000010L, out info2);
            if (errorCode == 0)
            {
                return (Utils.Compare(info.DnsForestName, info2.DnsForestName) == 0);
            }
            if (errorCode != 0x54b)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
            }
            return false;
        }

        internal bool isDomain()
        {
            if (this.contextType != DirectoryContextType.Domain)
            {
                return false;
            }
            if (!this.validated)
            {
                this.contextIsValid = IsContextValid(this, DirectoryContextType.Domain);
                this.validated = true;
            }
            return this.contextIsValid;
        }

        internal bool isNdnc()
        {
            if (this.contextType != DirectoryContextType.ApplicationPartition)
            {
                return false;
            }
            if (!this.validated)
            {
                this.contextIsValid = IsContextValid(this, DirectoryContextType.ApplicationPartition);
                this.validated = true;
            }
            return this.contextIsValid;
        }

        internal bool isRootDomain()
        {
            if (this.contextType != DirectoryContextType.Forest)
            {
                return false;
            }
            if (!this.validated)
            {
                this.contextIsValid = IsContextValid(this, DirectoryContextType.Forest);
                this.validated = true;
            }
            return this.contextIsValid;
        }

        internal bool isServer()
        {
            if (this.contextType != DirectoryContextType.DirectoryServer)
            {
                return false;
            }
            if (!this.validated)
            {
                if (w2k)
                {
                    this.contextIsValid = (IsContextValid(this, DirectoryContextType.DirectoryServer) && !IsContextValid(this, DirectoryContextType.Domain)) && !IsContextValid(this, DirectoryContextType.ApplicationPartition);
                }
                else
                {
                    this.contextIsValid = IsContextValid(this, DirectoryContextType.DirectoryServer);
                }
                this.validated = true;
            }
            return this.contextIsValid;
        }

        internal bool useServerBind()
        {
            if (this.ContextType != DirectoryContextType.DirectoryServer)
            {
                return (this.ContextType == DirectoryContextType.ConfigurationSet);
            }
            return true;
        }

        public DirectoryContextType ContextType
        {
            get
            {
                return this.contextType;
            }
        }

        internal NetworkCredential Credential
        {
            get
            {
                return this.credential;
            }
        }

        internal static bool DnsgetdcSupported
        {
            get
            {
                return dnsgetdcSupported;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        internal string Password
        {
            [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                if (this.passwordIsNull)
                {
                    return null;
                }
                return this.credential.Password;
            }
        }

        internal static bool ServerBindSupported
        {
            get
            {
                return serverBindSupported;
            }
        }

        public string UserName
        {
            get
            {
                if (this.usernameIsNull)
                {
                    return null;
                }
                return this.credential.UserName;
            }
        }
    }
}

