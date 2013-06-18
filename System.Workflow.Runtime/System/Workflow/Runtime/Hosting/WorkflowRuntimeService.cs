namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.Runtime;

    public abstract class WorkflowRuntimeService
    {
        private WorkflowRuntime _runtime;
        private WorkflowRuntimeServiceState state;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowRuntimeService()
        {
        }

        private void HandleStarted(object source, WorkflowRuntimeEventArgs e)
        {
            this.state = WorkflowRuntimeServiceState.Started;
            this.OnStarted();
        }

        private void HandleStopped(object source, WorkflowRuntimeEventArgs e)
        {
            this.state = WorkflowRuntimeServiceState.Stopped;
            this.OnStopped();
        }

        protected virtual void OnStarted()
        {
        }

        protected virtual void OnStopped()
        {
        }

        internal void RaiseExceptionNotHandledEvent(Exception exception, Guid instanceId)
        {
            this.Runtime.RaiseServicesExceptionNotHandledEvent(exception, instanceId);
        }

        protected void RaiseServicesExceptionNotHandledEvent(Exception exception, Guid instanceId)
        {
            this.Runtime.RaiseServicesExceptionNotHandledEvent(exception, instanceId);
        }

        internal void SetRuntime(WorkflowRuntime runtime)
        {
            if ((runtime == null) && (this._runtime != null))
            {
                this._runtime.Started -= new EventHandler<WorkflowRuntimeEventArgs>(this.HandleStarted);
                this._runtime.Stopped -= new EventHandler<WorkflowRuntimeEventArgs>(this.HandleStopped);
            }
            this._runtime = runtime;
            if (runtime != null)
            {
                this._runtime.Started += new EventHandler<WorkflowRuntimeEventArgs>(this.HandleStarted);
                this._runtime.Stopped += new EventHandler<WorkflowRuntimeEventArgs>(this.HandleStopped);
            }
        }

        protected internal virtual void Start()
        {
            if (this._runtime == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.ServiceNotAddedToRuntime, new object[] { base.GetType().Name }));
            }
            if (this.state.Equals(WorkflowRuntimeServiceState.Started))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.ServiceAlreadyStarted, new object[] { base.GetType().Name }));
            }
            this.state = WorkflowRuntimeServiceState.Starting;
        }

        protected internal virtual void Stop()
        {
            if (this._runtime == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.ServiceNotAddedToRuntime, new object[] { base.GetType().Name }));
            }
            if (this.state.Equals(WorkflowRuntimeServiceState.Stopped))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.ServiceNotStarted, new object[] { base.GetType().Name }));
            }
            this.state = WorkflowRuntimeServiceState.Stopping;
        }

        protected WorkflowRuntime Runtime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._runtime;
            }
        }

        protected WorkflowRuntimeServiceState State
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.state;
            }
        }
    }
}

