namespace System.Net
{
    using System;
    using System.Threading;

    internal class CallbackClosure
    {
        private System.AsyncCallback savedCallback;
        private ExecutionContext savedContext;

        internal CallbackClosure(ExecutionContext context, System.AsyncCallback callback)
        {
            if (callback != null)
            {
                this.savedCallback = callback;
                this.savedContext = context;
            }
        }

        internal bool IsCompatible(System.AsyncCallback callback)
        {
            if ((callback == null) || (this.savedCallback == null))
            {
                return false;
            }
            if (!object.Equals(this.savedCallback, callback))
            {
                return false;
            }
            return true;
        }

        internal System.AsyncCallback AsyncCallback
        {
            get
            {
                return this.savedCallback;
            }
        }

        internal ExecutionContext Context
        {
            get
            {
                return this.savedContext;
            }
        }
    }
}

