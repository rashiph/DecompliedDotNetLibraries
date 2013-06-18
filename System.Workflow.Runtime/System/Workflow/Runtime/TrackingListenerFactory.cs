namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Timers;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime.Tracking;

    internal class TrackingListenerFactory
    {
        private bool _initialized;
        private double _interval = 60000.0;
        private volatile object _listenerLock = new object();
        private Dictionary<Guid, WeakReference> _listeners = new Dictionary<Guid, WeakReference>();
        private System.Workflow.Runtime.TrackingProfileManager _profileManager = new System.Workflow.Runtime.TrackingProfileManager();
        private List<TrackingService> _services;
        private Timer _timer;

        internal TrackingListenerFactory()
        {
        }

        private void Cleanup(object sender, ElapsedEventArgs e)
        {
            List<Guid> list = new List<Guid>();
            if ((this._listeners != null) || (this._listeners.Count > 0))
            {
                lock (this._listenerLock)
                {
                    foreach (KeyValuePair<Guid, WeakReference> pair in this._listeners)
                    {
                        if (pair.Value.Target == null)
                        {
                            list.Add(pair.Key);
                        }
                    }
                    if (list.Count > 0)
                    {
                        foreach (Guid guid in list)
                        {
                            this._listeners.Remove(guid);
                        }
                    }
                }
            }
            lock (this)
            {
                if (this._timer != null)
                {
                    this._timer.Start();
                }
            }
        }

        private List<TrackingChannelWrapper> GetChannels(Activity schedule, WorkflowExecutor exec, Guid instanceID, Type workflowType, ref TrackingListenerBroker broker)
        {
            if (this._services == null)
            {
                return null;
            }
            bool flag = false;
            if (broker == null)
            {
                broker = new TrackingListenerBroker();
                flag = true;
            }
            List<TrackingChannelWrapper> list = new List<TrackingChannelWrapper>();
            List<string> callPath = null;
            Guid empty = Guid.Empty;
            Guid context = this.GetContext(exec.RootActivity);
            Guid callerContextGuid = Guid.Empty;
            Guid callerParentContextGuid = Guid.Empty;
            TrackingCallingState trackingCallingState = exec.TrackingCallingState;
            TrackingListenerBroker broker1 = (TrackingListenerBroker) exec.RootActivity.GetValue(WorkflowExecutor.TrackingListenerBrokerProperty);
            IList<string> collection = (trackingCallingState != null) ? trackingCallingState.CallerActivityPathProxy : null;
            if ((collection != null) && (collection.Count > 0))
            {
                callPath = new List<string>(collection);
                empty = trackingCallingState.CallerWorkflowInstanceId;
                callerContextGuid = trackingCallingState.CallerContextGuid;
                callerParentContextGuid = trackingCallingState.CallerParentContextGuid;
            }
            TrackingParameters parameters = new TrackingParameters(instanceID, workflowType, exec.WorkflowDefinition, callPath, empty, context, callerContextGuid, callerParentContextGuid);
            for (int i = 0; i < this._services.Count; i++)
            {
                TrackingChannel trackingChannel = null;
                Type trackingServiceType = this._services[i].GetType();
                RTTrackingProfile profile = null;
                if (flag)
                {
                    profile = this._profileManager.GetProfile(this._services[i], schedule);
                    if (profile == null)
                    {
                        continue;
                    }
                    broker.AddService(trackingServiceType, profile.Version);
                }
                else
                {
                    if (!broker.ContainsService(trackingServiceType))
                    {
                        continue;
                    }
                    if (broker.IsProfileInstance(trackingServiceType))
                    {
                        profile = this._profileManager.GetProfile(this._services[i], schedule, instanceID);
                        if (profile == null)
                        {
                            throw new InvalidOperationException(ExecutionStringManager.MissingProfileForService + trackingServiceType.ToString());
                        }
                        profile.IsPrivate = true;
                    }
                    else
                    {
                        Version version;
                        if (!broker.TryGetProfileVersionId(trackingServiceType, out version))
                        {
                            continue;
                        }
                        profile = this._profileManager.GetProfile(this._services[i], schedule, version);
                        if (profile == null)
                        {
                            throw new InvalidOperationException(ExecutionStringManager.MissingProfileForService + trackingServiceType.ToString() + ExecutionStringManager.MissingProfileForVersion + version.ToString());
                        }
                        if (broker.IsProfilePrivate(trackingServiceType))
                        {
                            profile = profile.Clone();
                            profile.IsPrivate = true;
                        }
                    }
                }
                trackingChannel = this._services[i].GetTrackingChannel(parameters);
                if (trackingChannel == null)
                {
                    throw new InvalidOperationException(ExecutionStringManager.NullChannel);
                }
                list.Add(new TrackingChannelWrapper(trackingChannel, this._services[i].GetType(), workflowType, profile));
            }
            return list;
        }

        internal Guid GetContext(Activity activity)
        {
            return ((ActivityExecutionContextInfo) ContextActivityUtils.ContextActivity(activity).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;
        }

        private TrackingListener GetListener(Activity sked, WorkflowExecutor skedExec, TrackingListenerBroker broker)
        {
            if ((sked == null) || (skedExec == null))
            {
                WorkflowTrace.Tracking.TraceEvent(TraceEventType.Error, 0, ExecutionStringManager.NullParameters);
                return null;
            }
            if ((this._services != null) && (this._services.Count > 0))
            {
                bool load = null != broker;
                List<TrackingChannelWrapper> channels = this.GetChannels(sked, skedExec, skedExec.InstanceId, sked.GetType(), ref broker);
                if ((channels != null) && (channels.Count != 0))
                {
                    return new TrackingListener(this, sked, skedExec, channels, broker, load);
                }
            }
            return null;
        }

        private TrackingListener GetListenerFromWRCache(Guid instanceId)
        {
            WeakReference reference = null;
            TrackingListener target = null;
            lock (this._listenerLock)
            {
                if (!this._listeners.TryGetValue(instanceId, out reference))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExecutionStringManager.ListenerNotInCache, new object[] { instanceId }));
                }
                target = reference.Target as TrackingListener;
                if (target == null)
                {
                    throw new ObjectDisposedException(string.Format(CultureInfo.InvariantCulture, ExecutionStringManager.ListenerNotInCacheDisposed, new object[] { instanceId }));
                }
            }
            return target;
        }

        internal TrackingListener GetTrackingListener(Activity sked, WorkflowExecutor skedExec)
        {
            if (!this._initialized)
            {
                this.Initialize(skedExec.WorkflowRuntime);
            }
            return this.GetListener(sked, skedExec, null);
        }

        internal TrackingListener GetTrackingListener(Activity sked, WorkflowExecutor skedExec, TrackingListenerBroker broker)
        {
            if (!this._initialized)
            {
                this.Initialize(skedExec.WorkflowRuntime);
            }
            if (broker == null)
            {
                WorkflowTrace.Tracking.TraceEvent(TraceEventType.Error, 0, ExecutionStringManager.NullTrackingBroker);
                return null;
            }
            return this.GetListener(sked, skedExec, broker);
        }

        internal void Initialize(WorkflowRuntime runtime)
        {
            lock (this)
            {
                this._services = runtime.TrackingServices;
                this._profileManager.Initialize(runtime);
                runtime.WorkflowExecutorInitializing += new EventHandler<WorkflowRuntime.WorkflowExecutorInitializingEventArgs>(this.WorkflowExecutorInitializing);
                this._timer = new Timer();
                this._timer.Interval = this._interval;
                this._timer.AutoReset = false;
                this._timer.Elapsed += new ElapsedEventHandler(this.Cleanup);
                this._timer.Start();
                this._initialized = true;
            }
        }

        internal void ReloadProfiles(WorkflowExecutor exec)
        {
            using (new ServiceEnvironment(exec.RootActivity))
            {
                using (exec.ExecutorLock.Enter())
                {
                    if (!exec.IsInstanceValid)
                    {
                        throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                    }
                    bool flag = exec.Suspend(ExecutionStringManager.TrackingProfileUpdate);
                    try
                    {
                        this.GetListenerFromWRCache(exec.InstanceId).ReloadProfiles(exec, exec.InstanceId);
                    }
                    finally
                    {
                        if (flag)
                        {
                            exec.Resume();
                        }
                    }
                }
            }
        }

        internal void ReloadProfiles(WorkflowExecutor exec, Guid instanceId, ref TrackingListenerBroker broker, ref List<TrackingChannelWrapper> channels)
        {
            Type workflowType = exec.WorkflowDefinition.GetType();
            foreach (TrackingService service in this._services)
            {
                TrackingProfile profile = null;
                TrackingChannelWrapper wrapper = null;
                if (service.TryReloadProfile(workflowType, instanceId, out profile))
                {
                    bool flag = false;
                    int index = 0;
                    while (index < channels.Count)
                    {
                        if (service.GetType() == channels[index].TrackingServiceType)
                        {
                            wrapper = channels[index];
                            flag = true;
                            break;
                        }
                        index++;
                    }
                    if (profile == null)
                    {
                        if (flag)
                        {
                            broker.RemoveService(wrapper.TrackingServiceType);
                            channels.RemoveAt(index);
                        }
                    }
                    else
                    {
                        RTTrackingProfile profile2 = new RTTrackingProfile(profile, exec.WorkflowDefinition, workflowType) {
                            IsPrivate = true
                        };
                        if (!flag)
                        {
                            List<string> callPath = null;
                            Guid empty = Guid.Empty;
                            TrackingCallingState trackingCallingState = exec.TrackingCallingState;
                            IList<string> collection = null;
                            Guid context = this.GetContext(exec.RootActivity);
                            Guid callerContextGuid = Guid.Empty;
                            Guid callerParentContextGuid = Guid.Empty;
                            if (trackingCallingState != null)
                            {
                                collection = trackingCallingState.CallerActivityPathProxy;
                                if ((collection != null) && (collection.Count > 0))
                                {
                                    callPath = new List<string>(collection);
                                    empty = trackingCallingState.CallerWorkflowInstanceId;
                                    callerContextGuid = trackingCallingState.CallerContextGuid;
                                    callerParentContextGuid = trackingCallingState.CallerParentContextGuid;
                                }
                            }
                            TrackingParameters parameters = new TrackingParameters(instanceId, workflowType, exec.WorkflowDefinition, callPath, empty, context, callerContextGuid, callerParentContextGuid);
                            TrackingChannelWrapper item = new TrackingChannelWrapper(service.GetTrackingChannel(parameters), service.GetType(), workflowType, profile2);
                            channels.Add(item);
                            Type type = service.GetType();
                            broker.AddService(type, profile2.Version);
                            broker.MakeProfileInstance(type);
                        }
                        else
                        {
                            wrapper.SetTrackingProfile(profile2);
                            broker.MakeProfileInstance(wrapper.TrackingServiceType);
                        }
                    }
                }
            }
        }

        internal void Uninitialize(WorkflowRuntime runtime)
        {
            lock (this)
            {
                this._profileManager.Uninitialize();
                runtime.WorkflowExecutorInitializing -= new EventHandler<WorkflowRuntime.WorkflowExecutorInitializingEventArgs>(this.WorkflowExecutorInitializing);
                this._timer.Elapsed -= new ElapsedEventHandler(this.Cleanup);
                this._timer.Stop();
                this._services = null;
                this._initialized = false;
                this._timer.Dispose();
                this._timer = null;
            }
        }

        private void WorkflowExecutionEvent(object sender, WorkflowExecutor.WorkflowExecutionEventArgs e)
        {
            switch (e.EventType)
            {
                case WorkflowEventInternal.Terminated:
                case WorkflowEventInternal.Aborted:
                case WorkflowEventInternal.Completed:
                {
                    WorkflowExecutor executor = (WorkflowExecutor) sender;
                    lock (this._listenerLock)
                    {
                        this._listeners.Remove(executor.ID);
                    }
                    break;
                }
                case WorkflowEventInternal.Aborting:
                    break;

                default:
                    return;
            }
        }

        private void WorkflowExecutorInitializing(object sender, WorkflowRuntime.WorkflowExecutorInitializingEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
            {
                throw new ArgumentException("sender");
            }
            WorkflowExecutor skedExec = (WorkflowExecutor) sender;
            skedExec.WorkflowExecutionEvent += new EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs>(this.WorkflowExecutionEvent);
            TrackingCallingState trackingCallingState = skedExec.TrackingCallingState;
            TrackingListenerBroker broker = (TrackingListenerBroker) skedExec.RootActivity.GetValue(WorkflowExecutor.TrackingListenerBrokerProperty);
            if (broker != null)
            {
                broker.ReplaceServices(skedExec.WorkflowRuntime.TrackingServiceReplacement);
            }
            TrackingListener target = null;
            WeakReference reference = null;
            if (e.Loading)
            {
                bool flag = false;
                lock (this._listenerLock)
                {
                    flag = this._listeners.TryGetValue(skedExec.InstanceId, out reference);
                }
                if (flag)
                {
                    try
                    {
                        target = reference.Target as TrackingListener;
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
                if (target != null)
                {
                    target.Broker = broker;
                    goto Label_01C8;
                }
                target = this.GetTrackingListener(skedExec.WorkflowDefinition, skedExec, broker);
                if (target == null)
                {
                    goto Label_01C8;
                }
                if (reference != null)
                {
                    reference.Target = target;
                    goto Label_01C8;
                }
                lock (this._listenerLock)
                {
                    this._listeners.Add(skedExec.ID, new WeakReference(target));
                    goto Label_01C8;
                }
            }
            target = this.GetTrackingListener(skedExec.WorkflowDefinition, skedExec);
            if (target != null)
            {
                skedExec.RootActivity.SetValue(WorkflowExecutor.TrackingListenerBrokerProperty, target.Broker);
                lock (this._listenerLock)
                {
                    this._listeners.Add(skedExec.ID, new WeakReference(target));
                    goto Label_01C8;
                }
            }
            skedExec.RootActivity.SetValue(WorkflowExecutor.TrackingListenerBrokerProperty, new TrackingListenerBroker());
        Label_01C8:
            if (target != null)
            {
                skedExec.WorkflowExecutionEvent += new EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs>(target.WorkflowExecutionEvent);
            }
        }

        internal System.Workflow.Runtime.TrackingProfileManager TrackingProfileManager
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._profileManager;
            }
        }
    }
}

