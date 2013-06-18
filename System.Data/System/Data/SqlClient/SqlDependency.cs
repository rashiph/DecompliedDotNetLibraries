namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.Sql;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Xml;

    public sealed class SqlDependency
    {
        private static readonly string _appDomainKey = Guid.NewGuid().ToString();
        private static readonly string _assemblyName = typeof(SqlDependencyProcessDispatcher).Assembly.FullName;
        private bool _dependencyFired;
        private object _eventHandlerLock;
        private List<EventContextPair> _eventList;
        private DateTime _expirationTime;
        private readonly string _id;
        private readonly int _objectID;
        private static int _objectTypeCount;
        private string _options;
        private static SqlDependencyProcessDispatcher _processDispatcher = null;
        private List<string> _serverList;
        private static Dictionary<string, Dictionary<IdentityUserNamePair, List<DatabaseServicePair>>> _serverUserHash = new Dictionary<string, Dictionary<IdentityUserNamePair, List<DatabaseServicePair>>>(StringComparer.OrdinalIgnoreCase);
        private static object _startStopLock = new object();
        private int _timeout;
        private static readonly string _typeName = typeof(SqlDependencyProcessDispatcher).FullName;
        internal const Bid.ApiGroup NotificationsTracePoints = Bid.ApiGroup.Dependency;

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("SqlDependency_OnChange")]
        public event OnChangeEventHandler OnChange
        {
            add
            {
                IntPtr ptr;
                Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.OnChange-Add|DEP> %d#", this.ObjectID);
                try
                {
                    if (value != null)
                    {
                        SqlNotificationEventArgs e = null;
                        lock (this._eventHandlerLock)
                        {
                            if (!this._dependencyFired)
                            {
                                Bid.NotificationsTrace("<sc.SqlDependency.OnChange-Add|DEP> Dependency has not fired, adding new event.\n");
                                EventContextPair item = new EventContextPair(value, this);
                                if (this._eventList.Contains(item))
                                {
                                    throw SQL.SqlDependencyEventNoDuplicate();
                                }
                                this._eventList.Add(item);
                            }
                            else
                            {
                                Bid.NotificationsTrace("<sc.SqlDependency.OnChange-Add|DEP> Dependency already fired, firing new event.\n");
                                e = new SqlNotificationEventArgs(SqlNotificationType.Subscribe, SqlNotificationInfo.AlreadyChanged, SqlNotificationSource.Client);
                            }
                        }
                        if (e != null)
                        {
                            value(this, e);
                        }
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
            remove
            {
                IntPtr ptr;
                Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.OnChange-Remove|DEP> %d#", this.ObjectID);
                try
                {
                    if (value != null)
                    {
                        EventContextPair item = new EventContextPair(value, this);
                        lock (this._eventHandlerLock)
                        {
                            int index = this._eventList.IndexOf(item);
                            if (0 <= index)
                            {
                                this._eventList.RemoveAt(index);
                            }
                        }
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public SqlDependency() : this(null, null, 0)
        {
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public SqlDependency(SqlCommand command) : this(command, null, 0)
        {
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public SqlDependency(SqlCommand command, string options, int timeout)
        {
            IntPtr ptr;
            this._id = Guid.NewGuid().ToString() + ";" + _appDomainKey;
            this._eventList = new List<EventContextPair>();
            this._eventHandlerLock = new object();
            this._expirationTime = DateTime.MaxValue;
            this._serverList = new List<string>();
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency|DEP> %d#, options: '%ls', timeout: '%d'", this.ObjectID, options, timeout);
            try
            {
                if (InOutOfProcHelper.InProc)
                {
                    throw SQL.SqlDepCannotBeCreatedInProc();
                }
                if (timeout < 0)
                {
                    throw SQL.InvalidSqlDependencyTimeout("timeout");
                }
                this._timeout = timeout;
                if (options != null)
                {
                    this._options = options;
                }
                this.AddCommandInternal(command);
                SqlDependencyPerAppDomainDispatcher.SingletonInstance.AddDependencyEntry(this);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        [System.Data.ResDescription("SqlDependency_AddCommandDependency"), System.Data.ResCategory("DataCategory_Data")]
        public void AddCommandDependency(SqlCommand command)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.AddCommandDependency|DEP> %d#", this.ObjectID);
            try
            {
                if (command == null)
                {
                    throw ADP.ArgumentNull("command");
                }
                this.AddCommandInternal(command);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void AddCommandInternal(SqlCommand cmd)
        {
            if (cmd != null)
            {
                IntPtr ptr;
                Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.AddCommandInternal|DEP> %d#, SqlCommand: %d#", this.ObjectID, cmd.ObjectID);
                try
                {
                    SqlConnection connection = cmd.Connection;
                    if (cmd.Notification != null)
                    {
                        if ((cmd._sqlDep == null) || (cmd._sqlDep != this))
                        {
                            Bid.NotificationsTrace("<sc.SqlDependency.AddCommandInternal|DEP|ERR> ERROR - throwing command has existing SqlNotificationRequest exception.\n");
                            throw SQL.SqlCommandHasExistingSqlNotificationRequest();
                        }
                    }
                    else
                    {
                        bool flag = false;
                        lock (this._eventHandlerLock)
                        {
                            if (!this._dependencyFired)
                            {
                                cmd.Notification = new SqlNotificationRequest();
                                cmd.Notification.Timeout = this._timeout;
                                if (this._options != null)
                                {
                                    cmd.Notification.Options = this._options;
                                }
                                cmd._sqlDep = this;
                            }
                            else if (this._eventList.Count == 0)
                            {
                                Bid.NotificationsTrace("<sc.SqlDependency.AddCommandInternal|DEP|ERR> ERROR - firing events, though it is unexpected we have events at this point.\n");
                                flag = true;
                            }
                        }
                        if (flag)
                        {
                            this.Invalidate(SqlNotificationType.Subscribe, SqlNotificationInfo.AlreadyChanged, SqlNotificationSource.Client);
                        }
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        internal void AddToServerList(string server)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.AddToServerList|DEP> %d#, server: '%ls'", this.ObjectID, server);
            try
            {
                lock (this._serverList)
                {
                    int index = this._serverList.BinarySearch(server, StringComparer.OrdinalIgnoreCase);
                    if (0 > index)
                    {
                        Bid.NotificationsTrace("<sc.SqlDependency.AddToServerList|DEP> Server not present in hashtable, adding server: '%ls'.\n", server);
                        index = ~index;
                        this._serverList.Insert(index, server);
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private static bool AddToServerUserHash(string server, IdentityUserNamePair identityUser, DatabaseServicePair databaseService)
        {
            bool flag2;
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.AddToServerUserHash|DEP> server: '%ls', database: '%ls', service: '%ls'", server, databaseService.Database, databaseService.Service);
            try
            {
                bool flag = false;
                lock (_serverUserHash)
                {
                    Dictionary<IdentityUserNamePair, List<DatabaseServicePair>> dictionary;
                    List<DatabaseServicePair> list;
                    if (!_serverUserHash.ContainsKey(server))
                    {
                        Bid.NotificationsTrace("<sc.SqlDependency.AddToServerUserHash|DEP> Hash did not contain server, adding.\n");
                        dictionary = new Dictionary<IdentityUserNamePair, List<DatabaseServicePair>>();
                        _serverUserHash.Add(server, dictionary);
                    }
                    else
                    {
                        dictionary = _serverUserHash[server];
                    }
                    if (!dictionary.ContainsKey(identityUser))
                    {
                        Bid.NotificationsTrace("<sc.SqlDependency.AddToServerUserHash|DEP> Hash contained server but not user, adding user.\n");
                        list = new List<DatabaseServicePair>();
                        dictionary.Add(identityUser, list);
                    }
                    else
                    {
                        list = dictionary[identityUser];
                    }
                    if (!list.Contains(databaseService))
                    {
                        Bid.NotificationsTrace("<sc.SqlDependency.AddToServerUserHash|DEP> Adding database.\n");
                        list.Add(databaseService);
                        flag = true;
                    }
                    else
                    {
                        Bid.NotificationsTrace("<sc.SqlDependency.AddToServerUserHash|DEP|ERR> ERROR - hash already contained server, user, and database - we will throw!.\n");
                    }
                }
                flag2 = flag;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag2;
        }

        private string ComputeCommandHash(string connectionString, SqlCommand command)
        {
            string str2;
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.ComputeCommandHash|DEP> %d#, SqlCommand: %d#", this.ObjectID, command.ObjectID);
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("{0};{1}", connectionString, command.CommandText);
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    object obj2 = command.Parameters[i].Value;
                    if ((obj2 == null) || (obj2 == DBNull.Value))
                    {
                        builder.Append("; NULL");
                    }
                    else
                    {
                        Type type = obj2.GetType();
                        if (type == typeof(byte[]))
                        {
                            builder.Append(";");
                            byte[] buffer = (byte[]) obj2;
                            for (int j = 0; j < buffer.Length; j++)
                            {
                                builder.Append(buffer[j].ToString("x2", CultureInfo.InvariantCulture));
                            }
                        }
                        else if (type == typeof(char[]))
                        {
                            builder.Append((char[]) obj2);
                        }
                        else if (type == typeof(XmlReader))
                        {
                            builder.Append(";");
                            builder.Append(Guid.NewGuid().ToString());
                        }
                        else
                        {
                            builder.Append(";");
                            builder.Append(obj2.ToString());
                        }
                    }
                }
                string str = builder.ToString();
                Bid.NotificationsTrace("<sc.SqlDependency.ComputeCommandHash|DEP> ComputeCommandHash result: '%ls'.\n", str);
                str2 = str;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return str2;
        }

        internal string ComputeHashAndAddToDispatcher(SqlCommand command)
        {
            string str2;
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.ComputeHashAndAddToDispatcher|DEP> %d#, SqlCommand: %d#", this.ObjectID, command.ObjectID);
            try
            {
                string commandHash = this.ComputeCommandHash(command.Connection.ConnectionString, command);
                string str = SqlDependencyPerAppDomainDispatcher.SingletonInstance.AddCommandEntry(commandHash, this);
                Bid.NotificationsTrace("<sc.SqlDependency.ComputeHashAndAddToDispatcher|DEP> computed id string: '%ls'.\n", str);
                str2 = str;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return str2;
        }

        internal bool ContainsServer(string server)
        {
            lock (this._serverList)
            {
                return this._serverList.Contains(server);
            }
        }

        [ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
        private static ObjectHandle CreateProcessDispatcher(_AppDomain masterDomain)
        {
            return masterDomain.CreateInstance(_assemblyName, _typeName);
        }

        internal static string FixupServiceOrDatabaseName(string name)
        {
            if (!ADP.IsEmpty(name))
            {
                return ("\"" + name.Replace("\"", "\"\"") + "\"");
            }
            return name;
        }

        internal static string GetDefaultComposedOptions(string server, string failoverServer, IdentityUserNamePair identityUser, string database)
        {
            string str2;
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.GetDefaultComposedOptions|DEP> server: '%ls', failoverServer: '%ls', database: '%ls'", server, failoverServer, database);
            try
            {
                string str;
                lock (_serverUserHash)
                {
                    if (!_serverUserHash.ContainsKey(server))
                    {
                        if (_serverUserHash.Count == 0)
                        {
                            Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP|ERR> ERROR - no start calls have been made, about to throw.\n");
                            throw SQL.SqlDepDefaultOptionsButNoStart();
                        }
                        if (ADP.IsEmpty(failoverServer) || !_serverUserHash.ContainsKey(failoverServer))
                        {
                            Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP|ERR> ERROR - not listening to this server, about to throw.\n");
                            throw SQL.SqlDependencyNoMatchingServerStart();
                        }
                        Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP> using failover server instead\n");
                        server = failoverServer;
                    }
                    Dictionary<IdentityUserNamePair, List<DatabaseServicePair>> dictionary = _serverUserHash[server];
                    List<DatabaseServicePair> list = null;
                    if (!dictionary.ContainsKey(identityUser))
                    {
                        if (dictionary.Count > 1)
                        {
                            Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP|ERR> ERROR - not listening for this user, but listening to more than one other user, about to throw.\n");
                            throw SQL.SqlDependencyNoMatchingServerStart();
                        }
                        foreach (KeyValuePair<IdentityUserNamePair, List<DatabaseServicePair>> pair3 in dictionary)
                        {
                            list = pair3.Value;
                            break;
                        }
                    }
                    else
                    {
                        list = dictionary[identityUser];
                    }
                    DatabaseServicePair item = new DatabaseServicePair(database, null);
                    DatabaseServicePair pair = null;
                    int index = list.IndexOf(item);
                    if (index != -1)
                    {
                        pair = list[index];
                    }
                    if (pair == null)
                    {
                        if (list.Count != 1)
                        {
                            Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP|ERR> ERROR - SqlDependency.Start called multiple times for this server/user, but no matching database.\n");
                            throw SQL.SqlDependencyNoMatchingServerDatabaseStart();
                        }
                        pair = list.ToArray()[0];
                        string str4 = FixupServiceOrDatabaseName(pair.Database);
                        string str3 = FixupServiceOrDatabaseName(pair.Service);
                        str = "Service=" + str3 + ";Local Database=" + str4;
                    }
                    else
                    {
                        database = FixupServiceOrDatabaseName(pair.Database);
                        string str5 = FixupServiceOrDatabaseName(pair.Service);
                        str = "Service=" + str5 + ";Local Database=" + database;
                    }
                }
                Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP> resulting options: '%ls'.\n", str);
                str2 = str;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return str2;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        private static SqlDependencyProcessDispatcher GetDeserializedObject(BinaryFormatter formatter, MemoryStream stream)
        {
            return (SqlDependencyProcessDispatcher) formatter.Deserialize(stream);
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        private static ObjRef GetObjRef(SqlDependencyProcessDispatcher _processDispatcher)
        {
            return RemotingServices.Marshal(_processDispatcher);
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        private static void GetSerializedObject(ObjRef objRef, BinaryFormatter formatter, MemoryStream stream)
        {
            formatter.Serialize(stream, objRef);
        }

        internal void Invalidate(SqlNotificationType type, SqlNotificationInfo info, SqlNotificationSource source)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.Invalidate|DEP> %d#", this.ObjectID);
            try
            {
                List<EventContextPair> list = null;
                lock (this._eventHandlerLock)
                {
                    if ((this._dependencyFired && (SqlNotificationInfo.AlreadyChanged != info)) && (SqlNotificationSource.Client != source))
                    {
                        Bid.NotificationsTrace("<sc.SqlDependency.Invalidate|DEP|ERR> ERROR - notification received twice - we should never enter this state!");
                    }
                    else
                    {
                        this._dependencyFired = true;
                        list = this._eventList;
                        this._eventList = new List<EventContextPair>();
                    }
                }
                if (list != null)
                {
                    Bid.NotificationsTrace("<sc.SqlDependency.Invalidate|DEP> Firing events.\n");
                    foreach (EventContextPair pair in list)
                    {
                        pair.Invoke(new SqlNotificationEventArgs(type, info, source));
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private static void ObtainProcessDispatcher()
        {
            byte[] data = SNINativeMethodWrapper.GetData();
            if (data == null)
            {
                Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP> nativeStorage null, obtaining dispatcher AppDomain and creating ProcessDispatcher.\n");
                _AppDomain defaultAppDomain = SNINativeMethodWrapper.GetDefaultAppDomain();
                if (defaultAppDomain != null)
                {
                    ObjectHandle handle = CreateProcessDispatcher(defaultAppDomain);
                    if (handle != null)
                    {
                        SqlDependencyProcessDispatcher dispatcher = (SqlDependencyProcessDispatcher) handle.Unwrap();
                        if (dispatcher != null)
                        {
                            _processDispatcher = dispatcher.SingletonProcessDispatcher;
                            ObjRef objRef = GetObjRef(_processDispatcher);
                            BinaryFormatter formatter2 = new BinaryFormatter();
                            MemoryStream stream = new MemoryStream();
                            GetSerializedObject(objRef, formatter2, stream);
                            SNINativeMethodWrapper.SetData(stream.GetBuffer());
                            return;
                        }
                        Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP|ERR> ERROR - ObjectHandle.Unwrap returned null!\n");
                        throw ADP.InternalError(ADP.InternalErrorCode.SqlDependencyObtainProcessDispatcherFailureObjectHandle);
                    }
                    Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP|ERR> ERROR - AppDomain.CreateInstance returned null!\n");
                    throw ADP.InternalError(ADP.InternalErrorCode.SqlDependencyProcessDispatcherFailureCreateInstance);
                }
                Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP|ERR> ERROR - unable to obtain default AppDomain!\n");
                throw ADP.InternalError(ADP.InternalErrorCode.SqlDependencyProcessDispatcherFailureAppDomain);
            }
            Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP> nativeStorage not null, obtaining existing dispatcher AppDomain and ProcessDispatcher.\n");
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream2 = new MemoryStream(data);
            _processDispatcher = GetDeserializedObject(formatter, stream2);
            Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP> processDispatcher obtained, ID: %d\n", _processDispatcher.ObjectID);
        }

        private static void RemoveFromServerUserHash(string server, IdentityUserNamePair identityUser, DatabaseServicePair databaseService)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.RemoveFromServerUserHash|DEP> server: '%ls', database: '%ls', service: '%ls'", server, databaseService.Database, databaseService.Service);
            try
            {
                lock (_serverUserHash)
                {
                    if (_serverUserHash.ContainsKey(server))
                    {
                        Dictionary<IdentityUserNamePair, List<DatabaseServicePair>> dictionary = _serverUserHash[server];
                        if (dictionary.ContainsKey(identityUser))
                        {
                            List<DatabaseServicePair> list = dictionary[identityUser];
                            int index = list.IndexOf(databaseService);
                            if (index >= 0)
                            {
                                Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP> Hash contained server, user, and database - removing database.\n");
                                list.RemoveAt(index);
                                if (list.Count == 0)
                                {
                                    Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP> databaseServiceList count 0, removing the list for this server and user.\n");
                                    dictionary.Remove(identityUser);
                                    if (dictionary.Count == 0)
                                    {
                                        Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP> identityDatabaseHash count 0, removing the hash for this server.\n");
                                        _serverUserHash.Remove(server);
                                    }
                                }
                            }
                            else
                            {
                                Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP|ERR> ERROR - hash contained server and user but not database!\n");
                            }
                        }
                        else
                        {
                            Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP|ERR> ERROR - hash contained server but not user!\n");
                        }
                    }
                    else
                    {
                        Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP|ERR> ERROR - hash did not contain server!\n");
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public static bool Start(string connectionString)
        {
            return Start(connectionString, null, true);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public static bool Start(string connectionString, string queue)
        {
            return Start(connectionString, queue, false);
        }

        internal static bool Start(string connectionString, string queue, bool useDefaults)
        {
            bool flag2;
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.Start|DEP> AppDomainKey: '%ls', queue: '%ls'", AppDomainKey, queue);
            try
            {
                if (InOutOfProcHelper.InProc)
                {
                    throw SQL.SqlDepCannotBeCreatedInProc();
                }
                if (ADP.IsEmpty(connectionString))
                {
                    if (connectionString == null)
                    {
                        throw ADP.ArgumentNull("connectionString");
                    }
                    throw ADP.Argument("connectionString");
                }
                if (!useDefaults && ADP.IsEmpty(queue))
                {
                    useDefaults = true;
                    queue = null;
                }
                new SqlConnectionString(connectionString).DemandPermission();
                bool errorOccurred = false;
                bool flag = false;
                lock (_startStopLock)
                {
                    try
                    {
                        if (_processDispatcher == null)
                        {
                            ObtainProcessDispatcher();
                        }
                        if (useDefaults)
                        {
                            string server = null;
                            DbConnectionPoolIdentity identity = null;
                            string user = null;
                            string database = null;
                            string service = null;
                            bool appDomainStart = false;
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try
                            {
                                flag = _processDispatcher.StartWithDefault(connectionString, out server, out identity, out user, out database, ref service, _appDomainKey, SqlDependencyPerAppDomainDispatcher.SingletonInstance, out errorOccurred, out appDomainStart);
                                Bid.NotificationsTrace("<sc.SqlDependency.Start|DEP> Start (defaults) returned: '%d', with service: '%ls', server: '%ls', database: '%ls'\n", flag, service, server, database);
                                goto Label_0183;
                            }
                            finally
                            {
                                if (appDomainStart && !errorOccurred)
                                {
                                    IdentityUserNamePair identityUser = new IdentityUserNamePair(identity, user);
                                    DatabaseServicePair databaseService = new DatabaseServicePair(database, service);
                                    if (!AddToServerUserHash(server, identityUser, databaseService))
                                    {
                                        try
                                        {
                                            Stop(connectionString, queue, useDefaults, true);
                                        }
                                        catch (Exception exception2)
                                        {
                                            if (!ADP.IsCatchableExceptionType(exception2))
                                            {
                                                throw;
                                            }
                                            ADP.TraceExceptionWithoutRethrow(exception2);
                                            Bid.NotificationsTrace("<sc.SqlDependency.Start|DEP|ERR> Exception occurred from Stop() after duplicate was found on Start().\n");
                                        }
                                        throw SQL.SqlDependencyDuplicateStart();
                                    }
                                }
                            }
                        }
                        flag = _processDispatcher.Start(connectionString, queue, _appDomainKey, SqlDependencyPerAppDomainDispatcher.SingletonInstance);
                        Bid.NotificationsTrace("<sc.SqlDependency.Start|DEP> Start (user provided queue) returned: '%d'\n", flag);
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        ADP.TraceExceptionWithoutRethrow(exception);
                        Bid.NotificationsTrace("<sc.SqlDependency.Start|DEP|ERR> Exception occurred from _processDispatcher.Start(...), calling Invalidate(...).\n");
                        throw;
                    }
                Label_0183:;
                }
                flag2 = flag;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag2;
        }

        internal void StartTimer(SqlNotificationRequest notificationRequest)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.StartTimer|DEP> %d#", this.ObjectID);
            try
            {
                if (this._expirationTime == DateTime.MaxValue)
                {
                    Bid.NotificationsTrace("<sc.SqlDependency.StartTimer|DEP> We've timed out, executing logic.\n");
                    int timeout = 0x69780;
                    if (this._timeout != 0)
                    {
                        timeout = this._timeout;
                    }
                    if (((notificationRequest != null) && (notificationRequest.Timeout < timeout)) && (notificationRequest.Timeout != 0))
                    {
                        timeout = notificationRequest.Timeout;
                    }
                    this._expirationTime = DateTime.UtcNow.AddSeconds((double) timeout);
                    SqlDependencyPerAppDomainDispatcher.SingletonInstance.StartTimer(this);
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public static bool Stop(string connectionString)
        {
            return Stop(connectionString, null, true, false);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public static bool Stop(string connectionString, string queue)
        {
            return Stop(connectionString, queue, false, false);
        }

        internal static bool Stop(string connectionString, string queue, bool useDefaults, bool startFailed)
        {
            bool flag2;
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependency.Stop|DEP> AppDomainKey: '%ls', queue: '%ls'", AppDomainKey, queue);
            try
            {
                if (InOutOfProcHelper.InProc)
                {
                    throw SQL.SqlDepCannotBeCreatedInProc();
                }
                if (ADP.IsEmpty(connectionString))
                {
                    if (connectionString == null)
                    {
                        throw ADP.ArgumentNull("connectionString");
                    }
                    throw ADP.Argument("connectionString");
                }
                if (!useDefaults && ADP.IsEmpty(queue))
                {
                    useDefaults = true;
                    queue = null;
                }
                new SqlConnectionString(connectionString).DemandPermission();
                bool flag = false;
                lock (_startStopLock)
                {
                    if (_processDispatcher != null)
                    {
                        try
                        {
                            string server = null;
                            DbConnectionPoolIdentity identity = null;
                            string user = null;
                            string database = null;
                            string queueService = null;
                            if (useDefaults)
                            {
                                bool flag3 = false;
                                RuntimeHelpers.PrepareConstrainedRegions();
                                try
                                {
                                    flag = _processDispatcher.Stop(connectionString, out server, out identity, out user, out database, ref queueService, _appDomainKey, out flag3);
                                    goto Label_0121;
                                }
                                finally
                                {
                                    if (flag3 && !startFailed)
                                    {
                                        IdentityUserNamePair identityUser = new IdentityUserNamePair(identity, user);
                                        DatabaseServicePair databaseService = new DatabaseServicePair(database, queueService);
                                        RemoveFromServerUserHash(server, identityUser, databaseService);
                                    }
                                }
                            }
                            bool appDomainStop = false;
                            flag = _processDispatcher.Stop(connectionString, out server, out identity, out user, out database, ref queue, _appDomainKey, out appDomainStop);
                        }
                        catch (Exception exception)
                        {
                            if (!ADP.IsCatchableExceptionType(exception))
                            {
                                throw;
                            }
                            ADP.TraceExceptionWithoutRethrow(exception);
                        }
                    }
                Label_0121:;
                }
                flag2 = flag;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag2;
        }

        internal static string AppDomainKey
        {
            get
            {
                return _appDomainKey;
            }
        }

        internal DateTime ExpirationTime
        {
            get
            {
                return this._expirationTime;
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("SqlDependency_HasChanges")]
        public bool HasChanges
        {
            get
            {
                return this._dependencyFired;
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("SqlDependency_Id")]
        public string Id
        {
            get
            {
                return this._id;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal string Options
        {
            get
            {
                string str = null;
                if (this._options != null)
                {
                    str = this._options;
                }
                return str;
            }
        }

        internal static SqlDependencyProcessDispatcher ProcessDispatcher
        {
            get
            {
                return _processDispatcher;
            }
        }

        internal int Timeout
        {
            get
            {
                return this._timeout;
            }
        }

        private class DatabaseServicePair
        {
            private string _database;
            private string _service;

            internal DatabaseServicePair(string database, string service)
            {
                this._database = database;
                this._service = service;
            }

            public override bool Equals(object value)
            {
                SqlDependency.DatabaseServicePair pair = (SqlDependency.DatabaseServicePair) value;
                bool flag = false;
                if (pair == null)
                {
                    return false;
                }
                if (this == pair)
                {
                    return true;
                }
                if (this._database == pair._database)
                {
                    flag = true;
                }
                return flag;
            }

            public override int GetHashCode()
            {
                return this._database.GetHashCode();
            }

            internal string Database
            {
                get
                {
                    return this._database;
                }
            }

            internal string Service
            {
                get
                {
                    return this._service;
                }
            }
        }

        internal class EventContextPair
        {
            private SqlNotificationEventArgs _args;
            private ExecutionContext _context;
            private static ContextCallback _contextCallback = new ContextCallback(SqlDependency.EventContextPair.InvokeCallback);
            private SqlDependency _dependency;
            private OnChangeEventHandler _eventHandler;

            internal EventContextPair(OnChangeEventHandler eventHandler, SqlDependency dependency)
            {
                this._eventHandler = eventHandler;
                this._context = ExecutionContext.Capture();
                this._dependency = dependency;
            }

            public override bool Equals(object value)
            {
                SqlDependency.EventContextPair pair = (SqlDependency.EventContextPair) value;
                bool flag = false;
                if (pair == null)
                {
                    return false;
                }
                if (this == pair)
                {
                    return true;
                }
                if (this._eventHandler == pair._eventHandler)
                {
                    flag = true;
                }
                return flag;
            }

            public override int GetHashCode()
            {
                return this._eventHandler.GetHashCode();
            }

            internal void Invoke(SqlNotificationEventArgs args)
            {
                this._args = args;
                ExecutionContext.Run(this._context, _contextCallback, this);
            }

            private static void InvokeCallback(object eventContextPair)
            {
                SqlDependency.EventContextPair pair = (SqlDependency.EventContextPair) eventContextPair;
                pair._eventHandler(pair._dependency, pair._args);
            }
        }

        internal class IdentityUserNamePair
        {
            private DbConnectionPoolIdentity _identity;
            private string _userName;

            internal IdentityUserNamePair(DbConnectionPoolIdentity identity, string userName)
            {
                this._identity = identity;
                this._userName = userName;
            }

            public override bool Equals(object value)
            {
                SqlDependency.IdentityUserNamePair pair = (SqlDependency.IdentityUserNamePair) value;
                bool flag = false;
                if (pair == null)
                {
                    return false;
                }
                if (this == pair)
                {
                    return true;
                }
                if (this._identity != null)
                {
                    if (this._identity.Equals(pair._identity))
                    {
                        flag = true;
                    }
                    return flag;
                }
                if (this._userName == pair._userName)
                {
                    flag = true;
                }
                return flag;
            }

            public override int GetHashCode()
            {
                if (this._identity != null)
                {
                    return this._identity.GetHashCode();
                }
                return this._userName.GetHashCode();
            }

            internal DbConnectionPoolIdentity Identity
            {
                get
                {
                    return this._identity;
                }
            }

            internal string UserName
            {
                get
                {
                    return this._userName;
                }
            }
        }
    }
}

