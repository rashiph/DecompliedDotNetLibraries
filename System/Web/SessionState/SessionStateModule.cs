namespace System.Web.SessionState
{
    using Microsoft.Win32;
    using System;
    using System.Configuration;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    public sealed class SessionStateModule : IHttpModule
    {
        private bool _acquireCalled;
        private ISessionIDManager _idManager;
        private bool _ignoreImpersonation;
        private readonly SessionOnEndTarget _onEndTarget = new SessionOnEndTarget();
        private IPartitionResolver _partitionResolver;
        private bool _releaseCalled;
        private SessionStateActions _rqActionFlags;
        private bool _rqAddedCookie;
        private HttpAsyncResult _rqAr;
        internal int _rqChangeImpersonationRefCount;
        private HttpContext _rqContext;
        private TimeSpan _rqExecutionTimeout;
        private ImpersonationContext _rqIctx;
        private string _rqId;
        private bool _rqIdNew;
        private int _rqInCallback;
        private bool _rqIsNewSession;
        private SessionStateStoreData _rqItem;
        private DateTime _rqLastPollCompleted;
        private object _rqLockId;
        private bool _rqReadonly;
        private ISessionStateItemCollection _rqSessionItems;
        private HttpSessionStateContainer _rqSessionState;
        private bool _rqSessionStateNotFound;
        private HttpStaticObjectsCollection _rqStaticObjects;
        private bool _rqSupportSessionIdReissue;
        private ImpersonationContext _rqTimerThreadImpersonationIctx;
        private SessionStateStoreProviderBase _store;
        private bool _supportSessionExpiry;
        private Timer _timer;
        private TimerCallback _timerCallback;
        private volatile int _timerId;
        private bool _usingAspnetSessionIdManager;
        private static readonly TimeSpan DEFAULT_DBG_EXECUTION_TIMEOUT = new TimeSpan(0, 0, 0x1c9c380);
        private static readonly TimeSpan LOCKED_ITEM_POLLING_DELTA = new TimeSpan(0x2625a0L);
        private static long LOCKED_ITEM_POLLING_INTERVAL = 500L;
        internal const int MAX_CACHE_BASED_TIMEOUT_MINUTES = 0x80520;
        internal const SessionStateMode MODE_DEFAULT = SessionStateMode.InProc;
        private static bool s_allowDelayedStateStoreItemCreation;
        private static bool s_allowInProcOptimization;
        private static bool s_canSkipEndRequestCall;
        internal static HttpCookieMode s_configCookieless;
        private static TimeSpan s_configExecutionTimeout;
        internal static SessionStateMode s_configMode;
        private static bool s_configRegenerateExpiredSessionId;
        private static HttpSessionStateContainer s_delayedSessionState = new HttpSessionStateContainer();
        private static ReadWriteSpinLock s_lock;
        private bool s_oneTimeInit;
        private static object s_PollIntervalRegLock = new object();
        private static bool s_PollIntervalRegLookedUp = false;
        private static bool s_sessionEverSet;
        private static int s_timeout;
        private static bool s_trustLevelInsufficient;
        private static bool s_useHostingIdentity;
        internal const string SQL_CONNECTION_STRING_DEFAULT = "data source=localhost;Integrated Security=SSPI";
        internal const string STATE_CONNECTION_STRING_DEFAULT = "tcpip=loopback:42424";
        internal const int TIMEOUT_DEFAULT = 20;

        public event EventHandler End
        {
            add
            {
                lock (this._onEndTarget)
                {
                    if ((this._store != null) && (this._onEndTarget.SessionEndEventHandlerCount == 0))
                    {
                        this._supportSessionExpiry = this._store.SetItemExpireCallback(new SessionStateItemExpireCallback(this._onEndTarget.RaiseSessionOnEnd));
                    }
                    this._onEndTarget.SessionEndEventHandlerCount++;
                }
            }
            remove
            {
                lock (this._onEndTarget)
                {
                    this._onEndTarget.SessionEndEventHandlerCount--;
                    if ((this._store != null) && (this._onEndTarget.SessionEndEventHandlerCount == 0))
                    {
                        this._store.SetItemExpireCallback(null);
                        this._supportSessionExpiry = false;
                    }
                }
            }
        }

        public event EventHandler Start;

        private IAsyncResult BeginAcquireState(object source, EventArgs e, AsyncCallback cb, object extraData)
        {
            IAsyncResult result;
            bool sessionStateItem = true;
            bool flag3 = false;
            this._acquireCalled = true;
            this._releaseCalled = false;
            this.ResetPerRequestFields();
            this._rqContext = ((HttpApplication) source).Context;
            this._rqAr = new HttpAsyncResult(cb, extraData);
            this.ChangeImpersonation(this._rqContext, false);
            try
            {
                if (EtwTrace.IsTraceEnabled(4, 8))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSION_DATA_BEGIN, this._rqContext.WorkerRequest);
                }
                this._store.InitializeRequest(this._rqContext);
                bool requiresSessionState = this._rqContext.RequiresSessionState;
                if (this._idManager.InitializeRequest(this._rqContext, false, out this._rqSupportSessionIdReissue))
                {
                    this._rqAr.Complete(true, null, null);
                    if (EtwTrace.IsTraceEnabled(4, 8))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSION_DATA_END, this._rqContext.WorkerRequest);
                    }
                    return this._rqAr;
                }
                if ((s_allowInProcOptimization && !s_sessionEverSet) && (!requiresSessionState || !((SessionIDManager) this._idManager).UseCookieless(this._rqContext)))
                {
                    flag3 = true;
                }
                else
                {
                    this._rqId = this._idManager.GetSessionID(this._rqContext);
                }
                if (!requiresSessionState)
                {
                    if (this._rqId != null)
                    {
                        this._store.ResetItemTimeout(this._rqContext, this._rqId);
                    }
                    this._rqAr.Complete(true, null, null);
                    if (EtwTrace.IsTraceEnabled(4, 8))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSION_DATA_END, this._rqContext.WorkerRequest);
                    }
                    return this._rqAr;
                }
                this._rqExecutionTimeout = this._rqContext.Timeout;
                if (this._rqExecutionTimeout == DEFAULT_DBG_EXECUTION_TIMEOUT)
                {
                    this._rqExecutionTimeout = s_configExecutionTimeout;
                }
                this._rqReadonly = this._rqContext.ReadOnlySessionState;
                if (this._rqId != null)
                {
                    sessionStateItem = this.GetSessionStateItem();
                }
                else if (!flag3)
                {
                    bool flag4 = this.CreateSessionId();
                    this._rqIdNew = true;
                    if (flag4)
                    {
                        if (s_configRegenerateExpiredSessionId)
                        {
                            this.CreateUninitializedSessionState();
                        }
                        this._rqAr.Complete(true, null, null);
                        if (EtwTrace.IsTraceEnabled(4, 8))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSION_DATA_END, this._rqContext.WorkerRequest);
                        }
                        return this._rqAr;
                    }
                }
                if (sessionStateItem)
                {
                    this.CompleteAcquireState();
                    this._rqAr.Complete(true, null, null);
                }
                result = this._rqAr;
            }
            finally
            {
                this.RestoreImpersonation();
            }
            return result;
        }

        private void ChangeImpersonation(HttpContext context, bool timerThread)
        {
            this._rqChangeImpersonationRefCount++;
            if (!this._ignoreImpersonation && (((s_configMode != SessionStateMode.SQLServer) || !((SqlSessionStateStore) this._store).KnowForSureNotUsingIntegratedSecurity) || !this._usingAspnetSessionIdManager))
            {
                if (s_useHostingIdentity)
                {
                    if (this._rqIctx == null)
                    {
                        this._rqIctx = new ApplicationImpersonationContext();
                    }
                }
                else if (timerThread)
                {
                    this._rqTimerThreadImpersonationIctx = new ClientImpersonationContext(context, false);
                }
            }
        }

        private static bool CheckTrustLevel(SessionStateSection config)
        {
            switch (config.Mode)
            {
                case SessionStateMode.StateServer:
                case SessionStateMode.SQLServer:
                    return HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium);
            }
            return true;
        }

        private void CompleteAcquireState()
        {
            bool flag = false;
            try
            {
                if (this._rqItem != null)
                {
                    this._rqSessionStateNotFound = false;
                    if ((this._rqActionFlags & SessionStateActions.InitializeItem) != SessionStateActions.None)
                    {
                        this._rqIsNewSession = true;
                    }
                    else
                    {
                        this._rqIsNewSession = false;
                    }
                }
                else
                {
                    this._rqIsNewSession = true;
                    this._rqSessionStateNotFound = true;
                    if (s_allowDelayedStateStoreItemCreation)
                    {
                        flag = true;
                    }
                    if ((!this._rqIdNew && s_configRegenerateExpiredSessionId) && (this._rqSupportSessionIdReissue && this.CreateSessionId()))
                    {
                        this.CreateUninitializedSessionState();
                        return;
                    }
                }
                if (flag)
                {
                    SessionStateUtility.AddDelayedHttpSessionStateToContext(this._rqContext, this);
                    this._rqSessionState = s_delayedSessionState;
                }
                else
                {
                    this.InitStateStoreItem(true);
                }
                if (this._rqIsNewSession)
                {
                    this.OnStart(EventArgs.Empty);
                }
            }
            finally
            {
                if (EtwTrace.IsTraceEnabled(4, 8))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSION_DATA_END, this._rqContext.WorkerRequest);
                }
            }
        }

        internal bool CreateSessionId()
        {
            bool flag;
            this._rqId = this._idManager.CreateSessionID(this._rqContext);
            this._idManager.SaveSessionID(this._rqContext, this._rqId, out flag, out this._rqAddedCookie);
            return flag;
        }

        private void CreateUninitializedSessionState()
        {
            this._store.CreateUninitializedItem(this._rqContext, this._rqId, s_timeout);
        }

        internal string DelayedGetSessionId()
        {
            this.ChangeImpersonation(this._rqContext, false);
            try
            {
                this._rqId = this._idManager.GetSessionID(this._rqContext);
                if (this._rqId == null)
                {
                    this.CreateSessionId();
                }
            }
            finally
            {
                this.RestoreImpersonation();
            }
            return this._rqId;
        }

        public void Dispose()
        {
            if (this._timer != null)
            {
                this._timer.Dispose();
            }
            if (this._store != null)
            {
                this._store.Dispose();
            }
        }

        private void EndAcquireState(IAsyncResult ar)
        {
            ((HttpAsyncResult) ar).End();
        }

        internal void EnsureReleaseState(HttpApplication app)
        {
            if ((HttpRuntime.UseIntegratedPipeline && this._acquireCalled) && !this._releaseCalled)
            {
                try
                {
                    this.OnReleaseState(app, null);
                }
                catch
                {
                }
            }
        }

        private bool GetSessionStateItem()
        {
            bool flag2;
            TimeSpan span;
            bool flag = true;
            if (this._rqReadonly)
            {
                this._rqItem = this._store.GetItem(this._rqContext, this._rqId, out flag2, out span, out this._rqLockId, out this._rqActionFlags);
            }
            else
            {
                this._rqItem = this._store.GetItemExclusive(this._rqContext, this._rqId, out flag2, out span, out this._rqLockId, out this._rqActionFlags);
                if ((((this._rqItem == null) && !flag2) && (this._rqId != null)) && ((s_configCookieless != HttpCookieMode.UseUri) || !s_configRegenerateExpiredSessionId))
                {
                    this.CreateUninitializedSessionState();
                    this._rqItem = this._store.GetItemExclusive(this._rqContext, this._rqId, out flag2, out span, out this._rqLockId, out this._rqActionFlags);
                }
            }
            if ((this._rqItem == null) && flag2)
            {
                if (span >= this._rqExecutionTimeout)
                {
                    this._store.ReleaseItemExclusive(this._rqContext, this._rqId, this._rqLockId);
                }
                flag = false;
                this.PollLockedSession();
            }
            return flag;
        }

        public void Init(HttpApplication app)
        {
            bool flag = false;
            SessionStateSection sessionState = RuntimeConfig.GetAppConfig().SessionState;
            if (!this.s_oneTimeInit)
            {
                s_lock.AcquireWriterLock();
                try
                {
                    if (!this.s_oneTimeInit)
                    {
                        this.InitModuleFromConfig(app, sessionState);
                        flag = true;
                        if (!CheckTrustLevel(sessionState))
                        {
                            s_trustLevelInsufficient = true;
                        }
                        s_timeout = (int) sessionState.Timeout.TotalMinutes;
                        s_useHostingIdentity = sessionState.UseHostingIdentity;
                        if ((sessionState.Mode == SessionStateMode.InProc) && this._usingAspnetSessionIdManager)
                        {
                            s_allowInProcOptimization = true;
                        }
                        if (((sessionState.Mode != SessionStateMode.Custom) && (sessionState.Mode != SessionStateMode.Off)) && !sessionState.RegenerateExpiredSessionId)
                        {
                            s_allowDelayedStateStoreItemCreation = true;
                        }
                        s_configExecutionTimeout = RuntimeConfig.GetConfig().HttpRuntime.ExecutionTimeout;
                        s_configRegenerateExpiredSessionId = sessionState.RegenerateExpiredSessionId;
                        s_configCookieless = sessionState.Cookieless;
                        s_configMode = sessionState.Mode;
                        this.s_oneTimeInit = true;
                    }
                }
                finally
                {
                    s_lock.ReleaseWriterLock();
                }
            }
            if (!flag)
            {
                this.InitModuleFromConfig(app, sessionState);
            }
            if (s_trustLevelInsufficient)
            {
                throw new HttpException(System.Web.SR.GetString("Session_state_need_higher_trust"));
            }
        }

        private SessionStateStoreProviderBase InitCustomStore(SessionStateSection config)
        {
            string customProvider = config.CustomProvider;
            if (string.IsNullOrEmpty(customProvider))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_session_custom_provider", new object[] { customProvider }), config.ElementInformation.Properties["customProvider"].Source, config.ElementInformation.Properties["customProvider"].LineNumber);
            }
            ProviderSettings settings = config.Providers[customProvider];
            if (settings == null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Missing_session_custom_provider", new object[] { customProvider }), config.ElementInformation.Properties["customProvider"].Source, config.ElementInformation.Properties["customProvider"].LineNumber);
            }
            return this.SecureInstantiateProvider(settings);
        }

        private void InitModuleFromConfig(HttpApplication app, SessionStateSection config)
        {
            if (config.Mode != SessionStateMode.Off)
            {
                app.AddOnAcquireRequestStateAsync(new BeginEventHandler(this.BeginAcquireState), new EndEventHandler(this.EndAcquireState));
                app.ReleaseRequestState += new EventHandler(this.OnReleaseState);
                app.EndRequest += new EventHandler(this.OnEndRequest);
                this._partitionResolver = this.InitPartitionResolver(config);
                switch (config.Mode)
                {
                    case SessionStateMode.InProc:
                        if (HttpRuntime.UseIntegratedPipeline)
                        {
                            s_canSkipEndRequestCall = true;
                        }
                        this._store = new InProcSessionStateStore();
                        this._store.Initialize(null, null);
                        break;

                    case SessionStateMode.StateServer:
                        if (HttpRuntime.UseIntegratedPipeline)
                        {
                            s_canSkipEndRequestCall = true;
                        }
                        this._store = new OutOfProcSessionStateStore();
                        ((OutOfProcSessionStateStore) this._store).Initialize(null, null, this._partitionResolver);
                        break;

                    case SessionStateMode.SQLServer:
                        this._store = new SqlSessionStateStore();
                        ((SqlSessionStateStore) this._store).Initialize(null, null, this._partitionResolver);
                        break;

                    case SessionStateMode.Custom:
                        this._store = this.InitCustomStore(config);
                        break;
                }
                this._idManager = this.InitSessionIDManager(config);
                if (((config.Mode == SessionStateMode.InProc) || (config.Mode == SessionStateMode.StateServer)) && this._usingAspnetSessionIdManager)
                {
                    this._ignoreImpersonation = true;
                }
            }
        }

        private IPartitionResolver InitPartitionResolver(SessionStateSection config)
        {
            string partitionResolverType = config.PartitionResolverType;
            if (string.IsNullOrEmpty(partitionResolverType))
            {
                return null;
            }
            if ((config.Mode != SessionStateMode.StateServer) && (config.Mode != SessionStateMode.SQLServer))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Cant_use_partition_resolve"), config.ElementInformation.Properties["partitionResolverType"].Source, config.ElementInformation.Properties["partitionResolverType"].LineNumber);
            }
            Type type = ConfigUtil.GetType(partitionResolverType, "partitionResolverType", config);
            ConfigUtil.CheckAssignableType(typeof(IPartitionResolver), type, config, "partitionResolverType");
            IPartitionResolver resolver = (IPartitionResolver) HttpRuntime.CreatePublicInstance(type);
            resolver.Initialize();
            return resolver;
        }

        private ISessionIDManager InitSessionIDManager(SessionStateSection config)
        {
            ISessionIDManager manager;
            string sessionIDManagerType = config.SessionIDManagerType;
            if (string.IsNullOrEmpty(sessionIDManagerType))
            {
                manager = new SessionIDManager();
                this._usingAspnetSessionIdManager = true;
            }
            else
            {
                Type type = ConfigUtil.GetType(sessionIDManagerType, "sessionIDManagerType", config);
                ConfigUtil.CheckAssignableType(typeof(ISessionIDManager), type, config, "sessionIDManagerType");
                manager = (ISessionIDManager) HttpRuntime.CreatePublicInstance(type);
            }
            manager.Initialize();
            return manager;
        }

        internal void InitStateStoreItem(bool addToContext)
        {
            this.ChangeImpersonation(this._rqContext, false);
            try
            {
                if (this._rqItem == null)
                {
                    this._rqItem = this._store.CreateNewStoreData(this._rqContext, s_timeout);
                }
                this._rqSessionItems = this._rqItem.Items;
                if (this._rqSessionItems == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Null_value_for_SessionStateItemCollection"));
                }
                this._rqStaticObjects = this._rqItem.StaticObjects;
                this._rqSessionItems.Dirty = false;
                this._rqSessionState = new HttpSessionStateContainer(this, this._rqId, this._rqSessionItems, this._rqStaticObjects, this._rqItem.Timeout, this._rqIsNewSession, s_configCookieless, s_configMode, this._rqReadonly);
                if (addToContext)
                {
                    SessionStateUtility.AddHttpSessionStateToContext(this._rqContext, this._rqSessionState);
                }
            }
            finally
            {
                this.RestoreImpersonation();
            }
        }

        [RegistryPermission(SecurityAction.Assert, Unrestricted=true)]
        private static void LookUpRegForPollInterval()
        {
            lock (s_PollIntervalRegLock)
            {
                if (!s_PollIntervalRegLookedUp)
                {
                    try
                    {
                        object obj2 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\ASP.NET", "SessionStateLockedItemPollInterval", 0);
                        if (((obj2 != null) && ((obj2 is int) || (obj2 is uint))) && (((int) obj2) > 0))
                        {
                            LOCKED_ITEM_POLLING_INTERVAL = (int) obj2;
                        }
                        s_PollIntervalRegLookedUp = true;
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void OnEndRequest(object source, EventArgs eventArgs)
        {
            HttpApplication application = (HttpApplication) source;
            HttpContext context = application.Context;
            if (context.RequiresSessionState)
            {
                this.ChangeImpersonation(context, false);
                try
                {
                    if (!this._releaseCalled)
                    {
                        if (this._acquireCalled)
                        {
                            this.OnReleaseState(source, eventArgs);
                        }
                        else
                        {
                            bool flag;
                            if (this._rqContext == null)
                            {
                                this._rqContext = context;
                            }
                            this._store.InitializeRequest(this._rqContext);
                            this._idManager.InitializeRequest(this._rqContext, true, out flag);
                            string sessionID = this._idManager.GetSessionID(context);
                            if (sessionID != null)
                            {
                                this._store.ResetItemTimeout(context, sessionID);
                            }
                        }
                    }
                    this._store.EndRequest(this._rqContext);
                }
                finally
                {
                    this._acquireCalled = false;
                    this._releaseCalled = false;
                    this.RestoreImpersonation();
                    this.ResetPerRequestFields();
                }
            }
        }

        private void OnReleaseState(object source, EventArgs eventArgs)
        {
            bool flag = false;
            this._releaseCalled = true;
            HttpApplication application = (HttpApplication) source;
            HttpContext context = application.Context;
            this.ChangeImpersonation(context, false);
            try
            {
                if (this._rqSessionState != null)
                {
                    bool delayed = this._rqSessionState == s_delayedSessionState;
                    SessionStateUtility.RemoveHttpSessionStateFromContext(this._rqContext, delayed);
                    if (((!this._rqSessionStateNotFound || (this._sessionStartEventHandler != null)) || (!delayed && this._rqSessionItems.Dirty)) || ((!delayed && (this._rqStaticObjects != null)) && !this._rqStaticObjects.NeverAccessed))
                    {
                        if (this._rqSessionState.IsAbandoned)
                        {
                            if (this._rqSessionStateNotFound)
                            {
                                if (this._supportSessionExpiry)
                                {
                                    if (delayed)
                                    {
                                        this.InitStateStoreItem(false);
                                    }
                                    this._onEndTarget.RaiseSessionOnEnd(this.ReleaseStateGetSessionID(), this._rqItem);
                                }
                            }
                            else
                            {
                                this._store.RemoveItem(this._rqContext, this.ReleaseStateGetSessionID(), this._rqLockId, this._rqItem);
                            }
                        }
                        else if (!this._rqReadonly || ((this._rqReadonly && this._rqIsNewSession) && ((this._sessionStartEventHandler != null) && !this.SessionIDManagerUseCookieless)))
                        {
                            if ((context.Error == null) && (((this._rqSessionStateNotFound || this._rqSessionItems.Dirty) || ((this._rqStaticObjects != null) && !this._rqStaticObjects.NeverAccessed)) || (this._rqItem.Timeout != this._rqSessionState.Timeout)))
                            {
                                if (delayed)
                                {
                                    this.InitStateStoreItem(false);
                                }
                                if (this._rqItem.Timeout != this._rqSessionState.Timeout)
                                {
                                    this._rqItem.Timeout = this._rqSessionState.Timeout;
                                }
                                s_sessionEverSet = true;
                                flag = true;
                                this._store.SetAndReleaseItemExclusive(this._rqContext, this.ReleaseStateGetSessionID(), this._rqItem, this._rqLockId, this._rqSessionStateNotFound);
                            }
                            else if (!this._rqSessionStateNotFound)
                            {
                                this._store.ReleaseItemExclusive(this._rqContext, this.ReleaseStateGetSessionID(), this._rqLockId);
                            }
                        }
                    }
                }
                if ((this._rqAddedCookie && !flag) && context.Response.IsBuffered())
                {
                    this._idManager.RemoveSessionID(this._rqContext);
                }
            }
            finally
            {
                this.RestoreImpersonation();
            }
            bool requiresSessionState = context.RequiresSessionState;
            if ((HttpRuntime.UseIntegratedPipeline && (context.NotificationContext.CurrentNotification == RequestNotification.ReleaseRequestState)) && (s_canSkipEndRequestCall || !requiresSessionState))
            {
                context.DisableNotifications(RequestNotification.EndRequest, 0);
                this._acquireCalled = false;
                this._releaseCalled = false;
                this.ResetPerRequestFields();
            }
        }

        private void OnStart(EventArgs e)
        {
            this.RaiseOnStart(e);
        }

        private void PollLockedSession()
        {
            if (this._timerCallback == null)
            {
                this._timerCallback = new TimerCallback(this.PollLockedSessionCallback);
            }
            if (this._timer == null)
            {
                this._timerId++;
                if (!s_PollIntervalRegLookedUp)
                {
                    LookUpRegForPollInterval();
                }
                this._timer = new Timer(this._timerCallback, (int) this._timerId, LOCKED_ITEM_POLLING_INTERVAL, LOCKED_ITEM_POLLING_INTERVAL);
            }
        }

        private void PollLockedSessionCallback(object state)
        {
            bool sessionStateItem = false;
            Exception error = null;
            if (Interlocked.CompareExchange(ref this._rqInCallback, 1, 0) == 0)
            {
                try
                {
                    int num = (int) state;
                    if ((num == this._timerId) && ((DateTime.UtcNow - this._rqLastPollCompleted) >= LOCKED_ITEM_POLLING_DELTA))
                    {
                        this.ChangeImpersonation(this._rqContext, true);
                        try
                        {
                            sessionStateItem = this.GetSessionStateItem();
                            this._rqLastPollCompleted = DateTime.UtcNow;
                            if (sessionStateItem)
                            {
                                this.ResetPollTimer();
                                this.CompleteAcquireState();
                            }
                        }
                        finally
                        {
                            this.RestoreImpersonation();
                        }
                    }
                }
                catch (Exception exception2)
                {
                    this.ResetPollTimer();
                    error = exception2;
                }
                finally
                {
                    Interlocked.Exchange(ref this._rqInCallback, 0);
                }
                if (sessionStateItem || (error != null))
                {
                    this._rqAr.Complete(false, null, error);
                }
            }
        }

        private void RaiseOnStart(EventArgs e)
        {
            if (this._sessionStartEventHandler != null)
            {
                if (HttpRuntime.ApartmentThreading || this._rqContext.InAspCompatMode)
                {
                    AspCompatApplicationStep.RaiseAspCompatEvent(this._rqContext, this._rqContext.ApplicationInstance, null, this._sessionStartEventHandler, this, e);
                }
                else
                {
                    if (HttpContext.Current == null)
                    {
                        DisposableHttpContextWrapper.SwitchContext(this._rqContext);
                    }
                    this._sessionStartEventHandler(this, e);
                }
            }
        }

        internal static void ReadConnectionString(SessionStateSection config, ref string cntString, string propName)
        {
            ConfigsHelper.GetRegistryStringAttribute(ref cntString, config, propName);
            System.Web.Configuration.HandlerBase.CheckAndReadConnectionString(ref cntString, true);
        }

        private string ReleaseStateGetSessionID()
        {
            if (this._rqId == null)
            {
                this.DelayedGetSessionId();
            }
            return this._rqId;
        }

        private void ResetPerRequestFields()
        {
            this._rqSessionState = null;
            this._rqId = null;
            this._rqSessionItems = null;
            this._rqStaticObjects = null;
            this._rqIsNewSession = false;
            this._rqSessionStateNotFound = true;
            this._rqReadonly = false;
            this._rqItem = null;
            this._rqContext = null;
            this._rqAr = null;
            this._rqLockId = null;
            this._rqInCallback = 0;
            this._rqLastPollCompleted = DateTime.MinValue;
            this._rqExecutionTimeout = TimeSpan.Zero;
            this._rqAddedCookie = false;
            this._rqIdNew = false;
            this._rqActionFlags = SessionStateActions.None;
            this._rqIctx = null;
            this._rqChangeImpersonationRefCount = 0;
            this._rqTimerThreadImpersonationIctx = null;
            this._rqSupportSessionIdReissue = false;
        }

        private void ResetPollTimer()
        {
            this._timerId++;
            if (this._timer != null)
            {
                this._timer.Dispose();
                this._timer = null;
            }
        }

        private void RestoreImpersonation()
        {
            this._rqChangeImpersonationRefCount--;
            if (this._rqChangeImpersonationRefCount == 0)
            {
                if (this._rqIctx != null)
                {
                    this._rqIctx.Undo();
                    this._rqIctx = null;
                }
                if (this._rqTimerThreadImpersonationIctx != null)
                {
                    this._rqTimerThreadImpersonationIctx.Undo();
                    this._rqTimerThreadImpersonationIctx = null;
                }
            }
        }

        [AspNetHostingPermission(SecurityAction.Assert, Level=AspNetHostingPermissionLevel.Low)]
        private SessionStateStoreProviderBase SecureInstantiateProvider(ProviderSettings settings)
        {
            return (SessionStateStoreProviderBase) ProvidersHelper.InstantiateProvider(settings, typeof(SessionStateStoreProviderBase));
        }

        internal bool SessionIDManagerUseCookieless
        {
            get
            {
                if (!this._usingAspnetSessionIdManager)
                {
                    return (s_configCookieless == HttpCookieMode.UseUri);
                }
                return ((SessionIDManager) this._idManager).UseCookieless(this._rqContext);
            }
        }
    }
}

