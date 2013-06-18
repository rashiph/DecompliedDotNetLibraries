namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Tracking;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public class ActivityContext
    {
        private ActivityExecutor executor;
        private System.Activities.ActivityInstance instance;
        private bool isDisposed;

        internal ActivityContext()
        {
        }

        internal ActivityContext(System.Activities.ActivityInstance instance, ActivityExecutor executor)
        {
            this.instance = instance;
            this.executor = executor;
            this.Activity = this.instance.Activity;
        }

        internal void Dispose()
        {
            this.isDisposed = true;
            this.instance = null;
            this.executor = null;
            this.Activity = null;
        }

        public T GetExtension<T>() where T: class
        {
            this.ThrowIfDisposed();
            return this.executor.GetExtension<T>();
        }

        public Location<T> GetLocation<T>(LocationReference locationReference)
        {
            this.ThrowIfDisposed();
            if (locationReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("locationReference");
            }
            System.Activities.Location innerLocation = locationReference.GetLocation(this);
            Location<T> location2 = innerLocation as Location<T>;
            if (location2 != null)
            {
                return location2;
            }
            if (locationReference.Type != typeof(T))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.LocationTypeMismatch(locationReference.Name, typeof(T), locationReference.Type)));
            }
            return new TypedLocationWrapper<T>(innerLocation);
        }

        public object GetValue(Argument argument)
        {
            this.ThrowIfDisposed();
            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }
            argument.ThrowIfNotInTree();
            return this.GetValueCore<object>(argument.RuntimeArgument);
        }

        public T GetValue<T>(InArgument<T> argument)
        {
            this.ThrowIfDisposed();
            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }
            argument.ThrowIfNotInTree();
            return this.GetValueCore<T>(argument.RuntimeArgument);
        }

        public T GetValue<T>(LocationReference locationReference)
        {
            this.ThrowIfDisposed();
            if (locationReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("locationReference");
            }
            return this.GetValueCore<T>(locationReference);
        }

        public T GetValue<T>(InOutArgument<T> argument)
        {
            this.ThrowIfDisposed();
            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }
            argument.ThrowIfNotInTree();
            return this.GetValueCore<T>(argument.RuntimeArgument);
        }

        public object GetValue(RuntimeArgument runtimeArgument)
        {
            this.ThrowIfDisposed();
            if (runtimeArgument == null)
            {
                throw FxTrace.Exception.ArgumentNull("runtimeArgument");
            }
            return this.GetValueCore<object>(runtimeArgument);
        }

        public T GetValue<T>(OutArgument<T> argument)
        {
            this.ThrowIfDisposed();
            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }
            argument.ThrowIfNotInTree();
            return this.GetValueCore<T>(argument.RuntimeArgument);
        }

        internal T GetValueCore<T>(LocationReference locationReference)
        {
            System.Activities.Location location = locationReference.GetLocation(this);
            Location<T> location2 = location as Location<T>;
            if (location2 != null)
            {
                return location2.Value;
            }
            return TypeHelper.Convert<T>(location.Value);
        }

        internal void Reinitialize(System.Activities.ActivityInstance instance, ActivityExecutor executor)
        {
            this.isDisposed = false;
            this.instance = instance;
            this.executor = executor;
            this.Activity = this.instance.Activity;
        }

        public void SetValue(Argument argument, object value)
        {
            this.ThrowIfDisposed();
            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }
            argument.ThrowIfNotInTree();
            this.SetValueCore<object>(argument.RuntimeArgument, value);
        }

        public void SetValue<T>(InArgument<T> argument, T value)
        {
            this.ThrowIfDisposed();
            if (argument != null)
            {
                argument.ThrowIfNotInTree();
                this.SetValueCore<T>(argument.RuntimeArgument, value);
            }
        }

        public void SetValue<T>(LocationReference locationReference, T value)
        {
            this.ThrowIfDisposed();
            if (locationReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("locationReference");
            }
            this.SetValueCore<T>(locationReference, value);
        }

        public void SetValue<T>(InOutArgument<T> argument, T value)
        {
            this.ThrowIfDisposed();
            if (argument != null)
            {
                argument.ThrowIfNotInTree();
                this.SetValueCore<T>(argument.RuntimeArgument, value);
            }
        }

        public void SetValue<T>(OutArgument<T> argument, T value)
        {
            this.ThrowIfDisposed();
            if (argument != null)
            {
                argument.ThrowIfNotInTree();
                this.SetValueCore<T>(argument.RuntimeArgument, value);
            }
        }

        internal void SetValueCore<T>(LocationReference locationReference, T value)
        {
            System.Activities.Location location = locationReference.GetLocation(this);
            Location<T> location2 = location as Location<T>;
            if (location2 != null)
            {
                location2.Value = value;
            }
            else
            {
                if (!TypeHelper.AreTypesCompatible(value, locationReference.Type))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotSetValueToLocation((value != null) ? value.GetType() : typeof(T), locationReference.Name, locationReference.Type)));
                }
                location.Value = value;
            }
        }

        internal void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(base.GetType().FullName, System.Activities.SR.AECDisposed));
            }
        }

        internal void TrackCore(CustomTrackingRecord record)
        {
            if (this.executor.ShouldTrack)
            {
                record.Activity = new ActivityInfo(this.instance);
                record.InstanceId = this.WorkflowInstanceId;
                this.executor.AddTrackingRecord(record);
            }
        }

        internal System.Activities.Activity Activity { get; set; }

        public string ActivityInstanceId
        {
            get
            {
                this.ThrowIfDisposed();
                return this.instance.Id;
            }
        }

        internal bool AllowChainedEnvironmentAccess { get; set; }

        internal System.Activities.ActivityInstance CurrentInstance
        {
            get
            {
                return this.instance;
            }
        }

        public WorkflowDataContext DataContext
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.instance.DataContext == null)
                {
                    this.instance.DataContext = new WorkflowDataContext(this.executor, this.instance);
                }
                return this.instance.DataContext;
            }
        }

        internal LocationEnvironment Environment
        {
            get
            {
                this.ThrowIfDisposed();
                return this.instance.Environment;
            }
        }

        internal bool IsDisposed
        {
            get
            {
                return this.isDisposed;
            }
        }

        public Guid WorkflowInstanceId
        {
            get
            {
                this.ThrowIfDisposed();
                return this.executor.WorkflowInstanceId;
            }
        }
    }
}

