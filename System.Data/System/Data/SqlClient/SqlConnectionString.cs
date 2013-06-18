namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class SqlConnectionString : DbConnectionOptions
    {
        private readonly System.Data.SqlClient.ApplicationIntent _applicationIntent;
        private readonly string _applicationName;
        private readonly bool _async;
        private readonly string _attachDBFileName;
        private readonly bool _connectionReset;
        private readonly int _connectTimeout;
        private readonly bool _contextConnection;
        private readonly string _currentLanguage;
        private readonly string _dataSource;
        private readonly bool _encrypt;
        private readonly bool _enlist;
        private readonly string _expandedAttachDBFilename;
        private readonly string _failoverPartner;
        private readonly string _initialCatalog;
        private readonly bool _integratedSecurity;
        private readonly int _loadBalanceTimeout;
        private readonly string _localDBInstance;
        private readonly bool _mars;
        private readonly int _maxPoolSize;
        private readonly int _minPoolSize;
        private readonly bool _multiSubnetFailover;
        private static Hashtable _netlibMapping;
        private readonly string _networkLibrary;
        private readonly int _packetSize;
        private readonly string _password;
        private readonly bool _persistSecurityInfo;
        private readonly bool _pooling;
        private readonly bool _replication;
        private static Hashtable _sqlClientSynonyms;
        private readonly TransactionBindingEnum _transactionBinding;
        private readonly bool _trustServerCertificate;
        private readonly TypeSystem _typeSystemVersion;
        private readonly string _userID;
        private readonly bool _userInstance;
        private readonly string _workstationId;
        internal const int SynonymCount = 0x15;

        internal SqlConnectionString(string connectionString) : base(connectionString, GetParseSynonyms(), false)
        {
            bool inProc = InOutOfProcHelper.InProc;
            this._integratedSecurity = base.ConvertValueToIntegratedSecurity();
            this._async = base.ConvertValueToBoolean("asynchronous processing", false);
            this._connectionReset = base.ConvertValueToBoolean("connection reset", true);
            this._contextConnection = base.ConvertValueToBoolean("context connection", false);
            this._encrypt = base.ConvertValueToBoolean("encrypt", false);
            this._enlist = base.ConvertValueToBoolean("enlist", ADP.IsWindowsNT);
            this._mars = base.ConvertValueToBoolean("multipleactiveresultsets", false);
            this._persistSecurityInfo = base.ConvertValueToBoolean("persist security info", false);
            this._pooling = base.ConvertValueToBoolean("pooling", true);
            this._replication = base.ConvertValueToBoolean("replication", false);
            this._userInstance = base.ConvertValueToBoolean("user instance", false);
            this._multiSubnetFailover = base.ConvertValueToBoolean("multisubnetfailover", false);
            this._connectTimeout = base.ConvertValueToInt32("connect timeout", 15);
            this._loadBalanceTimeout = base.ConvertValueToInt32("load balance timeout", 0);
            this._maxPoolSize = base.ConvertValueToInt32("max pool size", 100);
            this._minPoolSize = base.ConvertValueToInt32("min pool size", 0);
            this._packetSize = base.ConvertValueToInt32("packet size", 0x1f40);
            this._applicationIntent = this.ConvertValueToApplicationIntent();
            this._applicationName = base.ConvertValueToString("application name", ".Net SqlClient Data Provider");
            this._attachDBFileName = base.ConvertValueToString("attachdbfilename", "");
            this._currentLanguage = base.ConvertValueToString("current language", "");
            this._dataSource = base.ConvertValueToString("data source", "");
            this._localDBInstance = LocalDBAPI.GetLocalDbInstanceNameFromServerName(this._dataSource);
            this._failoverPartner = base.ConvertValueToString("failover partner", "");
            this._initialCatalog = base.ConvertValueToString("initial catalog", "");
            this._networkLibrary = base.ConvertValueToString("network library", null);
            this._password = base.ConvertValueToString("password", "");
            this._trustServerCertificate = base.ConvertValueToBoolean("trustservercertificate", false);
            string str = base.ConvertValueToString("type system version", null);
            string str2 = base.ConvertValueToString("transaction binding", null);
            this._userID = base.ConvertValueToString("user id", "");
            this._workstationId = base.ConvertValueToString("workstation id", null);
            if (this._contextConnection)
            {
                if (!inProc)
                {
                    throw SQL.ContextUnavailableOutOfProc();
                }
                foreach (DictionaryEntry entry in base.Parsetable)
                {
                    if ((((string) entry.Key) != "context connection") && (((string) entry.Key) != "type system version"))
                    {
                        throw SQL.ContextAllowsLimitedKeywords();
                    }
                }
            }
            if (!this._encrypt)
            {
                object obj2 = ADP.LocalMachineRegistryValue(@"Software\Microsoft\MSSQLServer\Client\SuperSocketNetLib", "Encrypt");
                if ((obj2 is int) && (1 == ((int) obj2)))
                {
                    this._encrypt = true;
                }
            }
            if (this._loadBalanceTimeout < 0)
            {
                throw ADP.InvalidConnectionOptionValue("load balance timeout");
            }
            if (this._connectTimeout < 0)
            {
                throw ADP.InvalidConnectionOptionValue("connect timeout");
            }
            if (this._maxPoolSize < 1)
            {
                throw ADP.InvalidConnectionOptionValue("max pool size");
            }
            if (this._minPoolSize < 0)
            {
                throw ADP.InvalidConnectionOptionValue("min pool size");
            }
            if (this._maxPoolSize < this._minPoolSize)
            {
                throw ADP.InvalidMinMaxPoolSizeValues();
            }
            if ((this._packetSize < 0x200) || (0x8000 < this._packetSize))
            {
                throw SQL.InvalidPacketSizeValue();
            }
            if (this._networkLibrary != null)
            {
                string key = this._networkLibrary.Trim().ToLower(CultureInfo.InvariantCulture);
                Hashtable hashtable = NetlibMapping();
                if (!hashtable.ContainsKey(key))
                {
                    throw ADP.InvalidConnectionOptionValue("network library");
                }
                this._networkLibrary = (string) hashtable[key];
            }
            else
            {
                this._networkLibrary = "";
            }
            this.ValidateValueLength(this._applicationName, 0x80, "application name");
            this.ValidateValueLength(this._currentLanguage, 0x80, "current language");
            this.ValidateValueLength(this._dataSource, 0x80, "data source");
            this.ValidateValueLength(this._failoverPartner, 0x80, "failover partner");
            this.ValidateValueLength(this._initialCatalog, 0x80, "initial catalog");
            this.ValidateValueLength(this._password, 0x80, "password");
            this.ValidateValueLength(this._userID, 0x80, "user id");
            if (this._workstationId != null)
            {
                this.ValidateValueLength(this._workstationId, 0x80, "workstation id");
            }
            if (!string.Equals("", this._failoverPartner, StringComparison.OrdinalIgnoreCase))
            {
                if (this._multiSubnetFailover)
                {
                    bool serverProvidedFailoverPartner = false;
                    throw SQL.MultiSubnetFailoverWithFailoverPartner(serverProvidedFailoverPartner);
                }
                if (string.Equals("", this._initialCatalog, StringComparison.OrdinalIgnoreCase))
                {
                    throw ADP.MissingConnectionOptionValue("failover partner", "initial catalog");
                }
            }
            string datadir = null;
            this._expandedAttachDBFilename = DbConnectionOptions.ExpandDataDirectory("attachdbfilename", this._attachDBFileName, ref datadir);
            if (this._expandedAttachDBFilename != null)
            {
                if (0 <= this._expandedAttachDBFilename.IndexOf('|'))
                {
                    throw ADP.InvalidConnectionOptionValue("attachdbfilename");
                }
                this.ValidateValueLength(this._expandedAttachDBFilename, 260, "attachdbfilename");
                if (this._localDBInstance == null)
                {
                    string host = this._dataSource;
                    string protocol = this._networkLibrary;
                    TdsParserStaticMethods.AliasRegistryLookup(ref host, ref protocol);
                    VerifyLocalHostAndFixup(ref host, true, false);
                }
            }
            else
            {
                if (0 <= this._attachDBFileName.IndexOf('|'))
                {
                    throw ADP.InvalidConnectionOptionValue("attachdbfilename");
                }
                this.ValidateValueLength(this._attachDBFileName, 260, "attachdbfilename");
            }
            if (this._async && inProc)
            {
                throw SQL.AsyncInProcNotSupported();
            }
            if (this._userInstance && !ADP.IsEmpty(this._failoverPartner))
            {
                throw SQL.UserInstanceFailoverNotCompatible();
            }
            if (ADP.IsEmpty(str))
            {
                str = "Latest";
            }
            if (str.Equals("Latest", StringComparison.OrdinalIgnoreCase))
            {
                this._typeSystemVersion = TypeSystem.Latest;
            }
            else if (str.Equals("SQL Server 2000", StringComparison.OrdinalIgnoreCase))
            {
                if (this._contextConnection)
                {
                    throw SQL.ContextAllowsOnlyTypeSystem2005();
                }
                this._typeSystemVersion = TypeSystem.SQLServer2000;
            }
            else if (str.Equals("SQL Server 2005", StringComparison.OrdinalIgnoreCase))
            {
                this._typeSystemVersion = TypeSystem.SQLServer2005;
            }
            else
            {
                if (!str.Equals("SQL Server 2008", StringComparison.OrdinalIgnoreCase))
                {
                    throw ADP.InvalidConnectionOptionValue("type system version");
                }
                this._typeSystemVersion = TypeSystem.Latest;
            }
            if (ADP.IsEmpty(str2))
            {
                str2 = "Implicit Unbind";
            }
            if (str2.Equals("Implicit Unbind", StringComparison.OrdinalIgnoreCase))
            {
                this._transactionBinding = TransactionBindingEnum.ImplicitUnbind;
            }
            else
            {
                if (!str2.Equals("Explicit Unbind", StringComparison.OrdinalIgnoreCase))
                {
                    throw ADP.InvalidConnectionOptionValue("transaction binding");
                }
                this._transactionBinding = TransactionBindingEnum.ExplicitUnbind;
            }
            if ((this._applicationIntent == System.Data.SqlClient.ApplicationIntent.ReadOnly) && !string.IsNullOrEmpty(this._failoverPartner))
            {
                throw SQL.ROR_FailoverNotSupportedConnString();
            }
        }

        internal SqlConnectionString(SqlConnectionString connectionOptions, string dataSource, bool userInstance, bool? setEnlistValue) : base(connectionOptions)
        {
            this._integratedSecurity = connectionOptions._integratedSecurity;
            this._async = connectionOptions._async;
            this._connectionReset = connectionOptions._connectionReset;
            this._contextConnection = connectionOptions._contextConnection;
            this._encrypt = connectionOptions._encrypt;
            if (setEnlistValue.HasValue)
            {
                this._enlist = setEnlistValue.Value;
            }
            else
            {
                this._enlist = connectionOptions._enlist;
            }
            this._mars = connectionOptions._mars;
            this._persistSecurityInfo = connectionOptions._persistSecurityInfo;
            this._pooling = connectionOptions._pooling;
            this._replication = connectionOptions._replication;
            this._userInstance = userInstance;
            this._connectTimeout = connectionOptions._connectTimeout;
            this._loadBalanceTimeout = connectionOptions._loadBalanceTimeout;
            this._maxPoolSize = connectionOptions._maxPoolSize;
            this._minPoolSize = connectionOptions._minPoolSize;
            this._multiSubnetFailover = connectionOptions._multiSubnetFailover;
            this._packetSize = connectionOptions._packetSize;
            this._applicationName = connectionOptions._applicationName;
            this._attachDBFileName = connectionOptions._attachDBFileName;
            this._currentLanguage = connectionOptions._currentLanguage;
            this._dataSource = dataSource;
            this._localDBInstance = LocalDBAPI.GetLocalDbInstanceNameFromServerName(this._dataSource);
            this._failoverPartner = connectionOptions._failoverPartner;
            this._initialCatalog = connectionOptions._initialCatalog;
            this._password = connectionOptions._password;
            this._userID = connectionOptions._userID;
            this._networkLibrary = connectionOptions._networkLibrary;
            this._workstationId = connectionOptions._workstationId;
            this._expandedAttachDBFilename = connectionOptions._expandedAttachDBFilename;
            this._typeSystemVersion = connectionOptions._typeSystemVersion;
            this._transactionBinding = connectionOptions._transactionBinding;
            this._applicationIntent = connectionOptions._applicationIntent;
            this.ValidateValueLength(this._dataSource, 0x80, "data source");
        }

        private static bool CompareHostName(ref string host, string name, bool fixup)
        {
            bool flag = false;
            if (host.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                if (fixup)
                {
                    host = ".";
                }
                return true;
            }
            if (!host.StartsWith(name + @"\", StringComparison.OrdinalIgnoreCase))
            {
                return flag;
            }
            if (fixup)
            {
                host = "." + host.Substring(name.Length);
            }
            return true;
        }

        internal System.Data.SqlClient.ApplicationIntent ConvertValueToApplicationIntent()
        {
            System.Data.SqlClient.ApplicationIntent intent;
            object obj2 = base.Parsetable["applicationintent"];
            if (obj2 == null)
            {
                return System.Data.SqlClient.ApplicationIntent.ReadWrite;
            }
            try
            {
                intent = DbConnectionStringBuilderUtil.ConvertToApplicationIntent("applicationintent", obj2);
            }
            catch (FormatException exception2)
            {
                throw ADP.InvalidConnectionOptionValue("applicationintent", exception2);
            }
            catch (OverflowException exception)
            {
                throw ADP.InvalidConnectionOptionValue("applicationintent", exception);
            }
            return intent;
        }

        protected internal override PermissionSet CreatePermissionSet()
        {
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new SqlClientPermission(this));
            return set;
        }

        protected internal override string Expand()
        {
            if (this._expandedAttachDBFilename != null)
            {
                return base.ExpandKeyword("attachdbfilename", this._expandedAttachDBFilename);
            }
            return base.Expand();
        }

        internal static Hashtable GetParseSynonyms()
        {
            Hashtable hashtable = _sqlClientSynonyms;
            if (hashtable == null)
            {
                hashtable = new Hashtable(0x34);
                hashtable.Add("applicationintent", "applicationintent");
                hashtable.Add("application name", "application name");
                hashtable.Add("asynchronous processing", "asynchronous processing");
                hashtable.Add("attachdbfilename", "attachdbfilename");
                hashtable.Add("connect timeout", "connect timeout");
                hashtable.Add("connection reset", "connection reset");
                hashtable.Add("context connection", "context connection");
                hashtable.Add("current language", "current language");
                hashtable.Add("data source", "data source");
                hashtable.Add("encrypt", "encrypt");
                hashtable.Add("enlist", "enlist");
                hashtable.Add("failover partner", "failover partner");
                hashtable.Add("initial catalog", "initial catalog");
                hashtable.Add("integrated security", "integrated security");
                hashtable.Add("load balance timeout", "load balance timeout");
                hashtable.Add("multipleactiveresultsets", "multipleactiveresultsets");
                hashtable.Add("max pool size", "max pool size");
                hashtable.Add("min pool size", "min pool size");
                hashtable.Add("multisubnetfailover", "multisubnetfailover");
                hashtable.Add("network library", "network library");
                hashtable.Add("packet size", "packet size");
                hashtable.Add("password", "password");
                hashtable.Add("persist security info", "persist security info");
                hashtable.Add("pooling", "pooling");
                hashtable.Add("replication", "replication");
                hashtable.Add("trustservercertificate", "trustservercertificate");
                hashtable.Add("transaction binding", "transaction binding");
                hashtable.Add("type system version", "type system version");
                hashtable.Add("user id", "user id");
                hashtable.Add("user instance", "user instance");
                hashtable.Add("workstation id", "workstation id");
                hashtable.Add("app", "application name");
                hashtable.Add("async", "asynchronous processing");
                hashtable.Add("extended properties", "attachdbfilename");
                hashtable.Add("initial file name", "attachdbfilename");
                hashtable.Add("connection timeout", "connect timeout");
                hashtable.Add("timeout", "connect timeout");
                hashtable.Add("language", "current language");
                hashtable.Add("addr", "data source");
                hashtable.Add("address", "data source");
                hashtable.Add("network address", "data source");
                hashtable.Add("server", "data source");
                hashtable.Add("database", "initial catalog");
                hashtable.Add("trusted_connection", "integrated security");
                hashtable.Add("connection lifetime", "load balance timeout");
                hashtable.Add("net", "network library");
                hashtable.Add("network", "network library");
                hashtable.Add("pwd", "password");
                hashtable.Add("persistsecurityinfo", "persist security info");
                hashtable.Add("uid", "user id");
                hashtable.Add("user", "user id");
                hashtable.Add("wsid", "workstation id");
                _sqlClientSynonyms = hashtable;
            }
            return hashtable;
        }

        internal static Hashtable NetlibMapping()
        {
            Hashtable hashtable = _netlibMapping;
            if (hashtable == null)
            {
                hashtable = new Hashtable(8);
                hashtable.Add("dbmssocn", "tcp");
                hashtable.Add("dbnmpntw", "np");
                hashtable.Add("dbmsrpcn", "rpc");
                hashtable.Add("dbmsvinn", "bv");
                hashtable.Add("dbmsadsn", "adsp");
                hashtable.Add("dbmsspxn", "spx");
                hashtable.Add("dbmsgnet", "via");
                hashtable.Add("dbmslpcn", "lpc");
                _netlibMapping = hashtable;
            }
            return hashtable;
        }

        internal string ObtainWorkstationId()
        {
            string workstationId = this.WorkstationId;
            if (workstationId == null)
            {
                workstationId = ADP.MachineName();
                this.ValidateValueLength(workstationId, 0x80, "workstation id");
            }
            return workstationId;
        }

        private void ValidateValueLength(string value, int limit, string key)
        {
            if (limit < value.Length)
            {
                throw ADP.InvalidConnectionOptionValueLength(key, limit);
            }
        }

        internal static bool ValidProtocal(string protocal)
        {
            string str;
            if (((str = protocal) == null) || ((!(str == "tcp") && !(str == "np")) && (!(str == "via") && !(str == "lpc"))))
            {
                return false;
            }
            return true;
        }

        internal static void VerifyLocalHostAndFixup(ref string host, bool enforceLocalHost, bool fixup)
        {
            if (ADP.IsEmpty(host))
            {
                if (fixup)
                {
                    host = ".";
                }
            }
            else if (!CompareHostName(ref host, ".", fixup) && !CompareHostName(ref host, "(local)", fixup))
            {
                string computerNameDnsFullyQualified = ADP.GetComputerNameDnsFullyQualified();
                if (!CompareHostName(ref host, computerNameDnsFullyQualified, fixup))
                {
                    int index = computerNameDnsFullyQualified.IndexOf('.');
                    if (((index <= 0) || !CompareHostName(ref host, computerNameDnsFullyQualified.Substring(0, index), fixup)) && enforceLocalHost)
                    {
                        throw ADP.InvalidConnectionOptionValue("attachdbfilename");
                    }
                }
            }
        }

        internal System.Data.SqlClient.ApplicationIntent ApplicationIntent
        {
            get
            {
                return this._applicationIntent;
            }
        }

        internal string ApplicationName
        {
            get
            {
                return this._applicationName;
            }
        }

        internal bool Asynchronous
        {
            get
            {
                return this._async;
            }
        }

        internal string AttachDBFilename
        {
            get
            {
                return this._attachDBFileName;
            }
        }

        internal bool ConnectionReset
        {
            get
            {
                return true;
            }
        }

        internal int ConnectTimeout
        {
            get
            {
                return this._connectTimeout;
            }
        }

        internal bool ContextConnection
        {
            get
            {
                return this._contextConnection;
            }
        }

        internal string CurrentLanguage
        {
            get
            {
                return this._currentLanguage;
            }
        }

        internal string DataSource
        {
            get
            {
                return this._dataSource;
            }
        }

        internal bool Encrypt
        {
            get
            {
                return this._encrypt;
            }
        }

        internal bool EnforceLocalHost
        {
            get
            {
                return ((this._expandedAttachDBFilename != null) && (null == this._localDBInstance));
            }
        }

        internal bool Enlist
        {
            get
            {
                return this._enlist;
            }
        }

        internal string FailoverPartner
        {
            get
            {
                return this._failoverPartner;
            }
        }

        internal string InitialCatalog
        {
            get
            {
                return this._initialCatalog;
            }
        }

        internal bool IntegratedSecurity
        {
            get
            {
                return this._integratedSecurity;
            }
        }

        internal int LoadBalanceTimeout
        {
            get
            {
                return this._loadBalanceTimeout;
            }
        }

        internal string LocalDBInstance
        {
            get
            {
                return this._localDBInstance;
            }
        }

        internal bool MARS
        {
            get
            {
                return this._mars;
            }
        }

        internal int MaxPoolSize
        {
            get
            {
                return this._maxPoolSize;
            }
        }

        internal int MinPoolSize
        {
            get
            {
                return this._minPoolSize;
            }
        }

        internal bool MultiSubnetFailover
        {
            get
            {
                return this._multiSubnetFailover;
            }
        }

        internal string NetworkLibrary
        {
            get
            {
                return this._networkLibrary;
            }
        }

        internal int PacketSize
        {
            get
            {
                return this._packetSize;
            }
        }

        internal string Password
        {
            get
            {
                return this._password;
            }
        }

        internal bool Pooling
        {
            get
            {
                return this._pooling;
            }
        }

        internal bool Replication
        {
            get
            {
                return this._replication;
            }
        }

        internal TransactionBindingEnum TransactionBinding
        {
            get
            {
                return this._transactionBinding;
            }
        }

        internal bool TrustServerCertificate
        {
            get
            {
                return this._trustServerCertificate;
            }
        }

        internal TypeSystem TypeSystemVersion
        {
            get
            {
                return this._typeSystemVersion;
            }
        }

        internal string UserID
        {
            get
            {
                return this._userID;
            }
        }

        internal bool UserInstance
        {
            get
            {
                return this._userInstance;
            }
        }

        internal string WorkstationId
        {
            get
            {
                return this._workstationId;
            }
        }

        internal enum TransactionBindingEnum
        {
            ImplicitUnbind,
            ExplicitUnbind
        }

        internal enum TypeSystem
        {
            Latest = 0x7d8,
            SQLServer2000 = 0x7d0,
            SQLServer2005 = 0x7d5,
            SQLServer2008 = 0x7d8
        }
    }
}

