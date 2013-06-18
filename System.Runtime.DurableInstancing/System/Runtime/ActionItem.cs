namespace System.Runtime
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Threading;

    internal abstract class ActionItem
    {
        [SecurityCritical]
        private SecurityContext context;
        private bool isScheduled;
        private bool lowPriority;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ActionItem()
        {
        }

        [SecurityCritical]
        private SecurityContext ExtractContext()
        {
            SecurityContext context = this.context;
            this.context = null;
            return context;
        }

        [SecurityCritical]
        protected abstract void Invoke();
        [SecurityCritical]
        protected void Schedule()
        {
            if (this.isScheduled)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ActionItemIsAlreadyScheduled));
            }
            this.isScheduled = true;
            if (PartialTrustHelpers.ShouldFlowSecurityContext)
            {
                this.context = PartialTrustHelpers.CaptureSecurityContextNoIdentityFlow();
            }
            if (this.context != null)
            {
                this.ScheduleCallback(CallbackHelper.InvokeWithContextCallback);
            }
            else
            {
                this.ScheduleCallback(CallbackHelper.InvokeWithoutContextCallback);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void Schedule(Action<object> callback, object state)
        {
            Schedule(callback, state, false);
        }

        [SecuritySafeCritical]
        public static void Schedule(Action<object> callback, object state, bool lowPriority)
        {
            if (PartialTrustHelpers.ShouldFlowSecurityContext || WaitCallbackActionItem.ShouldUseActivity)
            {
                new DefaultActionItem(callback, state, lowPriority).Schedule();
            }
            else
            {
                ScheduleCallback(callback, state, lowPriority);
            }
        }

        [SecurityCritical]
        private void ScheduleCallback(Action<object> callback)
        {
            ScheduleCallback(callback, this, this.lowPriority);
        }

        [SecurityCritical]
        private static void ScheduleCallback(Action<object> callback, object state, bool lowPriority)
        {
            if (lowPriority)
            {
                IOThreadScheduler.ScheduleCallbackLowPriNoFlow(callback, state);
            }
            else
            {
                IOThreadScheduler.ScheduleCallbackNoFlow(callback, state);
            }
        }

        [SecurityCritical]
        protected void ScheduleWithContext(SecurityContext context)
        {
            if (context == null)
            {
                throw Fx.Exception.ArgumentNull("context");
            }
            if (this.isScheduled)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ActionItemIsAlreadyScheduled));
            }
            this.isScheduled = true;
            this.context = context.CreateCopy();
            this.ScheduleCallback(CallbackHelper.InvokeWithContextCallback);
        }

        [SecurityCritical]
        protected void ScheduleWithoutContext()
        {
            if (this.isScheduled)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ActionItemIsAlreadyScheduled));
            }
            this.isScheduled = true;
            this.ScheduleCallback(CallbackHelper.InvokeWithoutContextCallback);
        }

        public bool LowPriority
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.lowPriority;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            protected set
            {
                this.lowPriority = value;
            }
        }

        [SecurityCritical]
        private static class CallbackHelper
        {
            private static Action<object> invokeWithContextCallback;
            private static Action<object> invokeWithoutContextCallback;
            private static ContextCallback onContextAppliedCallback;

            private static void InvokeWithContext(object state)
            {
                SecurityContext.Run(((ActionItem) state).ExtractContext(), OnContextAppliedCallback, state);
            }

            private static void InvokeWithoutContext(object state)
            {
                ((ActionItem) state).Invoke();
                ((ActionItem) state).isScheduled = false;
            }

            private static void OnContextApplied(object o)
            {
                ((ActionItem) o).Invoke();
                ((ActionItem) o).isScheduled = false;
            }

            public static Action<object> InvokeWithContextCallback
            {
                get
                {
                    if (invokeWithContextCallback == null)
                    {
                        invokeWithContextCallback = new Action<object>(ActionItem.CallbackHelper.InvokeWithContext);
                    }
                    return invokeWithContextCallback;
                }
            }

            public static Action<object> InvokeWithoutContextCallback
            {
                get
                {
                    if (invokeWithoutContextCallback == null)
                    {
                        invokeWithoutContextCallback = new Action<object>(ActionItem.CallbackHelper.InvokeWithoutContext);
                    }
                    return invokeWithoutContextCallback;
                }
            }

            public static ContextCallback OnContextAppliedCallback
            {
                get
                {
                    if (onContextAppliedCallback == null)
                    {
                        onContextAppliedCallback = new ContextCallback(ActionItem.CallbackHelper.OnContextApplied);
                    }
                    return onContextAppliedCallback;
                }
            }
        }

        private class DefaultActionItem : ActionItem
        {
            private Guid activityId;
            [SecurityCritical]
            private Action<object> callback;
            private bool flowActivityId;
            [SecurityCritical]
            private object state;

            [SecuritySafeCritical]
            public DefaultActionItem(Action<object> callback, object state, bool isLowPriority)
            {
                base.LowPriority = isLowPriority;
                this.callback = callback;
                this.state = state;
                if (WaitCallbackActionItem.ShouldUseActivity)
                {
                    this.flowActivityId = true;
                    this.activityId = DiagnosticTrace.ActivityId;
                }
            }

            [SecurityCritical]
            protected override void Invoke()
            {
                if (this.flowActivityId)
                {
                    Guid activityId = DiagnosticTrace.ActivityId;
                    try
                    {
                        DiagnosticTrace.ActivityId = this.activityId;
                        this.callback(this.state);
                    }
                    finally
                    {
                        DiagnosticTrace.ActivityId = activityId;
                    }
                }
                else
                {
                    this.callback(this.state);
                }
            }
        }
    }
}

