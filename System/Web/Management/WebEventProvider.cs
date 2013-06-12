namespace System.Web.Management
{
    using System;
    using System.Configuration.Provider;
    using System.Threading;
    using System.Web;

    public abstract class WebEventProvider : ProviderBase
    {
        private int _exceptionLogged;

        protected WebEventProvider()
        {
        }

        public abstract void Flush();
        internal void LogException(Exception e)
        {
            if (Interlocked.CompareExchange(ref this._exceptionLogged, 1, 0) == 0)
            {
                UnsafeNativeMethods.LogWebeventProviderFailure(HttpRuntime.AppDomainAppVirtualPath, this.Name, e.ToString());
            }
        }

        public abstract void ProcessEvent(WebBaseEvent raisedEvent);
        public abstract void Shutdown();
    }
}

