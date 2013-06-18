namespace System.Xaml
{
    using System;
    using System.Threading;

    internal class GCNotificationToken
    {
        private WaitCallback callback;
        private object state;

        private GCNotificationToken(WaitCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
        }

        ~GCNotificationToken()
        {
            ThreadPool.QueueUserWorkItem(this.callback, this.state);
        }

        internal static void RegisterCallback(WaitCallback callback, object state)
        {
            new GCNotificationToken(callback, state);
        }
    }
}

