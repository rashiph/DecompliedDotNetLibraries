namespace System.Web.Services.Protocols
{
    using System;
    using System.Threading;

    internal class UserToken
    {
        private SendOrPostCallback callback;
        private object userState;

        internal UserToken(SendOrPostCallback callback, object userState)
        {
            this.callback = callback;
            this.userState = userState;
        }

        internal SendOrPostCallback Callback
        {
            get
            {
                return this.callback;
            }
        }

        internal object UserState
        {
            get
            {
                return this.userState;
            }
        }
    }
}

