namespace System.Threading
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [StructLayout(LayoutKind.Sequential), ComVisible(false), DebuggerDisplay("IsCancellationRequested = {IsCancellationRequested}"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public struct CancellationToken
    {
        private CancellationTokenSource m_source;
        private static Action<object> s_ActionToActionObjShunt;
        public static CancellationToken None
        {
            get
            {
                return new CancellationToken();
            }
        }
        public bool IsCancellationRequested
        {
            get
            {
                return ((this.m_source != null) && this.m_source.IsCancellationRequested);
            }
        }
        public bool CanBeCanceled
        {
            get
            {
                return ((this.m_source != null) && this.m_source.CanBeCanceled);
            }
        }
        public System.Threading.WaitHandle WaitHandle
        {
            get
            {
                if (this.m_source == null)
                {
                    this.InitializeDefaultSource();
                }
                return this.m_source.WaitHandle;
            }
        }
        internal CancellationToken(CancellationTokenSource source)
        {
            this.m_source = source;
        }

        public CancellationToken(bool canceled)
        {
            this = new CancellationToken();
            if (canceled)
            {
                this.m_source = CancellationTokenSource.InternalGetStaticSource(canceled);
            }
        }

        private static void ActionToActionObjShunt(object obj)
        {
            Action action = obj as Action;
            action();
        }

        public CancellationTokenRegistration Register(Action callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return this.Register(s_ActionToActionObjShunt, callback, false, true);
        }

        public CancellationTokenRegistration Register(Action callback, bool useSynchronizationContext)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return this.Register(s_ActionToActionObjShunt, callback, useSynchronizationContext, true);
        }

        public CancellationTokenRegistration Register(Action<object> callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return this.Register(callback, state, false, true);
        }

        public CancellationTokenRegistration Register(Action<object> callback, object state, bool useSynchronizationContext)
        {
            return this.Register(callback, state, useSynchronizationContext, true);
        }

        internal CancellationTokenRegistration InternalRegisterWithoutEC(Action<object> callback, object state)
        {
            return this.Register(callback, state, false, false);
        }

        private CancellationTokenRegistration Register(Action<object> callback, object state, bool useSynchronizationContext, bool useExecutionContext)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            if (!this.CanBeCanceled)
            {
                return new CancellationTokenRegistration();
            }
            SynchronizationContext targetSyncContext = null;
            if (!this.IsCancellationRequested && useSynchronizationContext)
            {
                targetSyncContext = SynchronizationContext.Current;
            }
            ExecutionContext executionContext = null;
            if (!this.IsCancellationRequested && useExecutionContext)
            {
                executionContext = ExecutionContext.Capture();
            }
            return this.m_source.InternalRegister(callback, state, targetSyncContext, executionContext);
        }

        public bool Equals(CancellationToken other)
        {
            if ((this.m_source == null) && (other.m_source == null))
            {
                return true;
            }
            if (this.m_source == null)
            {
                return (other.m_source == CancellationTokenSource.InternalGetStaticSource(false));
            }
            if (other.m_source == null)
            {
                return (this.m_source == CancellationTokenSource.InternalGetStaticSource(false));
            }
            return (this.m_source == other.m_source);
        }

        public override bool Equals(object other)
        {
            return ((other is CancellationToken) && this.Equals((CancellationToken) other));
        }

        public override int GetHashCode()
        {
            if (this.m_source == null)
            {
                return CancellationTokenSource.InternalGetStaticSource(false).GetHashCode();
            }
            return this.m_source.GetHashCode();
        }

        public static bool operator ==(CancellationToken left, CancellationToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CancellationToken left, CancellationToken right)
        {
            return !left.Equals(right);
        }

        public void ThrowIfCancellationRequested()
        {
            if (this.IsCancellationRequested)
            {
                throw new OperationCanceledException(Environment.GetResourceString("OperationCanceled"), this);
            }
        }

        internal void ThrowIfSourceDisposed()
        {
            if ((this.m_source != null) && this.m_source.IsDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("CancellationToken_SourceDisposed"));
            }
        }

        private void InitializeDefaultSource()
        {
            this.m_source = CancellationTokenSource.InternalGetStaticSource(false);
        }

        static CancellationToken()
        {
            s_ActionToActionObjShunt = new Action<object>(CancellationToken.ActionToActionObjShunt);
        }
    }
}

