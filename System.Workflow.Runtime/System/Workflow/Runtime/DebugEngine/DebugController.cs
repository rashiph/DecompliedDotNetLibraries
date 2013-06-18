namespace System.Workflow.Runtime.DebugEngine
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Threading;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.Runtime;
    using System.Xml;

    public sealed class DebugController : MarshalByRefObject
    {
        private int attachTimeout;
        private Timer attachTimer;
        private IpcChannel channel;
        private IWorkflowDebugger controllerConduit;
        private static readonly string ControllerConduitTypeName = "ControllerConduitTypeName";
        private DebugControllerThread debugControllerThread;
        private ManualResetEvent eventConduitAttached;
        private object eventLock;
        private string hostName;
        private InstanceTable instanceTable;
        private bool isAttached;
        private bool isServiceContainerStarting;
        private bool isZombie;
        private Guid programId;
        private ProgramPublisher programPublisher;
        private const string rootExecutorGuid = "98fcdc7a-8ab4-4fb7-92d4-20f437285729";
        private WorkflowRuntime serviceContainer;
        private object syncRoot = new object();
        private Dictionary<Type, Guid> typeToGuid;
        private Dictionary<byte[], Guid> xomlHashToGuid;

        internal DebugController(WorkflowRuntime serviceContainer, string hostName)
        {
            if (serviceContainer == null)
            {
                throw new ArgumentNullException("serviceContainer");
            }
            try
            {
                this.programPublisher = new ProgramPublisher();
            }
            catch
            {
                return;
            }
            this.serviceContainer = serviceContainer;
            this.programId = Guid.Empty;
            this.controllerConduit = null;
            this.channel = null;
            this.isZombie = false;
            this.hostName = hostName;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(this.OnDomainUnload);
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(this.OnDomainUnload);
            this.serviceContainer.Started += new EventHandler<WorkflowRuntimeEventArgs>(this.Start);
            this.serviceContainer.Stopped += new EventHandler<WorkflowRuntimeEventArgs>(this.Stop);
        }

        internal void Attach(Guid programId, int attachTimeout, int detachPingInterval, out string hostName, out string uri, out int controllerThreadId, out bool isSynchronousAttach)
        {
            lock (this.syncRoot)
            {
                hostName = string.Empty;
                uri = string.Empty;
                controllerThreadId = 0;
                isSynchronousAttach = false;
                if (!this.isZombie)
                {
                    if (this.isAttached)
                    {
                        this.Detach();
                    }
                    this.isAttached = true;
                    this.programId = programId;
                    this.debugControllerThread = new DebugControllerThread();
                    this.instanceTable = new InstanceTable(this.debugControllerThread.ManagedThreadId);
                    this.typeToGuid = new Dictionary<Type, Guid>();
                    this.xomlHashToGuid = new Dictionary<byte[], Guid>(new DigestComparer());
                    this.debugControllerThread.RunThread(this.instanceTable);
                    IDictionary properties = new Hashtable();
                    properties["typeFilterLevel"] = "Full";
                    BinaryServerFormatterSinkProvider serverSinkProvider = new BinaryServerFormatterSinkProvider(properties, null);
                    Hashtable hashtable = new Hashtable();
                    hashtable["name"] = string.Empty;
                    hashtable["portName"] = this.programId.ToString();
                    IdentityReference reference = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Translate(typeof(NTAccount));
                    hashtable["authorizedGroup"] = reference.ToString();
                    this.channel = new IpcChannel(hashtable, null, serverSinkProvider);
                    ChannelServices.RegisterChannel(this.channel, true);
                    RemotingServices.Marshal(this, this.programId.ToString());
                    hostName = this.hostName;
                    uri = this.channel.GetUrlsForUri(this.programId.ToString())[0];
                    controllerThreadId = this.debugControllerThread.ThreadId;
                    isSynchronousAttach = !this.isServiceContainerStarting;
                    this.attachTimeout = attachTimeout;
                    this.attachTimer = new Timer(new TimerCallback(this.AttachTimerCallback), null, attachTimeout, detachPingInterval);
                }
            }
        }

        private void AttachTimerCallback(object state)
        {
            try
            {
                lock (this.syncRoot)
                {
                    if ((!this.isZombie && this.isAttached) && !Debugger.IsAttached)
                    {
                        this.attachTimer.Change(-1, -1);
                        this.Detach();
                    }
                }
            }
            catch
            {
            }
        }

        public void AttachToConduit(Uri url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            try
            {
                using (new DebuggerThreadMarker())
                {
                    try
                    {
                        RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryKeys.DebuggerSubKey);
                        if (key != null)
                        {
                            string str = key.GetValue(ControllerConduitTypeName, string.Empty) as string;
                            if (!string.IsNullOrEmpty(str) && (Type.GetType(str) != null))
                            {
                                this.controllerConduit = Activator.GetObject(Type.GetType(str), url.AbsolutePath) as IWorkflowDebugger;
                            }
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (this.controllerConduit == null)
                        {
                            this.controllerConduit = Activator.GetObject(Type.GetType("Microsoft.Workflow.DebugEngine.ControllerConduit, Microsoft.Workflow.DebugController, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), url.AbsolutePath) as IWorkflowDebugger;
                        }
                    }
                    if (this.controllerConduit != null)
                    {
                        ReadOnlyCollection<Type> onlys;
                        ReadOnlyCollection<Activity> onlys2;
                        ReadOnlyCollection<byte[]> onlys3;
                        this.eventLock = new object();
                        AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(this.OnAssemblyLoad);
                        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if ((!assembly.IsDynamic && !(assembly is AssemblyBuilder)) && !string.IsNullOrEmpty(assembly.Location))
                            {
                                this.controllerConduit.AssemblyLoaded(this.programId, assembly.Location, assembly.GlobalAssemblyCache);
                            }
                        }
                        this.serviceContainer.DefinitionDispenser.WorkflowDefinitionLoaded += new EventHandler<WorkflowDefinitionEventArgs>(this.ScheduleTypeLoaded);
                        this.serviceContainer.DefinitionDispenser.GetWorkflowTypes(out onlys, out onlys2);
                        for (int i = 0; i < onlys.Count; i++)
                        {
                            Type scheduleType = onlys[i];
                            Activity rootActivity = onlys2[i];
                            this.LoadExistingScheduleType(this.GetScheduleTypeId(scheduleType), scheduleType, false, rootActivity);
                        }
                        this.serviceContainer.DefinitionDispenser.GetWorkflowDefinitions(out onlys3, out onlys2);
                        for (int j = 0; j < onlys3.Count; j++)
                        {
                            byte[] scheduleDefHashCode = onlys3[j];
                            Activity activity2 = onlys2[j];
                            Activity activity3 = (Activity) activity2.GetValue(Activity.WorkflowDefinitionProperty);
                            ArrayList list = null;
                            if (activity3 != null)
                            {
                                list = (ArrayList) activity3.GetValue(WorkflowChanges.WorkflowChangeActionsProperty);
                            }
                            this.LoadExistingScheduleType(this.GetScheduleTypeId(scheduleDefHashCode), activity2.GetType(), (list != null) && (list.Count != 0), activity2);
                        }
                        this.serviceContainer.WorkflowExecutorInitializing += new EventHandler<WorkflowRuntime.WorkflowExecutorInitializingEventArgs>(this.InstanceInitializing);
                        foreach (WorkflowInstance instance in this.serviceContainer.GetLoadedWorkflows())
                        {
                            using (instance.GetWorkflowResourceUNSAFE().ExecutorLock.Enter())
                            {
                                this.LoadExistingInstance(instance, true);
                            }
                        }
                        this.eventConduitAttached.Set();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        internal void Close()
        {
            AppDomain.CurrentDomain.ProcessExit -= new EventHandler(this.OnDomainUnload);
            AppDomain.CurrentDomain.DomainUnload -= new EventHandler(this.OnDomainUnload);
            if (!this.isZombie)
            {
                this.Stop(null, new WorkflowRuntimeEventArgs(false));
            }
        }

        private void Detach()
        {
            using (new DebuggerThreadMarker())
            {
                lock (this.syncRoot)
                {
                    AppDomain.CurrentDomain.AssemblyLoad -= new AssemblyLoadEventHandler(this.OnAssemblyLoad);
                    if (!this.isZombie && this.isAttached)
                    {
                        this.isAttached = false;
                        this.programId = Guid.Empty;
                        if (this.debugControllerThread != null)
                        {
                            this.debugControllerThread.StopThread();
                            this.debugControllerThread = null;
                        }
                        if (this.attachTimer != null)
                        {
                            this.attachTimer.Change(-1, -1);
                            this.attachTimer = null;
                        }
                        RemotingServices.Disconnect(this);
                        if (this.channel != null)
                        {
                            ChannelServices.UnregisterChannel(this.channel);
                            this.channel = null;
                        }
                        this.controllerConduit = null;
                        this.eventConduitAttached.Reset();
                        this.instanceTable = null;
                        this.typeToGuid = null;
                        this.xomlHashToGuid = null;
                        if (!this.serviceContainer.IsZombie)
                        {
                            foreach (WorkflowInstance instance in this.serviceContainer.GetLoadedWorkflows())
                            {
                                WorkflowExecutor workflowResourceUNSAFE = instance.GetWorkflowResourceUNSAFE();
                                using (workflowResourceUNSAFE.ExecutorLock.Enter())
                                {
                                    if (workflowResourceUNSAFE.IsInstanceValid)
                                    {
                                        workflowResourceUNSAFE.WorkflowExecutionEvent -= new EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs>(this.OnInstanceEvent);
                                    }
                                }
                            }
                            this.serviceContainer.WorkflowExecutorInitializing -= new EventHandler<WorkflowRuntime.WorkflowExecutorInitializingEventArgs>(this.InstanceInitializing);
                            this.serviceContainer.DefinitionDispenser.WorkflowDefinitionLoaded -= new EventHandler<WorkflowDefinitionEventArgs>(this.ScheduleTypeLoaded);
                        }
                    }
                }
            }
        }

        private void EnumerateEventHandlersForActivity(Guid scheduleTypeId, Activity activity)
        {
            List<ActivityHandlerDescriptor> handlerMethods = new List<ActivityHandlerDescriptor>();
            MethodInfo method = activity.GetType().GetMethod("System.Workflow.ComponentModel.IDependencyObjectAccessor.GetInvocationList", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (EventInfo info2 in activity.GetType().GetEvents(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                DependencyProperty property = DependencyProperty.FromName(info2.Name, activity.GetType());
                if (property != null)
                {
                    try
                    {
                        foreach (Delegate delegate2 in method.MakeGenericMethod(new Type[] { property.PropertyType }).Invoke(activity, new object[] { property }) as Delegate[])
                        {
                            ActivityHandlerDescriptor descriptor;
                            MethodInfo info4 = delegate2.Method;
                            descriptor.Name = info4.DeclaringType.FullName + "." + info4.Name;
                            descriptor.Token = info4.MetadataToken;
                            handlerMethods.Add(descriptor);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            this.controllerConduit.UpdateHandlerMethodsForActivity(this.programId, scheduleTypeId, activity.QualifiedName, handlerMethods);
        }

        private int GetContextId(Activity activity)
        {
            return ContextActivityUtils.ContextId(ContextActivityUtils.ContextActivity(activity));
        }

        private string GetHierarchicalId(Activity activity)
        {
            string str = string.Empty;
            while (activity != null)
            {
                string str2 = string.Empty;
                Activity activity2 = ContextActivityUtils.ContextActivity(activity);
                int num = ContextActivityUtils.ContextId(activity2);
                str2 = activity.Name + (((num > 1) && (activity == activity2)) ? ("(" + num + ")") : string.Empty);
                str = (str.Length > 0) ? (str2 + "." + str) : str2;
                activity = activity.Parent;
            }
            return str;
        }

        private Guid GetScheduleTypeId(Type scheduleType)
        {
            lock (this.typeToGuid)
            {
                if (!this.typeToGuid.ContainsKey(scheduleType))
                {
                    this.typeToGuid[scheduleType] = Guid.NewGuid();
                }
                return this.typeToGuid[scheduleType];
            }
        }

        private Guid GetScheduleTypeId(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            Activity rootActivity = workflowCoreRuntime.RootActivity;
            if (workflowCoreRuntime.IsDynamicallyUpdated)
            {
                return workflowCoreRuntime.InstanceID;
            }
            if (string.IsNullOrEmpty(rootActivity.GetValue(Activity.WorkflowXamlMarkupProperty) as string))
            {
                return this.GetScheduleTypeId(rootActivity.GetType());
            }
            return this.GetScheduleTypeId(rootActivity.GetValue(WorkflowDefinitionDispenser.WorkflowDefinitionHashCodeProperty) as byte[]);
        }

        private Guid GetScheduleTypeId(byte[] scheduleDefHashCode)
        {
            lock (this.xomlHashToGuid)
            {
                if (!this.xomlHashToGuid.ContainsKey(scheduleDefHashCode))
                {
                    this.xomlHashToGuid[scheduleDefHashCode] = Guid.NewGuid();
                }
                return this.xomlHashToGuid[scheduleDefHashCode];
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal static void InitializeProcessSecurity()
        {
            Exception workerThreadException = null;
            ProcessSecurity security = new ProcessSecurity();
            Thread thread = new Thread(new ThreadStart(security.Initialize));
            security.exceptionNotification = (ExceptionNotification) Delegate.Combine(security.exceptionNotification, e => workerThreadException = e);
            thread.Start();
            thread.Join();
            if (workerThreadException != null)
            {
                throw workerThreadException;
            }
        }

        private void InstanceCompleted(object sender, WorkflowEventArgs args)
        {
            try
            {
                this.UnloadExistingInstance(args.WorkflowInstance);
            }
            catch
            {
            }
        }

        private void InstanceInitializing(object sender, WorkflowRuntime.WorkflowExecutorInitializingEventArgs e)
        {
            try
            {
                if (e.Loading)
                {
                    this.LoadExistingInstance(((WorkflowExecutor) sender).WorkflowInstance, true);
                }
                else
                {
                    this.LoadExistingInstance(((WorkflowExecutor) sender).WorkflowInstance, false);
                }
            }
            catch
            {
            }
        }

        private void InstanceTerminated(object sender, WorkflowEventArgs args)
        {
            try
            {
                this.UnloadExistingInstance(args.WorkflowInstance);
            }
            catch
            {
            }
        }

        private void InstanceUnloaded(object sender, WorkflowEventArgs args)
        {
            try
            {
                this.UnloadExistingInstance(args.WorkflowInstance);
            }
            catch
            {
            }
        }

        private void LoadExistingInstance(WorkflowInstance instance, bool attaching)
        {
            WorkflowExecutor workflowResourceUNSAFE = instance.GetWorkflowResourceUNSAFE();
            if (workflowResourceUNSAFE.IsInstanceValid)
            {
                IWorkflowCoreRuntime workflowCoreRuntime = workflowResourceUNSAFE;
                Activity rootActivity = workflowCoreRuntime.RootActivity;
                Guid scheduleTypeId = this.GetScheduleTypeId(workflowCoreRuntime);
                if (attaching && workflowCoreRuntime.IsDynamicallyUpdated)
                {
                    this.LoadExistingScheduleType(scheduleTypeId, rootActivity.GetType(), true, rootActivity);
                }
                this.instanceTable.AddInstance(instance.InstanceId, rootActivity);
                this.controllerConduit.InstanceCreated(this.programId, instance.InstanceId, scheduleTypeId);
                lock (this.eventLock)
                {
                    workflowResourceUNSAFE.WorkflowExecutionEvent += new EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs>(this.OnInstanceEvent);
                    foreach (Activity activity2 in WalkActivityTree(rootActivity))
                    {
                        this.UpdateActivityStatus(scheduleTypeId, instance.InstanceId, activity2);
                    }
                    ActivityExecutionContext rootContext = new ActivityExecutionContext(rootActivity);
                    foreach (ActivityExecutionContext context2 in WalkExecutionContextTree(rootContext))
                    {
                        foreach (Activity activity4 in WalkActivityTree(context2.Activity))
                        {
                            this.UpdateActivityStatus(scheduleTypeId, instance.InstanceId, activity4);
                        }
                    }
                }
            }
        }

        private void LoadExistingScheduleType(Guid scheduleTypeId, Type scheduleType, bool isDynamic, Activity rootActivity)
        {
            if (rootActivity == null)
            {
                throw new InvalidOperationException();
            }
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (XmlWriter writer2 = Helpers.CreateXmlWriter(writer))
                {
                    new WorkflowMarkupSerializer().Serialize(writer2, rootActivity);
                    string fileName = null;
                    string str2 = null;
                    System.Attribute[] customAttributes = scheduleType.GetCustomAttributes(typeof(WorkflowMarkupSourceAttribute), false) as System.Attribute[];
                    if ((customAttributes != null) && (customAttributes.Length == 1))
                    {
                        fileName = ((WorkflowMarkupSourceAttribute) customAttributes[0]).FileName;
                        str2 = ((WorkflowMarkupSourceAttribute) customAttributes[0]).MD5Digest;
                    }
                    this.controllerConduit.ScheduleTypeLoaded(this.programId, scheduleTypeId, scheduleType.Assembly.FullName, fileName, str2, isDynamic, scheduleType.FullName, scheduleType.Name, writer.ToString());
                }
            }
        }

        private void OnActivityExecuting(object sender, WorkflowExecutor.ActivityExecutingEventArgs eventArgs)
        {
            if (!this.isZombie && this.isAttached)
            {
                try
                {
                    lock (this.eventLock)
                    {
                        IWorkflowCoreRuntime workflowCoreRuntime = (IWorkflowCoreRuntime) sender;
                        Guid scheduleTypeId = this.GetScheduleTypeId(workflowCoreRuntime);
                        this.EnumerateEventHandlersForActivity(scheduleTypeId, eventArgs.Activity);
                        this.controllerConduit.BeforeActivityStatusChanged(this.programId, scheduleTypeId, workflowCoreRuntime.InstanceID, eventArgs.Activity.QualifiedName, this.GetHierarchicalId(eventArgs.Activity), eventArgs.Activity.ExecutionStatus, this.GetContextId(eventArgs.Activity));
                        this.controllerConduit.ActivityStatusChanged(this.programId, scheduleTypeId, workflowCoreRuntime.InstanceID, eventArgs.Activity.QualifiedName, this.GetHierarchicalId(eventArgs.Activity), eventArgs.Activity.ExecutionStatus, this.GetContextId(eventArgs.Activity));
                    }
                }
                catch
                {
                }
            }
        }

        private void OnActivityStatusChanged(object sender, WorkflowExecutor.ActivityStatusChangeEventArgs eventArgs)
        {
            if (!this.isZombie && this.isAttached)
            {
                try
                {
                    lock (this.eventLock)
                    {
                        if (eventArgs.Activity.ExecutionStatus != ActivityExecutionStatus.Executing)
                        {
                            IWorkflowCoreRuntime workflowCoreRuntime = (IWorkflowCoreRuntime) sender;
                            Guid scheduleTypeId = this.GetScheduleTypeId(workflowCoreRuntime);
                            if (eventArgs.Activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                            {
                                this.EnumerateEventHandlersForActivity(scheduleTypeId, eventArgs.Activity);
                            }
                            this.controllerConduit.BeforeActivityStatusChanged(this.programId, scheduleTypeId, workflowCoreRuntime.InstanceID, eventArgs.Activity.QualifiedName, this.GetHierarchicalId(eventArgs.Activity), eventArgs.Activity.ExecutionStatus, this.GetContextId(eventArgs.Activity));
                            this.controllerConduit.ActivityStatusChanged(this.programId, scheduleTypeId, workflowCoreRuntime.InstanceID, eventArgs.Activity.QualifiedName, this.GetHierarchicalId(eventArgs.Activity), eventArgs.Activity.ExecutionStatus, this.GetContextId(eventArgs.Activity));
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.Location != string.Empty)
            {
                try
                {
                    this.controllerConduit.AssemblyLoaded(this.programId, args.LoadedAssembly.Location, args.LoadedAssembly.GlobalAssemblyCache);
                }
                catch
                {
                }
            }
        }

        private void OnDomainUnload(object sender, EventArgs e)
        {
            this.Stop(null, null);
        }

        private void OnHandlerInvoked(object sender, EventArgs eventArgs)
        {
            if (!this.isZombie && this.isAttached)
            {
                try
                {
                    lock (this.eventLock)
                    {
                        IWorkflowCoreRuntime runtime = sender as IWorkflowCoreRuntime;
                        this.controllerConduit.HandlerInvoked(this.programId, runtime.InstanceID, System.Workflow.Runtime.DebugEngine.NativeMethods.GetCurrentThreadId(), this.GetHierarchicalId(runtime.CurrentActivity));
                    }
                }
                catch
                {
                }
            }
        }

        private void OnHandlerInvoking(object sender, EventArgs eventArgs)
        {
        }

        private void OnInstanceEvent(object sender, WorkflowExecutor.WorkflowExecutionEventArgs e)
        {
            WorkflowEventInternal eventType = e.EventType;
            if (eventType <= WorkflowEventInternal.Unloaded)
            {
                if (eventType != WorkflowEventInternal.Completed)
                {
                    if (eventType == WorkflowEventInternal.Unloaded)
                    {
                        this.InstanceUnloaded(sender, new WorkflowEventArgs(((WorkflowExecutor) sender).WorkflowInstance));
                    }
                    return;
                }
            }
            else
            {
                switch (eventType)
                {
                    case WorkflowEventInternal.Changed:
                        this.OnWorkflowChanged(sender, e);
                        return;

                    case WorkflowEventInternal.HandlerInvoking:
                        this.OnHandlerInvoking(sender, e);
                        return;

                    case WorkflowEventInternal.HandlerInvoked:
                        this.OnHandlerInvoked(sender, e);
                        return;

                    case WorkflowEventInternal.ActivityExecuting:
                        this.OnActivityExecuting(sender, (WorkflowExecutor.ActivityExecutingEventArgs) e);
                        return;

                    case WorkflowEventInternal.ActivityStatusChange:
                        this.OnActivityStatusChanged(sender, (WorkflowExecutor.ActivityStatusChangeEventArgs) e);
                        return;

                    case WorkflowEventInternal.Terminated:
                        this.InstanceTerminated(sender, new WorkflowEventArgs(((WorkflowExecutor) sender).WorkflowInstance));
                        return;
                }
                return;
            }
            this.InstanceCompleted(sender, new WorkflowEventArgs(((WorkflowExecutor) sender).WorkflowInstance));
        }

        private void OnWorkflowChanged(object sender, EventArgs eventArgs)
        {
            if (!this.isZombie && this.isAttached)
            {
                try
                {
                    lock (this.eventLock)
                    {
                        IWorkflowCoreRuntime runtime = (IWorkflowCoreRuntime) sender;
                        Activity rootActivity = this.instanceTable.GetRootActivity(runtime.InstanceID);
                        Guid instanceID = runtime.InstanceID;
                        this.LoadExistingScheduleType(instanceID, rootActivity.GetType(), true, rootActivity);
                        this.instanceTable.UpdateRootActivity(runtime.InstanceID, rootActivity);
                        this.controllerConduit.InstanceDynamicallyUpdated(this.programId, runtime.InstanceID, instanceID);
                    }
                }
                catch
                {
                }
            }
        }

        private void ScheduleTypeLoaded(object sender, WorkflowDefinitionEventArgs args)
        {
            try
            {
                if (args.WorkflowType != null)
                {
                    Activity workflowDefinition = ((WorkflowRuntime) sender).DefinitionDispenser.GetWorkflowDefinition(args.WorkflowType);
                    this.LoadExistingScheduleType(this.GetScheduleTypeId(args.WorkflowType), args.WorkflowType, false, workflowDefinition);
                }
                else
                {
                    Activity rootActivity = ((WorkflowRuntime) sender).DefinitionDispenser.GetWorkflowDefinition(args.WorkflowDefinitionHashCode);
                    this.LoadExistingScheduleType(this.GetScheduleTypeId(args.WorkflowDefinitionHashCode), rootActivity.GetType(), false, rootActivity);
                }
            }
            catch
            {
            }
        }

        private void Start(object source, WorkflowRuntimeEventArgs e)
        {
            this.isZombie = false;
            this.isAttached = false;
            this.eventConduitAttached = new ManualResetEvent(false);
            this.isServiceContainerStarting = true;
            bool flag = this.programPublisher.Publish(this);
            while ((flag && this.isAttached) && !this.eventConduitAttached.WaitOne(this.attachTimeout, false))
            {
            }
            this.isServiceContainerStarting = false;
        }

        private void Stop(object source, WorkflowRuntimeEventArgs e)
        {
            try
            {
                lock (this.syncRoot)
                {
                    this.Detach();
                    this.programPublisher.Unpublish();
                    this.isZombie = true;
                }
            }
            catch
            {
            }
        }

        private void UnloadExistingInstance(WorkflowInstance instance)
        {
            this.controllerConduit.InstanceCompleted(this.programId, instance.InstanceId);
            this.instanceTable.RemoveInstance(instance.InstanceId);
        }

        private void UpdateActivityStatus(Guid scheduleTypeId, Guid instanceId, Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                this.EnumerateEventHandlersForActivity(scheduleTypeId, activity);
            }
            if (activity.ExecutionStatus != ActivityExecutionStatus.Initialized)
            {
                int stateReaderId = ContextActivityUtils.ContextId(ContextActivityUtils.ContextActivity(activity));
                this.controllerConduit.SetInitialActivityStatus(this.programId, scheduleTypeId, instanceId, activity.QualifiedName, this.GetHierarchicalId(activity), activity.ExecutionStatus, stateReaderId);
            }
        }

        private static IEnumerable WalkActivityTree(Activity rootActivity)
        {
            if ((rootActivity != null) && rootActivity.Enabled)
            {
                yield return rootActivity;
                if (rootActivity is CompositeActivity)
                {
                    foreach (Activity iteratorVariable0 in ((CompositeActivity) rootActivity).Activities)
                    {
                        IEnumerator enumerator = WalkActivityTree(iteratorVariable0).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            Activity current = (Activity) enumerator.Current;
                            yield return current;
                        }
                    }
                }
            }
        }

        private static IEnumerable WalkExecutionContextTree(ActivityExecutionContext rootContext)
        {
            if (rootContext != null)
            {
                yield return rootContext;
                foreach (ActivityExecutionContext iteratorVariable0 in rootContext.ExecutionContextManager.ExecutionContexts)
                {
                    IEnumerator enumerator = WalkExecutionContextTree(iteratorVariable0).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        ActivityExecutionContext current = (ActivityExecutionContext) enumerator.Current;
                        yield return current;
                    }
                }
            }
        }



        private delegate void ExceptionNotification(Exception e);

        private class ProcessSecurity
        {
            internal DebugController.ExceptionNotification exceptionNotification;

            private int FindIndexInDacl(CommonAce newAce, RawAcl dacl)
            {
                int num = 0;
                num = 0;
                while (num < dacl.Count)
                {
                    if (((dacl[num] is CommonAce) && (((CommonAce) dacl[num]).SecurityIdentifier.Value == newAce.SecurityIdentifier.Value)) && (dacl[num].AceType == newAce.AceType))
                    {
                        return -1;
                    }
                    if (((newAce.AceType != AceType.AccessDenied) || (dacl[num].AceType != AceType.AccessDenied)) || (newAce.IsInherited || dacl[num].IsInherited))
                    {
                        if ((newAce.AceType == AceType.AccessDenied) && !newAce.IsInherited)
                        {
                            return num;
                        }
                        if ((((newAce.AceType != AceType.AccessAllowed) || (dacl[num].AceType != AceType.AccessAllowed)) || (newAce.IsInherited || dacl[num].IsInherited)) && ((newAce.AceType == AceType.AccessAllowed) && !newAce.IsInherited))
                        {
                            return num;
                        }
                    }
                    num++;
                }
                return num;
            }

            private RawAcl GetCurrentProcessTokenDacl()
            {
                RawAcl discretionaryAcl;
                IntPtr zero = IntPtr.Zero;
                IntPtr tokenHandle = IntPtr.Zero;
                IntPtr pSecurityDescriptor = IntPtr.Zero;
                try
                {
                    uint num;
                    zero = System.Workflow.Runtime.DebugEngine.NativeMethods.GetCurrentProcess();
                    if (!System.Workflow.Runtime.DebugEngine.NativeMethods.OpenProcessToken(zero, 0xf00ff, out tokenHandle))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    System.Workflow.Runtime.DebugEngine.NativeMethods.GetKernelObjectSecurity(tokenHandle, System.Workflow.Runtime.DebugEngine.NativeMethods.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, IntPtr.Zero, 0, out num);
                    Marshal.GetLastWin32Error();
                    pSecurityDescriptor = Marshal.AllocCoTaskMem((int) num);
                    if (!System.Workflow.Runtime.DebugEngine.NativeMethods.GetKernelObjectSecurity(tokenHandle, System.Workflow.Runtime.DebugEngine.NativeMethods.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, pSecurityDescriptor, num, out num))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    byte[] destination = new byte[num];
                    Marshal.Copy(pSecurityDescriptor, destination, 0, (int) num);
                    RawSecurityDescriptor descriptor = new RawSecurityDescriptor(destination, 0);
                    discretionaryAcl = descriptor.DiscretionaryAcl;
                }
                finally
                {
                    if (((zero != IntPtr.Zero) && (zero != ((IntPtr) (-1)))) && !System.Workflow.Runtime.DebugEngine.NativeMethods.CloseHandle(zero))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    if ((tokenHandle != IntPtr.Zero) && !System.Workflow.Runtime.DebugEngine.NativeMethods.CloseHandle(tokenHandle))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    if (pSecurityDescriptor != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pSecurityDescriptor);
                    }
                }
                return discretionaryAcl;
            }

            internal void Initialize()
            {
                try
                {
                    if (!System.Workflow.Runtime.DebugEngine.NativeMethods.RevertToSelf())
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    RawAcl currentProcessTokenDacl = this.GetCurrentProcessTokenDacl();
                    CommonAce newAce = new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, 8, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), false, null);
                    int index = this.FindIndexInDacl(newAce, currentProcessTokenDacl);
                    if (index != -1)
                    {
                        currentProcessTokenDacl.InsertAce(index, newAce);
                    }
                    this.SetCurrentProcessTokenDacl(currentProcessTokenDacl);
                }
                catch (Exception exception)
                {
                    if (this.exceptionNotification != null)
                    {
                        this.exceptionNotification(exception);
                    }
                }
            }

            private void SetCurrentProcessTokenDacl(RawAcl dacl)
            {
                IntPtr zero = IntPtr.Zero;
                IntPtr tokenHandle = IntPtr.Zero;
                IntPtr pSecurityDescriptor = IntPtr.Zero;
                try
                {
                    uint num;
                    zero = System.Workflow.Runtime.DebugEngine.NativeMethods.GetCurrentProcess();
                    if (!System.Workflow.Runtime.DebugEngine.NativeMethods.OpenProcessToken(zero, 0xf00ff, out tokenHandle))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    System.Workflow.Runtime.DebugEngine.NativeMethods.GetKernelObjectSecurity(tokenHandle, System.Workflow.Runtime.DebugEngine.NativeMethods.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, IntPtr.Zero, 0, out num);
                    Marshal.GetLastWin32Error();
                    pSecurityDescriptor = Marshal.AllocCoTaskMem((int) num);
                    if (!System.Workflow.Runtime.DebugEngine.NativeMethods.GetKernelObjectSecurity(tokenHandle, System.Workflow.Runtime.DebugEngine.NativeMethods.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, pSecurityDescriptor, num, out num))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    byte[] destination = new byte[num];
                    Marshal.Copy(pSecurityDescriptor, destination, 0, (int) num);
                    RawSecurityDescriptor descriptor = new RawSecurityDescriptor(destination, 0) {
                        DiscretionaryAcl = dacl
                    };
                    destination = new byte[descriptor.BinaryLength];
                    descriptor.GetBinaryForm(destination, 0);
                    Marshal.FreeCoTaskMem(pSecurityDescriptor);
                    pSecurityDescriptor = Marshal.AllocCoTaskMem(descriptor.BinaryLength);
                    Marshal.Copy(destination, 0, pSecurityDescriptor, descriptor.BinaryLength);
                    if (!System.Workflow.Runtime.DebugEngine.NativeMethods.SetKernelObjectSecurity(tokenHandle, System.Workflow.Runtime.DebugEngine.NativeMethods.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, pSecurityDescriptor))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                }
                finally
                {
                    if (((zero != IntPtr.Zero) && (zero != ((IntPtr) (-1)))) && !System.Workflow.Runtime.DebugEngine.NativeMethods.CloseHandle(zero))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    if ((tokenHandle != IntPtr.Zero) && !System.Workflow.Runtime.DebugEngine.NativeMethods.CloseHandle(tokenHandle))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    if (pSecurityDescriptor != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pSecurityDescriptor);
                    }
                }
            }
        }
    }
}

