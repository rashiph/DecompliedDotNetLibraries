namespace System.Activities
{
    using System;
    using System.Runtime;
    using System.Threading;

    internal static class SynchronizationContextHelper
    {
        private static WFDefaultSynchronizationContext defaultContext;

        public static SynchronizationContext CloneSynchronizationContext(SynchronizationContext context)
        {
            if (context is WFDefaultSynchronizationContext)
            {
                return defaultContext;
            }
            return context.CreateCopy();
        }

        public static SynchronizationContext GetDefaultSynchronizationContext()
        {
            if (defaultContext == null)
            {
                defaultContext = new WFDefaultSynchronizationContext();
            }
            return defaultContext;
        }

        private class WFDefaultSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object state)
            {
                new SendOrPostCallbackActionItem(d, state).PostWithNoContext();
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                d(state);
            }

            private class SendOrPostCallbackActionItem : ActionItem
            {
                private SendOrPostCallback callback;
                private object state;

                public SendOrPostCallbackActionItem(SendOrPostCallback callback, object state)
                {
                    this.callback = callback;
                    this.state = state;
                }

                protected override void Invoke()
                {
                    this.callback(this.state);
                }

                public void PostWithNoContext()
                {
                    base.ScheduleWithoutContext();
                }
            }
        }
    }
}

