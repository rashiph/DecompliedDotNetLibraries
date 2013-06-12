namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal abstract class BaseWebProxyFinder : IWebProxyFinder, IDisposable
    {
        private AutoWebProxyScriptEngine engine;
        private AutoWebProxyState state;

        public BaseWebProxyFinder(AutoWebProxyScriptEngine engine)
        {
            this.engine = engine;
        }

        public abstract void Abort();
        public void Dispose()
        {
            this.Dispose(true);
        }

        protected abstract void Dispose(bool disposing);
        public abstract bool GetProxies(Uri destination, out IList<string> proxyList);
        public virtual void Reset()
        {
            this.State = AutoWebProxyState.Uninitialized;
        }

        protected AutoWebProxyScriptEngine Engine
        {
            get
            {
                return this.engine;
            }
        }

        public bool IsUnrecognizedScheme
        {
            get
            {
                return (this.state == AutoWebProxyState.UnrecognizedScheme);
            }
        }

        public bool IsValid
        {
            get
            {
                if (this.state != AutoWebProxyState.Completed)
                {
                    return (this.state == AutoWebProxyState.Uninitialized);
                }
                return true;
            }
        }

        protected AutoWebProxyState State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }

        protected enum AutoWebProxyState
        {
            Uninitialized,
            DiscoveryFailure,
            DownloadFailure,
            CompilationFailure,
            UnrecognizedScheme,
            Completed
        }
    }
}

