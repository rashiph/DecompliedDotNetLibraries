namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Globalization;
    using System.Threading;

    internal class SqlDependencyPerAppDomainDispatcher : MarshalByRefObject
    {
        private Dictionary<string, string> _commandHashToNotificationId;
        private Dictionary<string, SqlDependency> _dependencyIdToDependencyHash;
        private DateTime _nextTimeout;
        private Dictionary<string, DependencyList> _notificationIdToDependenciesHash;
        private readonly int _objectID;
        private static int _objectTypeCount;
        private bool _SqlDependencyTimeOutTimerStarted;
        private Timer _timeoutTimer;
        internal static readonly SqlDependencyPerAppDomainDispatcher SingletonInstance = new SqlDependencyPerAppDomainDispatcher();

        private SqlDependencyPerAppDomainDispatcher()
        {
            IntPtr ptr;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher|DEP> %d#", this.ObjectID);
            try
            {
                this._dependencyIdToDependencyHash = new Dictionary<string, SqlDependency>();
                this._notificationIdToDependenciesHash = new Dictionary<string, DependencyList>();
                this._commandHashToNotificationId = new Dictionary<string, string>();
                this._timeoutTimer = new Timer(new TimerCallback(SqlDependencyPerAppDomainDispatcher.TimeoutTimerCallback), null, -1, -1);
                AppDomain.CurrentDomain.DomainUnload += new EventHandler(this.UnloadEventHandler);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal string AddCommandEntry(string commandHash, SqlDependency dep)
        {
            IntPtr ptr;
            string str = string.Empty;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.AddCommandEntry|DEP> %d#, commandHash: '%ls', SqlDependency: %d#", this.ObjectID, commandHash, dep.ObjectID);
            try
            {
                lock (this)
                {
                    if (!this._dependencyIdToDependencyHash.ContainsKey(dep.Id))
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.AddCommandEntry|DEP> Dependency not present in depId->dep hash, must have been invalidated.\n");
                        return str;
                    }
                    if (this._commandHashToNotificationId.TryGetValue(commandHash, out str))
                    {
                        DependencyList list2 = null;
                        if (!this._notificationIdToDependenciesHash.TryGetValue(str, out list2))
                        {
                            throw ADP.InternalError(ADP.InternalErrorCode.SqlDependencyCommandHashIsNotAssociatedWithNotification);
                        }
                        if (!list2.Contains(dep))
                        {
                            Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.AddCommandEntry|DEP> Dependency not present for commandHash, adding.\n");
                            list2.Add(dep);
                            return str;
                        }
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.AddCommandEntry|DEP> Dependency already present for commandHash.\n");
                        return str;
                    }
                    str = string.Format(CultureInfo.InvariantCulture, "{0};{1}", new object[] { SqlDependency.AppDomainKey, Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture) });
                    Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.AddCommandEntry|DEP> Creating new Dependencies list for commandHash.\n");
                    DependencyList list = new DependencyList(commandHash) {
                        dep
                    };
                    try
                    {
                    }
                    finally
                    {
                        this._commandHashToNotificationId.Add(commandHash, str);
                        this._notificationIdToDependenciesHash.Add(str, list);
                    }
                    return str;
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return str;
        }

        internal void AddDependencyEntry(SqlDependency dep)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.AddDependencyEntry|DEP> %d#, SqlDependency: %d#", this.ObjectID, dep.ObjectID);
            try
            {
                lock (this)
                {
                    this._dependencyIdToDependencyHash.Add(dep.Id, dep);
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal void InvalidateCommandID(SqlNotification sqlNotification)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.InvalidateCommandID|DEP> %d#, commandHash: '%ls'", this.ObjectID, sqlNotification.Key);
            try
            {
                List<SqlDependency> commandEntryWithRemove = null;
                lock (this)
                {
                    commandEntryWithRemove = this.LookupCommandEntryWithRemove(sqlNotification.Key);
                    if (commandEntryWithRemove != null)
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.InvalidateCommandID|DEP> commandHash found in hashtable.\n");
                        foreach (SqlDependency dependency in commandEntryWithRemove)
                        {
                            this.LookupDependencyEntryWithRemove(dependency.Id);
                            this.RemoveDependencyFromCommandToDependenciesHash(dependency);
                        }
                    }
                    else
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.InvalidateCommandID|DEP> commandHash NOT found in hashtable.\n");
                    }
                }
                if (commandEntryWithRemove != null)
                {
                    foreach (SqlDependency dependency2 in commandEntryWithRemove)
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.InvalidateCommandID|DEP> Dependency found in commandHash dependency ArrayList - calling invalidate.\n");
                        try
                        {
                            dependency2.Invalidate(sqlNotification.Type, sqlNotification.Info, sqlNotification.Source);
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
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void InvalidateServer(string server, SqlNotification sqlNotification)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.Invalidate|DEP> %d#, server: '%ls'", this.ObjectID, server);
            try
            {
                List<SqlDependency> list = new List<SqlDependency>();
                lock (this)
                {
                    foreach (KeyValuePair<string, SqlDependency> pair in this._dependencyIdToDependencyHash)
                    {
                        SqlDependency item = pair.Value;
                        if (item.ContainsServer(server))
                        {
                            list.Add(item);
                        }
                    }
                    foreach (SqlDependency dependency in list)
                    {
                        this.LookupDependencyEntryWithRemove(dependency.Id);
                        this.RemoveDependencyFromCommandToDependenciesHash(dependency);
                    }
                }
                foreach (SqlDependency dependency3 in list)
                {
                    try
                    {
                        dependency3.Invalidate(sqlNotification.Type, sqlNotification.Info, sqlNotification.Source);
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
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private List<SqlDependency> LookupCommandEntryWithRemove(string notificationId)
        {
            List<SqlDependency> list2;
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.LookupCommandEntryWithRemove|DEP> %d#, commandHash: '%ls'", this.ObjectID, notificationId);
            try
            {
                DependencyList list = null;
                lock (this)
                {
                    if (this._notificationIdToDependenciesHash.TryGetValue(notificationId, out list))
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.LookupDependencyEntriesWithRemove|DEP> Entries found in hashtable - removing.\n");
                        try
                        {
                        }
                        finally
                        {
                            this._notificationIdToDependenciesHash.Remove(notificationId);
                            this._commandHashToNotificationId.Remove(list.CommandHash);
                        }
                    }
                    else
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.LookupDependencyEntriesWithRemove|DEP> Entries NOT found in hashtable.\n");
                    }
                }
                list2 = list;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return list2;
        }

        internal SqlDependency LookupDependencyEntry(string id)
        {
            SqlDependency dependency2;
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.LookupDependencyEntry|DEP> %d#, Key: '%ls'", this.ObjectID, id);
            try
            {
                if (id == null)
                {
                    throw ADP.ArgumentNull("id");
                }
                if (ADP.IsEmpty(id))
                {
                    throw SQL.SqlDependencyIdMismatch();
                }
                SqlDependency dependency = null;
                lock (this)
                {
                    if (this._dependencyIdToDependencyHash.ContainsKey(id))
                    {
                        dependency = this._dependencyIdToDependencyHash[id];
                    }
                    else
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.LookupDependencyEntry|DEP|ERR> ERROR - dependency ID mismatch - not throwing.\n");
                    }
                }
                dependency2 = dependency;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return dependency2;
        }

        private void LookupDependencyEntryWithRemove(string id)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.LookupDependencyEntryWithRemove|DEP> %d#, id: '%ls'", this.ObjectID, id);
            try
            {
                lock (this)
                {
                    if (this._dependencyIdToDependencyHash.ContainsKey(id))
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.LookupDependencyEntryWithRemove|DEP> Entry found in hashtable - removing.\n");
                        this._dependencyIdToDependencyHash.Remove(id);
                        if (this._dependencyIdToDependencyHash.Count == 0)
                        {
                            this._timeoutTimer.Change(-1, -1);
                            this._SqlDependencyTimeOutTimerStarted = false;
                        }
                    }
                    else
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.LookupDependencyEntryWithRemove|DEP> Entry NOT found in hashtable.\n");
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void RemoveDependencyFromCommandToDependenciesHash(SqlDependency dependency)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.RemoveDependencyFromCommandToDependenciesHash|DEP> %d#, SqlDependency: %d#", this.ObjectID, dependency.ObjectID);
            try
            {
                lock (this)
                {
                    List<string> list = new List<string>();
                    List<string> list3 = new List<string>();
                    foreach (KeyValuePair<string, DependencyList> pair in this._notificationIdToDependenciesHash)
                    {
                        DependencyList list2 = pair.Value;
                        if (list2.Remove(dependency))
                        {
                            Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.RemoveDependencyFromCommandToDependenciesHash|DEP> Removed SqlDependency: %d#, with ID: '%ls'.\n", dependency.ObjectID, dependency.Id);
                            if (list2.Count == 0)
                            {
                                list.Add(pair.Key);
                                list3.Add(pair.Value.CommandHash);
                            }
                        }
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this._notificationIdToDependenciesHash.Remove(list[i]);
                            this._commandHashToNotificationId.Remove(list3[i]);
                        }
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void StartTimer(SqlDependency dep)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.StartTimer|DEP> %d#, SqlDependency: %d#", this.ObjectID, dep.ObjectID);
            try
            {
                lock (this)
                {
                    if (!this._SqlDependencyTimeOutTimerStarted)
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.StartTimer|DEP> Timer not yet started, starting.\n");
                        this._timeoutTimer.Change(0x3a98, 0x3a98);
                        this._nextTimeout = dep.ExpirationTime;
                        this._SqlDependencyTimeOutTimerStarted = true;
                    }
                    else if (this._nextTimeout > dep.ExpirationTime)
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.StartTimer|DEP> Timer already started, resetting time.\n");
                        this._nextTimeout = dep.ExpirationTime;
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private static void TimeoutTimerCallback(object state)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.TimeoutTimerCallback|DEP> AppDomainKey: '%ls'", SqlDependency.AppDomainKey);
            try
            {
                SqlDependency[] dependencyArray;
                lock (SingletonInstance)
                {
                    if (SingletonInstance._dependencyIdToDependencyHash.Count == 0)
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.TimeoutTimerCallback|DEP> No dependencies, exiting.\n");
                        return;
                    }
                    if (SingletonInstance._nextTimeout > DateTime.UtcNow)
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyPerAppDomainDispatcher.TimeoutTimerCallback|DEP> No timeouts expired, exiting.\n");
                        return;
                    }
                    dependencyArray = new SqlDependency[SingletonInstance._dependencyIdToDependencyHash.Count];
                    SingletonInstance._dependencyIdToDependencyHash.Values.CopyTo(dependencyArray, 0);
                }
                DateTime utcNow = DateTime.UtcNow;
                DateTime maxValue = DateTime.MaxValue;
                for (int i = 0; i < dependencyArray.Length; i++)
                {
                    if (dependencyArray[i].ExpirationTime <= utcNow)
                    {
                        try
                        {
                            dependencyArray[i].Invalidate(SqlNotificationType.Change, SqlNotificationInfo.Error, SqlNotificationSource.Timeout);
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
                    else
                    {
                        if (dependencyArray[i].ExpirationTime < maxValue)
                        {
                            maxValue = dependencyArray[i].ExpirationTime;
                        }
                        dependencyArray[i] = null;
                    }
                }
                lock (SingletonInstance)
                {
                    for (int j = 0; j < dependencyArray.Length; j++)
                    {
                        if (dependencyArray[j] != null)
                        {
                            SingletonInstance._dependencyIdToDependencyHash.Remove(dependencyArray[j].Id);
                        }
                    }
                    if (maxValue < SingletonInstance._nextTimeout)
                    {
                        SingletonInstance._nextTimeout = maxValue;
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void UnloadEventHandler(object sender, EventArgs e)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyPerAppDomainDispatcher.UnloadEventHandler|DEP> %d#", this.ObjectID);
            try
            {
                SqlDependencyProcessDispatcher processDispatcher = SqlDependency.ProcessDispatcher;
                if (processDispatcher != null)
                {
                    processDispatcher.QueueAppDomainUnloading(SqlDependency.AppDomainKey);
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        private sealed class DependencyList : List<SqlDependency>
        {
            public readonly string CommandHash;

            internal DependencyList(string commandHash)
            {
                this.CommandHash = commandHash;
            }
        }
    }
}

