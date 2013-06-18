namespace System.Web.Security
{
    using System;
    using System.Collections;
    using System.Configuration.Provider;
    using System.DirectoryServices;
    using System.DirectoryServices.ActiveDirectory;
    using System.DirectoryServices.Protocols;
    using System.Globalization;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Util;

    internal sealed class DirectoryInformation
    {
        private string adamPartitionDN;
        private TimeSpan adLockoutDuration;
        private string adspath;
        private System.DirectoryServices.AuthenticationTypes authenticationType;
        private System.DirectoryServices.AuthenticationTypes[,] authTypes;
        private int clientSearchTimeout = -1;
        private bool concurrentBindSupported;
        private ActiveDirectoryConnectionProtection connectionProtection;
        private string containerDN;
        private string creationContainerDN;
        private NetworkCredential credentials;
        private System.Web.Security.DirectoryType directoryType = System.Web.Security.DirectoryType.Unknown;
        private string domainName;
        private string forestName;
        private const int GC_PORT = 0xcc4;
        private const int GC_SSL_PORT = 0xcc5;
        private const string GUID_USERS_CONTAINER_W = "a9d1ca15768811d1aded00c04fd8d5cd";
        private bool isServer;
        private const string LDAP_CAP_ACTIVE_DIRECTORY_ADAM_OID = "1.2.840.113556.1.4.1851";
        private const string LDAP_CAP_ACTIVE_DIRECTORY_OID = "1.2.840.113556.1.4.800";
        private const string LDAP_SERVER_FAST_BIND_OID = "1.2.840.113556.1.4.1781";
        private AuthType ldapAuthType = AuthType.Basic;
        private AuthType[,] ldapAuthTypes;
        private int port = 0x185;
        private bool portSpecified;
        private DirectoryEntry rootdse;
        private string serverName;
        private int serverSearchTimeout = -1;
        internal const int SSL_PORT = 0x27c;

        internal DirectoryInformation(string adspath, NetworkCredential credentials, string connProtection, int clientSearchTimeout, int serverSearchTimeout, bool enablePasswordReset)
        {
            System.DirectoryServices.AuthenticationTypes[,] typesArray = new System.DirectoryServices.AuthenticationTypes[3, 2];
            typesArray[1, 0] = System.DirectoryServices.AuthenticationTypes.Encryption | System.DirectoryServices.AuthenticationTypes.Secure;
            typesArray[1, 1] = System.DirectoryServices.AuthenticationTypes.Encryption;
            typesArray[2, 0] = System.DirectoryServices.AuthenticationTypes.Sealing | System.DirectoryServices.AuthenticationTypes.Signing | System.DirectoryServices.AuthenticationTypes.Secure;
            typesArray[2, 1] = System.DirectoryServices.AuthenticationTypes.Sealing | System.DirectoryServices.AuthenticationTypes.Signing | System.DirectoryServices.AuthenticationTypes.Secure;
            this.authTypes = typesArray;
            this.ldapAuthTypes = new AuthType[,] { { AuthType.Negotiate, AuthType.Basic }, { AuthType.Negotiate, AuthType.Basic }, { AuthType.Negotiate, AuthType.Negotiate } };
            this.adspath = adspath;
            this.credentials = credentials;
            this.clientSearchTimeout = clientSearchTimeout;
            this.serverSearchTimeout = serverSearchTimeout;
            if (!adspath.StartsWith("LDAP", StringComparison.Ordinal))
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_OnlyLdap_supported"));
            }
            System.Web.Security.NativeComInterfaces.IAdsPathname pathname = (System.Web.Security.NativeComInterfaces.IAdsPathname) new System.Web.Security.NativeComInterfaces.Pathname();
            try
            {
                pathname.Set(adspath, 1);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147463168)
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_invalid_path"));
                }
                throw;
            }
            try
            {
                this.serverName = pathname.Retrieve(9);
            }
            catch (COMException exception2)
            {
                if (exception2.ErrorCode == -2147463168)
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_ServerlessADsPath_not_supported"));
                }
                throw;
            }
            this.creationContainerDN = this.containerDN = pathname.Retrieve(7);
            int index = this.serverName.IndexOf(':');
            if (index != -1)
            {
                string serverName = this.serverName;
                this.serverName = serverName.Substring(0, index);
                this.port = int.Parse(serverName.Substring(index + 1), NumberFormatInfo.InvariantInfo);
                this.portSpecified = true;
            }
            if (string.Compare(connProtection, "Secure", StringComparison.Ordinal) != 0)
            {
                goto Label_039F;
            }
            bool flag = false;
            bool flag2 = false;
            if (!this.IsDefaultCredential())
            {
                this.authenticationType = this.GetAuthenticationTypes(ActiveDirectoryConnectionProtection.Ssl, CredentialsType.NonWindows);
                this.ldapAuthType = this.GetLdapAuthenticationTypes(ActiveDirectoryConnectionProtection.Ssl, CredentialsType.NonWindows);
                try
                {
                    this.rootdse = new DirectoryEntry(this.GetADsPath("rootdse"), this.GetUsername(), this.GetPassword(), this.authenticationType);
                    this.rootdse.RefreshCache();
                    this.connectionProtection = ActiveDirectoryConnectionProtection.Ssl;
                    if (!this.portSpecified)
                    {
                        this.port = 0x27c;
                        this.portSpecified = true;
                    }
                    goto Label_0282;
                }
                catch (COMException exception3)
                {
                    if (exception3.ErrorCode != -2147023570)
                    {
                        if (exception3.ErrorCode != -2147016646)
                        {
                            throw;
                        }
                        flag = true;
                    }
                    else
                    {
                        flag2 = true;
                    }
                    goto Label_0282;
                }
            }
            flag2 = true;
        Label_0282:
            if (flag2)
            {
                this.authenticationType = this.GetAuthenticationTypes(ActiveDirectoryConnectionProtection.Ssl, CredentialsType.Windows);
                this.ldapAuthType = this.GetLdapAuthenticationTypes(ActiveDirectoryConnectionProtection.Ssl, CredentialsType.Windows);
                try
                {
                    this.rootdse = new DirectoryEntry(this.GetADsPath("rootdse"), this.GetUsername(), this.GetPassword(), this.authenticationType);
                    this.rootdse.RefreshCache();
                    this.connectionProtection = ActiveDirectoryConnectionProtection.Ssl;
                    if (!this.portSpecified)
                    {
                        this.port = 0x27c;
                        this.portSpecified = true;
                    }
                }
                catch (COMException exception4)
                {
                    if (exception4.ErrorCode != -2147016646)
                    {
                        throw;
                    }
                    flag = true;
                }
            }
            if (!flag)
            {
                goto Label_0405;
            }
            this.authenticationType = this.GetAuthenticationTypes(ActiveDirectoryConnectionProtection.SignAndSeal, CredentialsType.Windows);
            this.ldapAuthType = this.GetLdapAuthenticationTypes(ActiveDirectoryConnectionProtection.SignAndSeal, CredentialsType.Windows);
            try
            {
                this.rootdse = new DirectoryEntry(this.GetADsPath("rootdse"), this.GetUsername(), this.GetPassword(), this.authenticationType);
                this.rootdse.RefreshCache();
                this.connectionProtection = ActiveDirectoryConnectionProtection.SignAndSeal;
                goto Label_0405;
            }
            catch (COMException exception5)
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_Secure_connection_not_established", new object[] { exception5.Message }), exception5);
            }
        Label_039F:
            if (this.IsDefaultCredential())
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_Default_Creds_not_supported"));
            }
            this.authenticationType = this.GetAuthenticationTypes(this.connectionProtection, CredentialsType.NonWindows);
            this.ldapAuthType = this.GetLdapAuthenticationTypes(this.connectionProtection, CredentialsType.NonWindows);
            this.rootdse = new DirectoryEntry(this.GetADsPath("rootdse"), this.GetUsername(), this.GetPassword(), this.authenticationType);
        Label_0405:
            if (this.rootdse == null)
            {
                this.rootdse = new DirectoryEntry(this.GetADsPath("RootDSE"), this.GetUsername(), this.GetPassword(), this.authenticationType);
            }
            this.directoryType = this.GetDirectoryType();
            if ((this.directoryType == System.Web.Security.DirectoryType.ADAM) && (this.connectionProtection == ActiveDirectoryConnectionProtection.SignAndSeal))
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_Ssl_connection_not_established"));
            }
            if ((this.directoryType == System.Web.Security.DirectoryType.AD) && ((this.port == 0xcc4) || (this.port == 0xcc5)))
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_GCPortsNotSupported"));
            }
            if (string.IsNullOrEmpty(this.containerDN))
            {
                if (this.directoryType == System.Web.Security.DirectoryType.AD)
                {
                    this.containerDN = (string) this.rootdse.Properties["defaultNamingContext"].Value;
                    if (this.containerDN == null)
                    {
                        throw new ProviderException(System.Web.SR.GetString("ADMembership_DefContainer_not_specified"));
                    }
                    DirectoryEntry entry = new DirectoryEntry(this.GetADsPath("<WKGUID=a9d1ca15768811d1aded00c04fd8d5cd," + this.containerDN + ">"), this.GetUsername(), this.GetPassword(), this.authenticationType);
                    try
                    {
                        this.creationContainerDN = (string) System.Web.Security.PropertyManager.GetPropertyValue(entry, "distinguishedName");
                        goto Label_05DE;
                    }
                    catch (COMException exception6)
                    {
                        if (exception6.ErrorCode == -2147016656)
                        {
                            throw new ProviderException(System.Web.SR.GetString("ADMembership_DefContainer_does_not_exist"));
                        }
                        throw;
                    }
                }
                throw new ProviderException(System.Web.SR.GetString("ADMembership_Container_must_be_specified"));
            }
            DirectoryEntry directoryEntry = new DirectoryEntry(this.GetADsPath(this.containerDN), this.GetUsername(), this.GetPassword(), this.authenticationType);
            try
            {
                this.creationContainerDN = this.containerDN = (string) System.Web.Security.PropertyManager.GetPropertyValue(directoryEntry, "distinguishedName");
            }
            catch (COMException exception7)
            {
                if (exception7.ErrorCode == -2147016656)
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_Container_does_not_exist"));
                }
                throw;
            }
        Label_05DE:
            using (LdapConnection connection = new LdapConnection(new LdapDirectoryIdentifier(this.serverName + ":" + this.port), GetCredentialsWithDomain(credentials), this.ldapAuthType) { SessionOptions = { ProtocolVersion = 3 } })
            {
                SearchResponse response;
                connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
                this.SetSessionOptionsForSecureConnection(connection, false);
                connection.Bind();
                SearchRequest request = new SearchRequest {
                    DistinguishedName = this.containerDN,
                    Filter = "(objectClass=*)",
                    Scope = System.DirectoryServices.Protocols.SearchScope.Base
                };
                request.Attributes.Add("distinguishedName");
                request.Attributes.Add("objectClass");
                if (this.ServerSearchTimeout != -1)
                {
                    request.TimeLimit = new TimeSpan(0, this.ServerSearchTimeout, 0);
                }
                try
                {
                    response = (SearchResponse) connection.SendRequest(request);
                    if ((response.ResultCode == ResultCode.Referral) || (response.ResultCode == ResultCode.NoSuchObject))
                    {
                        throw new ProviderException(System.Web.SR.GetString("ADMembership_Container_does_not_exist"));
                    }
                    if (response.ResultCode != ResultCode.Success)
                    {
                        throw new ProviderException(response.ErrorMessage);
                    }
                }
                catch (DirectoryOperationException exception8)
                {
                    SearchResponse response2 = (SearchResponse) exception8.Response;
                    if (response2.ResultCode == ResultCode.NoSuchObject)
                    {
                        throw new ProviderException(System.Web.SR.GetString("ADMembership_Container_does_not_exist"));
                    }
                    throw;
                }
                DirectoryAttribute objectClass = response.Entries[0].Attributes["objectClass"];
                if (!this.ContainerIsSuperiorOfUser(objectClass))
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_Container_not_superior"));
                }
                if ((this.connectionProtection == ActiveDirectoryConnectionProtection.None) || (this.connectionProtection == ActiveDirectoryConnectionProtection.Ssl))
                {
                    this.concurrentBindSupported = this.IsConcurrentBindSupported(connection);
                }
            }
            if (this.directoryType == System.Web.Security.DirectoryType.ADAM)
            {
                this.adamPartitionDN = this.GetADAMPartitionFromContainer();
            }
            else if (enablePasswordReset)
            {
                DirectoryEntry entry3 = new DirectoryEntry(this.GetADsPath((string) System.Web.Security.PropertyManager.GetPropertyValue(this.rootdse, "defaultNamingContext")), this.GetUsername(), this.GetPassword(), this.AuthenticationTypes);
                System.Web.Security.NativeComInterfaces.IAdsLargeInteger propertyValue = (System.Web.Security.NativeComInterfaces.IAdsLargeInteger) System.Web.Security.PropertyManager.GetPropertyValue(entry3, "lockoutDuration");
                long num2 = (propertyValue.HighPart * 0x100000000L) + ((uint) propertyValue.LowPart);
                this.adLockoutDuration = new TimeSpan(-num2);
            }
        }

        private bool ContainerIsSuperiorOfUser(DirectoryAttribute objectClass)
        {
            ArrayList list = new ArrayList();
            DirectoryEntry entry = new DirectoryEntry(this.GetADsPath("schema") + "/user", this.GetUsername(), this.GetPassword(), this.AuthenticationTypes);
            ArrayList list2 = new ArrayList();
            bool flag = false;
            object obj2 = null;
            try
            {
                obj2 = entry.InvokeGet("DerivedFrom");
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147463155)
                {
                    throw;
                }
                flag = true;
            }
            if (!flag)
            {
                if (obj2 is ICollection)
                {
                    list2.AddRange((ICollection) obj2);
                }
                else
                {
                    list2.Add((string) obj2);
                }
            }
            list2.Add("user");
            DirectoryEntry searchRoot = new DirectoryEntry(this.GetADsPath((string) this.rootdse.Properties["schemaNamingContext"].Value), this.GetUsername(), this.GetPassword(), this.AuthenticationTypes);
            DirectorySearcher searcher = new DirectorySearcher(searchRoot) {
                Filter = "(&(objectClass=classSchema)(|"
            };
            foreach (string str in list2)
            {
                searcher.Filter = searcher.Filter + "(ldapDisplayName=" + str + ")";
            }
            searcher.Filter = searcher.Filter + "))";
            searcher.SearchScope = System.DirectoryServices.SearchScope.OneLevel;
            searcher.PropertiesToLoad.Add("possSuperiors");
            searcher.PropertiesToLoad.Add("systemPossSuperiors");
            using (SearchResultCollection results = searcher.FindAll())
            {
                foreach (SearchResult result in results)
                {
                    list.AddRange(result.Properties["possSuperiors"]);
                    list.AddRange(result.Properties["systemPossSuperiors"]);
                }
            }
            foreach (string str2 in objectClass.GetValues(typeof(string)))
            {
                if (list.Contains(str2))
                {
                    return true;
                }
            }
            return false;
        }

        internal LdapConnection CreateNewLdapConnection(AuthType authType)
        {
            LdapConnection connection = null;
            connection = new LdapConnection(new LdapDirectoryIdentifier(this.serverName + ":" + this.port)) {
                AuthType = authType
            };
            connection.SessionOptions.ProtocolVersion = 3;
            this.SetSessionOptionsForSecureConnection(connection, true);
            return connection;
        }

        private string GetADAMPartitionFromContainer()
        {
            string str = null;
            int num = 0x7fffffff;
            foreach (string str2 in this.rootdse.Properties["namingContexts"])
            {
                bool flag = this.containerDN.EndsWith(str2, StringComparison.Ordinal);
                int num2 = this.containerDN.LastIndexOf(str2, StringComparison.Ordinal);
                if ((flag && (num2 != -1)) && (num2 < num))
                {
                    str = str2;
                    num = num2;
                }
            }
            if (str == null)
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_No_ADAM_Partition"));
            }
            return str;
        }

        internal string GetADsPath(string dn)
        {
            string str = null;
            str = "LDAP://" + this.serverName;
            if (this.portSpecified)
            {
                str = str + ":" + this.port;
            }
            System.Web.Security.NativeComInterfaces.IAdsPathname pathname = (System.Web.Security.NativeComInterfaces.IAdsPathname) new System.Web.Security.NativeComInterfaces.Pathname();
            pathname.Set(dn, 4);
            pathname.EscapedMode = 2;
            return (str + "/" + pathname.Retrieve(7));
        }

        internal System.DirectoryServices.AuthenticationTypes GetAuthenticationTypes(ActiveDirectoryConnectionProtection connectionProtection, CredentialsType type)
        {
            return this.authTypes[(int) connectionProtection, (int) type];
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Read="USERNAME")]
        internal static NetworkCredential GetCredentialsWithDomain(NetworkCredential credentials)
        {
            if (credentials == null)
            {
                return new NetworkCredential(null, "");
            }
            string userName = credentials.UserName;
            string str2 = null;
            string password = null;
            string domain = null;
            if (!string.IsNullOrEmpty(userName))
            {
                int index = userName.IndexOf('\\');
                if (index != -1)
                {
                    domain = userName.Substring(0, index);
                    str2 = userName.Substring(index + 1);
                }
                else
                {
                    str2 = userName;
                }
                password = credentials.Password;
            }
            return new NetworkCredential(str2, password, domain);
        }

        private System.Web.Security.DirectoryType GetDirectoryType()
        {
            System.Web.Security.DirectoryType unknown = System.Web.Security.DirectoryType.Unknown;
            foreach (string str in this.rootdse.Properties["supportedCapabilities"])
            {
                if (StringUtil.EqualsIgnoreCase(str, "1.2.840.113556.1.4.1851"))
                {
                    unknown = System.Web.Security.DirectoryType.ADAM;
                    break;
                }
                if (StringUtil.EqualsIgnoreCase(str, "1.2.840.113556.1.4.800"))
                {
                    unknown = System.Web.Security.DirectoryType.AD;
                    break;
                }
            }
            if (unknown == System.Web.Security.DirectoryType.Unknown)
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_Valid_Targets"));
            }
            return unknown;
        }

        private static string GetErrorMessage(int errorCode)
        {
            uint num = (uint) errorCode;
            num = ((num & 0xffff) | 0x70000) | 0x80000000;
            StringBuilder lpBuffer = new StringBuilder(0x100);
            int length = System.Web.Security.NativeMethods.FormatMessageW(0x3200, 0, (int) num, 0, lpBuffer, lpBuffer.Capacity + 1, 0);
            if (length != 0)
            {
                return lpBuffer.ToString(0, length);
            }
            return System.Web.SR.GetString("ADMembership_Unknown_Error", new object[] { string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { errorCode }) });
        }

        internal AuthType GetLdapAuthenticationTypes(ActiveDirectoryConnectionProtection connectionProtection, CredentialsType type)
        {
            return this.ldapAuthTypes[(int) connectionProtection, (int) type];
        }

        internal string GetNetbiosDomainNameIfAvailable(string dnsDomainName)
        {
            DirectoryEntry searchRoot = new DirectoryEntry(this.GetADsPath("CN=Partitions," + ((string) System.Web.Security.PropertyManager.GetPropertyValue(this.rootdse, "configurationNamingContext"))), this.GetUsername(), this.GetPassword());
            DirectorySearcher searcher = new DirectorySearcher(searchRoot) {
                SearchScope = System.DirectoryServices.SearchScope.OneLevel
            };
            StringBuilder builder = new StringBuilder(15);
            builder.Append("(&(objectCategory=crossRef)(dnsRoot=");
            builder.Append(dnsDomainName);
            builder.Append(")(systemFlags:1.2.840.113556.1.4.804:=1)(systemFlags:1.2.840.113556.1.4.804:=2))");
            searcher.Filter = builder.ToString();
            searcher.PropertiesToLoad.Add("nETBIOSName");
            SearchResult res = searcher.FindOne();
            if ((res == null) || !res.Properties.Contains("nETBIOSName"))
            {
                return dnsDomainName;
            }
            return (string) System.Web.Security.PropertyManager.GetSearchResultPropertyValue(res, "nETBIOSName");
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Read="USERNAME")]
        internal string GetPassword()
        {
            if (this.credentials != null)
            {
                if (this.credentials.Password == null)
                {
                    return null;
                }
                if ((this.credentials.Password.Length != 0) || ((this.credentials.UserName != null) && (this.credentials.UserName.Length != 0)))
                {
                    return this.credentials.Password;
                }
            }
            return null;
        }

        internal string GetPdcIfDomain(string name)
        {
            IntPtr zero = IntPtr.Zero;
            uint flags = 0x40000090;
            int num2 = 0x54b;
            int errorCode = System.Web.Security.NativeMethods.DsGetDcName(null, name, IntPtr.Zero, null, flags, out zero);
            try
            {
                if (errorCode == 0)
                {
                    System.Web.Security.DomainControllerInfo structure = new System.Web.Security.DomainControllerInfo();
                    Marshal.PtrToStructure(zero, structure);
                    return structure.DomainControllerName.Substring(2);
                }
                if (errorCode != num2)
                {
                    throw new ProviderException(GetErrorMessage(errorCode));
                }
                return name;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    System.Web.Security.NativeMethods.NetApiBufferFree(zero);
                }
            }
            return null;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Read="USERNAME")]
        internal string GetUsername()
        {
            if (this.credentials != null)
            {
                if (this.credentials.UserName == null)
                {
                    return null;
                }
                if ((this.credentials.UserName.Length != 0) || ((this.credentials.Password != null) && (this.credentials.Password.Length != 0)))
                {
                    return this.credentials.UserName;
                }
            }
            return null;
        }

        internal void InitializeDomainAndForestName()
        {
            if (!this.isServer)
            {
                DirectoryContext context = new DirectoryContext(DirectoryContextType.Domain, this.serverName, this.GetUsername(), this.GetPassword());
                try
                {
                    Domain domain = Domain.GetDomain(context);
                    this.domainName = this.GetNetbiosDomainNameIfAvailable(domain.Name);
                    this.forestName = domain.Forest.Name;
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    this.isServer = true;
                }
            }
            if (this.isServer)
            {
                DirectoryContext context2 = new DirectoryContext(DirectoryContextType.DirectoryServer, this.serverName, this.GetUsername(), this.GetPassword());
                try
                {
                    Domain domain2 = Domain.GetDomain(context2);
                    this.domainName = this.GetNetbiosDomainNameIfAvailable(domain2.Name);
                    this.forestName = domain2.Forest.Name;
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_unable_to_contact_domain"));
                }
            }
        }

        private bool IsConcurrentBindSupported(LdapConnection ldapConnection)
        {
            SearchRequest request = new SearchRequest {
                Scope = System.DirectoryServices.Protocols.SearchScope.Base
            };
            request.Attributes.Add("supportedExtension");
            if (this.ServerSearchTimeout != -1)
            {
                request.TimeLimit = new TimeSpan(0, this.ServerSearchTimeout, 0);
            }
            SearchResponse response = (SearchResponse) ldapConnection.SendRequest(request);
            if (response.ResultCode != ResultCode.Success)
            {
                throw new ProviderException(response.ErrorMessage);
            }
            foreach (string str in response.Entries[0].Attributes["supportedExtension"].GetValues(typeof(string)))
            {
                if (StringUtil.EqualsIgnoreCase(str, "1.2.840.113556.1.4.1781"))
                {
                    return true;
                }
            }
            return false;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Read="USERNAME")]
        internal bool IsDefaultCredential()
        {
            return (((this.credentials.UserName == null) || (this.credentials.UserName.Length == 0)) && ((this.credentials.Password == null) || (this.credentials.Password.Length == 0)));
        }

        internal void SelectServer()
        {
            this.serverName = this.GetPdcIfDomain(this.serverName);
            this.isServer = true;
        }

        internal void SetSessionOptionsForSecureConnection(LdapConnection connection, bool useConcurrentBind)
        {
            if (this.connectionProtection == ActiveDirectoryConnectionProtection.Ssl)
            {
                connection.SessionOptions.SecureSocketLayer = true;
            }
            else if (this.connectionProtection == ActiveDirectoryConnectionProtection.SignAndSeal)
            {
                connection.SessionOptions.Signing = true;
                connection.SessionOptions.Sealing = true;
            }
            if (useConcurrentBind && this.concurrentBindSupported)
            {
                try
                {
                    connection.SessionOptions.FastConcurrentBind();
                }
                catch (PlatformNotSupportedException)
                {
                    this.concurrentBindSupported = false;
                }
                catch (DirectoryOperationException)
                {
                    this.concurrentBindSupported = false;
                }
            }
        }

        internal string ADAMPartitionDN
        {
            get
            {
                return this.adamPartitionDN;
            }
        }

        internal TimeSpan ADLockoutDuration
        {
            get
            {
                return this.adLockoutDuration;
            }
        }

        internal System.DirectoryServices.AuthenticationTypes AuthenticationTypes
        {
            get
            {
                return this.authenticationType;
            }
        }

        internal int ClientSearchTimeout
        {
            get
            {
                return this.clientSearchTimeout;
            }
        }

        internal bool ConcurrentBindSupported
        {
            get
            {
                return this.concurrentBindSupported;
            }
        }

        internal ActiveDirectoryConnectionProtection ConnectionProtection
        {
            get
            {
                return this.connectionProtection;
            }
        }

        internal string ContainerDN
        {
            get
            {
                return this.containerDN;
            }
        }

        internal string CreationContainerDN
        {
            get
            {
                return this.creationContainerDN;
            }
        }

        internal System.Web.Security.DirectoryType DirectoryType
        {
            get
            {
                return this.directoryType;
            }
        }

        internal string DomainName
        {
            get
            {
                return this.domainName;
            }
        }

        internal string ForestName
        {
            get
            {
                return this.forestName;
            }
        }

        internal int Port
        {
            get
            {
                return this.port;
            }
        }

        internal bool PortSpecified
        {
            get
            {
                return this.portSpecified;
            }
        }

        internal int ServerSearchTimeout
        {
            get
            {
                return this.serverSearchTimeout;
            }
        }
    }
}

