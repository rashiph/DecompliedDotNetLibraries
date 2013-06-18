namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Tracking;

    public class CodeActivityContext : ActivityContext
    {
        internal CodeActivityContext()
        {
        }

        internal CodeActivityContext(System.Activities.ActivityInstance instance, ActivityExecutor executor) : base(instance, executor)
        {
        }

        public THandle GetProperty<THandle>() where THandle: Handle
        {
            base.ThrowIfDisposed();
            if (base.CurrentInstance.PropertyManager != null)
            {
                return (THandle) base.CurrentInstance.PropertyManager.GetProperty(Handle.GetPropertyName(typeof(THandle)), base.Activity.MemberOf);
            }
            return default(THandle);
        }

        internal void Initialize(System.Activities.ActivityInstance instance, ActivityExecutor executor)
        {
            base.Reinitialize(instance, executor);
        }

        public void Track(CustomTrackingRecord record)
        {
            base.ThrowIfDisposed();
            if (record == null)
            {
                throw FxTrace.Exception.ArgumentNull("record");
            }
            base.TrackCore(record);
        }
    }
}

