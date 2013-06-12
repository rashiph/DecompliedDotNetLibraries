namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [TypeConverter(typeof(SqlConnectionStringBuilder.SqlConnectionStringBuilderConverter)), DefaultProperty("DataSource")]
    public sealed class SqlConnectionStringBuilder : DbConnectionStringBuilder
    {
        private System.Data.SqlClient.ApplicationIntent _applicationIntent;
        private string _applicationName;
        private bool _asynchronousProcessing;
        private string _attachDBFilename;
        private bool _connectionReset;
        private int _connectTimeout;
        private bool _contextConnection;
        private string _currentLanguage;
        private string _dataSource;
        private bool _encrypt;
        private bool _enlist;
        private string _failoverPartner;
        private string _initialCatalog;
        private bool _integratedSecurity;
        private static readonly Dictionary<string, Keywords> _keywords;
        private int _loadBalanceTimeout;
        private int _maxPoolSize;
        private int _minPoolSize;
        private bool _multipleActiveResultSets;
        private bool _multiSubnetFailover;
        private string _networkLibrary;
        private int _packetSize;
        private string _password;
        private bool _persistSecurityInfo;
        private bool _pooling;
        private bool _replication;
        private string _transactionBinding;
        private bool _trustServerCertificate;
        private string _typeSystemVersion;
        private string _userID;
        private bool _userInstance;
        private static readonly string[] _validKeywords;
        private string _workstationID;
        internal const int KeywordsCount = 0x1f;

        static SqlConnectionStringBuilder()
        {
            string[] strArray = new string[0x1f];
            strArray[30] = "ApplicationIntent";
            strArray[0x17] = "Application Name";
            strArray[12] = "Asynchronous Processing";
            strArray[2] = "AttachDbFilename";
            strArray[13] = "Connection Reset";
            strArray[0x1b] = "Context Connection";
            strArray[0x10] = "Connect Timeout";
            strArray[0x18] = "Current Language";
            strArray[0] = "Data Source";
            strArray[0x11] = "Encrypt";
            strArray[8] = "Enlist";
            strArray[1] = "Failover Partner";
            strArray[3] = "Initial Catalog";
            strArray[4] = "Integrated Security";
            strArray[0x13] = "Load Balance Timeout";
            strArray[11] = "Max Pool Size";
            strArray[10] = "Min Pool Size";
            strArray[14] = "MultipleActiveResultSets";
            strArray[0x1d] = "MultiSubnetFailover";
            strArray[20] = "Network Library";
            strArray[0x15] = "Packet Size";
            strArray[7] = "Password";
            strArray[5] = "Persist Security Info";
            strArray[9] = "Pooling";
            strArray[15] = "Replication";
            strArray[0x1c] = "Transaction Binding";
            strArray[0x12] = "TrustServerCertificate";
            strArray[0x16] = "Type System Version";
            strArray[6] = "User ID";
            strArray[0x1a] = "User Instance";
            strArray[0x19] = "Workstation ID";
            _validKeywords = strArray;
            Dictionary<string, Keywords> dictionary = new Dictionary<string, Keywords>(0x34, StringComparer.OrdinalIgnoreCase);
            dictionary.Add("ApplicationIntent", Keywords.ApplicationIntent);
            dictionary.Add("Application Name", Keywords.ApplicationName);
            dictionary.Add("Asynchronous Processing", Keywords.AsynchronousProcessing);
            dictionary.Add("AttachDbFilename", Keywords.AttachDBFilename);
            dictionary.Add("Connect Timeout", Keywords.ConnectTimeout);
            dictionary.Add("Connection Reset", Keywords.ConnectionReset);
            dictionary.Add("Context Connection", Keywords.ContextConnection);
            dictionary.Add("Current Language", Keywords.CurrentLanguage);
            dictionary.Add("Data Source", Keywords.DataSource);
            dictionary.Add("Encrypt", Keywords.Encrypt);
            dictionary.Add("Enlist", Keywords.Enlist);
            dictionary.Add("Failover Partner", Keywords.FailoverPartner);
            dictionary.Add("Initial Catalog", Keywords.InitialCatalog);
            dictionary.Add("Integrated Security", Keywords.IntegratedSecurity);
            dictionary.Add("Load Balance Timeout", Keywords.LoadBalanceTimeout);
            dictionary.Add("MultipleActiveResultSets", Keywords.MultipleActiveResultSets);
            dictionary.Add("Max Pool Size", Keywords.MaxPoolSize);
            dictionary.Add("Min Pool Size", Keywords.MinPoolSize);
            dictionary.Add("MultiSubnetFailover", Keywords.MultiSubnetFailover);
            dictionary.Add("Network Library", Keywords.NetworkLibrary);
            dictionary.Add("Packet Size", Keywords.PacketSize);
            dictionary.Add("Password", Keywords.Password);
            dictionary.Add("Persist Security Info", Keywords.PersistSecurityInfo);
            dictionary.Add("Pooling", Keywords.Pooling);
            dictionary.Add("Replication", Keywords.Replication);
            dictionary.Add("Transaction Binding", Keywords.TransactionBinding);
            dictionary.Add("TrustServerCertificate", Keywords.TrustServerCertificate);
            dictionary.Add("Type System Version", Keywords.TypeSystemVersion);
            dictionary.Add("User ID", Keywords.UserID);
            dictionary.Add("User Instance", Keywords.UserInstance);
            dictionary.Add("Workstation ID", Keywords.WorkstationID);
            dictionary.Add("app", Keywords.ApplicationName);
            dictionary.Add("async", Keywords.AsynchronousProcessing);
            dictionary.Add("extended properties", Keywords.AttachDBFilename);
            dictionary.Add("initial file name", Keywords.AttachDBFilename);
            dictionary.Add("connection timeout", Keywords.ConnectTimeout);
            dictionary.Add("timeout", Keywords.ConnectTimeout);
            dictionary.Add("language", Keywords.CurrentLanguage);
            dictionary.Add("addr", Keywords.DataSource);
            dictionary.Add("address", Keywords.DataSource);
            dictionary.Add("network address", Keywords.DataSource);
            dictionary.Add("server", Keywords.DataSource);
            dictionary.Add("database", Keywords.InitialCatalog);
            dictionary.Add("trusted_connection", Keywords.IntegratedSecurity);
            dictionary.Add("connection lifetime", Keywords.LoadBalanceTimeout);
            dictionary.Add("net", Keywords.NetworkLibrary);
            dictionary.Add("network", Keywords.NetworkLibrary);
            dictionary.Add("pwd", Keywords.Password);
            dictionary.Add("persistsecurityinfo", Keywords.PersistSecurityInfo);
            dictionary.Add("uid", Keywords.UserID);
            dictionary.Add("user", Keywords.UserID);
            dictionary.Add("wsid", Keywords.WorkstationID);
            _keywords = dictionary;
        }

        public SqlConnectionStringBuilder() : this(null)
        {
        }

        public SqlConnectionStringBuilder(string connectionString)
        {
            this._applicationName = ".Net SqlClient Data Provider";
            this._attachDBFilename = "";
            this._currentLanguage = "";
            this._dataSource = "";
            this._failoverPartner = "";
            this._initialCatalog = "";
            this._networkLibrary = "";
            this._password = "";
            this._transactionBinding = "Implicit Unbind";
            this._typeSystemVersion = "Latest";
            this._userID = "";
            this._workstationID = "";
            this._connectTimeout = 15;
            this._maxPoolSize = 100;
            this._packetSize = 0x1f40;
            this._connectionReset = true;
            this._enlist = true;
            this._pooling = true;
            if (!ADP.IsEmpty(connectionString))
            {
                base.ConnectionString = connectionString;
            }
        }

        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < _validKeywords.Length; i++)
            {
                this.Reset((Keywords) i);
            }
        }

        public override bool ContainsKey(string keyword)
        {
            ADP.CheckArgumentNull(keyword, "keyword");
            return _keywords.ContainsKey(keyword);
        }

        private static System.Data.SqlClient.ApplicationIntent ConvertToApplicationIntent(string keyword, object value)
        {
            return DbConnectionStringBuilderUtil.ConvertToApplicationIntent(keyword, value);
        }

        private static bool ConvertToBoolean(object value)
        {
            return DbConnectionStringBuilderUtil.ConvertToBoolean(value);
        }

        private static int ConvertToInt32(object value)
        {
            return DbConnectionStringBuilderUtil.ConvertToInt32(value);
        }

        private static bool ConvertToIntegratedSecurity(object value)
        {
            return DbConnectionStringBuilderUtil.ConvertToIntegratedSecurity(value);
        }

        private static string ConvertToString(object value)
        {
            return DbConnectionStringBuilderUtil.ConvertToString(value);
        }

        private object GetAt(Keywords index)
        {
            switch (index)
            {
                case Keywords.DataSource:
                    return this.DataSource;

                case Keywords.FailoverPartner:
                    return this.FailoverPartner;

                case Keywords.AttachDBFilename:
                    return this.AttachDBFilename;

                case Keywords.InitialCatalog:
                    return this.InitialCatalog;

                case Keywords.IntegratedSecurity:
                    return this.IntegratedSecurity;

                case Keywords.PersistSecurityInfo:
                    return this.PersistSecurityInfo;

                case Keywords.UserID:
                    return this.UserID;

                case Keywords.Password:
                    return this.Password;

                case Keywords.Enlist:
                    return this.Enlist;

                case Keywords.Pooling:
                    return this.Pooling;

                case Keywords.MinPoolSize:
                    return this.MinPoolSize;

                case Keywords.MaxPoolSize:
                    return this.MaxPoolSize;

                case Keywords.AsynchronousProcessing:
                    return this.AsynchronousProcessing;

                case Keywords.ConnectionReset:
                    return this.ConnectionReset;

                case Keywords.MultipleActiveResultSets:
                    return this.MultipleActiveResultSets;

                case Keywords.Replication:
                    return this.Replication;

                case Keywords.ConnectTimeout:
                    return this.ConnectTimeout;

                case Keywords.Encrypt:
                    return this.Encrypt;

                case Keywords.TrustServerCertificate:
                    return this.TrustServerCertificate;

                case Keywords.LoadBalanceTimeout:
                    return this.LoadBalanceTimeout;

                case Keywords.NetworkLibrary:
                    return this.NetworkLibrary;

                case Keywords.PacketSize:
                    return this.PacketSize;

                case Keywords.TypeSystemVersion:
                    return this.TypeSystemVersion;

                case Keywords.ApplicationName:
                    return this.ApplicationName;

                case Keywords.CurrentLanguage:
                    return this.CurrentLanguage;

                case Keywords.WorkstationID:
                    return this.WorkstationID;

                case Keywords.UserInstance:
                    return this.UserInstance;

                case Keywords.ContextConnection:
                    return this.ContextConnection;

                case Keywords.TransactionBinding:
                    return this.TransactionBinding;

                case Keywords.MultiSubnetFailover:
                    return this.MultiSubnetFailover;

                case Keywords.ApplicationIntent:
                    return this.ApplicationIntent;
            }
            throw ADP.KeywordNotSupported(_validKeywords[(int) index]);
        }

        private Keywords GetIndex(string keyword)
        {
            Keywords keywords;
            ADP.CheckArgumentNull(keyword, "keyword");
            if (!_keywords.TryGetValue(keyword, out keywords))
            {
                throw ADP.KeywordNotSupported(keyword);
            }
            return keywords;
        }

        protected override void GetProperties(Hashtable propertyDescriptors)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this, true))
            {
                bool flag2 = false;
                bool isReadOnly = false;
                string displayName = descriptor.DisplayName;
                if ("Integrated Security" == displayName)
                {
                    flag2 = true;
                    isReadOnly = descriptor.IsReadOnly;
                }
                else
                {
                    if (!("Password" == displayName) && !("User ID" == displayName))
                    {
                        continue;
                    }
                    isReadOnly = this.IntegratedSecurity;
                }
                Attribute[] attributesFromCollection = base.GetAttributesFromCollection(descriptor.Attributes);
                DbConnectionStringBuilderDescriptor descriptor2 = new DbConnectionStringBuilderDescriptor(descriptor.Name, descriptor.ComponentType, descriptor.PropertyType, isReadOnly, attributesFromCollection) {
                    RefreshOnChange = flag2
                };
                propertyDescriptors[displayName] = descriptor2;
            }
            base.GetProperties(propertyDescriptors);
        }

        public override bool Remove(string keyword)
        {
            Keywords keywords;
            ADP.CheckArgumentNull(keyword, "keyword");
            if (_keywords.TryGetValue(keyword, out keywords) && base.Remove(_validKeywords[(int) keywords]))
            {
                this.Reset(keywords);
                return true;
            }
            return false;
        }

        private void Reset(Keywords index)
        {
            switch (index)
            {
                case Keywords.DataSource:
                    this._dataSource = "";
                    return;

                case Keywords.FailoverPartner:
                    this._failoverPartner = "";
                    return;

                case Keywords.AttachDBFilename:
                    this._attachDBFilename = "";
                    return;

                case Keywords.InitialCatalog:
                    this._initialCatalog = "";
                    return;

                case Keywords.IntegratedSecurity:
                    this._integratedSecurity = false;
                    return;

                case Keywords.PersistSecurityInfo:
                    this._persistSecurityInfo = false;
                    return;

                case Keywords.UserID:
                    this._userID = "";
                    return;

                case Keywords.Password:
                    this._password = "";
                    return;

                case Keywords.Enlist:
                    this._enlist = true;
                    return;

                case Keywords.Pooling:
                    this._pooling = true;
                    return;

                case Keywords.MinPoolSize:
                    this._minPoolSize = 0;
                    return;

                case Keywords.MaxPoolSize:
                    this._maxPoolSize = 100;
                    return;

                case Keywords.AsynchronousProcessing:
                    this._asynchronousProcessing = false;
                    return;

                case Keywords.ConnectionReset:
                    this._connectionReset = true;
                    return;

                case Keywords.MultipleActiveResultSets:
                    this._multipleActiveResultSets = false;
                    return;

                case Keywords.Replication:
                    this._replication = false;
                    return;

                case Keywords.ConnectTimeout:
                    this._connectTimeout = 15;
                    return;

                case Keywords.Encrypt:
                    this._encrypt = false;
                    return;

                case Keywords.TrustServerCertificate:
                    this._trustServerCertificate = false;
                    return;

                case Keywords.LoadBalanceTimeout:
                    this._loadBalanceTimeout = 0;
                    return;

                case Keywords.NetworkLibrary:
                    this._networkLibrary = "";
                    return;

                case Keywords.PacketSize:
                    this._packetSize = 0x1f40;
                    return;

                case Keywords.TypeSystemVersion:
                    this._typeSystemVersion = "Latest";
                    return;

                case Keywords.ApplicationName:
                    this._applicationName = ".Net SqlClient Data Provider";
                    return;

                case Keywords.CurrentLanguage:
                    this._currentLanguage = "";
                    return;

                case Keywords.WorkstationID:
                    this._workstationID = "";
                    return;

                case Keywords.UserInstance:
                    this._userInstance = false;
                    return;

                case Keywords.ContextConnection:
                    this._contextConnection = false;
                    return;

                case Keywords.TransactionBinding:
                    this._transactionBinding = "Implicit Unbind";
                    return;

                case Keywords.MultiSubnetFailover:
                    this._multiSubnetFailover = false;
                    return;

                case Keywords.ApplicationIntent:
                    this._applicationIntent = System.Data.SqlClient.ApplicationIntent.ReadWrite;
                    return;
            }
            throw ADP.KeywordNotSupported(_validKeywords[(int) index]);
        }

        private void SetApplicationIntentValue(System.Data.SqlClient.ApplicationIntent value)
        {
            base["ApplicationIntent"] = DbConnectionStringBuilderUtil.ApplicationIntentToString(value);
        }

        private void SetValue(string keyword, bool value)
        {
            base[keyword] = value.ToString(null);
        }

        private void SetValue(string keyword, int value)
        {
            base[keyword] = value.ToString((IFormatProvider) null);
        }

        private void SetValue(string keyword, string value)
        {
            ADP.CheckArgumentNull(value, keyword);
            base[keyword] = value;
        }

        public override bool ShouldSerialize(string keyword)
        {
            Keywords keywords;
            ADP.CheckArgumentNull(keyword, "keyword");
            return (_keywords.TryGetValue(keyword, out keywords) && base.ShouldSerialize(_validKeywords[(int) keywords]));
        }

        public override bool TryGetValue(string keyword, out object value)
        {
            Keywords keywords;
            if (_keywords.TryGetValue(keyword, out keywords))
            {
                value = this.GetAt(keywords);
                return true;
            }
            value = null;
            return false;
        }

        [RefreshProperties(RefreshProperties.All), DisplayName("ApplicationIntent"), ResCategory("DataCategory_Initialization"), ResDescription("DbConnectionString_ApplicationIntent")]
        public System.Data.SqlClient.ApplicationIntent ApplicationIntent
        {
            get
            {
                return this._applicationIntent;
            }
            set
            {
                if (!DbConnectionStringBuilderUtil.IsValidApplicationIntentValue(value))
                {
                    throw ADP.InvalidEnumerationValue(typeof(System.Data.SqlClient.ApplicationIntent), (int) value);
                }
                this.SetApplicationIntentValue(value);
                this._applicationIntent = value;
            }
        }

        [ResCategory("DataCategory_Context"), DisplayName("Application Name"), ResDescription("DbConnectionString_ApplicationName"), RefreshProperties(RefreshProperties.All)]
        public string ApplicationName
        {
            get
            {
                return this._applicationName;
            }
            set
            {
                this.SetValue("Application Name", value);
                this._applicationName = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), ResDescription("DbConnectionString_AsynchronousProcessing"), ResCategory("DataCategory_Initialization"), DisplayName("Asynchronous Processing")]
        public bool AsynchronousProcessing
        {
            get
            {
                return this._asynchronousProcessing;
            }
            set
            {
                this.SetValue("Asynchronous Processing", value);
                this._asynchronousProcessing = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DisplayName("AttachDbFilename"), ResCategory("DataCategory_Source"), ResDescription("DbConnectionString_AttachDBFilename"), Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string AttachDBFilename
        {
            get
            {
                return this._attachDBFilename;
            }
            set
            {
                this.SetValue("AttachDbFilename", value);
                this._attachDBFilename = value;
            }
        }

        [Browsable(false), Obsolete("ConnectionReset has been deprecated.  SqlConnection will ignore the 'connection reset' keyword and always reset the connection"), DisplayName("Connection Reset"), ResCategory("DataCategory_Pooling"), ResDescription("DbConnectionString_ConnectionReset"), RefreshProperties(RefreshProperties.All)]
        public bool ConnectionReset
        {
            get
            {
                return this._connectionReset;
            }
            set
            {
                this.SetValue("Connection Reset", value);
                this._connectionReset = value;
            }
        }

        [DisplayName("Connect Timeout"), ResCategory("DataCategory_Initialization"), ResDescription("DbConnectionString_ConnectTimeout"), RefreshProperties(RefreshProperties.All)]
        public int ConnectTimeout
        {
            get
            {
                return this._connectTimeout;
            }
            set
            {
                if (value < 0)
                {
                    throw ADP.InvalidConnectionOptionValue("Connect Timeout");
                }
                this.SetValue("Connect Timeout", value);
                this._connectTimeout = value;
            }
        }

        [ResCategory("DataCategory_Source"), DisplayName("Context Connection"), ResDescription("DbConnectionString_ContextConnection"), RefreshProperties(RefreshProperties.All)]
        public bool ContextConnection
        {
            get
            {
                return this._contextConnection;
            }
            set
            {
                this.SetValue("Context Connection", value);
                this._contextConnection = value;
            }
        }

        [ResCategory("DataCategory_Initialization"), DisplayName("Current Language"), ResDescription("DbConnectionString_CurrentLanguage"), RefreshProperties(RefreshProperties.All)]
        public string CurrentLanguage
        {
            get
            {
                return this._currentLanguage;
            }
            set
            {
                this.SetValue("Current Language", value);
                this._currentLanguage = value;
            }
        }

        [ResDescription("DbConnectionString_DataSource"), ResCategory("DataCategory_Source"), DisplayName("Data Source"), RefreshProperties(RefreshProperties.All), TypeConverter(typeof(SqlDataSourceConverter))]
        public string DataSource
        {
            get
            {
                return this._dataSource;
            }
            set
            {
                this.SetValue("Data Source", value);
                this._dataSource = value;
            }
        }

        [ResDescription("DbConnectionString_Encrypt"), ResCategory("DataCategory_Security"), DisplayName("Encrypt"), RefreshProperties(RefreshProperties.All)]
        public bool Encrypt
        {
            get
            {
                return this._encrypt;
            }
            set
            {
                this.SetValue("Encrypt", value);
                this._encrypt = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Pooling"), DisplayName("Enlist"), ResDescription("DbConnectionString_Enlist")]
        public bool Enlist
        {
            get
            {
                return this._enlist;
            }
            set
            {
                this.SetValue("Enlist", value);
                this._enlist = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Source"), ResDescription("DbConnectionString_FailoverPartner"), DisplayName("Failover Partner"), TypeConverter(typeof(SqlDataSourceConverter))]
        public string FailoverPartner
        {
            get
            {
                return this._failoverPartner;
            }
            set
            {
                this.SetValue("Failover Partner", value);
                this._failoverPartner = value;
            }
        }

        [ResCategory("DataCategory_Source"), DisplayName("Initial Catalog"), ResDescription("DbConnectionString_InitialCatalog"), RefreshProperties(RefreshProperties.All), TypeConverter(typeof(SqlInitialCatalogConverter))]
        public string InitialCatalog
        {
            get
            {
                return this._initialCatalog;
            }
            set
            {
                this.SetValue("Initial Catalog", value);
                this._initialCatalog = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Security"), DisplayName("Integrated Security"), ResDescription("DbConnectionString_IntegratedSecurity")]
        public bool IntegratedSecurity
        {
            get
            {
                return this._integratedSecurity;
            }
            set
            {
                this.SetValue("Integrated Security", value);
                this._integratedSecurity = value;
            }
        }

        public override bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        public override object this[string keyword]
        {
            get
            {
                Keywords index = this.GetIndex(keyword);
                return this.GetAt(index);
            }
            set
            {
                Bid.Trace("<comm.SqlConnectionStringBuilder.set_Item|API> keyword='%ls'\n", keyword);
                if (value != null)
                {
                    switch (this.GetIndex(keyword))
                    {
                        case Keywords.DataSource:
                            this.DataSource = ConvertToString(value);
                            return;

                        case Keywords.FailoverPartner:
                            this.FailoverPartner = ConvertToString(value);
                            return;

                        case Keywords.AttachDBFilename:
                            this.AttachDBFilename = ConvertToString(value);
                            return;

                        case Keywords.InitialCatalog:
                            this.InitialCatalog = ConvertToString(value);
                            return;

                        case Keywords.IntegratedSecurity:
                            this.IntegratedSecurity = ConvertToIntegratedSecurity(value);
                            return;

                        case Keywords.PersistSecurityInfo:
                            this.PersistSecurityInfo = ConvertToBoolean(value);
                            return;

                        case Keywords.UserID:
                            this.UserID = ConvertToString(value);
                            return;

                        case Keywords.Password:
                            this.Password = ConvertToString(value);
                            return;

                        case Keywords.Enlist:
                            this.Enlist = ConvertToBoolean(value);
                            return;

                        case Keywords.Pooling:
                            this.Pooling = ConvertToBoolean(value);
                            return;

                        case Keywords.MinPoolSize:
                            this.MinPoolSize = ConvertToInt32(value);
                            return;

                        case Keywords.MaxPoolSize:
                            this.MaxPoolSize = ConvertToInt32(value);
                            return;

                        case Keywords.AsynchronousProcessing:
                            this.AsynchronousProcessing = ConvertToBoolean(value);
                            return;

                        case Keywords.ConnectionReset:
                            this.ConnectionReset = ConvertToBoolean(value);
                            return;

                        case Keywords.MultipleActiveResultSets:
                            this.MultipleActiveResultSets = ConvertToBoolean(value);
                            return;

                        case Keywords.Replication:
                            this.Replication = ConvertToBoolean(value);
                            return;

                        case Keywords.ConnectTimeout:
                            this.ConnectTimeout = ConvertToInt32(value);
                            return;

                        case Keywords.Encrypt:
                            this.Encrypt = ConvertToBoolean(value);
                            return;

                        case Keywords.TrustServerCertificate:
                            this.TrustServerCertificate = ConvertToBoolean(value);
                            return;

                        case Keywords.LoadBalanceTimeout:
                            this.LoadBalanceTimeout = ConvertToInt32(value);
                            return;

                        case Keywords.NetworkLibrary:
                            this.NetworkLibrary = ConvertToString(value);
                            return;

                        case Keywords.PacketSize:
                            this.PacketSize = ConvertToInt32(value);
                            return;

                        case Keywords.TypeSystemVersion:
                            this.TypeSystemVersion = ConvertToString(value);
                            return;

                        case Keywords.ApplicationName:
                            this.ApplicationName = ConvertToString(value);
                            return;

                        case Keywords.CurrentLanguage:
                            this.CurrentLanguage = ConvertToString(value);
                            return;

                        case Keywords.WorkstationID:
                            this.WorkstationID = ConvertToString(value);
                            return;

                        case Keywords.UserInstance:
                            this.UserInstance = ConvertToBoolean(value);
                            return;

                        case Keywords.ContextConnection:
                            this.ContextConnection = ConvertToBoolean(value);
                            return;

                        case Keywords.TransactionBinding:
                            this.TransactionBinding = ConvertToString(value);
                            return;

                        case Keywords.MultiSubnetFailover:
                            this.MultiSubnetFailover = ConvertToBoolean(value);
                            return;

                        case Keywords.ApplicationIntent:
                            this.ApplicationIntent = ConvertToApplicationIntent(keyword, value);
                            return;
                    }
                    throw ADP.KeywordNotSupported(keyword);
                }
                this.Remove(keyword);
            }
        }

        public override ICollection Keys
        {
            get
            {
                return new ReadOnlyCollection<string>(_validKeywords);
            }
        }

        [ResCategory("DataCategory_Pooling"), RefreshProperties(RefreshProperties.All), DisplayName("Load Balance Timeout"), ResDescription("DbConnectionString_LoadBalanceTimeout")]
        public int LoadBalanceTimeout
        {
            get
            {
                return this._loadBalanceTimeout;
            }
            set
            {
                if (value < 0)
                {
                    throw ADP.InvalidConnectionOptionValue("Load Balance Timeout");
                }
                this.SetValue("Load Balance Timeout", value);
                this._loadBalanceTimeout = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DisplayName("Max Pool Size"), ResDescription("DbConnectionString_MaxPoolSize"), ResCategory("DataCategory_Pooling")]
        public int MaxPoolSize
        {
            get
            {
                return this._maxPoolSize;
            }
            set
            {
                if (value < 1)
                {
                    throw ADP.InvalidConnectionOptionValue("Max Pool Size");
                }
                this.SetValue("Max Pool Size", value);
                this._maxPoolSize = value;
            }
        }

        [ResDescription("DbConnectionString_MinPoolSize"), ResCategory("DataCategory_Pooling"), DisplayName("Min Pool Size"), RefreshProperties(RefreshProperties.All)]
        public int MinPoolSize
        {
            get
            {
                return this._minPoolSize;
            }
            set
            {
                if (value < 0)
                {
                    throw ADP.InvalidConnectionOptionValue("Min Pool Size");
                }
                this.SetValue("Min Pool Size", value);
                this._minPoolSize = value;
            }
        }

        [DisplayName("MultipleActiveResultSets"), ResCategory("DataCategory_Advanced"), ResDescription("DbConnectionString_MultipleActiveResultSets"), RefreshProperties(RefreshProperties.All)]
        public bool MultipleActiveResultSets
        {
            get
            {
                return this._multipleActiveResultSets;
            }
            set
            {
                this.SetValue("MultipleActiveResultSets", value);
                this._multipleActiveResultSets = value;
            }
        }

        [ResDescription("DbConnectionString_MultiSubnetFailover"), RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Source"), DisplayName("MultiSubnetFailover")]
        public bool MultiSubnetFailover
        {
            get
            {
                return this._multiSubnetFailover;
            }
            set
            {
                this.SetValue("MultiSubnetFailover", value);
                this._multiSubnetFailover = value;
            }
        }

        [TypeConverter(typeof(NetworkLibraryConverter)), DisplayName("Network Library"), ResCategory("DataCategory_Advanced"), RefreshProperties(RefreshProperties.All), ResDescription("DbConnectionString_NetworkLibrary")]
        public string NetworkLibrary
        {
            get
            {
                return this._networkLibrary;
            }
            set
            {
                if (value != null)
                {
                    switch (value.Trim().ToLower(CultureInfo.InvariantCulture))
                    {
                        case "dbmsadsn":
                            value = "dbmsadsn";
                            goto Label_011F;

                        case "dbmsvinn":
                            value = "dbmsvinn";
                            goto Label_011F;

                        case "dbmsspxn":
                            value = "dbmsspxn";
                            goto Label_011F;

                        case "dbmsrpcn":
                            value = "dbmsrpcn";
                            goto Label_011F;

                        case "dbnmpntw":
                            value = "dbnmpntw";
                            goto Label_011F;

                        case "dbmslpcn":
                            value = "dbmslpcn";
                            goto Label_011F;

                        case "dbmssocn":
                            value = "dbmssocn";
                            goto Label_011F;

                        case "dbmsgnet":
                            value = "dbmsgnet";
                            goto Label_011F;
                    }
                    throw ADP.InvalidConnectionOptionValue("Network Library");
                }
            Label_011F:
                this.SetValue("Network Library", value);
                this._networkLibrary = value;
            }
        }

        [ResDescription("DbConnectionString_PacketSize"), RefreshProperties(RefreshProperties.All), DisplayName("Packet Size"), ResCategory("DataCategory_Advanced")]
        public int PacketSize
        {
            get
            {
                return this._packetSize;
            }
            set
            {
                if ((value < 0x200) || (0x8000 < value))
                {
                    throw SQL.InvalidPacketSizeValue();
                }
                this.SetValue("Packet Size", value);
                this._packetSize = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DisplayName("Password"), ResCategory("DataCategory_Security"), PasswordPropertyText(true), ResDescription("DbConnectionString_Password")]
        public string Password
        {
            get
            {
                return this._password;
            }
            set
            {
                this.SetValue("Password", value);
                this._password = value;
            }
        }

        [ResCategory("DataCategory_Security"), DisplayName("Persist Security Info"), RefreshProperties(RefreshProperties.All), ResDescription("DbConnectionString_PersistSecurityInfo")]
        public bool PersistSecurityInfo
        {
            get
            {
                return this._persistSecurityInfo;
            }
            set
            {
                this.SetValue("Persist Security Info", value);
                this._persistSecurityInfo = value;
            }
        }

        [ResDescription("DbConnectionString_Pooling"), RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Pooling"), DisplayName("Pooling")]
        public bool Pooling
        {
            get
            {
                return this._pooling;
            }
            set
            {
                this.SetValue("Pooling", value);
                this._pooling = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DisplayName("Replication"), ResDescription("DbConnectionString_Replication"), ResCategory("DataCategory_Replication")]
        public bool Replication
        {
            get
            {
                return this._replication;
            }
            set
            {
                this.SetValue("Replication", value);
                this._replication = value;
            }
        }

        [DisplayName("Transaction Binding"), ResDescription("DbConnectionString_TransactionBinding"), RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Advanced")]
        public string TransactionBinding
        {
            get
            {
                return this._transactionBinding;
            }
            set
            {
                this.SetValue("Transaction Binding", value);
                this._transactionBinding = value;
            }
        }

        [DisplayName("TrustServerCertificate"), ResCategory("DataCategory_Security"), ResDescription("DbConnectionString_TrustServerCertificate"), RefreshProperties(RefreshProperties.All)]
        public bool TrustServerCertificate
        {
            get
            {
                return this._trustServerCertificate;
            }
            set
            {
                this.SetValue("TrustServerCertificate", value);
                this._trustServerCertificate = value;
            }
        }

        [DisplayName("Type System Version"), ResCategory("DataCategory_Advanced"), RefreshProperties(RefreshProperties.All), ResDescription("DbConnectionString_TypeSystemVersion")]
        public string TypeSystemVersion
        {
            get
            {
                return this._typeSystemVersion;
            }
            set
            {
                this.SetValue("Type System Version", value);
                this._typeSystemVersion = value;
            }
        }

        [ResDescription("DbConnectionString_UserID"), DisplayName("User ID"), RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Security")]
        public string UserID
        {
            get
            {
                return this._userID;
            }
            set
            {
                this.SetValue("User ID", value);
                this._userID = value;
            }
        }

        [DisplayName("User Instance"), ResCategory("DataCategory_Source"), RefreshProperties(RefreshProperties.All), ResDescription("DbConnectionString_UserInstance")]
        public bool UserInstance
        {
            get
            {
                return this._userInstance;
            }
            set
            {
                this.SetValue("User Instance", value);
                this._userInstance = value;
            }
        }

        public override ICollection Values
        {
            get
            {
                object[] items = new object[_validKeywords.Length];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = this.GetAt((Keywords) i);
                }
                return new ReadOnlyCollection<object>(items);
            }
        }

        [RefreshProperties(RefreshProperties.All), ResDescription("DbConnectionString_WorkstationID"), DisplayName("Workstation ID"), ResCategory("DataCategory_Context")]
        public string WorkstationID
        {
            get
            {
                return this._workstationID;
            }
            set
            {
                this.SetValue("Workstation ID", value);
                this._workstationID = value;
            }
        }

        private enum Keywords
        {
            DataSource,
            FailoverPartner,
            AttachDBFilename,
            InitialCatalog,
            IntegratedSecurity,
            PersistSecurityInfo,
            UserID,
            Password,
            Enlist,
            Pooling,
            MinPoolSize,
            MaxPoolSize,
            AsynchronousProcessing,
            ConnectionReset,
            MultipleActiveResultSets,
            Replication,
            ConnectTimeout,
            Encrypt,
            TrustServerCertificate,
            LoadBalanceTimeout,
            NetworkLibrary,
            PacketSize,
            TypeSystemVersion,
            ApplicationName,
            CurrentLanguage,
            WorkstationID,
            UserInstance,
            ContextConnection,
            TransactionBinding,
            MultiSubnetFailover,
            ApplicationIntent,
            KeywordsCount
        }

        private sealed class NetworkLibraryConverter : TypeConverter
        {
            private TypeConverter.StandardValuesCollection _standardValues;
            private const string NamedPipes = "Named Pipes (DBNMPNTW)";
            private const string SharedMemory = "Shared Memory (DBMSSOCN)";
            private const string TCPIP = "TCP/IP (DBMSGNET)";
            private const string VIA = "VIA (DBMSGNET)";

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (!(typeof(string) == sourceType))
                {
                    return base.CanConvertFrom(context, sourceType);
                }
                return true;
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (!(typeof(string) == destinationType))
                {
                    return base.CanConvertTo(context, destinationType);
                }
                return true;
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                string x = value as string;
                if (x == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }
                x = x.Trim();
                if (StringComparer.OrdinalIgnoreCase.Equals(x, "Named Pipes (DBNMPNTW)"))
                {
                    return "dbnmpntw";
                }
                if (StringComparer.OrdinalIgnoreCase.Equals(x, "Shared Memory (DBMSSOCN)"))
                {
                    return "dbmslpcn";
                }
                if (StringComparer.OrdinalIgnoreCase.Equals(x, "TCP/IP (DBMSGNET)"))
                {
                    return "dbmssocn";
                }
                if (StringComparer.OrdinalIgnoreCase.Equals(x, "VIA (DBMSGNET)"))
                {
                    return "dbmsgnet";
                }
                return x;
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                string str2 = value as string;
                if ((str2 == null) || !(destinationType == typeof(string)))
                {
                    return base.ConvertTo(context, culture, value, destinationType);
                }
                string str = str2.Trim().ToLower(CultureInfo.InvariantCulture);
                if (str == null)
                {
                    return str2;
                }
                if (!(str == "dbnmpntw"))
                {
                    if (str == "dbmslpcn")
                    {
                        return "Shared Memory (DBMSSOCN)";
                    }
                    if (str == "dbmssocn")
                    {
                        return "TCP/IP (DBMSGNET)";
                    }
                    if (str == "dbmsgnet")
                    {
                        return "VIA (DBMSGNET)";
                    }
                    return str2;
                }
                return "Named Pipes (DBNMPNTW)";
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (context != null)
                {
                    object instance = context.Instance;
                }
                TypeConverter.StandardValuesCollection valuess = this._standardValues;
                if (valuess == null)
                {
                    string[] values = new string[] { "Named Pipes (DBNMPNTW)", "Shared Memory (DBMSSOCN)", "TCP/IP (DBMSGNET)", "VIA (DBMSGNET)" };
                    valuess = new TypeConverter.StandardValuesCollection(values);
                    this._standardValues = valuess;
                }
                return valuess;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        internal sealed class SqlConnectionStringBuilderConverter : ExpandableObjectConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((typeof(InstanceDescriptor) == destinationType) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == null)
                {
                    throw ADP.ArgumentNull("destinationType");
                }
                if (typeof(InstanceDescriptor) == destinationType)
                {
                    SqlConnectionStringBuilder options = value as SqlConnectionStringBuilder;
                    if (options != null)
                    {
                        return this.ConvertToInstanceDescriptor(options);
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            private InstanceDescriptor ConvertToInstanceDescriptor(SqlConnectionStringBuilder options)
            {
                Type[] types = new Type[] { typeof(string) };
                return new InstanceDescriptor(typeof(SqlConnectionStringBuilder).GetConstructor(types), new object[] { options.ConnectionString });
            }
        }

        private sealed class SqlDataSourceConverter : StringConverter
        {
            private TypeConverter.StandardValuesCollection _standardValues;

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                TypeConverter.StandardValuesCollection valuess = this._standardValues;
                if (this._standardValues == null)
                {
                    DataTable dataSources = SqlClientFactory.Instance.CreateDataSourceEnumerator().GetDataSources();
                    DataColumn column2 = dataSources.Columns["ServerName"];
                    DataColumn column = dataSources.Columns["InstanceName"];
                    DataRowCollection rows = dataSources.Rows;
                    string[] array = new string[rows.Count];
                    for (int i = 0; i < array.Length; i++)
                    {
                        string str2 = rows[i][column2] as string;
                        string str = rows[i][column] as string;
                        if (((str == null) || (str.Length == 0)) || ("MSSQLSERVER" == str))
                        {
                            array[i] = str2;
                        }
                        else
                        {
                            array[i] = str2 + @"\" + str;
                        }
                    }
                    Array.Sort<string>(array);
                    valuess = new TypeConverter.StandardValuesCollection(array);
                    this._standardValues = valuess;
                }
                return valuess;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        private sealed class SqlInitialCatalogConverter : StringConverter
        {
            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (!this.GetStandardValuesSupportedInternal(context))
                {
                    return null;
                }
                List<string> values = new List<string>();
                try
                {
                    SqlConnectionStringBuilder instance = (SqlConnectionStringBuilder) context.Instance;
                    using (SqlConnection connection = new SqlConnection())
                    {
                        connection.ConnectionString = instance.ConnectionString;
                        using (SqlCommand command = new SqlCommand("SELECT name FROM master.dbo.sysdatabases ORDER BY name", connection))
                        {
                            connection.Open();
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    values.Add(reader.GetString(0));
                                }
                            }
                        }
                    }
                }
                catch (SqlException exception)
                {
                    ADP.TraceExceptionWithoutRethrow(exception);
                }
                return new TypeConverter.StandardValuesCollection(values);
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return this.GetStandardValuesSupportedInternal(context);
            }

            private bool GetStandardValuesSupportedInternal(ITypeDescriptorContext context)
            {
                bool flag = false;
                if (context == null)
                {
                    return flag;
                }
                SqlConnectionStringBuilder instance = context.Instance as SqlConnectionStringBuilder;
                if (((instance == null) || (0 >= instance.DataSource.Length)) || (!instance.IntegratedSecurity && (0 >= instance.UserID.Length)))
                {
                    return flag;
                }
                return true;
            }
        }
    }
}

