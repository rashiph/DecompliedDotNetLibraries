namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Threading;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.Runtime.Configuration;
    using System.Workflow.Runtime.DebugEngine;
    using System.Workflow.Runtime.Hosting;
    using System.Workflow.Runtime.Tracking;
    using System.Xml;

    public class WorkflowRuntime : IServiceProvider, IDisposable
    {
        private NameValueConfigurationCollection _configurationParameters;
        private bool _disposed;
        private string _name;
        private System.Workflow.Runtime.PerformanceCounterManager _performanceCounterManager;
        private static Dictionary<Guid, WeakReference> _runtimes = new Dictionary<Guid, WeakReference>();
        private static object _runtimesLock = new object();
        private Dictionary<Type, List<object>> _services;
        private object _servicesLock;
        private bool _startedServices;
        private object _startStopLock;
        private System.Workflow.Runtime.TrackingListenerFactory _trackingFactory;
        private Dictionary<string, Type> _trackingServiceReplacement;
        private Guid _uid;
        private WorkflowDefinitionDispenser _workflowDefinitionDispenser;
        private DebugController debugController;
        internal const string DefaultName = "WorkflowRuntime";
        private BooleanSwitch disableWorkflowDebugging;
        private bool isInstanceStarted;
        private FanOutOnKeyDictionary<Guid, WorkflowExecutor> workflowExecutors;

        public event EventHandler<ServicesExceptionNotHandledEventArgs> ServicesExceptionNotHandled;

        public event EventHandler<WorkflowRuntimeEventArgs> Started;

        public event EventHandler<WorkflowRuntimeEventArgs> Stopped;

        public event EventHandler<WorkflowEventArgs> WorkflowAborted;

        public event EventHandler<WorkflowCompletedEventArgs> WorkflowCompleted;

        public event EventHandler<WorkflowEventArgs> WorkflowCreated;

        internal event EventHandler<WorkflowEventArgs> WorkflowDynamicallyChanged;

        internal event EventHandler<WorkflowExecutorInitializingEventArgs> WorkflowExecutorInitializing;

        public event EventHandler<WorkflowEventArgs> WorkflowIdled;

        public event EventHandler<WorkflowEventArgs> WorkflowLoaded;

        public event EventHandler<WorkflowEventArgs> WorkflowPersisted;

        public event EventHandler<WorkflowEventArgs> WorkflowResumed;

        public event EventHandler<WorkflowEventArgs> WorkflowStarted;

        public event EventHandler<WorkflowSuspendedEventArgs> WorkflowSuspended;

        public event EventHandler<WorkflowTerminatedEventArgs> WorkflowTerminated;

        public event EventHandler<WorkflowEventArgs> WorkflowUnloaded;

        static WorkflowRuntime()
        {
            Activity.ActivityResolve += new ActivityResolveEventHandler(WorkflowRuntime.OnActivityDefinitionResolve);
            Activity.WorkflowChangeActionsResolve += new WorkflowChangeActionsResolveEventHandler(WorkflowRuntime.OnWorkflowChangeActionsResolve);
        }

        public WorkflowRuntime()
        {
            this._servicesLock = new object();
            this._startStopLock = new object();
            this._uid = Guid.NewGuid();
            this.disableWorkflowDebugging = new BooleanSwitch("DisableWorkflowDebugging", "Disables workflow debugging in host");
            this._trackingFactory = new System.Workflow.Runtime.TrackingListenerFactory();
            this._services = new Dictionary<Type, List<object>>();
            this.PrivateInitialize(null);
        }

        public WorkflowRuntime(string configSectionName)
        {
            this._servicesLock = new object();
            this._startStopLock = new object();
            this._uid = Guid.NewGuid();
            this.disableWorkflowDebugging = new BooleanSwitch("DisableWorkflowDebugging", "Disables workflow debugging in host");
            this._trackingFactory = new System.Workflow.Runtime.TrackingListenerFactory();
            this._services = new Dictionary<Type, List<object>>();
            if (configSectionName == null)
            {
                throw new ArgumentNullException("configSectionName");
            }
            WorkflowRuntimeSection settings = ConfigurationManager.GetSection(configSectionName) as WorkflowRuntimeSection;
            if (settings == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.ConfigurationSectionNotFound, new object[] { configSectionName }), "configSectionName");
            }
            this.PrivateInitialize(settings);
        }

        public WorkflowRuntime(WorkflowRuntimeSection settings)
        {
            this._servicesLock = new object();
            this._startStopLock = new object();
            this._uid = Guid.NewGuid();
            this.disableWorkflowDebugging = new BooleanSwitch("DisableWorkflowDebugging", "Disables workflow debugging in host");
            this._trackingFactory = new System.Workflow.Runtime.TrackingListenerFactory();
            this._services = new Dictionary<Type, List<object>>();
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            this.PrivateInitialize(settings);
        }

        private void _OnServiceEvent(WorkflowExecutor sched, bool unregister, EventHandler<WorkflowEventArgs> handler)
        {
            try
            {
                WorkflowEventArgs e = new WorkflowEventArgs(sched.WorkflowInstance);
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            catch (Exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime:OnService Event threw an exception.");
                throw;
            }
            finally
            {
                if (unregister)
                {
                    this._unRegister(sched);
                }
            }
        }

        private void _unRegister(WorkflowExecutor executor)
        {
            this.TryRemoveWorkflowExecutor(executor.InstanceId, executor);
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime::_removeInstance, instance:{0}, hc:{1}", new object[] { executor.InstanceId, executor.GetHashCode() });
            WorkflowTrace.Runtime.Flush();
            WorkflowTrace.Tracking.Flush();
            WorkflowTrace.Host.Flush();
        }

        public void AddService(object service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }
            this.VerifyInternalState();
            using (new EventContext(new object[0]))
            {
                lock (this._startStopLock)
                {
                    this.AddServiceImpl(service);
                }
            }
        }

        private void AddServiceFromSettings(WorkflowRuntimeServiceElement serviceSettings)
        {
            object service = null;
            Type type = Type.GetType(serviceSettings.Type, true);
            ConstructorInfo info = null;
            ConstructorInfo info2 = null;
            ConstructorInfo info3 = null;
            foreach (ConstructorInfo info4 in type.GetConstructors())
            {
                ParameterInfo[] parameters = info4.GetParameters();
                if (parameters.Length == 1)
                {
                    if (typeof(IServiceProvider).IsAssignableFrom(parameters[0].ParameterType))
                    {
                        info2 = info4;
                    }
                    else if (typeof(NameValueCollection).IsAssignableFrom(parameters[0].ParameterType))
                    {
                        info3 = info4;
                    }
                }
                else if (((parameters.Length == 2) && typeof(IServiceProvider).IsAssignableFrom(parameters[0].ParameterType)) && typeof(NameValueCollection).IsAssignableFrom(parameters[1].ParameterType))
                {
                    info = info4;
                    break;
                }
            }
            if (info != null)
            {
                service = info.Invoke(new object[] { this, serviceSettings.Parameters });
            }
            else if (info2 != null)
            {
                service = info2.Invoke(new object[] { this });
            }
            else if (info3 != null)
            {
                service = info3.Invoke(new object[] { serviceSettings.Parameters });
            }
            else
            {
                service = Activator.CreateInstance(type);
            }
            this.AddServiceImpl(service);
        }

        private void AddServiceImpl(object service)
        {
            lock (this._servicesLock)
            {
                if (this.GetAllServices(service.GetType()).Contains(service))
                {
                    throw new InvalidOperationException(ExecutionStringManager.CantAddServiceTwice);
                }
                if (this._startedServices && this.IsCoreService(service))
                {
                    throw new InvalidOperationException(ExecutionStringManager.CantChangeImmutableContainer);
                }
                Type baseType = service.GetType();
                if (baseType.IsSubclassOf(typeof(TrackingService)))
                {
                    this.AddTrackingServiceReplacementInfo(baseType);
                }
                foreach (Type type2 in baseType.GetInterfaces())
                {
                    List<object> list;
                    if (this._services.ContainsKey(type2))
                    {
                        list = this._services[type2];
                    }
                    else
                    {
                        list = new List<object>();
                        this._services.Add(type2, list);
                    }
                    list.Add(service);
                }
                while (baseType != null)
                {
                    List<object> list2 = null;
                    if (this._services.ContainsKey(baseType))
                    {
                        list2 = this._services[baseType];
                    }
                    else
                    {
                        list2 = new List<object>();
                        this._services.Add(baseType, list2);
                    }
                    list2.Add(service);
                    baseType = baseType.BaseType;
                }
            }
            WorkflowRuntimeService service2 = service as WorkflowRuntimeService;
            if (service2 != null)
            {
                service2.SetRuntime(this);
                if (this._startedServices)
                {
                    service2.Start();
                }
            }
        }

        private void AddTrackingServiceReplacementInfo(Type type)
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(PreviousTrackingServiceAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                foreach (object obj2 in customAttributes)
                {
                    if (this._trackingServiceReplacement == null)
                    {
                        this._trackingServiceReplacement = new Dictionary<string, Type>();
                    }
                    this._trackingServiceReplacement.Add(((PreviousTrackingServiceAttribute) obj2).AssemblyQualifiedName, type);
                }
            }
        }

        internal static void ClearTrackingProfileCache()
        {
            lock (_runtimesLock)
            {
                foreach (WeakReference reference in _runtimes.Values)
                {
                    WorkflowRuntime target = reference.Target as WorkflowRuntime;
                    if (((target != null) && (target.TrackingListenerFactory != null)) && (target.TrackingListenerFactory.TrackingProfileManager != null))
                    {
                        target.TrackingListenerFactory.TrackingProfileManager.ClearCacheImpl();
                    }
                }
            }
        }

        private WorkflowCompletedEventArgs CreateCompletedEventArgs(WorkflowExecutor exec)
        {
            WorkflowCompletedEventArgs args = new WorkflowCompletedEventArgs(exec.WorkflowInstance, exec.WorkflowDefinition);
            foreach (PropertyInfo info in this._workflowDefinitionDispenser.GetOutputParameters(exec.RootActivity))
            {
                args.OutputParameters.Add(info.Name, info.GetValue(exec.RootActivity, null));
            }
            return args;
        }

        internal static TypeProvider CreateTypeProvider(Activity rootActivity)
        {
            TypeProvider provider = new TypeProvider(null);
            Type type = rootActivity.GetType();
            provider.SetLocalAssembly(type.Assembly);
            provider.AddAssembly(type.Assembly);
            foreach (AssemblyName name in type.Assembly.GetReferencedAssemblies())
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(name);
                    if (assembly != null)
                    {
                        provider.AddAssembly(assembly);
                    }
                }
                catch
                {
                }
                if ((assembly == null) && (name.CodeBase != null))
                {
                    provider.AddAssemblyReference(name.CodeBase);
                }
            }
            return provider;
        }

        public WorkflowInstance CreateWorkflow(Type workflowType)
        {
            if (workflowType == null)
            {
                throw new ArgumentNullException("workflowType");
            }
            if (!typeof(Activity).IsAssignableFrom(workflowType))
            {
                throw new ArgumentException(ExecutionStringManager.TypeMustImplementRootActivity, "workflowType");
            }
            this.VerifyInternalState();
            return this.InternalCreateWorkflow(new CreationContext(workflowType, null, null, null), Guid.NewGuid());
        }

        public WorkflowInstance CreateWorkflow(XmlReader workflowDefinitionReader)
        {
            if (workflowDefinitionReader == null)
            {
                throw new ArgumentNullException("workflowDefinitionReader");
            }
            this.VerifyInternalState();
            return this.CreateWorkflow(workflowDefinitionReader, null, null);
        }

        public WorkflowInstance CreateWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues)
        {
            return this.CreateWorkflow(workflowType, namedArgumentValues, Guid.NewGuid());
        }

        public WorkflowInstance CreateWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues, Guid instanceId)
        {
            if (workflowType == null)
            {
                throw new ArgumentNullException("workflowType");
            }
            if (!typeof(Activity).IsAssignableFrom(workflowType))
            {
                throw new ArgumentException(ExecutionStringManager.TypeMustImplementRootActivity, "workflowType");
            }
            this.VerifyInternalState();
            return this.InternalCreateWorkflow(new CreationContext(workflowType, null, null, namedArgumentValues), instanceId);
        }

        public WorkflowInstance CreateWorkflow(XmlReader workflowDefinitionReader, XmlReader rulesReader, Dictionary<string, object> namedArgumentValues)
        {
            return this.CreateWorkflow(workflowDefinitionReader, rulesReader, namedArgumentValues, Guid.NewGuid());
        }

        public WorkflowInstance CreateWorkflow(XmlReader workflowDefinitionReader, XmlReader rulesReader, Dictionary<string, object> namedArgumentValues, Guid instanceId)
        {
            if (workflowDefinitionReader == null)
            {
                throw new ArgumentNullException("workflowDefinitionReader");
            }
            this.VerifyInternalState();
            CreationContext context = new CreationContext(workflowDefinitionReader, rulesReader, namedArgumentValues);
            return this.InternalCreateWorkflow(context, instanceId);
        }

        public void Dispose()
        {
            lock (this._startStopLock)
            {
                if (!this._disposed)
                {
                    if (this.debugController != null)
                    {
                        this.debugController.Close();
                    }
                    this._workflowDefinitionDispenser.Dispose();
                    this._startedServices = false;
                    this._disposed = true;
                }
            }
            lock (_runtimesLock)
            {
                if (_runtimes.ContainsKey(this._uid))
                {
                    _runtimes.Remove(this._uid);
                }
            }
        }

        private void DynamicUpdateCommit(object sender, WorkflowExecutor.DynamicUpdateEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidArgumentType, new object[] { "sender", typeof(WorkflowExecutor).ToString() }));
            }
            WorkflowExecutor schedule = (WorkflowExecutor) sender;
            this.OnScheduleDynamicallyChanged(schedule);
        }

        public ReadOnlyCollection<T> GetAllServices<T>()
        {
            this.VerifyInternalState();
            List<T> list = new List<T>();
            foreach (T local in this.GetAllServices(typeof(T)))
            {
                list.Add(local);
            }
            return new ReadOnlyCollection<T>(list);
        }

        public ReadOnlyCollection<object> GetAllServices(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            this.VerifyInternalState();
            lock (this._servicesLock)
            {
                List<object> list = new List<object>();
                if (this._services.ContainsKey(serviceType))
                {
                    list.AddRange(this._services[serviceType]);
                }
                return new ReadOnlyCollection<object>(list);
            }
        }

        public ReadOnlyCollection<WorkflowInstance> GetLoadedWorkflows()
        {
            this.VerifyInternalState();
            List<WorkflowInstance> list = new List<WorkflowInstance>();
            foreach (WorkflowExecutor executor in this.GetWorkflowExecutors())
            {
                list.Add(executor.WorkflowInstance);
            }
            return list.AsReadOnly();
        }

        public T GetService<T>()
        {
            this.VerifyInternalState();
            return (T) this.GetService(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            this.VerifyInternalState();
            lock (this._servicesLock)
            {
                object obj2 = null;
                if (this._services.ContainsKey(serviceType))
                {
                    List<object> list = this._services[serviceType];
                    if (list.Count > 1)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MoreThanOneService, new object[] { serviceType.ToString() }));
                    }
                    if (list.Count == 1)
                    {
                        obj2 = list[0];
                    }
                }
                return obj2;
            }
        }

        public WorkflowInstance GetWorkflow(Guid instanceId)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "instanceId" }));
            }
            this.VerifyInternalState();
            if (!this.IsStarted)
            {
                throw new InvalidOperationException(ExecutionStringManager.WorkflowRuntimeNotStarted);
            }
            return this.Load(instanceId, null, null).WorkflowInstance;
        }

        internal Activity GetWorkflowDefinition(Type workflowType)
        {
            if (workflowType == null)
            {
                throw new ArgumentNullException("workflowType");
            }
            this.VerifyInternalState();
            return this._workflowDefinitionDispenser.GetRootActivity(workflowType, false, true);
        }

        private WorkflowExecutor GetWorkflowExecutor(Guid instanceId, CreationContext context)
        {
            WorkflowExecutor executor2;
            try
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime dispensing resource, instanceId: {0}", new object[] { instanceId });
                WorkflowExecutor executor = this.Load(instanceId, context, null);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime dispensing resource instanceId: {0}, hc: {1}", new object[] { instanceId, executor.GetHashCode() });
                executor2 = executor;
            }
            catch (OutOfMemoryException)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime dispensing resource, can't create service due to OOM!(1), instance, {0}", new object[] { instanceId });
                throw;
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime dispensing resource, can't create service due to unexpected exception!(2), instance, {0}, exception, {1}", new object[] { instanceId, exception });
                throw;
            }
            return executor2;
        }

        private IList<WorkflowExecutor> GetWorkflowExecutors()
        {
            List<WorkflowExecutor> list = new List<WorkflowExecutor>();
            foreach (Dictionary<Guid, WorkflowExecutor> dictionary in this.workflowExecutors)
            {
                lock (dictionary)
                {
                    foreach (WorkflowExecutor executor in dictionary.Values)
                    {
                        if ((executor != null) && executor.IsInstanceValid)
                        {
                            list.Add(executor);
                        }
                    }
                }
            }
            return list;
        }

        private Activity InitializeExecutor(Guid instanceId, CreationContext context, WorkflowExecutor executor, WorkflowInstance workflowInstance)
        {
            Activity rootActivity = null;
            if ((context != null) && context.IsActivation)
            {
                Activity activity2 = null;
                string str = null;
                string rulesText = null;
                if (context.Type != null)
                {
                    activity2 = this._workflowDefinitionDispenser.GetRootActivity(context.Type, false, true);
                    rootActivity = this._workflowDefinitionDispenser.GetRootActivity(context.Type, true, false);
                }
                else if (context.XomlReader != null)
                {
                    try
                    {
                        context.XomlReader.MoveToContent();
                        while (!context.XomlReader.EOF && !context.XomlReader.IsStartElement())
                        {
                            context.XomlReader.Read();
                        }
                        str = context.XomlReader.ReadOuterXml();
                        if (context.RulesReader != null)
                        {
                            context.RulesReader.MoveToContent();
                            while (!context.RulesReader.EOF && !context.RulesReader.IsStartElement())
                            {
                                context.RulesReader.Read();
                            }
                            rulesText = context.RulesReader.ReadOuterXml();
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new ArgumentException(ExecutionStringManager.InvalidXAML, exception);
                    }
                    if (string.IsNullOrEmpty(str))
                    {
                        throw new ArgumentException(ExecutionStringManager.InvalidXAML);
                    }
                    activity2 = this._workflowDefinitionDispenser.GetRootActivity(str, rulesText, false, true);
                    rootActivity = this._workflowDefinitionDispenser.GetRootActivity(str, rulesText, true, false);
                }
                rootActivity.SetValue(Activity.WorkflowDefinitionProperty, activity2);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Creating instance " + instanceId.ToString());
                context.Created = true;
                executor.Initialize(rootActivity, context.InvokerExecutor, context.InvokeActivityID, instanceId, context.Args, workflowInstance);
                return rootActivity;
            }
            if (this.WorkflowPersistenceService == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceService, new object[] { instanceId });
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, message);
                throw new InvalidOperationException(message);
            }
            using (new RuntimeEnvironment(this))
            {
                rootActivity = this.WorkflowPersistenceService.LoadWorkflowInstanceState(instanceId);
            }
            if (rootActivity == null)
            {
                throw new InvalidOperationException(string.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.InstanceNotFound, new object[] { instanceId }));
            }
            executor.Reload(rootActivity, workflowInstance);
            return rootActivity;
        }

        internal WorkflowInstance InternalCreateWorkflow(CreationContext context, Guid instanceId)
        {
            using (new WorkflowTraceTransfer(instanceId))
            {
                this.VerifyInternalState();
                if (!this.IsStarted)
                {
                    this.StartRuntime();
                }
                WorkflowExecutor workflowExecutor = this.GetWorkflowExecutor(instanceId, context);
                if (!context.Created)
                {
                    throw new InvalidOperationException(ExecutionStringManager.WorkflowWithIdAlreadyExists);
                }
                return workflowExecutor.WorkflowInstance;
            }
        }

        private bool IsCoreService(object service)
        {
            return ((((service is WorkflowSchedulerService) || (service is System.Workflow.Runtime.Hosting.WorkflowPersistenceService)) || ((service is TrackingService) || (service is WorkflowCommitWorkBatchService))) || (service is WorkflowLoaderService));
        }

        internal WorkflowExecutor Load(WorkflowInstance instance)
        {
            return this.Load(instance.InstanceId, null, instance);
        }

        internal WorkflowExecutor Load(Guid key, CreationContext context, WorkflowInstance workflowInstance)
        {
            WorkflowExecutor executor;
            Dictionary<Guid, WorkflowExecutor> dictionary = this.workflowExecutors[key];
            lock (dictionary)
            {
                if (!this.IsStarted)
                {
                    throw new InvalidOperationException(ExecutionStringManager.WorkflowRuntimeNotStarted);
                }
                if (dictionary.TryGetValue(key, out executor) && executor.IsInstanceValid)
                {
                    return executor;
                }
                executor = new WorkflowExecutor(key);
                if (workflowInstance == null)
                {
                    workflowInstance = new WorkflowInstance(key, this);
                }
                this.InitializeExecutor(key, context, executor, workflowInstance);
                try
                {
                    WorkflowTrace.Host.TraceInformation("WorkflowRuntime:: replacing unusable executor for key {0} with new one (hc: {1})", new object[] { key, executor.GetHashCode() });
                    dictionary[key] = executor;
                    this.RegisterExecutor((context != null) && context.IsActivation, executor);
                }
                catch
                {
                    WorkflowExecutor executor2;
                    if (dictionary.TryGetValue(key, out executor2) && object.Equals(executor, executor2))
                    {
                        dictionary.Remove(key);
                    }
                    throw;
                }
            }
            executor.Registered((context != null) && context.IsActivation);
            return executor;
        }

        private static Activity OnActivityDefinitionResolve(object sender, ActivityResolveEventArgs e)
        {
            WorkflowRuntime serviceProvider = e.ServiceProvider as WorkflowRuntime;
            if (serviceProvider == null)
            {
                serviceProvider = RuntimeEnvironment.CurrentRuntime;
            }
            if (serviceProvider == null)
            {
                return null;
            }
            if (e.Type != null)
            {
                return serviceProvider._workflowDefinitionDispenser.GetRootActivity(e.Type, e.CreateNewDefinition, e.InitializeForRuntime);
            }
            return serviceProvider._workflowDefinitionDispenser.GetRootActivity(e.WorkflowMarkup, e.RulesMarkup, e.CreateNewDefinition, e.InitializeForRuntime);
        }

        internal void OnIdle(WorkflowExecutor executor)
        {
            try
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Received OnIdle Event for instance, {0}", new object[] { executor.InstanceId });
                WorkflowInstance workflowInstance = executor.WorkflowInstance;
                if (this.WorkflowIdled != null)
                {
                    this.WorkflowIdled(this, new WorkflowEventArgs(workflowInstance));
                }
            }
            catch (Exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Warning, 0, "OnIdle Event for instance, {0} threw an exception", new object[] { executor.InstanceId });
                throw;
            }
        }

        internal void OnScheduleAborted(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleAborted event raised for instance Id {0}", new object[] { schedule.InstanceId });
            this._OnServiceEvent(schedule, true, this.WorkflowAborted);
        }

        internal void OnScheduleCompleted(WorkflowExecutor schedule, WorkflowCompletedEventArgs args)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleCompleted event raised for instance Id {0}", new object[] { schedule.InstanceId });
            try
            {
                if (this.WorkflowCompleted != null)
                {
                    this.WorkflowCompleted(this, args);
                }
            }
            catch (Exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime:OnScheduleCompleted Event threw an exception.");
                throw;
            }
            finally
            {
                this._unRegister(schedule);
            }
        }

        internal void OnScheduleDynamicallyChanged(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleDynamicallyChanged event raised for instance Id {0}", new object[] { schedule.InstanceId });
            this._OnServiceEvent(schedule, false, this.WorkflowDynamicallyChanged);
        }

        internal void OnScheduleLoaded(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleLoaded event raised for instance Id {0}", new object[] { schedule.InstanceId });
            this._OnServiceEvent(schedule, false, this.WorkflowLoaded);
        }

        internal void OnSchedulePersisted(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:SchedulePersisted event raised for instance Id {0}", new object[] { schedule.InstanceId });
            this._OnServiceEvent(schedule, false, this.WorkflowPersisted);
        }

        internal void OnScheduleResumed(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleResumed event raised for instance Id {0}", new object[] { schedule.InstanceId });
            this._OnServiceEvent(schedule, false, this.WorkflowResumed);
        }

        internal void OnScheduleSuspended(WorkflowExecutor schedule, WorkflowSuspendedEventArgs args)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleSuspension event raised for instance Id {0}", new object[] { schedule.InstanceId });
            try
            {
                if (this.WorkflowSuspended != null)
                {
                    this.WorkflowSuspended(this, args);
                }
            }
            catch (Exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime:OnScheduleSuspended Event threw an exception.");
                throw;
            }
        }

        internal void OnScheduleTerminated(WorkflowExecutor schedule, WorkflowTerminatedEventArgs args)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleTermination event raised for instance Id {0}", new object[] { schedule.InstanceId });
            try
            {
                if (this.WorkflowTerminated != null)
                {
                    this.WorkflowTerminated(this, args);
                }
            }
            catch (Exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime:OnScheduleTerminated Event threw an exception.");
                throw;
            }
            finally
            {
                this._unRegister(schedule);
            }
        }

        internal void OnScheduleUnloaded(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleUnloaded event raised for instance Id {0}", new object[] { schedule.InstanceId });
            this._OnServiceEvent(schedule, true, this.WorkflowUnloaded);
        }

        private static ArrayList OnWorkflowChangeActionsResolve(object sender, WorkflowChangeActionsResolveEventArgs e)
        {
            ArrayList list = null;
            WorkflowRuntime currentRuntime = RuntimeEnvironment.CurrentRuntime;
            if (currentRuntime == null)
            {
                return list;
            }
            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
            ServiceContainer container = new ServiceContainer();
            ITypeProvider service = currentRuntime.GetService<ITypeProvider>();
            if (service != null)
            {
                container.AddService(typeof(ITypeProvider), service);
            }
            else if (sender is Activity)
            {
                container.AddService(typeof(ITypeProvider), CreateTypeProvider(sender as Activity));
            }
            DesignerSerializationManager manager = new DesignerSerializationManager(container);
            using (manager.CreateSession())
            {
                using (StringReader reader = new StringReader(e.WorkflowChangesMarkup))
                {
                    using (XmlReader reader2 = XmlReader.Create(reader))
                    {
                        WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(manager);
                        return (serializer.Deserialize(serializationManager, reader2) as ArrayList);
                    }
                }
            }
        }

        private void PrivateInitialize(WorkflowRuntimeSection settings)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Created WorkflowRuntime {0}", new object[] { this._uid });
            this._workflowDefinitionDispenser = new WorkflowDefinitionDispenser(this, (settings != null) ? settings.ValidateOnCreate : true, (settings != null) ? settings.WorkflowDefinitionCacheCapacity : 0);
            this.workflowExecutors = new FanOutOnKeyDictionary<Guid, WorkflowExecutor>((Environment.ProcessorCount * 4) - 1);
            this._name = "WorkflowRuntime";
            if ((settings == null) || settings.EnablePerformanceCounters)
            {
                this.PerformanceCounterManager = new System.Workflow.Runtime.PerformanceCounterManager();
            }
            if (settings != null)
            {
                this._name = settings.Name;
                this._configurationParameters = settings.CommonParameters;
                foreach (WorkflowRuntimeServiceElement element in settings.Services)
                {
                    this.AddServiceFromSettings(element);
                }
            }
            if (!this.disableWorkflowDebugging.Enabled)
            {
                DebugController.InitializeProcessSecurity();
                this.debugController = new DebugController(this, this._name);
            }
            lock (_runtimesLock)
            {
                if (!_runtimes.ContainsKey(this._uid))
                {
                    _runtimes.Add(this._uid, new WeakReference(this));
                }
            }
        }

        internal void RaiseServicesExceptionNotHandledEvent(Exception exception, Guid instanceId)
        {
            this.VerifyInternalState();
            WorkflowTrace.Host.TraceEvent(TraceEventType.Critical, 0, "WorkflowRuntime:ServicesExceptionNotHandled event raised for instance Id {0} " + exception.ToString(), new object[] { instanceId });
            EventHandler<ServicesExceptionNotHandledEventArgs> servicesExceptionNotHandled = this.ServicesExceptionNotHandled;
            if (servicesExceptionNotHandled != null)
            {
                servicesExceptionNotHandled(this, new ServicesExceptionNotHandledEventArgs(exception, instanceId));
            }
        }

        private void RegisterExecutor(bool isActivation, WorkflowExecutor executor)
        {
            if (isActivation)
            {
                executor.RegisterWithRuntime(this);
            }
            else
            {
                executor.ReRegisterWithRuntime(this);
            }
        }

        public void RemoveService(object service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }
            this.VerifyInternalState();
            using (new EventContext(new object[0]))
            {
                lock (this._startStopLock)
                {
                    lock (this._servicesLock)
                    {
                        if (this._startedServices && this.IsCoreService(service))
                        {
                            throw new InvalidOperationException(ExecutionStringManager.CantChangeImmutableContainer);
                        }
                        if (!this.GetAllServices(service.GetType()).Contains(service))
                        {
                            throw new InvalidOperationException(ExecutionStringManager.CantRemoveServiceNotContained);
                        }
                        Type type = service.GetType();
                        if (type.IsSubclassOf(typeof(TrackingService)))
                        {
                            this.RemoveTrackingServiceReplacementInfo(type);
                        }
                        foreach (List<object> list in this._services.Values)
                        {
                            if (list.Contains(service))
                            {
                                list.Remove(service);
                            }
                        }
                    }
                    WorkflowRuntimeService service2 = service as WorkflowRuntimeService;
                    if (service2 != null)
                    {
                        if (this._startedServices)
                        {
                            service2.Stop();
                        }
                        service2.SetRuntime(null);
                    }
                }
            }
        }

        private void RemoveTrackingServiceReplacementInfo(Type type)
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(PreviousTrackingServiceAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                foreach (object obj2 in customAttributes)
                {
                    string assemblyQualifiedName = ((PreviousTrackingServiceAttribute) obj2).AssemblyQualifiedName;
                    if (this._trackingServiceReplacement.ContainsKey(assemblyQualifiedName))
                    {
                        this._trackingServiceReplacement.Remove(assemblyQualifiedName);
                    }
                }
            }
        }

        internal void ReplaceWorkflowExecutor(Guid instanceId, WorkflowExecutor oldWorkflowExecutor, WorkflowExecutor newWorkflowExecutor)
        {
            Dictionary<Guid, WorkflowExecutor> dictionary = this.workflowExecutors[instanceId];
            lock (dictionary)
            {
                oldWorkflowExecutor.IsInstanceValid = false;
                WorkflowTrace.Host.TraceInformation("WorkflowRuntime:: replacing old executor for key {0} with new one", new object[] { instanceId });
                dictionary[instanceId] = newWorkflowExecutor;
            }
        }

        public void StartRuntime()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Starting WorkflowRuntime {0}", new object[] { this._uid });
            lock (this._startStopLock)
            {
                this.VerifyInternalState();
                if (!this._startedServices)
                {
                    if (this.GetAllServices(typeof(WorkflowCommitWorkBatchService)).Count == 0)
                    {
                        this.AddServiceImpl(new DefaultWorkflowCommitWorkBatchService());
                    }
                    if (this.GetAllServices(typeof(WorkflowSchedulerService)).Count == 0)
                    {
                        this.AddServiceImpl(new DefaultWorkflowSchedulerService());
                    }
                    if (this.GetAllServices(typeof(WorkflowLoaderService)).Count == 0)
                    {
                        this.AddServiceImpl(new DefaultWorkflowLoaderService());
                    }
                    if (this.GetAllServices(typeof(WorkflowCommitWorkBatchService)).Count != 1)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidWorkflowRuntimeConfiguration, new object[] { typeof(WorkflowCommitWorkBatchService).Name }));
                    }
                    if (this.GetAllServices(typeof(WorkflowSchedulerService)).Count != 1)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidWorkflowRuntimeConfiguration, new object[] { typeof(WorkflowSchedulerService).Name }));
                    }
                    if (this.GetAllServices(typeof(WorkflowLoaderService)).Count != 1)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidWorkflowRuntimeConfiguration, new object[] { typeof(WorkflowLoaderService).Name }));
                    }
                    if (this.GetAllServices(typeof(System.Workflow.Runtime.Hosting.WorkflowPersistenceService)).Count > 1)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidWorkflowRuntimeConfiguration, new object[] { typeof(System.Workflow.Runtime.Hosting.WorkflowPersistenceService).Name }));
                    }
                    if (this.GetAllServices(typeof(WorkflowTimerService)).Count == 0)
                    {
                        this.AddServiceImpl(new WorkflowTimerService());
                    }
                    this.isInstanceStarted = true;
                    this._trackingFactory.Initialize(this);
                    if (this.PerformanceCounterManager != null)
                    {
                        this.PerformanceCounterManager.Initialize(this);
                        this.PerformanceCounterManager.SetInstanceName(this.Name);
                    }
                    foreach (WorkflowRuntimeService service in this.GetAllServices<WorkflowRuntimeService>())
                    {
                        service.Start();
                    }
                    this._startedServices = true;
                    using (new EventContext(new object[0]))
                    {
                        EventHandler<WorkflowRuntimeEventArgs> started = this.Started;
                        if (started != null)
                        {
                            started(this, new WorkflowRuntimeEventArgs(this.isInstanceStarted));
                        }
                    }
                }
            }
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Started WorkflowRuntime {0}", new object[] { this._uid });
        }

        public void StopRuntime()
        {
            this.VerifyInternalState();
            using (new EventContext(new object[0]))
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Stopping WorkflowRuntime {0}", new object[] { this._uid });
                lock (this._startStopLock)
                {
                    if (this._startedServices)
                    {
                        try
                        {
                            this.isInstanceStarted = false;
                            if (this.WorkflowPersistenceService != null)
                            {
                                for (IList<WorkflowExecutor> list = this.GetWorkflowExecutors(); (list != null) && (list.Count > 0); list = this.GetWorkflowExecutors())
                                {
                                    foreach (WorkflowExecutor executor in list)
                                    {
                                        if (executor.IsInstanceValid)
                                        {
                                            try
                                            {
                                                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Calling Unload on instance {0} executor hc {1}", new object[] { executor.InstanceIdString, executor.GetHashCode() });
                                                executor.Unload();
                                            }
                                            catch (ExecutorLocksHeldException)
                                            {
                                            }
                                            catch (InvalidOperationException)
                                            {
                                                if (executor.IsInstanceValid)
                                                {
                                                    this.isInstanceStarted = true;
                                                    throw;
                                                }
                                            }
                                            catch
                                            {
                                                this.isInstanceStarted = true;
                                                throw;
                                            }
                                        }
                                    }
                                }
                            }
                            this.StopServices();
                            this._startedServices = false;
                            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Stopped WorkflowRuntime {0}", new object[] { this._uid });
                            this._trackingFactory.Uninitialize(this);
                            if (this.PerformanceCounterManager != null)
                            {
                                this.PerformanceCounterManager.Uninitialize(this);
                            }
                            EventHandler<WorkflowRuntimeEventArgs> stopped = this.Stopped;
                            if (stopped != null)
                            {
                                stopped(this, new WorkflowRuntimeEventArgs(this.isInstanceStarted));
                            }
                        }
                        catch (Exception)
                        {
                            WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime::StartUnload Unexpected Exception");
                            throw;
                        }
                        finally
                        {
                            this.isInstanceStarted = false;
                        }
                    }
                }
            }
        }

        private void StopServices()
        {
            foreach (WorkflowRuntimeService service in this.GetAllServices<WorkflowRuntimeService>())
            {
                service.Stop();
            }
        }

        private bool TryRemoveWorkflowExecutor(Guid instanceId, WorkflowExecutor executor)
        {
            Dictionary<Guid, WorkflowExecutor> dictionary = this.workflowExecutors[instanceId];
            lock (dictionary)
            {
                WorkflowExecutor executor2;
                if (dictionary.TryGetValue(instanceId, out executor2) && object.Equals(executor, executor2))
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime::TryRemoveWorkflowExecutor, instance:{0}, hc:{1}", new object[] { executor.InstanceIdString, executor.GetHashCode() });
                    return dictionary.Remove(instanceId);
                }
                return false;
            }
        }

        private void VerifyInternalState()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("WorkflowRuntime");
            }
        }

        private void WorkflowExecutionEvent(object sender, WorkflowExecutor.WorkflowExecutionEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
            {
                throw new ArgumentException("sender");
            }
            WorkflowExecutor schedule = (WorkflowExecutor) sender;
            switch (e.EventType)
            {
                case WorkflowEventInternal.Created:
                    if (this.WorkflowCreated == null)
                    {
                        break;
                    }
                    this.WorkflowCreated(this, new WorkflowEventArgs(schedule.WorkflowInstance));
                    return;

                case WorkflowEventInternal.Completing:
                case WorkflowEventInternal.SchedulerEmpty:
                case WorkflowEventInternal.Suspending:
                case WorkflowEventInternal.Resuming:
                case WorkflowEventInternal.Persisting:
                case WorkflowEventInternal.Unloading:
                case WorkflowEventInternal.Exception:
                case WorkflowEventInternal.Terminating:
                case WorkflowEventInternal.Aborting:
                    break;

                case WorkflowEventInternal.Completed:
                    this.OnScheduleCompleted(schedule, this.CreateCompletedEventArgs(schedule));
                    return;

                case WorkflowEventInternal.Idle:
                    this.OnIdle(schedule);
                    return;

                case WorkflowEventInternal.Suspended:
                {
                    WorkflowExecutor.WorkflowExecutionSuspendedEventArgs args2 = (WorkflowExecutor.WorkflowExecutionSuspendedEventArgs) e;
                    this.OnScheduleSuspended(schedule, new WorkflowSuspendedEventArgs(schedule.WorkflowInstance, args2.Error));
                    return;
                }
                case WorkflowEventInternal.Resumed:
                    this.OnScheduleResumed(schedule);
                    return;

                case WorkflowEventInternal.Persisted:
                    this.OnSchedulePersisted(schedule);
                    return;

                case WorkflowEventInternal.Unloaded:
                    this.OnScheduleUnloaded(schedule);
                    return;

                case WorkflowEventInternal.Loaded:
                    this.OnScheduleLoaded(schedule);
                    return;

                case WorkflowEventInternal.Terminated:
                {
                    WorkflowExecutor.WorkflowExecutionTerminatedEventArgs args = (WorkflowExecutor.WorkflowExecutionTerminatedEventArgs) e;
                    if (args.Exception == null)
                    {
                        this.OnScheduleTerminated(schedule, new WorkflowTerminatedEventArgs(schedule.WorkflowInstance, args.Error));
                        return;
                    }
                    this.OnScheduleTerminated(schedule, new WorkflowTerminatedEventArgs(schedule.WorkflowInstance, args.Exception));
                    return;
                }
                case WorkflowEventInternal.Aborted:
                    this.OnScheduleAborted(schedule);
                    return;

                case WorkflowEventInternal.DynamicChangeCommit:
                    this.DynamicUpdateCommit(schedule, (WorkflowExecutor.DynamicUpdateEventArgs) e);
                    break;

                case WorkflowEventInternal.Started:
                    if (this.WorkflowStarted == null)
                    {
                        break;
                    }
                    this.WorkflowStarted(this, new WorkflowEventArgs(schedule.WorkflowInstance));
                    return;

                default:
                    return;
            }
        }

        internal void WorkflowExecutorCreated(WorkflowExecutor workflowExecutor, bool loaded)
        {
            EventHandler<WorkflowExecutorInitializingEventArgs> workflowExecutorInitializing = this.WorkflowExecutorInitializing;
            if (workflowExecutorInitializing != null)
            {
                workflowExecutorInitializing(workflowExecutor, new WorkflowExecutorInitializingEventArgs(loaded));
            }
            workflowExecutor.WorkflowExecutionEvent += new EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs>(this.WorkflowExecutionEvent);
        }

        internal NameValueConfigurationCollection CommonParameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._configurationParameters;
            }
        }

        internal WorkflowDefinitionDispenser DefinitionDispenser
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._workflowDefinitionDispenser;
            }
        }

        public bool IsStarted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._startedServices;
            }
        }

        internal bool IsZombie
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._disposed;
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._name;
            }
            set
            {
                lock (this._startStopLock)
                {
                    if (this._startedServices)
                    {
                        throw new InvalidOperationException(ExecutionStringManager.CantChangeNameAfterStart);
                    }
                    this.VerifyInternalState();
                    this._name = value;
                }
            }
        }

        internal System.Workflow.Runtime.PerformanceCounterManager PerformanceCounterManager
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._performanceCounterManager;
            }
            private set
            {
                this._performanceCounterManager = value;
            }
        }

        internal WorkflowSchedulerService SchedulerService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetService<WorkflowSchedulerService>();
            }
        }

        internal System.Workflow.Runtime.TrackingListenerFactory TrackingListenerFactory
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._trackingFactory;
            }
        }

        internal Dictionary<string, Type> TrackingServiceReplacement
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._trackingServiceReplacement;
            }
        }

        internal List<TrackingService> TrackingServices
        {
            get
            {
                List<TrackingService> list = new List<TrackingService>();
                foreach (TrackingService service in this.GetAllServices(typeof(TrackingService)))
                {
                    list.Add(service);
                }
                return list;
            }
        }

        internal WorkflowCommitWorkBatchService TransactionService
        {
            get
            {
                return (WorkflowCommitWorkBatchService) this.GetService(typeof(WorkflowCommitWorkBatchService));
            }
        }

        internal System.Workflow.Runtime.Hosting.WorkflowPersistenceService WorkflowPersistenceService
        {
            get
            {
                return (System.Workflow.Runtime.Hosting.WorkflowPersistenceService) this.GetService(typeof(System.Workflow.Runtime.Hosting.WorkflowPersistenceService));
            }
        }

        internal sealed class EventContext : IDisposable
        {
            [ThreadStatic]
            private static object threadData;

            public EventContext(params object[] ignored)
            {
                if (threadData != null)
                {
                    throw new InvalidOperationException(ExecutionStringManager.CannotCauseEventInEvent);
                }
                threadData = this;
            }

            void IDisposable.Dispose()
            {
                threadData = null;
            }
        }

        internal sealed class WorkflowExecutorInitializingEventArgs : EventArgs
        {
            private bool _loading;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal WorkflowExecutorInitializingEventArgs(bool loading)
            {
                this._loading = loading;
            }

            internal bool Loading
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._loading;
                }
            }
        }
    }
}

