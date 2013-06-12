namespace System.Web
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Web.Util;

    internal class HttpApplicationFactory
    {
        private string _appFilename;
        private bool _appOnEndCalled;
        private bool _appOnStartCalled;
        private static IHttpHandler _customApplication;
        private MethodInfo[] _eventHandlerMethods;
        private ICollection _fileDependencies;
        private Stack _freeList = new Stack();
        private bool _inited;
        private const int _maxFreeSpecialAppInstances = 20;
        private int _minFreeAppInstances;
        private int _numFreeAppInstances;
        private int _numFreeSpecialAppInstances;
        private MethodInfo _onEndMethod;
        private int _onEndParamCount;
        private MethodInfo _onStartMethod;
        private int _onStartParamCount;
        private EventHandler _sessionOnEndEventHandlerAspCompatHelper;
        private MethodInfo _sessionOnEndMethod;
        private int _sessionOnEndParamCount;
        private Stack _specialFreeList = new Stack();
        private HttpApplicationState _state;
        private static HttpApplicationFactory _theApplicationFactory = new HttpApplicationFactory();
        private Type _theApplicationType;
        internal const string applicationFileName = "global.asax";

        internal HttpApplicationFactory()
        {
            this._sessionOnEndEventHandlerAspCompatHelper = new EventHandler(this.SessionOnEndEventHandlerAspCompatHelper);
        }

        private void CompileApplication()
        {
            this._theApplicationType = BuildManager.GetGlobalAsaxType();
            BuildResultCompiledGlobalAsaxType globalAsaxBuildResult = BuildManager.GetGlobalAsaxBuildResult();
            if (globalAsaxBuildResult != null)
            {
                if (globalAsaxBuildResult.HasAppOrSessionObjects)
                {
                    this.GetAppStateByParsingGlobalAsax();
                }
                this._fileDependencies = globalAsaxBuildResult.VirtualPathDependencies;
            }
            if (this._state == null)
            {
                this._state = new HttpApplicationState();
            }
            this.ReflectOnApplicationType();
        }

        private void Dispose()
        {
            ArrayList list = new ArrayList();
            lock (this._freeList)
            {
                while (this._numFreeAppInstances > 0)
                {
                    list.Add(this._freeList.Pop());
                    this._numFreeAppInstances--;
                }
            }
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                ((HttpApplication) list[i]).DisposeInternal();
            }
            if (this._appOnStartCalled && !this._appOnEndCalled)
            {
                lock (this)
                {
                    if (!this._appOnEndCalled)
                    {
                        this.FireApplicationOnEnd();
                        this._appOnEndCalled = true;
                    }
                }
            }
        }

        internal static void EndApplication()
        {
            _theApplicationFactory.Dispose();
        }

        internal static void EndSession(HttpSessionState session, object eventSource, EventArgs eventArgs)
        {
            _theApplicationFactory.FireSessionOnEnd(session, eventSource, eventArgs);
        }

        private void EnsureAppStartCalled(HttpContext context)
        {
            if (!this._appOnStartCalled)
            {
                lock (this)
                {
                    if (!this._appOnStartCalled)
                    {
                        using (new DisposableHttpContextWrapper(context))
                        {
                            WebBaseEvent.RaiseSystemEvent(this, 0x3e9);
                            this.FireApplicationOnStart(context);
                        }
                        this._appOnStartCalled = true;
                    }
                }
            }
        }

        internal static void EnsureAppStartCalledForIntegratedMode(HttpContext context, HttpApplication app)
        {
            if (!_theApplicationFactory._appOnStartCalled)
            {
                Exception innerException = null;
                lock (_theApplicationFactory)
                {
                    if (!_theApplicationFactory._appOnStartCalled)
                    {
                        using (new DisposableHttpContextWrapper(context))
                        {
                            WebBaseEvent.RaiseSystemEvent(_theApplicationFactory, 0x3e9);
                            if (_theApplicationFactory._onStartMethod != null)
                            {
                                app.ProcessSpecialRequest(context, _theApplicationFactory._onStartMethod, _theApplicationFactory._onStartParamCount, _theApplicationFactory, EventArgs.Empty, null);
                            }
                        }
                    }
                    _theApplicationFactory._appOnStartCalled = true;
                    innerException = context.Error;
                }
                if (innerException != null)
                {
                    throw new HttpException(innerException.Message, innerException);
                }
            }
        }

        private void EnsureInited()
        {
            if (!this._inited)
            {
                lock (this)
                {
                    if (!this._inited)
                    {
                        this.Init();
                        this._inited = true;
                    }
                }
            }
        }

        private void FireApplicationOnEnd()
        {
            if (this._onEndMethod != null)
            {
                HttpApplication specialApplicationInstance = this.GetSpecialApplicationInstance();
                specialApplicationInstance.ProcessSpecialRequest(null, this._onEndMethod, this._onEndParamCount, this, EventArgs.Empty, null);
                this.RecycleSpecialApplicationInstance(specialApplicationInstance);
            }
        }

        private void FireApplicationOnError(Exception error)
        {
            HttpApplication specialApplicationInstance = this.GetSpecialApplicationInstance();
            specialApplicationInstance.RaiseErrorWithoutContext(error);
            this.RecycleSpecialApplicationInstance(specialApplicationInstance);
        }

        private void FireApplicationOnStart(HttpContext context)
        {
            if (this._onStartMethod != null)
            {
                HttpApplication specialApplicationInstance = this.GetSpecialApplicationInstance();
                specialApplicationInstance.ProcessSpecialRequest(context, this._onStartMethod, this._onStartParamCount, this, EventArgs.Empty, null);
                this.RecycleSpecialApplicationInstance(specialApplicationInstance);
            }
        }

        private void FireSessionOnEnd(HttpSessionState session, object eventSource, EventArgs eventArgs)
        {
            if (this._sessionOnEndMethod != null)
            {
                HttpApplication specialApplicationInstance = this.GetSpecialApplicationInstance();
                if (AspCompatApplicationStep.AnyStaObjectsInSessionState(session) || HttpRuntime.ApartmentThreading)
                {
                    AspCompatSessionOnEndHelper source = new AspCompatSessionOnEndHelper(specialApplicationInstance, session, eventSource, eventArgs);
                    AspCompatApplicationStep.RaiseAspCompatEvent(null, specialApplicationInstance, session.SessionID, this._sessionOnEndEventHandlerAspCompatHelper, source, EventArgs.Empty);
                }
                else
                {
                    specialApplicationInstance.ProcessSpecialRequest(null, this._sessionOnEndMethod, this._sessionOnEndParamCount, eventSource, eventArgs, session);
                }
                this.RecycleSpecialApplicationInstance(specialApplicationInstance);
            }
        }

        internal static string GetApplicationFile()
        {
            return Path.Combine(HttpRuntime.AppDomainAppPathInternal, "global.asax");
        }

        internal static IHttpHandler GetApplicationInstance(HttpContext context)
        {
            if (_customApplication != null)
            {
                return _customApplication;
            }
            if (context.Request.IsDebuggingRequest)
            {
                return new HttpDebugHandler();
            }
            _theApplicationFactory.EnsureInited();
            _theApplicationFactory.EnsureAppStartCalled(context);
            return _theApplicationFactory.GetNormalApplicationInstance(context);
        }

        private void GetAppStateByParsingGlobalAsax()
        {
            using (new ApplicationImpersonationContext())
            {
                if (FileUtil.FileExists(this._appFilename))
                {
                    ApplicationFileParser parser = new ApplicationFileParser();
                    AssemblySet referencedAssemblies = Util.GetReferencedAssemblies(this._theApplicationType.Assembly);
                    referencedAssemblies.Add(typeof(string).Assembly);
                    VirtualPath virtualPath = HttpRuntime.AppDomainAppVirtualPathObject.SimpleCombine("global.asax");
                    parser.Parse(referencedAssemblies, virtualPath);
                    this._state = new HttpApplicationState(parser.ApplicationObjects, parser.SessionObjects);
                }
            }
        }

        private HttpApplication GetNormalApplicationInstance(HttpContext context)
        {
            HttpApplication application = null;
            lock (this._freeList)
            {
                if (this._numFreeAppInstances > 0)
                {
                    application = (HttpApplication) this._freeList.Pop();
                    this._numFreeAppInstances--;
                    if (this._numFreeAppInstances < this._minFreeAppInstances)
                    {
                        this._minFreeAppInstances = this._numFreeAppInstances;
                    }
                }
            }
            if (application == null)
            {
                application = (HttpApplication) HttpRuntime.CreateNonPublicInstance(this._theApplicationType);
                using (new ApplicationImpersonationContext())
                {
                    application.InitInternal(context, this._state, this._eventHandlerMethods);
                }
            }
            return application;
        }

        internal static HttpApplication GetPipelineApplicationInstance(IntPtr appContext, HttpContext context)
        {
            _theApplicationFactory.EnsureInited();
            return _theApplicationFactory.GetSpecialApplicationInstance(appContext, context);
        }

        private HttpApplication GetSpecialApplicationInstance()
        {
            return this.GetSpecialApplicationInstance(IntPtr.Zero, null);
        }

        private HttpApplication GetSpecialApplicationInstance(IntPtr appContext, HttpContext context)
        {
            HttpApplication application = null;
            lock (this._specialFreeList)
            {
                if (this._numFreeSpecialAppInstances > 0)
                {
                    application = (HttpApplication) this._specialFreeList.Pop();
                    this._numFreeSpecialAppInstances--;
                }
            }
            if (application == null)
            {
                using (new DisposableHttpContextWrapper(context))
                {
                    application = (HttpApplication) HttpRuntime.CreateNonPublicInstance(this._theApplicationType);
                    using (new ApplicationImpersonationContext())
                    {
                        application.InitSpecial(this._state, this._eventHandlerMethods, appContext, context);
                    }
                }
            }
            return application;
        }

        private void Init()
        {
            if (_customApplication == null)
            {
                try
                {
                    try
                    {
                        this._appFilename = GetApplicationFile();
                        this.CompileApplication();
                    }
                    finally
                    {
                        this.SetupChangesMonitor();
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        private void OnAppFileChange(object sender, FileChangeEvent e)
        {
            HttpRuntime.ShutdownAppDomain(ApplicationShutdownReason.ChangeInGlobalAsax, "Change in GLOBAL.ASAX");
        }

        internal static void RaiseError(Exception error)
        {
            _theApplicationFactory.EnsureInited();
            _theApplicationFactory.FireApplicationOnError(error);
        }

        internal static void RecycleApplicationInstance(HttpApplication app)
        {
            _theApplicationFactory.RecycleNormalApplicationInstance(app);
        }

        private void RecycleNormalApplicationInstance(HttpApplication app)
        {
            lock (this._freeList)
            {
                this._freeList.Push(app);
                this._numFreeAppInstances++;
            }
        }

        internal static void RecyclePipelineApplicationInstance(HttpApplication app)
        {
            _theApplicationFactory.RecycleSpecialApplicationInstance(app);
        }

        private void RecycleSpecialApplicationInstance(HttpApplication app)
        {
            if (this._numFreeSpecialAppInstances < 20)
            {
                lock (this._specialFreeList)
                {
                    this._specialFreeList.Push(app);
                    this._numFreeSpecialAppInstances++;
                }
            }
        }

        private void ReflectOnApplicationType()
        {
            ArrayList list = new ArrayList();
            foreach (MethodInfo info in this._theApplicationType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (this.ReflectOnMethodInfoIfItLooksLikeEventHandler(info))
                {
                    list.Add(info);
                }
            }
            Type baseType = this._theApplicationType.BaseType;
            if ((baseType != null) && (baseType != typeof(HttpApplication)))
            {
                foreach (MethodInfo info2 in baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    if (info2.IsPrivate && this.ReflectOnMethodInfoIfItLooksLikeEventHandler(info2))
                    {
                        list.Add(info2);
                    }
                }
            }
            this._eventHandlerMethods = new MethodInfo[list.Count];
            for (int i = 0; i < this._eventHandlerMethods.Length; i++)
            {
                this._eventHandlerMethods[i] = (MethodInfo) list[i];
            }
        }

        private bool ReflectOnMethodInfoIfItLooksLikeEventHandler(MethodInfo m)
        {
            ParameterInfo[] parameters;
            string str;
            if (m.ReturnType == typeof(void))
            {
                parameters = m.GetParameters();
                switch (parameters.Length)
                {
                    case 0:
                        goto Label_0089;

                    case 2:
                        if (!(parameters[0].ParameterType != typeof(object)))
                        {
                            if ((parameters[1].ParameterType != typeof(EventArgs)) && !parameters[1].ParameterType.IsSubclassOf(typeof(EventArgs)))
                            {
                                return false;
                            }
                            goto Label_0089;
                        }
                        return false;
                }
            }
            return false;
        Label_0089:
            str = m.Name;
            int index = str.IndexOf('_');
            if ((index <= 0) || (index > (str.Length - 1)))
            {
                return false;
            }
            if (StringUtil.EqualsIgnoreCase(str, "Application_OnStart") || StringUtil.EqualsIgnoreCase(str, "Application_Start"))
            {
                this._onStartMethod = m;
                this._onStartParamCount = parameters.Length;
            }
            else if (StringUtil.EqualsIgnoreCase(str, "Application_OnEnd") || StringUtil.EqualsIgnoreCase(str, "Application_End"))
            {
                this._onEndMethod = m;
                this._onEndParamCount = parameters.Length;
            }
            else if (StringUtil.EqualsIgnoreCase(str, "Session_OnEnd") || StringUtil.EqualsIgnoreCase(str, "Session_End"))
            {
                this._sessionOnEndMethod = m;
                this._sessionOnEndParamCount = parameters.Length;
            }
            return true;
        }

        private void SessionOnEndEventHandlerAspCompatHelper(object eventSource, EventArgs eventArgs)
        {
            AspCompatSessionOnEndHelper helper = (AspCompatSessionOnEndHelper) eventSource;
            helper.Application.ProcessSpecialRequest(null, this._sessionOnEndMethod, this._sessionOnEndParamCount, helper.Source, helper.Args, helper.Session);
        }

        internal static void SetCustomApplication(IHttpHandler customApplication)
        {
            if (HttpRuntime.AppDomainAppIdInternal == null)
            {
                _customApplication = customApplication;
            }
        }

        private void SetupChangesMonitor()
        {
            FileChangeEventHandler callback = new FileChangeEventHandler(this.OnAppFileChange);
            HttpRuntime.FileChangesMonitor.StartMonitoringFile(this._appFilename, callback);
            if (this._fileDependencies != null)
            {
                foreach (string str in this._fileDependencies)
                {
                    HttpRuntime.FileChangesMonitor.StartMonitoringFile(HostingEnvironment.MapPathInternal(str), callback);
                }
            }
        }

        internal static void SetupFileChangeNotifications()
        {
            if (HttpRuntime.CodegenDirInternal != null)
            {
                _theApplicationFactory.EnsureInited();
            }
        }

        private void TrimApplicationInstanceFreeList()
        {
            int num = this._minFreeAppInstances;
            this._minFreeAppInstances = this._numFreeAppInstances;
            if (num > 1)
            {
                ArrayList list = null;
                lock (this._freeList)
                {
                    if (this._numFreeAppInstances > 1)
                    {
                        list = new ArrayList();
                        for (int i = ((this._numFreeAppInstances * 3) / 100) + 1; i > 0; i--)
                        {
                            list.Add(this._freeList.Pop());
                            this._numFreeAppInstances--;
                        }
                        this._minFreeAppInstances = this._numFreeAppInstances;
                    }
                }
                if (list != null)
                {
                    foreach (HttpApplication application in list)
                    {
                        application.DisposeInternal();
                    }
                }
            }
        }

        internal static void TrimApplicationInstances()
        {
            if (_theApplicationFactory != null)
            {
                _theApplicationFactory.TrimApplicationInstanceFreeList();
            }
        }

        internal static HttpApplicationState ApplicationState
        {
            get
            {
                HttpApplicationState state = _theApplicationFactory._state;
                if (state == null)
                {
                    state = new HttpApplicationState();
                }
                return state;
            }
        }

        private class AspCompatSessionOnEndHelper
        {
            private HttpApplication _app;
            private EventArgs _eventArgs;
            private object _eventSource;
            private HttpSessionState _session;

            internal AspCompatSessionOnEndHelper(HttpApplication app, HttpSessionState session, object eventSource, EventArgs eventArgs)
            {
                this._app = app;
                this._session = session;
                this._eventSource = eventSource;
                this._eventArgs = eventArgs;
            }

            internal HttpApplication Application
            {
                get
                {
                    return this._app;
                }
            }

            internal EventArgs Args
            {
                get
                {
                    return this._eventArgs;
                }
            }

            internal HttpSessionState Session
            {
                get
                {
                    return this._session;
                }
            }

            internal object Source
            {
                get
                {
                    return this._eventSource;
                }
            }
        }
    }
}

