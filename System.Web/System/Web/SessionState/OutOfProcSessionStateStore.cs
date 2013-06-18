namespace System.Web.SessionState
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Util;

    internal sealed class OutOfProcSessionStateStore : SessionStateStoreProviderBase
    {
        private StateServerPartitionInfo _partitionInfo;
        private IPartitionResolver _partitionResolver;
        internal static readonly IntPtr INVALID_SOCKET = System.Web.UnsafeNativeMethods.INVALID_HANDLE_VALUE;
        private static bool s_configCompressionEnabled;
        private static string s_configPartitionResolverType;
        private static string s_configStateConnectionString;
        private static string s_configStateConnectionStringFileName;
        private static int s_configStateConnectionStringLineNumber;
        private static ReadWriteSpinLock s_lock;
        private static int s_networkTimeout;
        private static EventHandler s_onAppDomainUnload;
        private static bool s_oneTimeInited;
        private static PartitionManager s_partitionManager;
        private static StateServerPartitionInfo s_singlePartitionInfo;
        private static string s_uribase;
        private static bool s_usePartition;
        internal const int STATE_NETWORK_TIMEOUT_DEFAULT = 10;
        internal static readonly int WHIDBEY_MAJOR_VERSION = 2;

        internal static HttpException CreateConnectionException(string server, int port, int hr)
        {
            if (s_usePartition)
            {
                return new HttpException(System.Web.SR.GetString("Cant_make_session_request_partition_resolver", new object[] { s_configPartitionResolverType, server, port.ToString(CultureInfo.InvariantCulture) }), hr);
            }
            return new HttpException(System.Web.SR.GetString("Cant_make_session_request"), hr);
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return SessionStateUtility.CreateLegitStoreData(context, null, null, timeout);
        }

        internal IPartitionInfo CreatePartitionInfo(string stateConnectionString)
        {
            string str;
            int num;
            try
            {
                string[] strArray = stateConnectionString.Split(new char[] { '=' });
                if ((strArray.Length != 2) || (strArray[0] != "tcpip"))
                {
                    throw new ArgumentException("stateConnectionString");
                }
                strArray = strArray[1].Split(new char[] { ':' });
                if (strArray.Length != 2)
                {
                    throw new ArgumentException("stateConnectionString");
                }
                str = strArray[0];
                num = ushort.Parse(strArray[1], CultureInfo.InvariantCulture);
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] > '\x007f')
                    {
                        throw new ArgumentException("stateConnectionString");
                    }
                }
            }
            catch
            {
                if (s_usePartition)
                {
                    throw new HttpException(System.Web.SR.GetString("Error_parsing_state_server_partition_resolver_string", new object[] { s_configPartitionResolverType }));
                }
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_value_for_sessionstate_stateConnectionString", new object[] { s_configStateConnectionString }), s_configStateConnectionStringFileName, s_configStateConnectionStringLineNumber);
            }
            int hr = System.Web.UnsafeNativeMethods.SessionNDConnectToService(str);
            if (hr != 0)
            {
                throw CreateConnectionException(str, num, hr);
            }
            return new StateServerPartitionInfo(new ResourcePool(new TimeSpan(0, 0, 5), 0x7fffffff), str, num);
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            System.Web.UnsafeNativeMethods.SessionNDMakeRequestResults results;
            byte[] buffer;
            int num;
            SessionStateUtility.SerializeStoreData(this.CreateNewStoreData(context, timeout), 0, out buffer, out num, s_configCompressionEnabled);
            this.MakeRequest(System.Web.UnsafeNativeMethods.StateProtocolVerb.PUT, id, System.Web.UnsafeNativeMethods.StateProtocolExclusive.NONE, 1, timeout, 0, buffer, num, s_networkTimeout, out results);
        }

        public override void Dispose()
        {
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal unsafe SessionStateStoreData DoGet(HttpContext context, string id, System.Web.UnsafeNativeMethods.StateProtocolExclusive exclusiveAccess, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            SessionStateStoreData data = null;
            UnmanagedMemoryStream stream = null;
            System.Web.UnsafeNativeMethods.SessionNDMakeRequestResults results;
            locked = false;
            lockId = null;
            lockAge = TimeSpan.Zero;
            actionFlags = SessionStateActions.None;
            results.content = IntPtr.Zero;
            try
            {
                this.MakeRequest(System.Web.UnsafeNativeMethods.StateProtocolVerb.GET, id, exclusiveAccess, 0, 0, 0, null, 0, s_networkTimeout, out results);
                int httpStatus = results.httpStatus;
                if (httpStatus != 200)
                {
                    if (httpStatus != 0x1a7)
                    {
                        return data;
                    }
                }
                else
                {
                    int contentLength = results.contentLength;
                    if (contentLength > 0)
                    {
                        try
                        {
                            stream = new UnmanagedMemoryStream((byte*) results.content, (long) contentLength);
                            data = SessionStateUtility.DeserializeStoreData(context, stream, s_configCompressionEnabled);
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Close();
                            }
                        }
                        lockId = results.lockCookie;
                        actionFlags = (SessionStateActions) results.actionFlags;
                    }
                    return data;
                }
                if (0 <= results.lockAge)
                {
                    if (results.lockAge < 0x1e13380)
                    {
                        lockAge = new TimeSpan(0, 0, results.lockAge);
                    }
                    else
                    {
                        lockAge = TimeSpan.Zero;
                    }
                }
                else
                {
                    DateTime now = DateTime.Now;
                    if ((0L < results.lockDate) && (results.lockDate < now.Ticks))
                    {
                        lockAge = (TimeSpan) (now - new DateTime(results.lockDate));
                    }
                    else
                    {
                        lockAge = TimeSpan.Zero;
                    }
                }
                locked = true;
                lockId = results.lockCookie;
            }
            finally
            {
                if (results.content != IntPtr.Zero)
                {
                    System.Web.UnsafeNativeMethods.SessionNDFreeBody(new HandleRef(this, results.content));
                }
            }
            return data;
        }

        public override void EndRequest(HttpContext context)
        {
        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            return this.DoGet(context, id, System.Web.UnsafeNativeMethods.StateProtocolExclusive.NONE, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            return this.DoGet(context, id, System.Web.UnsafeNativeMethods.StateProtocolExclusive.ACQUIRE, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "State Server Session State Provider";
            }
            base.Initialize(name, config);
            if (!s_oneTimeInited)
            {
                s_lock.AcquireWriterLock();
                try
                {
                    if (!s_oneTimeInited)
                    {
                        this.OneTimeInit();
                    }
                }
                finally
                {
                    s_lock.ReleaseWriterLock();
                }
            }
            if (!s_usePartition)
            {
                this._partitionInfo = s_singlePartitionInfo;
            }
        }

        internal override void Initialize(string name, NameValueCollection config, IPartitionResolver partitionResolver)
        {
            this._partitionResolver = partitionResolver;
            this.Initialize(name, config);
        }

        public override void InitializeRequest(HttpContext context)
        {
            if (s_usePartition)
            {
                this._partitionInfo = null;
            }
        }

        private void MakeRequest(System.Web.UnsafeNativeMethods.StateProtocolVerb verb, string id, System.Web.UnsafeNativeMethods.StateProtocolExclusive exclusiveAccess, int extraFlags, int timeout, int lockCookie, byte[] buf, int cb, int networkTimeout, out System.Web.UnsafeNativeMethods.SessionNDMakeRequestResults results)
        {
            int num;
            OutOfProcConnection o = null;
            bool checkVersion = false;
            SessionIDManager.CheckIdLength(id, true);
            if (this._partitionInfo == null)
            {
                this._partitionInfo = (StateServerPartitionInfo) s_partitionManager.GetPartition(this._partitionResolver, id);
                if (this._partitionInfo == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Bad_partition_resolver_connection_string", new object[] { "PartitionManager" }));
                }
            }
            try
            {
                HandleRef ref2;
                o = (OutOfProcConnection) this._partitionInfo.RetrieveResource();
                if (o != null)
                {
                    ref2 = new HandleRef(this, o._socketHandle.Handle);
                }
                else
                {
                    ref2 = new HandleRef(this, INVALID_SOCKET);
                }
                if (this._partitionInfo.StateServerVersion == -1)
                {
                    checkVersion = true;
                }
                string uri = HttpUtility.UrlEncode(s_uribase + id);
                num = System.Web.UnsafeNativeMethods.SessionNDMakeRequest(ref2, this._partitionInfo.Server, this._partitionInfo.Port, networkTimeout, verb, uri, exclusiveAccess, extraFlags, timeout, lockCookie, buf, cb, checkVersion, out results);
                if (o != null)
                {
                    if (results.socket == INVALID_SOCKET)
                    {
                        o.Detach();
                        o = null;
                    }
                    else if (results.socket != ref2.Handle)
                    {
                        o._socketHandle = new HandleRef(this, results.socket);
                    }
                }
                else if (results.socket != INVALID_SOCKET)
                {
                    o = new OutOfProcConnection(results.socket);
                }
                if (o != null)
                {
                    this._partitionInfo.StoreResource(o);
                }
            }
            catch
            {
                if (o != null)
                {
                    o.Dispose();
                }
                throw;
            }
            if (num == 0)
            {
                if (results.httpStatus == 400)
                {
                    if (s_usePartition)
                    {
                        throw new HttpException(System.Web.SR.GetString("Bad_state_server_request_partition_resolver", new object[] { s_configPartitionResolverType, this._partitionInfo.Server, this._partitionInfo.Port.ToString(CultureInfo.InvariantCulture) }));
                    }
                    throw new HttpException(System.Web.SR.GetString("Bad_state_server_request"));
                }
                if (checkVersion)
                {
                    this._partitionInfo.StateServerVersion = results.stateServerMajVer;
                    if (this._partitionInfo.StateServerVersion < WHIDBEY_MAJOR_VERSION)
                    {
                        if (s_usePartition)
                        {
                            throw new HttpException(System.Web.SR.GetString("Need_v2_State_Server_partition_resolver", new object[] { s_configPartitionResolverType, this._partitionInfo.Server, this._partitionInfo.Port.ToString(CultureInfo.InvariantCulture) }));
                        }
                        throw new HttpException(System.Web.SR.GetString("Need_v2_State_Server"));
                    }
                }
            }
            else
            {
                HttpException exception = CreateConnectionException(this._partitionInfo.Server, this._partitionInfo.Port, num);
                string str2 = null;
                switch (results.lastPhase)
                {
                    case 0:
                        str2 = System.Web.SR.GetString("State_Server_detailed_error_phase0");
                        break;

                    case 1:
                        str2 = System.Web.SR.GetString("State_Server_detailed_error_phase1");
                        break;

                    case 2:
                        str2 = System.Web.SR.GetString("State_Server_detailed_error_phase2");
                        break;

                    case 3:
                        str2 = System.Web.SR.GetString("State_Server_detailed_error_phase3");
                        break;
                }
                WebBaseEvent.RaiseSystemEvent(System.Web.SR.GetString("State_Server_detailed_error", new object[] { str2, "0x" + num.ToString("X08", CultureInfo.InvariantCulture), cb.ToString(CultureInfo.InvariantCulture) }), this, 0xbc1, 0xc360, exception);
                throw exception;
            }
        }

        private void OnAppDomainUnload(object unusedObject, EventArgs unusedEventArgs)
        {
            Thread.GetDomain().DomainUnload -= s_onAppDomainUnload;
            if (this._partitionResolver == null)
            {
                if (s_singlePartitionInfo != null)
                {
                    s_singlePartitionInfo.Dispose();
                }
            }
            else if (s_partitionManager != null)
            {
                s_partitionManager.Dispose();
            }
        }

        private void OneTimeInit()
        {
            SessionStateSection sessionState = RuntimeConfig.GetAppConfig().SessionState;
            s_configPartitionResolverType = sessionState.PartitionResolverType;
            s_configStateConnectionString = sessionState.StateConnectionString;
            s_configStateConnectionStringFileName = sessionState.ElementInformation.Properties["stateConnectionString"].Source;
            s_configStateConnectionStringLineNumber = sessionState.ElementInformation.Properties["stateConnectionString"].LineNumber;
            s_configCompressionEnabled = sessionState.CompressionEnabled;
            if (this._partitionResolver == null)
            {
                string stateConnectionString = sessionState.StateConnectionString;
                SessionStateModule.ReadConnectionString(sessionState, ref stateConnectionString, "stateConnectionString");
                s_singlePartitionInfo = (StateServerPartitionInfo) this.CreatePartitionInfo(stateConnectionString);
            }
            else
            {
                s_usePartition = true;
                s_partitionManager = new PartitionManager(new System.Web.CreatePartitionInfo(this.CreatePartitionInfo));
            }
            s_networkTimeout = (int) sessionState.StateNetworkTimeout.TotalSeconds;
            string appDomainAppIdInternal = HttpRuntime.AppDomainAppIdInternal;
            string str3 = MachineKeySection.HashAndBase64EncodeString(appDomainAppIdInternal);
            if (appDomainAppIdInternal.StartsWith("/", StringComparison.Ordinal))
            {
                s_uribase = appDomainAppIdInternal + "(" + str3 + ")/";
            }
            else
            {
                s_uribase = "/" + appDomainAppIdInternal + "(" + str3 + ")/";
            }
            s_onAppDomainUnload = new EventHandler(this.OnAppDomainUnload);
            Thread.GetDomain().DomainUnload += s_onAppDomainUnload;
            s_oneTimeInited = true;
        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            System.Web.UnsafeNativeMethods.SessionNDMakeRequestResults results;
            int lockCookie = (int) lockId;
            this.MakeRequest(System.Web.UnsafeNativeMethods.StateProtocolVerb.GET, id, System.Web.UnsafeNativeMethods.StateProtocolExclusive.RELEASE, 0, 0, lockCookie, null, 0, s_networkTimeout, out results);
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            System.Web.UnsafeNativeMethods.SessionNDMakeRequestResults results;
            int lockCookie = (int) lockId;
            this.MakeRequest(System.Web.UnsafeNativeMethods.StateProtocolVerb.DELETE, id, System.Web.UnsafeNativeMethods.StateProtocolExclusive.NONE, 0, 0, lockCookie, null, 0, s_networkTimeout, out results);
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            System.Web.UnsafeNativeMethods.SessionNDMakeRequestResults results;
            this.MakeRequest(System.Web.UnsafeNativeMethods.StateProtocolVerb.HEAD, id, System.Web.UnsafeNativeMethods.StateProtocolExclusive.NONE, 0, 0, 0, null, 0, s_networkTimeout, out results);
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            System.Web.UnsafeNativeMethods.SessionNDMakeRequestResults results;
            byte[] buffer;
            int num;
            int num2;
            try
            {
                SessionStateUtility.SerializeStoreData(item, 0, out buffer, out num, s_configCompressionEnabled);
            }
            catch
            {
                if (!newItem)
                {
                    this.ReleaseItemExclusive(context, id, lockId);
                }
                throw;
            }
            if (lockId == null)
            {
                num2 = 0;
            }
            else
            {
                num2 = (int) lockId;
            }
            this.MakeRequest(System.Web.UnsafeNativeMethods.StateProtocolVerb.PUT, id, System.Web.UnsafeNativeMethods.StateProtocolExclusive.NONE, 0, item.Timeout, num2, buffer, num, s_networkTimeout, out results);
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        private class OutOfProcConnection : IDisposable
        {
            internal HandleRef _socketHandle;

            internal OutOfProcConnection(IntPtr socket)
            {
                this._socketHandle = new HandleRef(this, socket);
                PerfCounters.IncrementCounter(AppPerfCounter.SESSION_STATE_SERVER_CONNECTIONS);
            }

            internal void Detach()
            {
                this._socketHandle = new HandleRef(this, OutOfProcSessionStateStore.INVALID_SOCKET);
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool dummy)
            {
                if (this._socketHandle.Handle != OutOfProcSessionStateStore.INVALID_SOCKET)
                {
                    System.Web.UnsafeNativeMethods.SessionNDCloseConnection(this._socketHandle);
                    this._socketHandle = new HandleRef(this, OutOfProcSessionStateStore.INVALID_SOCKET);
                    PerfCounters.DecrementCounter(AppPerfCounter.SESSION_STATE_SERVER_CONNECTIONS);
                }
            }

            ~OutOfProcConnection()
            {
                this.Dispose(false);
            }
        }

        private class StateServerPartitionInfo : PartitionInfo
        {
            private int _port;
            private string _server;
            private int _stateServerVersion;

            internal StateServerPartitionInfo(ResourcePool rpool, string server, int port) : base(rpool)
            {
                this._server = server;
                this._port = port;
                this._stateServerVersion = -1;
            }

            internal int Port
            {
                get
                {
                    return this._port;
                }
            }

            internal string Server
            {
                get
                {
                    return this._server;
                }
            }

            internal int StateServerVersion
            {
                get
                {
                    return this._stateServerVersion;
                }
                set
                {
                    this._stateServerVersion = value;
                }
            }

            protected override string TracingPartitionString
            {
                get
                {
                    return (this.Server + ":" + this.Port);
                }
            }
        }
    }
}

