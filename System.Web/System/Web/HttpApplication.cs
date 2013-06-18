namespace System.Web
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Configuration.Common;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Web.Util;

    [ToolboxItem(false)]
    public class HttpApplication : IHttpAsyncHandler, IHttpHandler, IComponent, IDisposable
    {
        private EventArgs _appEvent;
        private bool _appLevelAutoCulture;
        private bool _appLevelAutoUICulture;
        private CultureInfo _appLevelCulture;
        private CultureInfo _appLevelUICulture;
        private RequestNotification _appPostNotifications;
        private RequestNotification _appRequestNotifications;
        private HttpAsyncResult _ar;
        private AsyncAppEventHandlersTable _asyncEvents;
        private HttpContext _context;
        private string _currentModuleCollectionKey = "global.asax";
        private byte[] _entityBuffer;
        private EventHandlerList _events;
        private Hashtable _handlerFactories = new Hashtable();
        private ArrayList _handlerRecycleList;
        private bool _hideRequestResponse;
        private HttpContext _initContext;
        private bool _initInternalCompleted;
        private static bool _initSpecialCompleted;
        private Exception _lastError;
        private HttpModuleCollection _moduleCollection;
        private static List<ModuleConfigurationInfo> _moduleConfigInfo;
        private PipelineModuleStepContainer[] _moduleContainers;
        private static Hashtable _moduleIndexMap = new Hashtable();
        private Dictionary<string, RequestNotification> _pipelineEventMasks;
        private WaitCallback _resumeStepsWaitCallback;
        private CultureInfo _savedAppLevelCulture;
        private CultureInfo _savedAppLevelUICulture;
        private HttpSessionState _session;
        private ISite _site;
        private HttpApplicationState _state;
        private StepManager _stepManager;
        private bool _timeoutManagerInitialized;
        internal static readonly string AutoCulture = "auto";
        private static readonly object EventAcquireRequestState = new object();
        private static readonly object EventAuthenticateRequest = new object();
        private static readonly object EventAuthorizeRequest = new object();
        private static readonly object EventBeginRequest = new object();
        private static readonly object EventDefaultAuthentication = new object();
        private static readonly object EventDisposed = new object();
        private static readonly object EventEndRequest = new object();
        private static readonly object EventErrorRecorded = new object();
        private static readonly object EventLogRequest = new object();
        private static readonly object EventMapRequestHandler = new object();
        private static readonly object EventPostAcquireRequestState = new object();
        private static readonly object EventPostAuthenticateRequest = new object();
        private static readonly object EventPostAuthorizeRequest = new object();
        private static readonly object EventPostLogRequest = new object();
        private static readonly object EventPostMapRequestHandler = new object();
        private static readonly object EventPostReleaseRequestState = new object();
        private static readonly object EventPostRequestHandlerExecute = new object();
        private static readonly object EventPostResolveRequestCache = new object();
        private static readonly object EventPostUpdateRequestCache = new object();
        private static readonly object EventPreRequestHandlerExecute = new object();
        private static readonly object EventPreSendRequestContent = new object();
        private static readonly object EventPreSendRequestHeaders = new object();
        private static readonly object EventReleaseRequestState = new object();
        private static readonly object EventResolveRequestCache = new object();
        private static readonly object EventUpdateRequestCache = new object();
        internal const string IMPLICIT_FILTER_MODULE = "AspNetFilterModule";
        internal const string IMPLICIT_HANDLER = "ManagedPipelineHandler";
        internal const string MANAGED_PRECONDITION = "managedHandler";

        public event EventHandler AcquireRequestState
        {
            add
            {
                this.AddSyncEventHookup(EventAcquireRequestState, value, RequestNotification.AcquireRequestState);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventAcquireRequestState, value, RequestNotification.AcquireRequestState);
            }
        }

        public event EventHandler AuthenticateRequest
        {
            add
            {
                this.AddSyncEventHookup(EventAuthenticateRequest, value, RequestNotification.AuthenticateRequest);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventAuthenticateRequest, value, RequestNotification.AuthenticateRequest);
            }
        }

        public event EventHandler AuthorizeRequest
        {
            add
            {
                this.AddSyncEventHookup(EventAuthorizeRequest, value, RequestNotification.AuthorizeRequest);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventAuthorizeRequest, value, RequestNotification.AuthorizeRequest);
            }
        }

        public event EventHandler BeginRequest
        {
            add
            {
                this.AddSyncEventHookup(EventBeginRequest, value, RequestNotification.BeginRequest);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventBeginRequest, value, RequestNotification.BeginRequest);
            }
        }

        internal event EventHandler DefaultAuthentication
        {
            add
            {
                this.AddSyncEventHookup(EventDefaultAuthentication, value, RequestNotification.AuthenticateRequest);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventDefaultAuthentication, value, RequestNotification.AuthenticateRequest);
            }
        }

        public event EventHandler Disposed
        {
            add
            {
                this.Events.AddHandler(EventDisposed, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventDisposed, value);
            }
        }

        public event EventHandler EndRequest
        {
            add
            {
                this.AddSyncEventHookup(EventEndRequest, value, RequestNotification.EndRequest);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventEndRequest, value, RequestNotification.EndRequest);
            }
        }

        public event EventHandler Error
        {
            add
            {
                this.Events.AddHandler(EventErrorRecorded, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventErrorRecorded, value);
            }
        }

        public event EventHandler LogRequest
        {
            add
            {
                if (!HttpRuntime.UseIntegratedPipeline)
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
                }
                this.AddSyncEventHookup(EventLogRequest, value, RequestNotification.LogRequest);
            }
            remove
            {
                if (!HttpRuntime.UseIntegratedPipeline)
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
                }
                this.RemoveSyncEventHookup(EventLogRequest, value, RequestNotification.LogRequest);
            }
        }

        public event EventHandler MapRequestHandler
        {
            add
            {
                if (!HttpRuntime.UseIntegratedPipeline)
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
                }
                this.AddSyncEventHookup(EventMapRequestHandler, value, RequestNotification.MapRequestHandler);
            }
            remove
            {
                if (!HttpRuntime.UseIntegratedPipeline)
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
                }
                this.RemoveSyncEventHookup(EventMapRequestHandler, value, RequestNotification.MapRequestHandler);
            }
        }

        public event EventHandler PostAcquireRequestState
        {
            add
            {
                this.AddSyncEventHookup(EventPostAcquireRequestState, value, RequestNotification.AcquireRequestState, true);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventPostAcquireRequestState, value, RequestNotification.AcquireRequestState, true);
            }
        }

        public event EventHandler PostAuthenticateRequest
        {
            add
            {
                this.AddSyncEventHookup(EventPostAuthenticateRequest, value, RequestNotification.AuthenticateRequest, true);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventPostAuthenticateRequest, value, RequestNotification.AuthenticateRequest, true);
            }
        }

        public event EventHandler PostAuthorizeRequest
        {
            add
            {
                this.AddSyncEventHookup(EventPostAuthorizeRequest, value, RequestNotification.AuthorizeRequest, true);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventPostAuthorizeRequest, value, RequestNotification.AuthorizeRequest, true);
            }
        }

        public event EventHandler PostLogRequest
        {
            add
            {
                if (!HttpRuntime.UseIntegratedPipeline)
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
                }
                this.AddSyncEventHookup(EventPostLogRequest, value, RequestNotification.LogRequest, true);
            }
            remove
            {
                if (!HttpRuntime.UseIntegratedPipeline)
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
                }
                this.RemoveSyncEventHookup(EventPostLogRequest, value, RequestNotification.LogRequest, true);
            }
        }

        public event EventHandler PostMapRequestHandler
        {
            add
            {
                this.AddSyncEventHookup(EventPostMapRequestHandler, value, RequestNotification.MapRequestHandler, true);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventPostMapRequestHandler, value, RequestNotification.MapRequestHandler);
            }
        }

        public event EventHandler PostReleaseRequestState
        {
            add
            {
                this.AddSyncEventHookup(EventPostReleaseRequestState, value, RequestNotification.ReleaseRequestState, true);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventPostReleaseRequestState, value, RequestNotification.ReleaseRequestState, true);
            }
        }

        public event EventHandler PostRequestHandlerExecute
        {
            add
            {
                this.AddSyncEventHookup(EventPostRequestHandlerExecute, value, RequestNotification.ExecuteRequestHandler, true);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventPostRequestHandlerExecute, value, RequestNotification.ExecuteRequestHandler, true);
            }
        }

        public event EventHandler PostResolveRequestCache
        {
            add
            {
                this.AddSyncEventHookup(EventPostResolveRequestCache, value, RequestNotification.ResolveRequestCache, true);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventPostResolveRequestCache, value, RequestNotification.ResolveRequestCache, true);
            }
        }

        public event EventHandler PostUpdateRequestCache
        {
            add
            {
                this.AddSyncEventHookup(EventPostUpdateRequestCache, value, RequestNotification.UpdateRequestCache, true);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventPostUpdateRequestCache, value, RequestNotification.UpdateRequestCache, true);
            }
        }

        public event EventHandler PreRequestHandlerExecute
        {
            add
            {
                this.AddSyncEventHookup(EventPreRequestHandlerExecute, value, RequestNotification.PreExecuteRequestHandler);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventPreRequestHandlerExecute, value, RequestNotification.PreExecuteRequestHandler);
            }
        }

        public event EventHandler PreSendRequestContent
        {
            add
            {
                this.AddSendResponseEventHookup(EventPreSendRequestContent, value);
            }
            remove
            {
                this.RemoveSendResponseEventHookup(EventPreSendRequestContent, value);
            }
        }

        public event EventHandler PreSendRequestHeaders
        {
            add
            {
                this.AddSendResponseEventHookup(EventPreSendRequestHeaders, value);
            }
            remove
            {
                this.RemoveSendResponseEventHookup(EventPreSendRequestHeaders, value);
            }
        }

        public event EventHandler ReleaseRequestState
        {
            add
            {
                this.AddSyncEventHookup(EventReleaseRequestState, value, RequestNotification.ReleaseRequestState);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventReleaseRequestState, value, RequestNotification.ReleaseRequestState);
            }
        }

        public event EventHandler ResolveRequestCache
        {
            add
            {
                this.AddSyncEventHookup(EventResolveRequestCache, value, RequestNotification.ResolveRequestCache);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventResolveRequestCache, value, RequestNotification.ResolveRequestCache);
            }
        }

        public event EventHandler UpdateRequestCache
        {
            add
            {
                this.AddSyncEventHookup(EventUpdateRequestCache, value, RequestNotification.UpdateRequestCache);
            }
            remove
            {
                this.RemoveSyncEventHookup(EventUpdateRequestCache, value, RequestNotification.UpdateRequestCache);
            }
        }

        internal void AcquireNotifcationContextLock(ref bool locked)
        {
            Monitor.Enter(this._stepManager, ref locked);
        }

        private void AddEventMapping(string moduleName, RequestNotification requestNotification, bool isPostNotification, IExecutionStep step)
        {
            this.ThrowIfEventBindingDisallowed();
            if (this.IsContainerInitalizationAllowed)
            {
                PipelineModuleStepContainer moduleContainer = this.GetModuleContainer(moduleName);
                if (moduleContainer != null)
                {
                    moduleContainer.AddEvent(requestNotification, isPostNotification, step);
                }
            }
        }

        public void AddOnAcquireRequestStateAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnAcquireRequestStateAsync(bh, eh, null);
        }

        public void AddOnAcquireRequestStateAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventAcquireRequestState, beginHandler, endHandler, state, RequestNotification.AcquireRequestState, false, this);
        }

        public void AddOnAuthenticateRequestAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnAuthenticateRequestAsync(bh, eh, null);
        }

        public void AddOnAuthenticateRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventAuthenticateRequest, beginHandler, endHandler, state, RequestNotification.AuthenticateRequest, false, this);
        }

        public void AddOnAuthorizeRequestAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnAuthorizeRequestAsync(bh, eh, null);
        }

        public void AddOnAuthorizeRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventAuthorizeRequest, beginHandler, endHandler, state, RequestNotification.AuthorizeRequest, false, this);
        }

        public void AddOnBeginRequestAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnBeginRequestAsync(bh, eh, null);
        }

        public void AddOnBeginRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventBeginRequest, beginHandler, endHandler, state, RequestNotification.BeginRequest, false, this);
        }

        public void AddOnEndRequestAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnEndRequestAsync(bh, eh, null);
        }

        public void AddOnEndRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventEndRequest, beginHandler, endHandler, state, RequestNotification.EndRequest, false, this);
        }

        public void AddOnLogRequestAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            if (!HttpRuntime.UseIntegratedPipeline)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
            }
            this.AddOnLogRequestAsync(bh, eh, null);
        }

        public void AddOnLogRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            if (!HttpRuntime.UseIntegratedPipeline)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
            }
            this.AsyncEvents.AddHandler(EventLogRequest, beginHandler, endHandler, state, RequestNotification.LogRequest, false, this);
        }

        public void AddOnMapRequestHandlerAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            if (!HttpRuntime.UseIntegratedPipeline)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
            }
            this.AddOnMapRequestHandlerAsync(bh, eh, null);
        }

        public void AddOnMapRequestHandlerAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            if (!HttpRuntime.UseIntegratedPipeline)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
            }
            this.AsyncEvents.AddHandler(EventMapRequestHandler, beginHandler, endHandler, state, RequestNotification.MapRequestHandler, false, this);
        }

        public void AddOnPostAcquireRequestStateAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnPostAcquireRequestStateAsync(bh, eh, null);
        }

        public void AddOnPostAcquireRequestStateAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventPostAcquireRequestState, beginHandler, endHandler, state, RequestNotification.AcquireRequestState, true, this);
        }

        public void AddOnPostAuthenticateRequestAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnPostAuthenticateRequestAsync(bh, eh, null);
        }

        public void AddOnPostAuthenticateRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventPostAuthenticateRequest, beginHandler, endHandler, state, RequestNotification.AuthenticateRequest, true, this);
        }

        public void AddOnPostAuthorizeRequestAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnPostAuthorizeRequestAsync(bh, eh, null);
        }

        public void AddOnPostAuthorizeRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventPostAuthorizeRequest, beginHandler, endHandler, state, RequestNotification.AuthorizeRequest, true, this);
        }

        public void AddOnPostLogRequestAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            if (!HttpRuntime.UseIntegratedPipeline)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
            }
            this.AddOnPostLogRequestAsync(bh, eh, null);
        }

        public void AddOnPostLogRequestAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            if (!HttpRuntime.UseIntegratedPipeline)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
            }
            this.AsyncEvents.AddHandler(EventPostLogRequest, beginHandler, endHandler, state, RequestNotification.LogRequest, true, this);
        }

        public void AddOnPostMapRequestHandlerAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnPostMapRequestHandlerAsync(bh, eh, null);
        }

        public void AddOnPostMapRequestHandlerAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventPostMapRequestHandler, beginHandler, endHandler, state, RequestNotification.MapRequestHandler, true, this);
        }

        public void AddOnPostReleaseRequestStateAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnPostReleaseRequestStateAsync(bh, eh, null);
        }

        public void AddOnPostReleaseRequestStateAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventPostReleaseRequestState, beginHandler, endHandler, state, RequestNotification.ReleaseRequestState, true, this);
        }

        public void AddOnPostRequestHandlerExecuteAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnPostRequestHandlerExecuteAsync(bh, eh, null);
        }

        public void AddOnPostRequestHandlerExecuteAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventPostRequestHandlerExecute, beginHandler, endHandler, state, RequestNotification.ExecuteRequestHandler, true, this);
        }

        public void AddOnPostResolveRequestCacheAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnPostResolveRequestCacheAsync(bh, eh, null);
        }

        public void AddOnPostResolveRequestCacheAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventPostResolveRequestCache, beginHandler, endHandler, state, RequestNotification.ResolveRequestCache, true, this);
        }

        public void AddOnPostUpdateRequestCacheAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnPostUpdateRequestCacheAsync(bh, eh, null);
        }

        public void AddOnPostUpdateRequestCacheAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventPostUpdateRequestCache, beginHandler, endHandler, state, RequestNotification.UpdateRequestCache, true, this);
        }

        public void AddOnPreRequestHandlerExecuteAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnPreRequestHandlerExecuteAsync(bh, eh, null);
        }

        public void AddOnPreRequestHandlerExecuteAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventPreRequestHandlerExecute, beginHandler, endHandler, state, RequestNotification.PreExecuteRequestHandler, false, this);
        }

        public void AddOnReleaseRequestStateAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnReleaseRequestStateAsync(bh, eh, null);
        }

        public void AddOnReleaseRequestStateAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventReleaseRequestState, beginHandler, endHandler, state, RequestNotification.ReleaseRequestState, false, this);
        }

        public void AddOnResolveRequestCacheAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnResolveRequestCacheAsync(bh, eh, null);
        }

        public void AddOnResolveRequestCacheAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventResolveRequestCache, beginHandler, endHandler, state, RequestNotification.ResolveRequestCache, false, this);
        }

        public void AddOnUpdateRequestCacheAsync(BeginEventHandler bh, EndEventHandler eh)
        {
            this.AddOnUpdateRequestCacheAsync(bh, eh, null);
        }

        public void AddOnUpdateRequestCacheAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
        {
            this.AsyncEvents.AddHandler(EventUpdateRequestCache, beginHandler, endHandler, state, RequestNotification.UpdateRequestCache, false, this);
        }

        private void AddSendResponseEventHookup(object key, Delegate handler)
        {
            this.ThrowIfEventBindingDisallowed();
            this.Events.AddHandler(key, handler);
            if (this.IsContainerInitalizationAllowed)
            {
                PipelineModuleStepContainer moduleContainer = this.GetModuleContainer(this.CurrentModuleCollectionKey);
                if (moduleContainer != null)
                {
                    bool isHeaders = key == EventPreSendRequestHeaders;
                    SendResponseExecutionStep step = new SendResponseExecutionStep(this, (EventHandler) handler, isHeaders);
                    moduleContainer.AddEvent(RequestNotification.SendResponse, false, step);
                }
            }
        }

        internal void AddSyncEventHookup(object key, Delegate handler, RequestNotification notification)
        {
            this.AddSyncEventHookup(key, handler, notification, false);
        }

        private void AddSyncEventHookup(object key, Delegate handler, RequestNotification notification, bool isPostNotification)
        {
            this.ThrowIfEventBindingDisallowed();
            this.Events.AddHandler(key, handler);
            if (this.IsContainerInitalizationAllowed)
            {
                PipelineModuleStepContainer moduleContainer = this.GetModuleContainer(this.CurrentModuleCollectionKey);
                if (moduleContainer != null)
                {
                    SyncEventExecutionStep step = new SyncEventExecutionStep(this, (EventHandler) handler);
                    moduleContainer.AddEvent(notification, isPostNotification, step);
                }
            }
        }

        internal void AssignContext(HttpContext context)
        {
            if (this._context == null)
            {
                this._stepManager.InitRequest();
                this._context = context;
                this._context.ApplicationInstance = this;
                if (this._context.TraceIsEnabled)
                {
                    HttpRuntime.Profile.StartRequest(this._context);
                }
                this._context.SetImpersonationEnabled();
            }
        }

        internal IAsyncResult BeginProcessRequestNotification(HttpContext context, AsyncCallback cb)
        {
            if (this._context == null)
            {
                this.AssignContext(context);
            }
            context.CurrentModuleEventIndex = -1;
            HttpAsyncResult result = new HttpAsyncResult(cb, context);
            context.NotificationContext.AsyncResult = result;
            this.ResumeSteps(null);
            return result;
        }

        private void BuildEventMaskDictionary(Dictionary<string, RequestNotification> eventMask)
        {
            eventMask["BeginRequest"] = RequestNotification.BeginRequest;
            eventMask["AuthenticateRequest"] = RequestNotification.AuthenticateRequest;
            eventMask["PostAuthenticateRequest"] = RequestNotification.AuthenticateRequest;
            eventMask["AuthorizeRequest"] = RequestNotification.AuthorizeRequest;
            eventMask["PostAuthorizeRequest"] = RequestNotification.AuthorizeRequest;
            eventMask["ResolveRequestCache"] = RequestNotification.ResolveRequestCache;
            eventMask["PostResolveRequestCache"] = RequestNotification.ResolveRequestCache;
            eventMask["MapRequestHandler"] = RequestNotification.MapRequestHandler;
            eventMask["PostMapRequestHandler"] = RequestNotification.MapRequestHandler;
            eventMask["AcquireRequestState"] = RequestNotification.AcquireRequestState;
            eventMask["PostAcquireRequestState"] = RequestNotification.AcquireRequestState;
            eventMask["PreRequestHandlerExecute"] = RequestNotification.PreExecuteRequestHandler;
            eventMask["PostRequestHandlerExecute"] = RequestNotification.ExecuteRequestHandler;
            eventMask["ReleaseRequestState"] = RequestNotification.ReleaseRequestState;
            eventMask["PostReleaseRequestState"] = RequestNotification.ReleaseRequestState;
            eventMask["UpdateRequestCache"] = RequestNotification.UpdateRequestCache;
            eventMask["PostUpdateRequestCache"] = RequestNotification.UpdateRequestCache;
            eventMask["LogRequest"] = RequestNotification.LogRequest;
            eventMask["PostLogRequest"] = RequestNotification.LogRequest;
            eventMask["EndRequest"] = RequestNotification.EndRequest;
            eventMask["PreSendRequestHeaders"] = RequestNotification.SendResponse;
            eventMask["PreSendRequestContent"] = RequestNotification.SendResponse;
        }

        private HttpModuleCollection BuildIntegratedModuleCollection(List<ModuleConfigurationInfo> moduleList)
        {
            HttpModuleCollection modules = new HttpModuleCollection();
            foreach (ModuleConfigurationInfo info in moduleList)
            {
                ModulesEntry entry = new ModulesEntry(info.Name, info.Type, "type", null);
                modules.AddModule(entry.ModuleName, entry.Create());
            }
            return modules;
        }

        internal void ClearError()
        {
            this._lastError = null;
        }

        public void CompleteRequest()
        {
            this._stepManager.CompleteRequest();
        }

        private void CreateEventExecutionSteps(object eventIndex, ArrayList steps)
        {
            AsyncAppEventHandler handler = this.AsyncEvents[eventIndex];
            if (handler != null)
            {
                handler.CreateExecutionSteps(this, steps);
            }
            EventHandler handler2 = (EventHandler) this.Events[eventIndex];
            if (handler2 != null)
            {
                Delegate[] invocationList = handler2.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    steps.Add(new SyncEventExecutionStep(this, (EventHandler) invocationList[i]));
                }
            }
        }

        public virtual void Dispose()
        {
            this._site = null;
            if (this._events != null)
            {
                try
                {
                    EventHandler handler = (EventHandler) this._events[EventDisposed];
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
                finally
                {
                    this._events.Dispose();
                }
            }
        }

        internal void DisposeInternal()
        {
            PerfCounters.DecrementCounter(AppPerfCounter.PIPELINES);
            try
            {
                this.Dispose();
            }
            catch (Exception exception)
            {
                this.RecordError(exception);
            }
            if (this._moduleCollection != null)
            {
                int count = this._moduleCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        if (HttpRuntime.UseIntegratedPipeline)
                        {
                            this._currentModuleCollectionKey = this._moduleCollection.GetKey(i);
                        }
                        this._moduleCollection[i].Dispose();
                    }
                    catch
                    {
                    }
                }
                this._moduleCollection = null;
            }
        }

        internal RequestNotificationStatus EndProcessRequestNotification(IAsyncResult result)
        {
            HttpAsyncResult result2 = (HttpAsyncResult) result;
            if (result2.Error != null)
            {
                throw result2.Error;
            }
            return result2.Status;
        }

        internal void EnsureReleaseState()
        {
            if (this._moduleCollection != null)
            {
                for (int i = 0; i < this._moduleCollection.Count; i++)
                {
                    IHttpModule module = this._moduleCollection.Get(i);
                    if (module is SessionStateModule)
                    {
                        ((SessionStateModule) module).EnsureReleaseState(this);
                        return;
                    }
                }
            }
        }

        internal Exception ExecuteStep(IExecutionStep step, ref bool completedSynchronously)
        {
            Exception exception = null;
            try
            {
                try
                {
                    if (step.IsCancellable)
                    {
                        this._context.BeginCancellablePeriod();
                        try
                        {
                            step.Execute();
                        }
                        finally
                        {
                            this._context.EndCancellablePeriod();
                        }
                        this._context.WaitForExceptionIfCancelled();
                    }
                    else
                    {
                        step.Execute();
                    }
                    if (!step.CompletedSynchronously)
                    {
                        completedSynchronously = false;
                        return null;
                    }
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                    if (ImpersonationContext.CurrentThreadTokenExists)
                    {
                        exception2.Data["ASPIMPERSONATING"] = string.Empty;
                    }
                    if ((exception2 is ThreadAbortException) && ((Thread.CurrentThread.ThreadState & System.Threading.ThreadState.AbortRequested) == System.Threading.ThreadState.Running))
                    {
                        exception = null;
                        this._stepManager.CompleteRequest();
                    }
                }
                catch
                {
                }
            }
            catch (ThreadAbortException exception3)
            {
                if ((exception3.ExceptionState != null) && (exception3.ExceptionState is CancelModuleException))
                {
                    CancelModuleException exceptionState = (CancelModuleException) exception3.ExceptionState;
                    if (exceptionState.Timeout)
                    {
                        exception = new HttpException(System.Web.SR.GetString("Request_timed_out"), null, 0xbb9);
                        PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_TIMED_OUT);
                    }
                    else
                    {
                        exception = null;
                        this._stepManager.CompleteRequest();
                    }
                    Thread.ResetAbort();
                }
            }
            completedSynchronously = true;
            return exception;
        }

        [SecurityPermission(SecurityAction.Assert, ControlPrincipal=true)]
        internal static WindowsIdentity GetCurrentWindowsIdentityWithAssert()
        {
            return WindowsIdentity.GetCurrent();
        }

        private IHttpHandlerFactory GetFactory(string type)
        {
            HandlerFactoryCache cache = (HandlerFactoryCache) this._handlerFactories[type];
            if (cache == null)
            {
                cache = new HandlerFactoryCache(type);
                this._handlerFactories[type] = cache;
            }
            return cache.Factory;
        }

        private IHttpHandlerFactory GetFactory(HttpHandlerAction mapping)
        {
            HandlerFactoryCache cache = (HandlerFactoryCache) this._handlerFactories[mapping.Type];
            if (cache == null)
            {
                cache = new HandlerFactoryCache(mapping);
                this._handlerFactories[mapping.Type] = cache;
            }
            return cache.Factory;
        }

        internal static string GetFallbackCulture(string culture)
        {
            if ((culture.Length > 5) && (culture.IndexOf(':') == 4))
            {
                return culture.Substring(5);
            }
            return null;
        }

        private HttpHandlerAction GetHandlerMapping(HttpContext context, string requestType, VirtualPath path, bool useAppConfig)
        {
            CachedPathData pathData = null;
            HandlerMappingMemo cachedHandler = null;
            HttpHandlerAction mapping = null;
            if (!useAppConfig)
            {
                pathData = context.GetPathData(path);
                cachedHandler = pathData.CachedHandler;
                if ((cachedHandler != null) && !cachedHandler.IsMatch(requestType, path))
                {
                    cachedHandler = null;
                }
            }
            if (cachedHandler == null)
            {
                mapping = (useAppConfig ? RuntimeConfig.GetAppConfig().HttpHandlers : RuntimeConfig.GetConfig(context).HttpHandlers).FindMapping(requestType, path);
                if (!useAppConfig)
                {
                    cachedHandler = new HandlerMappingMemo(mapping, requestType, path);
                    pathData.CachedHandler = cachedHandler;
                }
                return mapping;
            }
            return cachedHandler.Mapping;
        }

        private HttpModuleCollection GetModuleCollection(IntPtr appContext)
        {
            if (_moduleConfigInfo == null)
            {
                List<ModuleConfigurationInfo> list = null;
                IntPtr zero = IntPtr.Zero;
                IntPtr bstrModuleName = IntPtr.Zero;
                int cchModuleName = 0;
                IntPtr bstrModuleType = IntPtr.Zero;
                int cchModuleType = 0;
                IntPtr bstrModulePrecondition = IntPtr.Zero;
                int cchModulePrecondition = 0;
                try
                {
                    int count = 0;
                    int num5 = UnsafeIISMethods.MgdGetModuleCollection(IntPtr.Zero, appContext, out zero, out count);
                    if (num5 < 0)
                    {
                        throw new HttpException(System.Web.SR.GetString("Cant_Read_Native_Modules", new object[] { num5.ToString("X8", CultureInfo.InvariantCulture) }));
                    }
                    list = new List<ModuleConfigurationInfo>(count);
                    for (uint i = 0; i < count; i++)
                    {
                        num5 = UnsafeIISMethods.MgdGetNextModule(zero, ref i, out bstrModuleName, out cchModuleName, out bstrModuleType, out cchModuleType, out bstrModulePrecondition, out cchModulePrecondition);
                        if (num5 < 0)
                        {
                            throw new HttpException(System.Web.SR.GetString("Cant_Read_Native_Modules", new object[] { num5.ToString("X8", CultureInfo.InvariantCulture) }));
                        }
                        string str = (cchModuleName > 0) ? StringUtil.StringFromWCharPtr(bstrModuleName, cchModuleName) : null;
                        string str2 = (cchModuleType > 0) ? StringUtil.StringFromWCharPtr(bstrModuleType, cchModuleType) : null;
                        string condition = (cchModulePrecondition > 0) ? StringUtil.StringFromWCharPtr(bstrModulePrecondition, cchModulePrecondition) : string.Empty;
                        Marshal.FreeBSTR(bstrModuleName);
                        bstrModuleName = IntPtr.Zero;
                        cchModuleName = 0;
                        Marshal.FreeBSTR(bstrModuleType);
                        bstrModuleType = IntPtr.Zero;
                        cchModuleType = 0;
                        Marshal.FreeBSTR(bstrModulePrecondition);
                        bstrModulePrecondition = IntPtr.Zero;
                        cchModulePrecondition = 0;
                        if (!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(str2))
                        {
                            list.Add(new ModuleConfigurationInfo(str, str2, condition));
                        }
                    }
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.Release(zero);
                        zero = IntPtr.Zero;
                    }
                    if (bstrModuleName != IntPtr.Zero)
                    {
                        Marshal.FreeBSTR(bstrModuleName);
                        bstrModuleName = IntPtr.Zero;
                    }
                    if (bstrModuleType != IntPtr.Zero)
                    {
                        Marshal.FreeBSTR(bstrModuleType);
                        bstrModuleType = IntPtr.Zero;
                    }
                    if (bstrModulePrecondition != IntPtr.Zero)
                    {
                        Marshal.FreeBSTR(bstrModulePrecondition);
                        bstrModulePrecondition = IntPtr.Zero;
                    }
                }
                _moduleConfigInfo = list;
            }
            return this.BuildIntegratedModuleCollection(_moduleConfigInfo);
        }

        private PipelineModuleStepContainer GetModuleContainer(string moduleName)
        {
            object obj2 = _moduleIndexMap[moduleName];
            if (obj2 == null)
            {
                return null;
            }
            int index = (int) obj2;
            return this.ModuleContainers[index];
        }

        public virtual string GetOutputCacheProviderName(HttpContext context)
        {
            return OutputCache.DefaultProviderName;
        }

        public virtual string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (StringUtil.EqualsIgnoreCase(custom, "browser"))
            {
                return context.Request.Browser.Type;
            }
            return null;
        }

        private bool HasEventSubscription(object eventIndex)
        {
            bool flag = false;
            AsyncAppEventHandler handler = this.AsyncEvents[eventIndex];
            if ((handler != null) && (handler.Count > 0))
            {
                handler.Reset();
                flag = true;
            }
            EventHandler handler2 = (EventHandler) this.Events[eventIndex];
            if (handler2 != null)
            {
                Delegate[] invocationList = handler2.GetInvocationList();
                if (invocationList.Length > 0)
                {
                    flag = true;
                }
                foreach (Delegate delegate2 in invocationList)
                {
                    this.Events.RemoveHandler(eventIndex, delegate2);
                }
            }
            return flag;
        }

        private void HookupEventHandlersForApplicationAndModules(MethodInfo[] handlers)
        {
            this._currentModuleCollectionKey = "global.asax";
            if (this._pipelineEventMasks == null)
            {
                Dictionary<string, RequestNotification> eventMask = new Dictionary<string, RequestNotification>();
                this.BuildEventMaskDictionary(eventMask);
                if (this._pipelineEventMasks == null)
                {
                    this._pipelineEventMasks = eventMask;
                }
            }
            for (int i = 0; i < handlers.Length; i++)
            {
                MethodInfo arglessMethod = handlers[i];
                string name = arglessMethod.Name;
                int index = name.IndexOf('_');
                string str2 = name.Substring(0, index);
                object obj2 = null;
                if (StringUtil.EqualsIgnoreCase(str2, "Application"))
                {
                    obj2 = this;
                }
                else if (this._moduleCollection != null)
                {
                    obj2 = this._moduleCollection[str2];
                }
                if (obj2 != null)
                {
                    Type componentType = obj2.GetType();
                    EventDescriptorCollection events = TypeDescriptor.GetEvents(componentType);
                    string str3 = name.Substring(index + 1);
                    EventDescriptor descriptor = events.Find(str3, true);
                    if ((descriptor == null) && StringUtil.EqualsIgnoreCase(str3.Substring(0, 2), "on"))
                    {
                        str3 = str3.Substring(2);
                        descriptor = events.Find(str3, true);
                    }
                    MethodInfo addMethod = null;
                    if (descriptor != null)
                    {
                        EventInfo info3 = componentType.GetEvent(descriptor.Name);
                        if (info3 != null)
                        {
                            addMethod = info3.GetAddMethod();
                        }
                    }
                    if (addMethod != null)
                    {
                        ParameterInfo[] parameters = addMethod.GetParameters();
                        if (parameters.Length == 1)
                        {
                            Delegate handler = null;
                            if (arglessMethod.GetParameters().Length == 0)
                            {
                                if (parameters[0].ParameterType != typeof(EventHandler))
                                {
                                    continue;
                                }
                                ArglessEventHandlerProxy proxy = new ArglessEventHandlerProxy(this, arglessMethod);
                                handler = proxy.Handler;
                            }
                            else
                            {
                                try
                                {
                                    handler = Delegate.CreateDelegate(parameters[0].ParameterType, this, name);
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                            try
                            {
                                addMethod.Invoke(obj2, new object[] { handler });
                            }
                            catch
                            {
                                if (HttpRuntime.UseIntegratedPipeline)
                                {
                                    throw;
                                }
                            }
                            if ((str3 != null) && this._pipelineEventMasks.ContainsKey(str3))
                            {
                                if (!StringUtil.StringStartsWith(str3, "Post"))
                                {
                                    this._appRequestNotifications |= (RequestNotification) this._pipelineEventMasks[str3];
                                }
                                else
                                {
                                    this._appPostNotifications |= (RequestNotification) this._pipelineEventMasks[str3];
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void Init()
        {
        }

        private void InitAppLevelCulture()
        {
            GlobalizationSection globalization = RuntimeConfig.GetAppConfig().Globalization;
            string culture = globalization.Culture;
            string uICulture = globalization.UICulture;
            if (!string.IsNullOrEmpty(culture))
            {
                if (StringUtil.StringStartsWithIgnoreCase(culture, AutoCulture))
                {
                    this._appLevelAutoCulture = true;
                    if (GetFallbackCulture(culture) != null)
                    {
                        this._appLevelCulture = HttpServerUtility.CreateReadOnlyCultureInfo(culture.Substring(5));
                    }
                }
                else
                {
                    this._appLevelAutoCulture = false;
                    this._appLevelCulture = HttpServerUtility.CreateReadOnlyCultureInfo(globalization.Culture);
                }
            }
            if (!string.IsNullOrEmpty(uICulture))
            {
                if (StringUtil.StringStartsWithIgnoreCase(uICulture, AutoCulture))
                {
                    this._appLevelAutoUICulture = true;
                    if (GetFallbackCulture(uICulture) != null)
                    {
                        this._appLevelUICulture = HttpServerUtility.CreateReadOnlyCultureInfo(uICulture.Substring(5));
                    }
                }
                else
                {
                    this._appLevelAutoUICulture = false;
                    this._appLevelUICulture = HttpServerUtility.CreateReadOnlyCultureInfo(globalization.UICulture);
                }
            }
        }

        private void InitIntegratedModules()
        {
            this._moduleCollection = this.BuildIntegratedModuleCollection(_moduleConfigInfo);
            this.InitModulesCommon();
        }

        internal void InitInternal(HttpContext context, HttpApplicationState state, MethodInfo[] handlers)
        {
            this._state = state;
            PerfCounters.IncrementCounter(AppPerfCounter.PIPELINES);
            try
            {
                try
                {
                    this._initContext = context;
                    this._initContext.ApplicationInstance = this;
                    context.ConfigurationPath = context.Request.ApplicationPathObject;
                    using (new DisposableHttpContextWrapper(context))
                    {
                        if (HttpRuntime.UseIntegratedPipeline)
                        {
                            try
                            {
                                context.HideRequestResponse = true;
                                this._hideRequestResponse = true;
                                this.InitIntegratedModules();
                                goto Label_006B;
                            }
                            finally
                            {
                                context.HideRequestResponse = false;
                                this._hideRequestResponse = false;
                            }
                        }
                        this.InitModules();
                    Label_006B:
                        if (handlers != null)
                        {
                            this.HookupEventHandlersForApplicationAndModules(handlers);
                        }
                        this._context = context;
                        if (HttpRuntime.UseIntegratedPipeline && (this._context != null))
                        {
                            this._context.HideRequestResponse = true;
                        }
                        this._hideRequestResponse = true;
                        try
                        {
                            this.Init();
                        }
                        catch (Exception exception)
                        {
                            this.RecordError(exception);
                        }
                    }
                    if (HttpRuntime.UseIntegratedPipeline && (this._context != null))
                    {
                        this._context.HideRequestResponse = false;
                    }
                    this._hideRequestResponse = false;
                    this._context = null;
                    this._resumeStepsWaitCallback = new WaitCallback(this.ResumeStepsWaitCallback);
                    if (HttpRuntime.UseIntegratedPipeline)
                    {
                        this._stepManager = new PipelineStepManager(this);
                    }
                    else
                    {
                        this._stepManager = new ApplicationStepManager(this);
                    }
                    this._stepManager.BuildSteps(this._resumeStepsWaitCallback);
                }
                finally
                {
                    this._initInternalCompleted = true;
                    context.ConfigurationPath = null;
                    this._initContext.ApplicationInstance = null;
                    this._initContext = null;
                }
            }
            catch
            {
                throw;
            }
        }

        private void InitModules()
        {
            this._moduleCollection = RuntimeConfig.GetAppConfig().HttpModules.CreateModules();
            this.InitModulesCommon();
        }

        private void InitModulesCommon()
        {
            int count = this._moduleCollection.Count;
            for (int i = 0; i < count; i++)
            {
                this._currentModuleCollectionKey = this._moduleCollection.GetKey(i);
                this._moduleCollection[i].Init(this);
            }
            this._currentModuleCollectionKey = null;
            this.InitAppLevelCulture();
        }

        internal void InitSpecial(HttpApplicationState state, MethodInfo[] handlers, IntPtr appContext, HttpContext context)
        {
            this._state = state;
            try
            {
                if (context != null)
                {
                    this._initContext = context;
                    this._initContext.ApplicationInstance = this;
                }
                if (appContext != IntPtr.Zero)
                {
                    using (new ApplicationImpersonationContext())
                    {
                        HttpRuntime.CheckApplicationEnabled();
                    }
                    this.InitAppLevelCulture();
                    this.RegisterEventSubscriptionsWithIIS(appContext, context, handlers);
                }
                else
                {
                    this.InitAppLevelCulture();
                    if (handlers != null)
                    {
                        this.HookupEventHandlersForApplicationAndModules(handlers);
                    }
                }
                if ((appContext != IntPtr.Zero) && ((this._appPostNotifications != 0) || (this._appRequestNotifications != 0)))
                {
                    this.RegisterIntegratedEvent(appContext, "global.asax", this._appRequestNotifications, this._appPostNotifications, base.GetType().FullName, "managedHandler", false);
                }
            }
            finally
            {
                _initSpecialCompleted = true;
                if (this._initContext != null)
                {
                    this._initContext.ApplicationInstance = null;
                    this._initContext = null;
                }
            }
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.RestrictedMemberAccess)]
        private void InvokeMethodWithAssert(MethodInfo method, int paramCount, object eventSource, EventArgs eventArgs)
        {
            if (paramCount == 0)
            {
                method.Invoke(this, new object[0]);
            }
            else
            {
                method.Invoke(this, new object[] { eventSource, eventArgs });
            }
        }

        internal IHttpHandler MapHttpHandler(HttpContext context, string requestType, VirtualPath path, string pathTranslated, bool useAppConfig)
        {
            IHttpHandler handler = (context.ServerExecuteDepth == 0) ? context.RemapHandlerInstance : null;
            using (new ApplicationImpersonationContext())
            {
                if (handler != null)
                {
                    return handler;
                }
                HttpHandlerAction mapping = this.GetHandlerMapping(context, requestType, path, useAppConfig);
                if (mapping == null)
                {
                    PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_FOUND);
                    PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_FAILED);
                    throw new HttpException(System.Web.SR.GetString("Http_handler_not_found_for_request_type", new object[] { requestType }));
                }
                IHttpHandlerFactory factory = this.GetFactory(mapping);
                try
                {
                    IHttpHandlerFactory2 factory2 = factory as IHttpHandlerFactory2;
                    if (factory2 != null)
                    {
                        handler = factory2.GetHandler(context, requestType, path, pathTranslated);
                    }
                    else
                    {
                        handler = factory.GetHandler(context, requestType, path.VirtualPathString, pathTranslated);
                    }
                }
                catch (FileNotFoundException exception)
                {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                    {
                        throw new HttpException(0x194, null, exception);
                    }
                    throw new HttpException(0x194, null);
                }
                catch (DirectoryNotFoundException exception2)
                {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                    {
                        throw new HttpException(0x194, null, exception2);
                    }
                    throw new HttpException(0x194, null);
                }
                catch (PathTooLongException exception3)
                {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                    {
                        throw new HttpException(0x19e, null, exception3);
                    }
                    throw new HttpException(0x19e, null);
                }
                if (this._handlerRecycleList == null)
                {
                    this._handlerRecycleList = new ArrayList();
                }
                this._handlerRecycleList.Add(new HandlerWithFactory(handler, factory));
            }
            return handler;
        }

        internal IHttpHandler MapIntegratedHttpHandler(HttpContext context, string requestType, VirtualPath path, string pathTranslated, bool useAppConfig, bool convertNativeStaticFileModule)
        {
            IHttpHandler handler = null;
            using (new ApplicationImpersonationContext())
            {
                string virtualPathString = path.VirtualPathString;
                if (useAppConfig)
                {
                    int startIndex = virtualPathString.LastIndexOf('/') + 1;
                    if ((startIndex != 0) && (startIndex < virtualPathString.Length))
                    {
                        virtualPathString = UrlPath.SimpleCombine(HttpRuntime.AppDomainAppVirtualPathString, virtualPathString.Substring(startIndex));
                    }
                    else
                    {
                        virtualPathString = HttpRuntime.AppDomainAppVirtualPathString;
                    }
                }
                string str = (context.WorkerRequest as IIS7WorkerRequest).MapHandlerAndGetHandlerTypeString(requestType, virtualPathString, convertNativeStaticFileModule);
                if (str == null)
                {
                    PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_FOUND);
                    PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_FAILED);
                    throw new HttpException(System.Web.SR.GetString("Http_handler_not_found_for_request_type", new object[] { requestType }));
                }
                if (string.IsNullOrEmpty(str))
                {
                    return handler;
                }
                IHttpHandlerFactory factory = this.GetFactory(str);
                try
                {
                    handler = factory.GetHandler(context, requestType, path.VirtualPathString, pathTranslated);
                }
                catch (FileNotFoundException exception)
                {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                    {
                        throw new HttpException(0x194, null, exception);
                    }
                    throw new HttpException(0x194, null);
                }
                catch (DirectoryNotFoundException exception2)
                {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                    {
                        throw new HttpException(0x194, null, exception2);
                    }
                    throw new HttpException(0x194, null);
                }
                catch (PathTooLongException exception3)
                {
                    if (HttpRuntime.HasPathDiscoveryPermission(pathTranslated))
                    {
                        throw new HttpException(0x19e, null, exception3);
                    }
                    throw new HttpException(0x19e, null);
                }
                if (this._handlerRecycleList == null)
                {
                    this._handlerRecycleList = new ArrayList();
                }
                this._handlerRecycleList.Add(new HandlerWithFactory(handler, factory));
            }
            return handler;
        }

        internal ThreadContext OnThreadEnter()
        {
            return this.OnThreadEnterPrivate(true);
        }

        internal ThreadContext OnThreadEnter(bool setImpersonationContext)
        {
            return this.OnThreadEnterPrivate(setImpersonationContext);
        }

        private ThreadContext OnThreadEnterPrivate(bool setImpersonationContext)
        {
            ThreadContext context = new ThreadContext(this._context);
            context.Enter(setImpersonationContext);
            if (!this._timeoutManagerInitialized)
            {
                this._context.EnsureTimeout();
                HttpRuntime.RequestTimeoutManager.Add(this._context);
                this._timeoutManagerInitialized = true;
            }
            return context;
        }

        private void ProcessEventSubscriptions(out RequestNotification requestNotifications, out RequestNotification postRequestNotifications)
        {
            requestNotifications = 0;
            postRequestNotifications = 0;
            if (this.HasEventSubscription(EventBeginRequest))
            {
                requestNotifications |= RequestNotification.BeginRequest;
            }
            if (this.HasEventSubscription(EventAuthenticateRequest))
            {
                requestNotifications |= RequestNotification.AuthenticateRequest;
            }
            if (this.HasEventSubscription(EventPostAuthenticateRequest))
            {
                postRequestNotifications |= RequestNotification.AuthenticateRequest;
            }
            if (this.HasEventSubscription(EventAuthorizeRequest))
            {
                requestNotifications |= RequestNotification.AuthorizeRequest;
            }
            if (this.HasEventSubscription(EventPostAuthorizeRequest))
            {
                postRequestNotifications |= RequestNotification.AuthorizeRequest;
            }
            if (this.HasEventSubscription(EventResolveRequestCache))
            {
                requestNotifications |= RequestNotification.ResolveRequestCache;
            }
            if (this.HasEventSubscription(EventPostResolveRequestCache))
            {
                postRequestNotifications |= RequestNotification.ResolveRequestCache;
            }
            if (this.HasEventSubscription(EventMapRequestHandler))
            {
                requestNotifications |= RequestNotification.MapRequestHandler;
            }
            if (this.HasEventSubscription(EventPostMapRequestHandler))
            {
                postRequestNotifications |= RequestNotification.MapRequestHandler;
            }
            if (this.HasEventSubscription(EventAcquireRequestState))
            {
                requestNotifications |= RequestNotification.AcquireRequestState;
            }
            if (this.HasEventSubscription(EventPostAcquireRequestState))
            {
                postRequestNotifications |= RequestNotification.AcquireRequestState;
            }
            if (this.HasEventSubscription(EventPreRequestHandlerExecute))
            {
                requestNotifications |= RequestNotification.PreExecuteRequestHandler;
            }
            if (this.HasEventSubscription(EventPostRequestHandlerExecute))
            {
                postRequestNotifications |= RequestNotification.ExecuteRequestHandler;
            }
            if (this.HasEventSubscription(EventReleaseRequestState))
            {
                requestNotifications |= RequestNotification.ReleaseRequestState;
            }
            if (this.HasEventSubscription(EventPostReleaseRequestState))
            {
                postRequestNotifications |= RequestNotification.ReleaseRequestState;
            }
            if (this.HasEventSubscription(EventUpdateRequestCache))
            {
                requestNotifications |= RequestNotification.UpdateRequestCache;
            }
            if (this.HasEventSubscription(EventPostUpdateRequestCache))
            {
                postRequestNotifications |= RequestNotification.UpdateRequestCache;
            }
            if (this.HasEventSubscription(EventLogRequest))
            {
                requestNotifications |= RequestNotification.LogRequest;
            }
            if (this.HasEventSubscription(EventPostLogRequest))
            {
                postRequestNotifications |= RequestNotification.LogRequest;
            }
            if (this.HasEventSubscription(EventEndRequest))
            {
                requestNotifications |= RequestNotification.EndRequest;
            }
            if (this.HasEventSubscription(EventPreSendRequestHeaders))
            {
                requestNotifications |= RequestNotification.SendResponse;
            }
            if (this.HasEventSubscription(EventPreSendRequestContent))
            {
                requestNotifications |= RequestNotification.SendResponse;
            }
        }

        internal void ProcessSpecialRequest(HttpContext context, MethodInfo method, int paramCount, object eventSource, EventArgs eventArgs, HttpSessionState session)
        {
            this._context = context;
            if (HttpRuntime.UseIntegratedPipeline && (this._context != null))
            {
                this._context.HideRequestResponse = true;
            }
            this._hideRequestResponse = true;
            this._session = session;
            this._lastError = null;
            using (new DisposableHttpContextWrapper(context))
            {
                using (new ApplicationImpersonationContext())
                {
                    try
                    {
                        this.SetAppLevelCulture();
                        this.InvokeMethodWithAssert(method, paramCount, eventSource, eventArgs);
                    }
                    catch (Exception exception)
                    {
                        Exception innerException;
                        if (exception is TargetInvocationException)
                        {
                            innerException = exception.InnerException;
                        }
                        else
                        {
                            innerException = exception;
                        }
                        this.RecordError(innerException);
                        if (context == null)
                        {
                            try
                            {
                                WebBaseEvent.RaiseRuntimeError(innerException, this);
                            }
                            catch
                            {
                            }
                        }
                    }
                    finally
                    {
                        if (this._state != null)
                        {
                            this._state.EnsureUnLock();
                        }
                        this.RestoreAppLevelCulture();
                        if (HttpRuntime.UseIntegratedPipeline && (this._context != null))
                        {
                            this._context.HideRequestResponse = false;
                        }
                        this._hideRequestResponse = false;
                        this._context = null;
                        this._session = null;
                        this._lastError = null;
                        this._appEvent = null;
                    }
                }
            }
        }

        internal void RaiseErrorWithoutContext(Exception error)
        {
            try
            {
                try
                {
                    this.SetAppLevelCulture();
                    this._lastError = error;
                    this.RaiseOnError();
                }
                finally
                {
                    if (this._state != null)
                    {
                        this._state.EnsureUnLock();
                    }
                    this.RestoreAppLevelCulture();
                    this._lastError = null;
                    this._appEvent = null;
                }
            }
            catch
            {
                throw;
            }
        }

        private void RaiseOnError()
        {
            EventHandler handler = (EventHandler) this.Events[EventErrorRecorded];
            if (handler != null)
            {
                try
                {
                    handler(this, this.AppEvent);
                }
                catch (Exception exception)
                {
                    if (this._context != null)
                    {
                        this._context.AddError(exception);
                    }
                }
            }
        }

        internal void RaiseOnPreSendRequestContent()
        {
            EventHandler handler = (EventHandler) this.Events[EventPreSendRequestContent];
            if (handler != null)
            {
                try
                {
                    handler(this, this.AppEvent);
                }
                catch (Exception exception)
                {
                    this.RecordError(exception);
                }
            }
        }

        internal void RaiseOnPreSendRequestHeaders()
        {
            EventHandler handler = (EventHandler) this.Events[EventPreSendRequestHeaders];
            if (handler != null)
            {
                try
                {
                    handler(this, this.AppEvent);
                }
                catch (Exception exception)
                {
                    this.RecordError(exception);
                }
            }
        }

        private void RecordError(Exception error)
        {
            bool flag = true;
            if (this._context != null)
            {
                if (this._context.Error != null)
                {
                    flag = false;
                }
                this._context.AddError(error);
            }
            else
            {
                if (this._lastError != null)
                {
                    flag = false;
                }
                this._lastError = error;
            }
            if (flag)
            {
                this.RaiseOnError();
            }
        }

        private void RecycleHandlers()
        {
            if (this._handlerRecycleList != null)
            {
                int count = this._handlerRecycleList.Count;
                for (int i = 0; i < count; i++)
                {
                    ((HandlerWithFactory) this._handlerRecycleList[i]).Recycle();
                }
                this._handlerRecycleList = null;
            }
        }

        private void RegisterEventSubscriptionsWithIIS(IntPtr appContext, HttpContext context, MethodInfo[] handlers)
        {
            RequestNotification notification;
            RequestNotification notification2;
            this.RegisterIntegratedEvent(appContext, "AspNetFilterModule", RequestNotification.LogRequest | RequestNotification.UpdateRequestCache, 0, string.Empty, string.Empty, true);
            this._moduleCollection = this.GetModuleCollection(appContext);
            if (handlers != null)
            {
                this.HookupEventHandlersForApplicationAndModules(handlers);
            }
            HttpApplicationFactory.EnsureAppStartCalledForIntegratedMode(context, this);
            this._currentModuleCollectionKey = "global.asax";
            try
            {
                this._hideRequestResponse = true;
                context.HideRequestResponse = true;
                this._context = context;
                this.Init();
            }
            catch (Exception exception)
            {
                this.RecordError(exception);
                Exception error = context.Error;
                if (error != null)
                {
                    throw error;
                }
            }
            finally
            {
                this._context = null;
                context.HideRequestResponse = false;
                this._hideRequestResponse = false;
            }
            this.ProcessEventSubscriptions(out notification, out notification2);
            this._appRequestNotifications |= notification;
            this._appPostNotifications |= notification2;
            for (int i = 0; i < this._moduleCollection.Count; i++)
            {
                this._currentModuleCollectionKey = this._moduleCollection.GetKey(i);
                IHttpModule module = this._moduleCollection.Get(i);
                ModuleConfigurationInfo info = _moduleConfigInfo[i];
                module.Init(this);
                this.ProcessEventSubscriptions(out notification, out notification2);
                if ((notification != 0) || (notification2 != 0))
                {
                    this.RegisterIntegratedEvent(appContext, info.Name, notification, notification2, info.Type, info.Precondition, false);
                }
            }
            this.RegisterIntegratedEvent(appContext, "ManagedPipelineHandler", RequestNotification.ExecuteRequestHandler | RequestNotification.MapRequestHandler, 0, string.Empty, string.Empty, false);
        }

        private void RegisterIntegratedEvent(IntPtr appContext, string moduleName, RequestNotification requestNotifications, RequestNotification postRequestNotifications, string moduleType, string modulePrecondition, bool useHighPriority)
        {
            int count;
            if (_moduleIndexMap.ContainsKey(moduleName))
            {
                count = (int) _moduleIndexMap[moduleName];
            }
            else
            {
                count = _moduleIndexMap.Count;
                _moduleIndexMap[moduleName] = count;
            }
            if (UnsafeIISMethods.MgdRegisterEventSubscription(appContext, moduleName, requestNotifications, postRequestNotifications, moduleType, modulePrecondition, new IntPtr(count), useHighPriority) < 0)
            {
                throw new HttpException(System.Web.SR.GetString("Failed_Pipeline_Subscription", new object[] { moduleName }));
            }
        }

        internal void ReleaseAppInstance()
        {
            if (this._context != null)
            {
                if (this._context.TraceIsEnabled)
                {
                    HttpRuntime.Profile.EndRequest(this._context);
                }
                this._context.ClearReferences();
                if (this._timeoutManagerInitialized)
                {
                    HttpRuntime.RequestTimeoutManager.Remove(this._context);
                    this._timeoutManagerInitialized = false;
                }
            }
            this.RecycleHandlers();
            if (this.AsyncResult != null)
            {
                this.AsyncResult = null;
            }
            this._context = null;
            this.AppEvent = null;
            HttpApplicationFactory.RecycleApplicationInstance(this);
        }

        internal void ReleaseNotifcationContextLock()
        {
            Monitor.Exit(this._stepManager);
        }

        private void RemoveSendResponseEventHookup(object key, Delegate handler)
        {
            this.ThrowIfEventBindingDisallowed();
            this.Events.RemoveHandler(key, handler);
            if (this.IsContainerInitalizationAllowed)
            {
                PipelineModuleStepContainer moduleContainer = this.GetModuleContainer(this.CurrentModuleCollectionKey);
                if (moduleContainer != null)
                {
                    moduleContainer.RemoveEvent(RequestNotification.SendResponse, false, handler);
                }
            }
        }

        internal void RemoveSyncEventHookup(object key, Delegate handler, RequestNotification notification)
        {
            this.RemoveSyncEventHookup(key, handler, notification, false);
        }

        internal void RemoveSyncEventHookup(object key, Delegate handler, RequestNotification notification, bool isPostNotification)
        {
            this.ThrowIfEventBindingDisallowed();
            this.Events.RemoveHandler(key, handler);
            if (this.IsContainerInitalizationAllowed)
            {
                PipelineModuleStepContainer moduleContainer = this.GetModuleContainer(this.CurrentModuleCollectionKey);
                if (moduleContainer != null)
                {
                    moduleContainer.RemoveEvent(notification, isPostNotification, handler);
                }
            }
        }

        private void RestoreAppLevelCulture()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
            if (this._savedAppLevelCulture != null)
            {
                if (currentCulture != this._savedAppLevelCulture)
                {
                    HttpRuntime.SetCurrentThreadCultureWithAssert(this._savedAppLevelCulture);
                }
                this._savedAppLevelCulture = null;
            }
            if (this._savedAppLevelUICulture != null)
            {
                if (currentUICulture != this._savedAppLevelUICulture)
                {
                    Thread.CurrentThread.CurrentUICulture = this._savedAppLevelUICulture;
                }
                this._savedAppLevelUICulture = null;
            }
        }

        private void ResumeSteps(Exception error)
        {
            this._stepManager.ResumeSteps(error);
        }

        private void ResumeStepsFromThreadPoolThread(Exception error)
        {
            if (Thread.CurrentThread.IsThreadPoolThread)
            {
                this.ResumeSteps(error);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(this._resumeStepsWaitCallback, error);
            }
        }

        private void ResumeStepsWaitCallback(object error)
        {
            this.ResumeSteps(error as Exception);
        }

        private void SetAppLevelCulture()
        {
            CultureInfo cultureInfo = null;
            CultureInfo info2 = null;
            CultureInfo info3 = null;
            string name = null;
            if ((this._appLevelAutoCulture || this._appLevelAutoUICulture) && ((this._context != null) && !this._context.HideRequestResponse))
            {
                name = this._context.UserLanguageFromContext();
                if (name != null)
                {
                    try
                    {
                        info3 = HttpServerUtility.CreateReadOnlySpecificCultureInfo(name);
                    }
                    catch
                    {
                    }
                }
            }
            cultureInfo = this._appLevelCulture;
            info2 = this._appLevelUICulture;
            if (info3 != null)
            {
                if (this._appLevelAutoCulture)
                {
                    cultureInfo = info3;
                }
                if (this._appLevelAutoUICulture)
                {
                    info2 = info3;
                }
            }
            this._savedAppLevelCulture = Thread.CurrentThread.CurrentCulture;
            this._savedAppLevelUICulture = Thread.CurrentThread.CurrentUICulture;
            if ((cultureInfo != null) && (cultureInfo != Thread.CurrentThread.CurrentCulture))
            {
                HttpRuntime.SetCurrentThreadCultureWithAssert(cultureInfo);
            }
            if ((info2 != null) && (info2 != Thread.CurrentThread.CurrentUICulture))
            {
                Thread.CurrentThread.CurrentUICulture = info2;
            }
        }

        [SecurityPermission(SecurityAction.Assert, ControlPrincipal=true)]
        internal static void SetCurrentPrincipalWithAssert(IPrincipal user)
        {
            Thread.CurrentPrincipal = user;
        }

        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            this._context = context;
            this._context.ApplicationInstance = this;
            this._stepManager.InitRequest();
            this._context.Root();
            HttpAsyncResult result = new HttpAsyncResult(cb, extraData);
            this.AsyncResult = result;
            if (this._context.TraceIsEnabled)
            {
                HttpRuntime.Profile.StartRequest(this._context);
            }
            this.ResumeSteps(null);
            return result;
        }

        void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        {
            HttpAsyncResult result2 = (HttpAsyncResult) result;
            if (result2.Error != null)
            {
                throw result2.Error;
            }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            throw new HttpException(System.Web.SR.GetString("Sync_not_supported"));
        }

        private void ThrowIfEventBindingDisallowed()
        {
            if ((HttpRuntime.UseIntegratedPipeline && _initSpecialCompleted) && this._initInternalCompleted)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Event_Binding_Disallowed"));
            }
        }

        internal EventArgs AppEvent
        {
            get
            {
                if (this._appEvent == null)
                {
                    this._appEvent = EventArgs.Empty;
                }
                return this._appEvent;
            }
            set
            {
                this._appEvent = null;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpApplicationState Application
        {
            get
            {
                return this._state;
            }
        }

        private AsyncAppEventHandlersTable AsyncEvents
        {
            get
            {
                if (this._asyncEvents == null)
                {
                    this._asyncEvents = new AsyncAppEventHandlersTable();
                }
                return this._asyncEvents;
            }
        }

        internal HttpAsyncResult AsyncResult
        {
            get
            {
                if (!HttpRuntime.UseIntegratedPipeline)
                {
                    return this._ar;
                }
                if (this._context.NotificationContext == null)
                {
                    return null;
                }
                return this._context.NotificationContext.AsyncResult;
            }
            set
            {
                if (HttpRuntime.UseIntegratedPipeline)
                {
                    this._context.NotificationContext.AsyncResult = value;
                }
                else
                {
                    this._ar = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public HttpContext Context
        {
            get
            {
                if (this._context == null)
                {
                    return this._initContext;
                }
                return this._context;
            }
        }

        internal string CurrentModuleCollectionKey
        {
            get
            {
                if (this._currentModuleCollectionKey != null)
                {
                    return this._currentModuleCollectionKey;
                }
                return "UnknownModule";
            }
        }

        private PipelineModuleStepContainer CurrentModuleContainer
        {
            get
            {
                return this.ModuleContainers[this._context.CurrentModuleIndex];
            }
        }

        internal byte[] EntityBuffer
        {
            get
            {
                if (this._entityBuffer == null)
                {
                    this._entityBuffer = new byte[0x2000];
                }
                return this._entityBuffer;
            }
        }

        protected EventHandlerList Events
        {
            get
            {
                if (this._events == null)
                {
                    this._events = new EventHandlerList();
                }
                return this._events;
            }
        }

        internal static List<ModuleConfigurationInfo> IntegratedModuleList
        {
            get
            {
                return _moduleConfigInfo;
            }
        }

        private bool IsContainerInitalizationAllowed
        {
            get
            {
                return ((HttpRuntime.UseIntegratedPipeline && _initSpecialCompleted) && !this._initInternalCompleted);
            }
        }

        internal bool IsRequestCompleted
        {
            get
            {
                if (this._stepManager == null)
                {
                    return false;
                }
                return this._stepManager.IsCompleted;
            }
        }

        internal Exception LastError
        {
            get
            {
                if (this._context == null)
                {
                    return this._lastError;
                }
                return this._context.Error;
            }
        }

        private PipelineModuleStepContainer[] ModuleContainers
        {
            get
            {
                if (this._moduleContainers == null)
                {
                    this._moduleContainers = new PipelineModuleStepContainer[_moduleIndexMap.Count];
                    for (int i = 0; i < this._moduleContainers.Length; i++)
                    {
                        this._moduleContainers[i] = new PipelineModuleStepContainer();
                    }
                }
                return this._moduleContainers;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public HttpModuleCollection Modules
        {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
            get
            {
                if (this._moduleCollection == null)
                {
                    this._moduleCollection = new HttpModuleCollection();
                }
                return this._moduleCollection;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public HttpRequest Request
        {
            get
            {
                HttpRequest request = null;
                if ((this._context != null) && !this._hideRequestResponse)
                {
                    request = this._context.Request;
                }
                if (request == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Request_not_available"));
                }
                return request;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpResponse Response
        {
            get
            {
                HttpResponse response = null;
                if ((this._context != null) && !this._hideRequestResponse)
                {
                    response = this._context.Response;
                }
                if (response == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Response_not_available"));
                }
                return response;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpServerUtility Server
        {
            get
            {
                if (this._context != null)
                {
                    return this._context.Server;
                }
                return new HttpServerUtility(this);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public HttpSessionState Session
        {
            get
            {
                HttpSessionState session = null;
                if (this._session != null)
                {
                    session = this._session;
                }
                else if (this._context != null)
                {
                    session = this._context.Session;
                }
                if (session == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Session_not_available"));
                }
                return session;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISite Site
        {
            get
            {
                return this._site;
            }
            set
            {
                this._site = value;
            }
        }

        bool IHttpHandler.IsReusable
        {
            get
            {
                return true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public IPrincipal User
        {
            get
            {
                if (this._context == null)
                {
                    throw new HttpException(System.Web.SR.GetString("User_not_available"));
                }
                return this._context.User;
            }
        }

        internal class ApplicationStepManager : HttpApplication.StepManager
        {
            private int _currentStepIndex;
            private int _endRequestStepIndex;
            private HttpApplication.IExecutionStep[] _execSteps;
            private int _numStepCalls;
            private int _numSyncStepCalls;
            private WaitCallback _resumeStepsWaitCallback;

            internal ApplicationStepManager(HttpApplication app) : base(app)
            {
            }

            internal override void BuildSteps(WaitCallback stepCallback)
            {
                ArrayList steps = new ArrayList();
                HttpApplication app = base._application;
                bool flag = false;
                UrlMappingsSection urlMappings = RuntimeConfig.GetConfig().UrlMappings;
                flag = urlMappings.IsEnabled && (urlMappings.UrlMappings.Count > 0);
                steps.Add(new HttpApplication.ValidateRequestExecutionStep(app));
                steps.Add(new HttpApplication.ValidatePathExecutionStep(app));
                if (flag)
                {
                    steps.Add(new HttpApplication.UrlMappingsExecutionStep(app));
                }
                app.CreateEventExecutionSteps(HttpApplication.EventBeginRequest, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventAuthenticateRequest, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventDefaultAuthentication, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostAuthenticateRequest, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventAuthorizeRequest, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostAuthorizeRequest, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventResolveRequestCache, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostResolveRequestCache, steps);
                steps.Add(new HttpApplication.MapHandlerExecutionStep(app));
                app.CreateEventExecutionSteps(HttpApplication.EventPostMapRequestHandler, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventAcquireRequestState, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostAcquireRequestState, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPreRequestHandlerExecute, steps);
                steps.Add(new HttpApplication.CallHandlerExecutionStep(app));
                app.CreateEventExecutionSteps(HttpApplication.EventPostRequestHandlerExecute, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventReleaseRequestState, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostReleaseRequestState, steps);
                steps.Add(new HttpApplication.CallFilterExecutionStep(app));
                app.CreateEventExecutionSteps(HttpApplication.EventUpdateRequestCache, steps);
                app.CreateEventExecutionSteps(HttpApplication.EventPostUpdateRequestCache, steps);
                this._endRequestStepIndex = steps.Count;
                app.CreateEventExecutionSteps(HttpApplication.EventEndRequest, steps);
                steps.Add(new HttpApplication.NoopExecutionStep());
                this._execSteps = new HttpApplication.IExecutionStep[steps.Count];
                steps.CopyTo(this._execSteps);
                this._resumeStepsWaitCallback = stepCallback;
            }

            internal override void InitRequest()
            {
                this._currentStepIndex = -1;
                this._numStepCalls = 0;
                this._numSyncStepCalls = 0;
                base._requestCompleted = false;
            }

            [DebuggerStepperBoundary]
            internal override void ResumeSteps(Exception error)
            {
                bool flag = false;
                bool completedSynchronously = true;
                HttpApplication application = base._application;
                HttpContext context = application.Context;
                HttpApplication.ThreadContext context2 = null;
                AspNetSynchronizationContext syncContext = context.SyncContext;
                lock (base._application)
                {
                    try
                    {
                        context2 = application.OnThreadEnter();
                    }
                    catch (Exception exception)
                    {
                        if (error == null)
                        {
                            error = exception;
                        }
                    }
                    try
                    {
                        try
                        {
                        Label_0045:
                            if (syncContext.Error != null)
                            {
                                error = syncContext.Error;
                                syncContext.ClearError();
                            }
                            if (error != null)
                            {
                                application.RecordError(error);
                                error = null;
                            }
                            if (syncContext.PendingOperationsCount > 0)
                            {
                                syncContext.SetLastCompletionWorkItem(this._resumeStepsWaitCallback);
                            }
                            else
                            {
                                if ((this._currentStepIndex < this._endRequestStepIndex) && ((context.Error != null) || base._requestCompleted))
                                {
                                    context.Response.FilterOutput();
                                    this._currentStepIndex = this._endRequestStepIndex;
                                }
                                else
                                {
                                    this._currentStepIndex++;
                                }
                                if (this._currentStepIndex >= this._execSteps.Length)
                                {
                                    flag = true;
                                }
                                else
                                {
                                    this._numStepCalls++;
                                    context.SyncContext.Enable();
                                    error = application.ExecuteStep(this._execSteps[this._currentStepIndex], ref completedSynchronously);
                                    if (completedSynchronously)
                                    {
                                        this._numSyncStepCalls++;
                                        goto Label_0045;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (context2 != null)
                            {
                                try
                                {
                                    context2.Leave();
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
                if (flag)
                {
                    context.Unroot();
                    application.AsyncResult.Complete(this._numStepCalls == this._numSyncStepCalls, null, null);
                    application.ReleaseAppInstance();
                }
            }
        }

        internal class AsyncAppEventHandler
        {
            private ArrayList _beginHandlers = new ArrayList();
            private int _count = 0;
            private ArrayList _endHandlers = new ArrayList();
            private ArrayList _stateObjects = new ArrayList();

            internal AsyncAppEventHandler()
            {
            }

            internal void Add(BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
            {
                this._beginHandlers.Add(beginHandler);
                this._endHandlers.Add(endHandler);
                this._stateObjects.Add(state);
                this._count++;
            }

            internal void CreateExecutionSteps(HttpApplication app, ArrayList steps)
            {
                for (int i = 0; i < this._count; i++)
                {
                    steps.Add(new HttpApplication.AsyncEventExecutionStep(app, (BeginEventHandler) this._beginHandlers[i], (EndEventHandler) this._endHandlers[i], this._stateObjects[i]));
                }
            }

            internal void Reset()
            {
                this._count = 0;
                this._beginHandlers.Clear();
                this._endHandlers.Clear();
                this._stateObjects.Clear();
            }

            internal int Count
            {
                get
                {
                    return this._count;
                }
            }
        }

        internal class AsyncAppEventHandlersTable
        {
            private Hashtable _table;

            internal void AddHandler(object eventId, BeginEventHandler beginHandler, EndEventHandler endHandler, object state, RequestNotification requestNotification, bool isPost, HttpApplication app)
            {
                if (this._table == null)
                {
                    this._table = new Hashtable();
                }
                HttpApplication.AsyncAppEventHandler handler = (HttpApplication.AsyncAppEventHandler) this._table[eventId];
                if (handler == null)
                {
                    handler = new HttpApplication.AsyncAppEventHandler();
                    this._table[eventId] = handler;
                }
                handler.Add(beginHandler, endHandler, state);
                if (HttpRuntime.UseIntegratedPipeline)
                {
                    HttpApplication.AsyncEventExecutionStep step = new HttpApplication.AsyncEventExecutionStep(app, beginHandler, endHandler, state);
                    app.AddEventMapping(app.CurrentModuleCollectionKey, requestNotification, isPost, step);
                }
            }

            internal HttpApplication.AsyncAppEventHandler this[object eventId]
            {
                get
                {
                    if (this._table == null)
                    {
                        return null;
                    }
                    return (HttpApplication.AsyncAppEventHandler) this._table[eventId];
                }
            }
        }

        internal class AsyncEventExecutionStep : HttpApplication.IExecutionStep
        {
            private HttpApplication _application;
            private BeginEventHandler _beginHandler;
            private AsyncCallback _completionCallback;
            private EndEventHandler _endHandler;
            private object _state;
            private bool _sync;
            private string _targetTypeStr;

            internal AsyncEventExecutionStep(HttpApplication app, BeginEventHandler beginHandler, EndEventHandler endHandler, object state) : this(app, beginHandler, endHandler, state, HttpRuntime.UseIntegratedPipeline)
            {
            }

            internal AsyncEventExecutionStep(HttpApplication app, BeginEventHandler beginHandler, EndEventHandler endHandler, object state, bool useIntegratedPipeline)
            {
                this._application = app;
                this._beginHandler = beginHandler;
                this._endHandler = endHandler;
                this._state = state;
                this._completionCallback = new AsyncCallback(this.OnAsyncEventCompletion);
            }

            private void OnAsyncEventCompletion(IAsyncResult ar)
            {
                if (!ar.CompletedSynchronously)
                {
                    HttpContext context = this._application.Context;
                    Exception error = null;
                    try
                    {
                        this._endHandler(ar);
                    }
                    catch (Exception exception2)
                    {
                        error = exception2;
                    }
                    if (EtwTrace.IsTraceEnabled(5, 2))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_LEAVE, context.WorkerRequest, this._targetTypeStr);
                    }
                    context.SetStartTime();
                    if (HttpRuntime.IsLegacyCas)
                    {
                        this.ResumeStepsWithAssert(error);
                    }
                    else
                    {
                        this.ResumeSteps(error);
                    }
                }
            }

            private void ResumeSteps(Exception error)
            {
                this._application.ResumeStepsFromThreadPoolThread(error);
            }

            [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
            private void ResumeStepsWithAssert(Exception error)
            {
                this.ResumeSteps(error);
            }

            void HttpApplication.IExecutionStep.Execute()
            {
                this._sync = false;
                if (EtwTrace.IsTraceEnabled(5, 2))
                {
                    this._targetTypeStr = this._beginHandler.Method.ReflectedType.ToString();
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_ENTER, this._application.Context.WorkerRequest, this._targetTypeStr);
                }
                IAsyncResult ar = this._beginHandler(this._application, this._application.AppEvent, this._completionCallback, this._state);
                if (ar.CompletedSynchronously)
                {
                    this._sync = true;
                    this._endHandler(ar);
                    if (EtwTrace.IsTraceEnabled(5, 2))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_LEAVE, this._application.Context.WorkerRequest, this._targetTypeStr);
                    }
                }
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                get
                {
                    return this._sync;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                get
                {
                    return false;
                }
            }
        }

        internal class CallFilterExecutionStep : HttpApplication.IExecutionStep
        {
            private HttpApplication _application;

            internal CallFilterExecutionStep(HttpApplication app)
            {
                this._application = app;
            }

            void HttpApplication.IExecutionStep.Execute()
            {
                try
                {
                    this._application.Context.Response.FilterOutput();
                }
                finally
                {
                    if (HttpRuntime.UseIntegratedPipeline && (this._application.Context.CurrentNotification == RequestNotification.UpdateRequestCache))
                    {
                        this._application.Context.DisableNotifications(RequestNotification.LogRequest, 0);
                    }
                }
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                get
                {
                    return true;
                }
            }
        }

        internal class CallHandlerExecutionStep : HttpApplication.IExecutionStep
        {
            private HttpApplication _application;
            private AsyncCallback _completionCallback;
            private IHttpAsyncHandler _handler;
            private bool _sync;

            internal CallHandlerExecutionStep(HttpApplication app)
            {
                this._application = app;
                this._completionCallback = new AsyncCallback(this.OnAsyncHandlerCompletion);
            }

            private void OnAsyncHandlerCompletion(IAsyncResult ar)
            {
                if (!ar.CompletedSynchronously)
                {
                    HttpContext context = this._application.Context;
                    Exception error = null;
                    try
                    {
                        try
                        {
                            this._handler.EndProcessRequest(ar);
                        }
                        finally
                        {
                            context.Response.GenerateResponseHeadersForHandler();
                        }
                    }
                    catch (Exception exception2)
                    {
                        if ((exception2 is ThreadAbortException) || ((exception2.InnerException != null) && (exception2.InnerException is ThreadAbortException)))
                        {
                            this._application.CompleteRequest();
                        }
                        else
                        {
                            error = exception2;
                        }
                    }
                    if (EtwTrace.IsTraceEnabled(4, 4))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_HTTPHANDLER_LEAVE, context.WorkerRequest);
                    }
                    this._handler = null;
                    context.SetStartTime();
                    if (HttpRuntime.IsLegacyCas)
                    {
                        this.ResumeStepsWithAssert(error);
                    }
                    else
                    {
                        this.ResumeSteps(error);
                    }
                }
            }

            private void ResumeSteps(Exception error)
            {
                this._application.ResumeStepsFromThreadPoolThread(error);
            }

            [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
            private void ResumeStepsWithAssert(Exception error)
            {
                this.ResumeSteps(error);
            }

            void HttpApplication.IExecutionStep.Execute()
            {
                HttpContext context = this._application.Context;
                IHttpHandler handler = context.Handler;
                if (EtwTrace.IsTraceEnabled(4, 4))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_HTTPHANDLER_ENTER, context.WorkerRequest);
                }
                if ((handler != null) && HttpRuntime.UseIntegratedPipeline)
                {
                    IIS7WorkerRequest workerRequest = context.WorkerRequest as IIS7WorkerRequest;
                    if ((workerRequest != null) && workerRequest.IsHandlerExecutionDenied())
                    {
                        this._sync = true;
                        HttpException exception = new HttpException(0x193, System.Web.SR.GetString("Handler_access_denied"));
                        exception.SetFormatter(new PageForbiddenErrorFormatter(context.Request.Path, System.Web.SR.GetString("Handler_access_denied")));
                        throw exception;
                    }
                }
                if (handler == null)
                {
                    this._sync = true;
                }
                else if (handler is IHttpAsyncHandler)
                {
                    IHttpAsyncHandler handler2 = (IHttpAsyncHandler) handler;
                    this._sync = false;
                    this._handler = handler2;
                    IAsyncResult result = handler2.BeginProcessRequest(context, this._completionCallback, null);
                    if (result.CompletedSynchronously)
                    {
                        this._sync = true;
                        this._handler = null;
                        try
                        {
                            handler2.EndProcessRequest(result);
                        }
                        finally
                        {
                            context.Response.GenerateResponseHeadersForHandler();
                        }
                        if (EtwTrace.IsTraceEnabled(4, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_HTTPHANDLER_LEAVE, context.WorkerRequest);
                        }
                    }
                }
                else
                {
                    this._sync = true;
                    context.SyncContext.SetSyncCaller();
                    try
                    {
                        handler.ProcessRequest(context);
                    }
                    finally
                    {
                        context.SyncContext.ResetSyncCaller();
                        if (EtwTrace.IsTraceEnabled(4, 4))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_HTTPHANDLER_LEAVE, context.WorkerRequest);
                        }
                        context.Response.GenerateResponseHeadersForHandler();
                    }
                }
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                get
                {
                    return this._sync;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                get
                {
                    return !(this._application.Context.Handler is IHttpAsyncHandler);
                }
            }
        }

        internal class CancelModuleException
        {
            private bool _timeout;

            internal CancelModuleException(bool timeout)
            {
                this._timeout = timeout;
            }

            internal bool Timeout
            {
                get
                {
                    return this._timeout;
                }
            }
        }

        internal interface IExecutionStep
        {
            void Execute();

            bool CompletedSynchronously { get; }

            bool IsCancellable { get; }
        }

        internal class MapHandlerExecutionStep : HttpApplication.IExecutionStep
        {
            private HttpApplication _application;

            internal MapHandlerExecutionStep(HttpApplication app)
            {
                this._application = app;
            }

            void HttpApplication.IExecutionStep.Execute()
            {
                HttpContext context = this._application.Context;
                HttpRequest request = context.Request;
                if (EtwTrace.IsTraceEnabled(5, 1))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_MAPHANDLER_ENTER, context.WorkerRequest);
                }
                context.Handler = this._application.MapHttpHandler(context, request.RequestType, request.FilePathObject, request.PhysicalPathInternal, false);
                if (EtwTrace.IsTraceEnabled(5, 1))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_MAPHANDLER_LEAVE, context.WorkerRequest);
                }
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                get
                {
                    return false;
                }
            }
        }

        internal class MaterializeHandlerExecutionStep : HttpApplication.IExecutionStep
        {
            private HttpApplication _application;

            internal MaterializeHandlerExecutionStep(HttpApplication app)
            {
                this._application = app;
            }

            void HttpApplication.IExecutionStep.Execute()
            {
                HttpContext httpContext = this._application.Context;
                HttpRequest request = httpContext.Request;
                IHttpHandler handler = null;
                string managedHandlerType = null;
                if (EtwTrace.IsTraceEnabled(5, 1))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_MAPHANDLER_ENTER, httpContext.WorkerRequest);
                }
                IIS7WorkerRequest workerRequest = httpContext.WorkerRequest as IIS7WorkerRequest;
                if (httpContext.RemapHandlerInstance != null)
                {
                    httpContext.Handler = httpContext.RemapHandlerInstance;
                }
                else if (request.RewrittenUrl != null)
                {
                    bool flag;
                    managedHandlerType = workerRequest.ReMapHandlerAndGetHandlerTypeString(httpContext, request.Path, out flag);
                    if (!flag)
                    {
                        throw new HttpException(0x194, System.Web.SR.GetString("Http_handler_not_found_for_request_type", new object[] { request.RequestType }));
                    }
                }
                else
                {
                    managedHandlerType = workerRequest.GetManagedHandlerType();
                }
                if (!string.IsNullOrEmpty(managedHandlerType))
                {
                    IHttpHandlerFactory factory = this._application.GetFactory(managedHandlerType);
                    string physicalPathInternal = request.PhysicalPathInternal;
                    try
                    {
                        handler = factory.GetHandler(httpContext, request.RequestType, request.FilePath, physicalPathInternal);
                    }
                    catch (FileNotFoundException exception)
                    {
                        if (HttpRuntime.HasPathDiscoveryPermission(physicalPathInternal))
                        {
                            throw new HttpException(0x194, null, exception);
                        }
                        throw new HttpException(0x194, null);
                    }
                    catch (DirectoryNotFoundException exception2)
                    {
                        if (HttpRuntime.HasPathDiscoveryPermission(physicalPathInternal))
                        {
                            throw new HttpException(0x194, null, exception2);
                        }
                        throw new HttpException(0x194, null);
                    }
                    catch (PathTooLongException exception3)
                    {
                        if (HttpRuntime.HasPathDiscoveryPermission(physicalPathInternal))
                        {
                            throw new HttpException(0x19e, null, exception3);
                        }
                        throw new HttpException(0x19e, null);
                    }
                    httpContext.Handler = handler;
                    if (this._application._handlerRecycleList == null)
                    {
                        this._application._handlerRecycleList = new ArrayList();
                    }
                    this._application._handlerRecycleList.Add(new HandlerWithFactory(handler, factory));
                }
                if (EtwTrace.IsTraceEnabled(5, 1))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_MAPHANDLER_LEAVE, httpContext.WorkerRequest);
                }
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                get
                {
                    return false;
                }
            }
        }

        internal class NoopExecutionStep : HttpApplication.IExecutionStep
        {
            internal NoopExecutionStep()
            {
            }

            void HttpApplication.IExecutionStep.Execute()
            {
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                get
                {
                    return false;
                }
            }
        }

        internal class PipelineStepManager : HttpApplication.StepManager
        {
            private WaitCallback _resumeStepsWaitCallback;
            private bool _validateInputCalled;
            private bool _validatePathCalled;

            internal PipelineStepManager(HttpApplication app) : base(app)
            {
            }

            internal override void BuildSteps(WaitCallback stepCallback)
            {
                HttpApplication app = base._application;
                HttpApplication.IExecutionStep step = new HttpApplication.MaterializeHandlerExecutionStep(app);
                app.AddEventMapping("ManagedPipelineHandler", RequestNotification.MapRequestHandler, false, step);
                HttpApplication.IExecutionStep step2 = new HttpApplication.CallHandlerExecutionStep(app);
                app.AddEventMapping("ManagedPipelineHandler", RequestNotification.ExecuteRequestHandler, false, step2);
                HttpApplication.IExecutionStep step3 = new HttpApplication.CallFilterExecutionStep(app);
                app.AddEventMapping("AspNetFilterModule", RequestNotification.UpdateRequestCache, false, step3);
                app.AddEventMapping("AspNetFilterModule", RequestNotification.LogRequest, false, step3);
                this._resumeStepsWaitCallback = stepCallback;
            }

            internal override void InitRequest()
            {
                base._requestCompleted = false;
                this._validatePathCalled = false;
                this._validateInputCalled = false;
            }

            [DebuggerStepperBoundary]
            internal override void ResumeSteps(Exception error)
            {
                HttpContext context = base._application.Context;
                IIS7WorkerRequest workerRequest = context.WorkerRequest as IIS7WorkerRequest;
                AspNetSynchronizationContext syncContext = context.SyncContext;
                RequestNotificationStatus status = RequestNotificationStatus.Continue;
                HttpApplication.ThreadContext indicateCompletionContext = null;
                bool synchronous = false;
                bool flag2 = false;
                bool completedSynchronously = false;
                int num = base._application.CurrentModuleContainer.GetEventCount(context.CurrentNotification, context.IsPostNotification) - 1;
                bool isReEntry = context.NotificationContext.IsReEntry;
                if (!isReEntry)
                {
                    Monitor.Enter(base._application);
                }
                try
                {
                    bool locked = false;
                    try
                    {
                        if (!isReEntry)
                        {
                            if (context.InIndicateCompletion)
                            {
                                indicateCompletionContext = context.IndicateCompletionContext;
                                if (context.UsesImpersonation)
                                {
                                    indicateCompletionContext.SetImpersonationContext();
                                }
                            }
                            else
                            {
                                indicateCompletionContext = base._application.OnThreadEnter(context.UsesImpersonation);
                            }
                        }
                    Label_00A8:
                        if (syncContext.Error != null)
                        {
                            error = syncContext.Error;
                            syncContext.ClearError();
                        }
                        if (error != null)
                        {
                            base._application.RecordError(error);
                            error = null;
                        }
                        if (!this._validateInputCalled || !this._validatePathCalled)
                        {
                            error = this.ValidateHelper(context);
                            if (error != null)
                            {
                                goto Label_00A8;
                            }
                        }
                        if (syncContext.PendingOperationsCount > 0)
                        {
                            context.NotificationContext.PendingAsyncCompletion = true;
                            syncContext.SetLastCompletionWorkItem(this._resumeStepsWaitCallback);
                        }
                        else
                        {
                            bool flag6 = (((context.NotificationContext.Error != null) || context.NotificationContext.RequestCompleted) && (context.CurrentNotification != RequestNotification.LogRequest)) && (context.CurrentNotification != RequestNotification.EndRequest);
                            if (flag6 || (context.CurrentModuleEventIndex == num))
                            {
                                status = flag6 ? RequestNotificationStatus.FinishRequest : RequestNotificationStatus.Continue;
                                if (context.NotificationContext.PendingAsyncCompletion)
                                {
                                    context.Response.SyncStatusIntegrated();
                                    context.NotificationContext.PendingAsyncCompletion = false;
                                    synchronous = false;
                                    flag2 = true;
                                    goto Label_0337;
                                }
                                if (flag6 || (UnsafeIISMethods.MgdGetNextNotification(workerRequest.RequestContext, RequestNotificationStatus.Continue) != 1))
                                {
                                    synchronous = true;
                                    flag2 = true;
                                    goto Label_0337;
                                }
                                int currentModuleIndex = 0;
                                bool isPostNotification = false;
                                int currentNotification = 0;
                                UnsafeIISMethods.MgdGetCurrentNotificationInfo(workerRequest.RequestContext, out currentModuleIndex, out isPostNotification, out currentNotification);
                                context.CurrentModuleIndex = currentModuleIndex;
                                context.IsPostNotification = isPostNotification;
                                context.CurrentNotification = (RequestNotification) currentNotification;
                                context.CurrentModuleEventIndex = -1;
                                num = base._application.CurrentModuleContainer.GetEventCount(context.CurrentNotification, context.IsPostNotification) - 1;
                            }
                            context.CurrentModuleEventIndex++;
                            HttpApplication.IExecutionStep step = base._application.CurrentModuleContainer.GetNextEvent(context.CurrentNotification, context.IsPostNotification, context.CurrentModuleEventIndex);
                            context.SyncContext.Enable();
                            completedSynchronously = false;
                            error = base._application.ExecuteStep(step, ref completedSynchronously);
                            if (!completedSynchronously)
                            {
                                base._application.AcquireNotifcationContextLock(ref locked);
                                context.NotificationContext.PendingAsyncCompletion = true;
                            }
                            else
                            {
                                context.Response.SyncStatusIntegrated();
                                goto Label_00A8;
                            }
                        }
                    }
                    finally
                    {
                        if (locked)
                        {
                            base._application.ReleaseNotifcationContextLock();
                        }
                        if (indicateCompletionContext != null)
                        {
                            if (context.InIndicateCompletion)
                            {
                                if (synchronous)
                                {
                                    indicateCompletionContext.Synchronize(context);
                                    indicateCompletionContext.UndoImpersonationContext();
                                    goto Label_0336;
                                }
                                if (indicateCompletionContext.HasLeaveBeenCalled)
                                {
                                    goto Label_0336;
                                }
                                lock (indicateCompletionContext)
                                {
                                    if (!indicateCompletionContext.HasLeaveBeenCalled)
                                    {
                                        indicateCompletionContext.Leave();
                                        context.IndicateCompletionContext = null;
                                        context.InIndicateCompletion = false;
                                    }
                                    goto Label_0336;
                                }
                            }
                            if (synchronous)
                            {
                                indicateCompletionContext.Synchronize(context);
                                context.IndicateCompletionContext = indicateCompletionContext;
                                indicateCompletionContext.UndoImpersonationContext();
                            }
                            else
                            {
                                indicateCompletionContext.Leave();
                            }
                        }
                    Label_0336:;
                    }
                Label_0337:
                    if (flag2)
                    {
                        base._application.AsyncResult.Complete(synchronous, null, null, status);
                    }
                }
                finally
                {
                    if (!isReEntry)
                    {
                        Monitor.Exit(base._application);
                    }
                }
            }

            private Exception ValidateHelper(HttpContext context)
            {
                if (!this._validateInputCalled)
                {
                    this._validateInputCalled = true;
                    try
                    {
                        context.Request.ValidateInputIfRequiredByConfig();
                    }
                    catch (Exception exception)
                    {
                        return exception;
                    }
                }
                if (!this._validatePathCalled)
                {
                    this._validatePathCalled = true;
                    try
                    {
                        context.ValidatePath();
                    }
                    catch (Exception exception2)
                    {
                        return exception2;
                    }
                }
                return null;
            }
        }

        internal class SendResponseExecutionStep : HttpApplication.IExecutionStep
        {
            private HttpApplication _application;
            private EventHandler _handler;
            private bool _isHeaders;

            internal SendResponseExecutionStep(HttpApplication app, EventHandler handler, bool isHeaders)
            {
                this._application = app;
                this._handler = handler;
                this._isHeaders = isHeaders;
            }

            void HttpApplication.IExecutionStep.Execute()
            {
                if ((this._application.Context.IsSendResponseHeaders && this._isHeaders) || !this._isHeaders)
                {
                    string str = null;
                    if (this._handler != null)
                    {
                        if (EtwTrace.IsTraceEnabled(5, 2))
                        {
                            str = this._handler.Method.ReflectedType.ToString();
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_ENTER, this._application.Context.WorkerRequest, str);
                        }
                        this._handler(this._application, this._application.AppEvent);
                        if (EtwTrace.IsTraceEnabled(5, 2))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_LEAVE, this._application.Context.WorkerRequest, str);
                        }
                    }
                }
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                get
                {
                    return true;
                }
            }
        }

        internal abstract class StepManager
        {
            protected HttpApplication _application;
            protected bool _requestCompleted;

            internal StepManager(HttpApplication application)
            {
                this._application = application;
            }

            internal abstract void BuildSteps(WaitCallback stepCallback);
            internal void CompleteRequest()
            {
                this._requestCompleted = true;
                if (HttpRuntime.UseIntegratedPipeline)
                {
                    HttpContext context = this._application.Context;
                    if ((context != null) && (context.NotificationContext != null))
                    {
                        context.NotificationContext.RequestCompleted = true;
                    }
                }
            }

            internal abstract void InitRequest();
            internal abstract void ResumeSteps(Exception error);

            internal bool IsCompleted
            {
                get
                {
                    return this._requestCompleted;
                }
            }
        }

        internal class SyncEventExecutionStep : HttpApplication.IExecutionStep
        {
            private HttpApplication _application;
            private EventHandler _handler;

            internal SyncEventExecutionStep(HttpApplication app, EventHandler handler)
            {
                this._application = app;
                this._handler = handler;
            }

            void HttpApplication.IExecutionStep.Execute()
            {
                string str = null;
                if (this._handler != null)
                {
                    if (EtwTrace.IsTraceEnabled(5, 2))
                    {
                        str = this._handler.Method.ReflectedType.ToString();
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_ENTER, this._application.Context.WorkerRequest, str);
                    }
                    this._handler(this._application, this._application.AppEvent);
                    if (EtwTrace.IsTraceEnabled(5, 2))
                    {
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_PIPELINE_LEAVE, this._application.Context.WorkerRequest, str);
                    }
                }
            }

            internal EventHandler Handler
            {
                get
                {
                    return this._handler;
                }
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                get
                {
                    return true;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                get
                {
                    return true;
                }
            }
        }

        internal class ThreadContext
        {
            private HttpContext _context;
            private bool _hasLeaveBeenCalled;
            private ImpersonationContext _impersonationContext;
            private HttpContext _savedContext;
            private CultureInfo _savedCulture;
            private IPrincipal _savedPrincipal;
            private SynchronizationContext _savedSynchronizationContext;
            private CultureInfo _savedUICulture;
            private bool _setThread;
            private CultureInfo _setThreadCulture;
            private CultureInfo _setThreadUICulture;

            internal ThreadContext(HttpContext context)
            {
                this._context = context;
            }

            internal void Enter(bool setImpersonationContext)
            {
                this._savedContext = DisposableHttpContextWrapper.SwitchContext(this._context);
                if (setImpersonationContext)
                {
                    this.SetImpersonationContext();
                }
                this._savedSynchronizationContext = AsyncOperationManager.SynchronizationContext;
                AsyncOperationManager.SynchronizationContext = this._context.SyncContext;
                Guid requestTraceIdentifier = this._context.WorkerRequest.RequestTraceIdentifier;
                if (!(requestTraceIdentifier == Guid.Empty))
                {
                    CallContext.LogicalSetData("E2ETrace.ActivityID", requestTraceIdentifier);
                }
                this._context.ResetSqlDependencyCookie();
                this._savedPrincipal = Thread.CurrentPrincipal;
                HttpApplication.SetCurrentPrincipalWithAssert(this._context.User);
                this.SetRequestLevelCulture(this._context);
                if (this._context.CurrentThread == null)
                {
                    this._setThread = true;
                    this._context.CurrentThread = Thread.CurrentThread;
                }
            }

            internal void Leave()
            {
                this._hasLeaveBeenCalled = true;
                if (this._setThread)
                {
                    this._context.CurrentThread = null;
                }
                HttpApplicationFactory.ApplicationState.EnsureUnLock();
                this.UndoImpersonationContext();
                this.RestoreRequestLevelCulture();
                AsyncOperationManager.SynchronizationContext = this._savedSynchronizationContext;
                HttpApplication.SetCurrentPrincipalWithAssert(this._savedPrincipal);
                this._context.RemoveSqlDependencyCookie();
                DisposableHttpContextWrapper.SwitchContext(this._savedContext);
                this._savedContext = null;
            }

            private void RestoreRequestLevelCulture()
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
                if (this._savedCulture != null)
                {
                    if (currentCulture != this._savedCulture)
                    {
                        HttpRuntime.SetCurrentThreadCultureWithAssert(this._savedCulture);
                        if (this._context != null)
                        {
                            this._context.DynamicCulture = currentCulture;
                        }
                    }
                    this._savedCulture = null;
                }
                if (this._savedUICulture != null)
                {
                    if (currentUICulture != this._savedUICulture)
                    {
                        Thread.CurrentThread.CurrentUICulture = this._savedUICulture;
                        if (this._context != null)
                        {
                            this._context.DynamicUICulture = currentUICulture;
                        }
                    }
                    this._savedUICulture = null;
                }
            }

            internal void SetImpersonationContext()
            {
                if (this._impersonationContext == null)
                {
                    this._impersonationContext = new ClientImpersonationContext(this._context);
                }
            }

            private void SetRequestLevelCulture(HttpContext context)
            {
                CultureInfo cultureInfo = null;
                CultureInfo dynamicUICulture = null;
                GlobalizationSection globalization = RuntimeConfig.GetConfig(context).Globalization;
                if (!string.IsNullOrEmpty(globalization.Culture))
                {
                    cultureInfo = context.CultureFromConfig(globalization.Culture, true);
                }
                if (!string.IsNullOrEmpty(globalization.UICulture))
                {
                    dynamicUICulture = context.CultureFromConfig(globalization.UICulture, false);
                }
                if (context.DynamicCulture != null)
                {
                    cultureInfo = context.DynamicCulture;
                }
                if (context.DynamicUICulture != null)
                {
                    dynamicUICulture = context.DynamicUICulture;
                }
                Page currentHandler = context.CurrentHandler as Page;
                if (currentHandler != null)
                {
                    if (currentHandler.DynamicCulture != null)
                    {
                        cultureInfo = currentHandler.DynamicCulture;
                    }
                    if (currentHandler.DynamicUICulture != null)
                    {
                        dynamicUICulture = currentHandler.DynamicUICulture;
                    }
                }
                this._savedCulture = Thread.CurrentThread.CurrentCulture;
                this._savedUICulture = Thread.CurrentThread.CurrentUICulture;
                if ((cultureInfo != null) && (cultureInfo != Thread.CurrentThread.CurrentCulture))
                {
                    HttpRuntime.SetCurrentThreadCultureWithAssert(cultureInfo);
                    this._setThreadCulture = cultureInfo;
                }
                if ((dynamicUICulture != null) && (dynamicUICulture != Thread.CurrentThread.CurrentUICulture))
                {
                    Thread.CurrentThread.CurrentUICulture = dynamicUICulture;
                    this._setThreadUICulture = dynamicUICulture;
                }
            }

            internal void Synchronize(HttpContext context)
            {
                context.DynamicCulture = Thread.CurrentThread.CurrentCulture;
                context.DynamicUICulture = Thread.CurrentThread.CurrentUICulture;
            }

            internal void UndoImpersonationContext()
            {
                if (this._impersonationContext != null)
                {
                    this._impersonationContext.Undo();
                    this._impersonationContext = null;
                }
            }

            internal bool HasLeaveBeenCalled
            {
                get
                {
                    return this._hasLeaveBeenCalled;
                }
            }
        }

        internal class UrlMappingsExecutionStep : HttpApplication.IExecutionStep
        {
            private HttpApplication _application;

            internal UrlMappingsExecutionStep(HttpApplication app)
            {
                this._application = app;
            }

            void HttpApplication.IExecutionStep.Execute()
            {
                UrlMappingsModule.UrlMappingRewritePath(this._application.Context);
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                get
                {
                    return false;
                }
            }
        }

        internal class ValidatePathExecutionStep : HttpApplication.IExecutionStep
        {
            private HttpApplication _application;

            internal ValidatePathExecutionStep(HttpApplication app)
            {
                this._application = app;
            }

            void HttpApplication.IExecutionStep.Execute()
            {
                this._application.Context.ValidatePath();
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                get
                {
                    return false;
                }
            }
        }

        internal class ValidateRequestExecutionStep : HttpApplication.IExecutionStep
        {
            private HttpApplication _application;

            internal ValidateRequestExecutionStep(HttpApplication app)
            {
                this._application = app;
            }

            void HttpApplication.IExecutionStep.Execute()
            {
                this._application.Context.Request.ValidateInputIfRequiredByConfig();
            }

            bool HttpApplication.IExecutionStep.CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            bool HttpApplication.IExecutionStep.IsCancellable
            {
                get
                {
                    return false;
                }
            }
        }
    }
}

