namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime.Tracking;

    internal class TrackingListener
    {
        private TrackingListenerBroker _broker;
        private List<TrackingChannelWrapper> _channels;
        private TrackingListenerFactory _factory;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TrackingListener()
        {
        }

        internal TrackingListener(TrackingListenerFactory factory, Activity sked, WorkflowExecutor exec, List<TrackingChannelWrapper> channels, TrackingListenerBroker broker, bool load)
        {
            if ((sked == null) || (broker == null))
            {
                WorkflowTrace.Tracking.TraceEvent(TraceEventType.Error, 0, ExecutionStringManager.NullParameters);
            }
            else
            {
                this._factory = factory;
                this._channels = channels;
                this._broker = broker;
                this._broker.TrackingListener = this;
            }
        }

        internal void ActivityStatusChange(object sender, WorkflowExecutor.ActivityStatusChangeEventArgs e)
        {
            WorkflowTrace.Tracking.TraceInformation("TrackingListener::ActivityStatusChange - Received Activity Status Change Event for activity {0}", new object[] { e.Activity.QualifiedName });
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
            {
                throw new ArgumentException("sender");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            WorkflowExecutor exec = (WorkflowExecutor) sender;
            if ((this._channels == null) || (this._channels.Count <= 0))
            {
                WorkflowTrace.Tracking.TraceEvent(TraceEventType.Error, 0, ExecutionStringManager.NoChannels);
            }
            else
            {
                Activity activity = e.Activity;
                if (this.SubscriptionRequired(activity, exec))
                {
                    Guid empty = Guid.Empty;
                    Guid contextGuid = Guid.Empty;
                    this.GetContext(activity, exec, out contextGuid, out empty);
                    DateTime utcNow = DateTime.UtcNow;
                    int nextEventOrderId = this._broker.GetNextEventOrderId();
                    foreach (TrackingChannelWrapper wrapper in this._channels)
                    {
                        ActivityTrackingRecord record = new ActivityTrackingRecord(activity.GetType(), activity.QualifiedName, contextGuid, empty, activity.ExecutionStatus, utcNow, nextEventOrderId, null);
                        if (wrapper.GetTrackingProfile(exec).TryTrackActivityEvent(activity, activity.ExecutionStatus, exec, record))
                        {
                            wrapper.TrackingChannel.Send(record);
                        }
                    }
                }
            }
        }

        internal void DynamicUpdateBegin(object sender, WorkflowExecutor.DynamicUpdateEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
            {
                throw new ArgumentException("sender");
            }
            WorkflowExecutor exec = (WorkflowExecutor) sender;
            if (e.ChangeActions != null)
            {
                this.MakeProfilesPrivate(exec);
                foreach (TrackingChannelWrapper wrapper in this._channels)
                {
                    wrapper.GetTrackingProfile(exec).WorkflowChangeBegin(e.ChangeActions);
                }
            }
        }

        internal void DynamicUpdateCommit(object sender, WorkflowExecutor.DynamicUpdateEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
            {
                throw new ArgumentException("sender");
            }
            WorkflowExecutor skedExec = (WorkflowExecutor) sender;
            DateTime utcNow = DateTime.UtcNow;
            foreach (TrackingChannelWrapper wrapper in this._channels)
            {
                wrapper.GetTrackingProfile(skedExec).WorkflowChangeCommit();
            }
            int nextEventOrderId = this._broker.GetNextEventOrderId();
            foreach (TrackingChannelWrapper wrapper2 in this._channels)
            {
                WorkflowTrackingRecord record = new WorkflowTrackingRecord(TrackingWorkflowEvent.Changed, utcNow, nextEventOrderId, new TrackingWorkflowChangedEventArgs(e.ChangeActions, skedExec.WorkflowDefinition));
                if (wrapper2.GetTrackingProfile(skedExec).TryTrackInstanceEvent(TrackingWorkflowEvent.Changed, record))
                {
                    wrapper2.TrackingChannel.Send(record);
                }
            }
        }

        internal void DynamicUpdateRollback(object sender, WorkflowExecutor.DynamicUpdateEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
            {
                throw new ArgumentException("sender");
            }
            WorkflowExecutor skedExec = (WorkflowExecutor) sender;
            foreach (TrackingChannelWrapper wrapper in this._channels)
            {
                wrapper.GetTrackingProfile(skedExec).WorkflowChangeRollback();
            }
        }

        private void GetContext(Activity activity, WorkflowExecutor exec, out Guid contextGuid, out Guid parentContextGuid)
        {
            contextGuid = this._factory.GetContext(activity);
            if (activity.Parent != null)
            {
                parentContextGuid = this._factory.GetContext(activity.Parent);
            }
            else
            {
                parentContextGuid = contextGuid;
            }
        }

        private void MakeProfilesPrivate(WorkflowExecutor exec)
        {
            foreach (TrackingChannelWrapper wrapper in this._channels)
            {
                wrapper.MakeProfilePrivate(exec);
                this._broker.MakeProfilePrivate(wrapper.TrackingServiceType);
            }
        }

        private void NotifyChannels(TrackingWorkflowEvent evt, WorkflowExecutor.WorkflowExecutionEventArgs e, WorkflowExecutor exec)
        {
            DateTime utcNow = DateTime.UtcNow;
            int nextEventOrderId = this._broker.GetNextEventOrderId();
            foreach (TrackingChannelWrapper wrapper in this._channels)
            {
                WorkflowExecutor.WorkflowExecutionTerminatingEventArgs args2;
                WorkflowTrackingRecord record;
                EventArgs eventArgs = null;
                switch (evt)
                {
                    case TrackingWorkflowEvent.Exception:
                    {
                        WorkflowExecutor.WorkflowExecutionExceptionEventArgs args3 = (WorkflowExecutor.WorkflowExecutionExceptionEventArgs) e;
                        eventArgs = new TrackingWorkflowExceptionEventArgs(args3.Exception, args3.CurrentPath, args3.OriginalPath, args3.ContextGuid, args3.ParentContextGuid);
                        goto Label_00BC;
                    }
                    case TrackingWorkflowEvent.Terminated:
                        args2 = (WorkflowExecutor.WorkflowExecutionTerminatingEventArgs) e;
                        if (args2.Exception == null)
                        {
                            break;
                        }
                        eventArgs = new TrackingWorkflowTerminatedEventArgs(args2.Exception);
                        goto Label_00BC;

                    case TrackingWorkflowEvent.Suspended:
                        eventArgs = new TrackingWorkflowSuspendedEventArgs(((WorkflowExecutor.WorkflowExecutionSuspendingEventArgs) e).Error);
                        goto Label_00BC;

                    default:
                        goto Label_00BC;
                }
                eventArgs = new TrackingWorkflowTerminatedEventArgs(args2.Error);
            Label_00BC:
                record = new WorkflowTrackingRecord(evt, utcNow, nextEventOrderId, eventArgs);
                if (wrapper.GetTrackingProfile(exec).TryTrackInstanceEvent(evt, record))
                {
                    wrapper.TrackingChannel.Send(record);
                }
            }
        }

        private void NotifyChannelsOfCompletionOrTermination()
        {
            foreach (TrackingChannelWrapper wrapper in this._channels)
            {
                wrapper.TrackingChannel.InstanceCompletedOrTerminated();
            }
        }

        internal void ReloadProfiles(WorkflowExecutor exec, Guid instanceId)
        {
            this._factory.ReloadProfiles(exec, instanceId, ref this._broker, ref this._channels);
        }

        private bool SubscriptionRequired(Activity activity, WorkflowExecutor exec)
        {
            bool flag = false;
            foreach (TrackingChannelWrapper wrapper in this._channels)
            {
                if (wrapper.GetTrackingProfile(exec).ActivitySubscriptionNeeded(activity) && !flag)
                {
                    flag = true;
                }
            }
            return flag;
        }

        internal void UserTrackPoint(object sender, WorkflowExecutor.UserTrackPointEventArgs e)
        {
            Guid guid;
            Guid guid2;
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
            {
                throw new ArgumentException("sender is not WorkflowExecutor");
            }
            WorkflowExecutor exec = (WorkflowExecutor) sender;
            Activity activity = e.Activity;
            DateTime utcNow = DateTime.UtcNow;
            int nextEventOrderId = this._broker.GetNextEventOrderId();
            this.GetContext(activity, exec, out guid2, out guid);
            foreach (TrackingChannelWrapper wrapper in this._channels)
            {
                UserTrackingRecord record = new UserTrackingRecord(activity.GetType(), activity.QualifiedName, guid2, guid, utcNow, nextEventOrderId, e.Key, e.Args);
                if (wrapper.GetTrackingProfile(exec).TryTrackUserEvent(activity, e.Key, e.Args, exec, record))
                {
                    wrapper.TrackingChannel.Send(record);
                }
            }
        }

        internal void WorkflowExecutionEvent(object sender, WorkflowExecutor.WorkflowExecutionEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            WorkflowExecutor exec = sender as WorkflowExecutor;
            if (exec == null)
            {
                throw new ArgumentException(ExecutionStringManager.InvalidSenderWorkflowExecutor);
            }
            switch (e.EventType)
            {
                case WorkflowEventInternal.Completing:
                    this.NotifyChannels(TrackingWorkflowEvent.Completed, e, exec);
                    this.NotifyChannelsOfCompletionOrTermination();
                    return;

                case WorkflowEventInternal.Completed:
                case WorkflowEventInternal.Idle:
                case WorkflowEventInternal.Suspended:
                case WorkflowEventInternal.Resumed:
                case WorkflowEventInternal.Persisted:
                case WorkflowEventInternal.Unloaded:
                case WorkflowEventInternal.Loaded:
                case WorkflowEventInternal.Terminated:
                case WorkflowEventInternal.Aborted:
                case WorkflowEventInternal.Runnable:
                case WorkflowEventInternal.Executing:
                case WorkflowEventInternal.NotExecuting:
                case WorkflowEventInternal.ActivityStateCreated:
                case WorkflowEventInternal.HandlerEntered:
                case WorkflowEventInternal.HandlerExited:
                    break;

                case WorkflowEventInternal.SchedulerEmpty:
                    this.NotifyChannels(TrackingWorkflowEvent.Idle, e, exec);
                    return;

                case WorkflowEventInternal.Suspending:
                    this.NotifyChannels(TrackingWorkflowEvent.Suspended, e, exec);
                    return;

                case WorkflowEventInternal.Resuming:
                    this.NotifyChannels(TrackingWorkflowEvent.Resumed, e, exec);
                    return;

                case WorkflowEventInternal.Persisting:
                    this.NotifyChannels(TrackingWorkflowEvent.Persisted, e, exec);
                    return;

                case WorkflowEventInternal.Unloading:
                    this.NotifyChannels(TrackingWorkflowEvent.Unloaded, e, exec);
                    return;

                case WorkflowEventInternal.Exception:
                    this.NotifyChannels(TrackingWorkflowEvent.Exception, e, exec);
                    return;

                case WorkflowEventInternal.Terminating:
                    this.NotifyChannels(TrackingWorkflowEvent.Terminated, e, exec);
                    this.NotifyChannelsOfCompletionOrTermination();
                    return;

                case WorkflowEventInternal.Aborting:
                    this.NotifyChannels(TrackingWorkflowEvent.Aborted, e, exec);
                    return;

                case WorkflowEventInternal.UserTrackPoint:
                    this.UserTrackPoint(exec, (WorkflowExecutor.UserTrackPointEventArgs) e);
                    return;

                case WorkflowEventInternal.ActivityStatusChange:
                    this.ActivityStatusChange(exec, (WorkflowExecutor.ActivityStatusChangeEventArgs) e);
                    return;

                case WorkflowEventInternal.DynamicChangeBegin:
                    this.DynamicUpdateBegin(exec, (WorkflowExecutor.DynamicUpdateEventArgs) e);
                    return;

                case WorkflowEventInternal.DynamicChangeRollback:
                    this.DynamicUpdateRollback(exec, (WorkflowExecutor.DynamicUpdateEventArgs) e);
                    return;

                case WorkflowEventInternal.DynamicChangeCommit:
                    this.DynamicUpdateCommit(exec, (WorkflowExecutor.DynamicUpdateEventArgs) e);
                    break;

                case WorkflowEventInternal.Creating:
                    this.NotifyChannels(TrackingWorkflowEvent.Created, e, exec);
                    return;

                case WorkflowEventInternal.Starting:
                    this.NotifyChannels(TrackingWorkflowEvent.Started, e, exec);
                    return;

                case WorkflowEventInternal.Loading:
                    this.NotifyChannels(TrackingWorkflowEvent.Loaded, e, exec);
                    return;

                default:
                    return;
            }
        }

        internal TrackingListenerBroker Broker
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._broker;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._broker = value;
            }
        }
    }
}

