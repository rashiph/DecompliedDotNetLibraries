namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [StructLayout(LayoutKind.Sequential), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public struct CancellationTokenRegistration : IEquatable<CancellationTokenRegistration>, IDisposable
    {
        private readonly CancellationTokenSource m_tokenSource;
        private readonly CancellationCallbackInfo m_callbackInfo;
        private readonly SparselyPopulatedArrayAddInfo<CancellationCallbackInfo> m_registrationInfo;
        internal CancellationTokenRegistration(CancellationTokenSource tokenSource, CancellationCallbackInfo callbackInfo, SparselyPopulatedArrayAddInfo<CancellationCallbackInfo> registrationInfo)
        {
            this.m_tokenSource = tokenSource;
            this.m_callbackInfo = callbackInfo;
            this.m_registrationInfo = registrationInfo;
        }

        internal bool TryDeregister()
        {
            if (this.m_registrationInfo.Source == null)
            {
                return false;
            }
            if (this.m_registrationInfo.Source.SafeAtomicRemove(this.m_registrationInfo.Index, this.m_callbackInfo) != this.m_callbackInfo)
            {
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            if (this.m_tokenSource != null)
            {
                this.m_tokenSource.ThrowIfDisposed();
            }
            bool flag = this.TryDeregister();
            if ((((this.m_tokenSource != null) && this.m_tokenSource.IsCancellationRequested) && (!this.m_tokenSource.IsCancellationCompleted && !flag)) && (this.m_tokenSource.ThreadIDExecutingCallbacks != Thread.CurrentThread.ManagedThreadId))
            {
                this.m_tokenSource.WaitForCallbackToComplete(this.m_callbackInfo);
            }
        }

        public static bool operator ==(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return ((obj is CancellationTokenRegistration) && this.Equals((CancellationTokenRegistration) obj));
        }

        public bool Equals(CancellationTokenRegistration other)
        {
            return ((((this.m_tokenSource == other.m_tokenSource) && (this.m_callbackInfo == other.m_callbackInfo)) && (this.m_registrationInfo.Source == other.m_registrationInfo.Source)) && (this.m_registrationInfo.Index == other.m_registrationInfo.Index));
        }

        public override int GetHashCode()
        {
            if (this.m_registrationInfo.Source != null)
            {
                return (this.m_registrationInfo.Source.GetHashCode() ^ this.m_registrationInfo.Index.GetHashCode());
            }
            return this.m_registrationInfo.Index.GetHashCode();
        }
    }
}

