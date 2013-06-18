namespace <CrtImplementationDetails>
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Threading;

    internal class ModuleUninitializer : Stack
    {
        internal static ModuleUninitializer _ModuleUninitializer = new ModuleUninitializer();
        private static object @lock = new object();

        [SecuritySafeCritical]
        private ModuleUninitializer()
        {
            EventHandler handler = new EventHandler(this.SingletonDomainUnload);
            AppDomain.CurrentDomain.DomainUnload += handler;
            AppDomain.CurrentDomain.ProcessExit += handler;
        }

        [SecuritySafeCritical]
        internal void AddHandler(EventHandler handler)
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                Monitor.Enter(@lock, ref lockTaken);
                RuntimeHelpers.PrepareDelegate(handler);
                this.Push(handler);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(@lock);
                }
            }
        }

        [PrePrepareMethod, SecurityCritical]
        private void SingletonDomainUnload(object source, EventArgs arguments)
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                Monitor.Enter(@lock, ref lockTaken);
                IEnumerator enumerator = this.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        ((EventHandler) enumerator.Current)(source, arguments);
                    }
                }
                finally
                {
                    IEnumerator enumerator2 = enumerator;
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(@lock);
                }
            }
        }
    }
}

