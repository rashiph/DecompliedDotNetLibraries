namespace System.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(false), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public sealed class CancellationTokenSource : IDisposable
    {
        private static readonly CancellationTokenSource _staticSource_NotCancelable = new CancellationTokenSource(false);
        private static readonly CancellationTokenSource _staticSource_Set = new CancellationTokenSource(true);
        private const int CANNOT_BE_CANCELED = 0;
        private bool m_disposed;
        private volatile CancellationCallbackInfo m_executingCallback;
        private volatile ManualResetEvent m_kernelEvent;
        private List<CancellationTokenRegistration> m_linkingRegistrations;
        private volatile SparselyPopulatedArray<CancellationCallbackInfo>[] m_registeredCallbacksLists;
        private volatile int m_state;
        private volatile int m_threadIDExecutingCallbacks;
        private const int NOT_CANCELED = 1;
        private const int NOTIFYING = 2;
        private const int NOTIFYINGCOMPLETE = 3;
        private static readonly Action<object> s_LinkedTokenCancelDelegate = new Action<object>(CancellationTokenSource.LinkedTokenCancelDelegate);
        private static readonly int s_nLists = ((PlatformHelper.ProcessorCount > 0x18) ? 0x18 : PlatformHelper.ProcessorCount);

        public CancellationTokenSource()
        {
            this.m_threadIDExecutingCallbacks = -1;
            this.m_state = 1;
        }

        private CancellationTokenSource(bool set)
        {
            this.m_threadIDExecutingCallbacks = -1;
            this.m_state = set ? 3 : 0;
        }

        public void Cancel()
        {
            this.Cancel(false);
        }

        public void Cancel(bool throwOnFirstException)
        {
            this.ThrowIfDisposed();
            this.NotifyCancellation(throwOnFirstException);
        }

        private void CancellationCallbackCoreWork_OnSyncContext(object obj)
        {
            CancellationCallbackCoreWorkArguments arguments = (CancellationCallbackCoreWorkArguments) obj;
            CancellationCallbackInfo info = arguments.m_currArrayFragment.SafeAtomicRemove(arguments.m_currArrayIndex, this.m_executingCallback);
            if (info == this.m_executingCallback)
            {
                if (info.TargetExecutionContext != null)
                {
                    info.CancellationTokenSource.ThreadIDExecutingCallbacks = Thread.CurrentThread.ManagedThreadId;
                }
                info.ExecuteCallback();
            }
        }

        public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }
            if (tokens.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("CancellationToken_CreateLinkedToken_TokensIsEmpty"));
            }
            CancellationTokenSource state = new CancellationTokenSource {
                m_linkingRegistrations = new List<CancellationTokenRegistration>()
            };
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].CanBeCanceled)
                {
                    state.m_linkingRegistrations.Add(tokens[i].InternalRegisterWithoutEC(s_LinkedTokenCancelDelegate, state));
                }
            }
            return state;
        }

        public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2)
        {
            CancellationTokenSource state = new CancellationTokenSource();
            if (token1.CanBeCanceled)
            {
                state.m_linkingRegistrations = new List<CancellationTokenRegistration>();
                state.m_linkingRegistrations.Add(token1.InternalRegisterWithoutEC(s_LinkedTokenCancelDelegate, state));
            }
            if (token2.CanBeCanceled)
            {
                if (state.m_linkingRegistrations == null)
                {
                    state.m_linkingRegistrations = new List<CancellationTokenRegistration>();
                }
                state.m_linkingRegistrations.Add(token2.InternalRegisterWithoutEC(s_LinkedTokenCancelDelegate, state));
            }
            return state;
        }

        public void Dispose()
        {
            if (!this.m_disposed)
            {
                if (this.m_linkingRegistrations != null)
                {
                    foreach (CancellationTokenRegistration registration in this.m_linkingRegistrations)
                    {
                        registration.Dispose();
                    }
                    this.m_linkingRegistrations = null;
                }
                this.m_registeredCallbacksLists = null;
                if (this.m_kernelEvent != null)
                {
                    this.m_kernelEvent.Close();
                    this.m_kernelEvent = null;
                }
                this.m_disposed = true;
            }
        }

        private void ExecuteCallbackHandlers(bool throwOnFirstException)
        {
            List<Exception> innerExceptions = null;
            SparselyPopulatedArray<CancellationCallbackInfo>[] registeredCallbacksLists = this.m_registeredCallbacksLists;
            if (registeredCallbacksLists == null)
            {
                Interlocked.Exchange(ref this.m_state, 3);
            }
            else
            {
                try
                {
                    for (int i = 0; i < registeredCallbacksLists.Length; i++)
                    {
                        SparselyPopulatedArray<CancellationCallbackInfo> array = registeredCallbacksLists[i];
                        if (array != null)
                        {
                            for (SparselyPopulatedArrayFragment<CancellationCallbackInfo> fragment = array.Tail; fragment != null; fragment = fragment.Prev)
                            {
                                for (int j = fragment.Length - 1; j >= 0; j--)
                                {
                                    this.m_executingCallback = fragment[j];
                                    if (this.m_executingCallback != null)
                                    {
                                        CancellationCallbackCoreWorkArguments state = new CancellationCallbackCoreWorkArguments(fragment, j);
                                        try
                                        {
                                            if (this.m_executingCallback.TargetSyncContext != null)
                                            {
                                                this.m_executingCallback.TargetSyncContext.Send(new SendOrPostCallback(this.CancellationCallbackCoreWork_OnSyncContext), state);
                                                this.ThreadIDExecutingCallbacks = Thread.CurrentThread.ManagedThreadId;
                                            }
                                            else
                                            {
                                                this.CancellationCallbackCoreWork_OnSyncContext(state);
                                            }
                                        }
                                        catch (Exception exception)
                                        {
                                            if (throwOnFirstException)
                                            {
                                                throw;
                                            }
                                            if (innerExceptions == null)
                                            {
                                                innerExceptions = new List<Exception>();
                                            }
                                            innerExceptions.Add(exception);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    this.m_state = 3;
                    this.m_executingCallback = null;
                    Thread.MemoryBarrier();
                }
                if (innerExceptions != null)
                {
                    throw new AggregateException(innerExceptions);
                }
            }
        }

        internal static CancellationTokenSource InternalGetStaticSource(bool set)
        {
            if (!set)
            {
                return _staticSource_NotCancelable;
            }
            return _staticSource_Set;
        }

        internal CancellationTokenRegistration InternalRegister(Action<object> callback, object stateForCallback, SynchronizationContext targetSyncContext, ExecutionContext executionContext)
        {
            this.ThrowIfDisposed();
            if (!this.IsCancellationRequested)
            {
                int index = Thread.CurrentThread.ManagedThreadId % s_nLists;
                CancellationCallbackInfo element = new CancellationCallbackInfo(callback, stateForCallback, targetSyncContext, executionContext, this);
                if (this.m_registeredCallbacksLists == null)
                {
                    SparselyPopulatedArray<CancellationCallbackInfo>[] arrayArray = new SparselyPopulatedArray<CancellationCallbackInfo>[s_nLists];
                    Interlocked.CompareExchange<SparselyPopulatedArray<CancellationCallbackInfo>[]>(ref this.m_registeredCallbacksLists, arrayArray, null);
                }
                if (this.m_registeredCallbacksLists[index] == null)
                {
                    SparselyPopulatedArray<CancellationCallbackInfo> array = new SparselyPopulatedArray<CancellationCallbackInfo>(4);
                    Interlocked.CompareExchange<SparselyPopulatedArray<CancellationCallbackInfo>>(ref this.m_registeredCallbacksLists[index], array, null);
                }
                SparselyPopulatedArrayAddInfo<CancellationCallbackInfo> registrationInfo = this.m_registeredCallbacksLists[index].Add(element);
                CancellationTokenRegistration registration = new CancellationTokenRegistration(this, element, registrationInfo);
                if (!this.IsCancellationRequested)
                {
                    return registration;
                }
                if (!registration.TryDeregister())
                {
                    this.WaitForCallbackToComplete(element);
                    return new CancellationTokenRegistration();
                }
            }
            callback(stateForCallback);
            return new CancellationTokenRegistration();
        }

        private static void LinkedTokenCancelDelegate(object source)
        {
            (source as CancellationTokenSource).Cancel();
        }

        private void NotifyCancellation(bool throwOnFirstException)
        {
            if (!this.IsCancellationRequested && (Interlocked.CompareExchange(ref this.m_state, 2, 1) == 1))
            {
                this.ThreadIDExecutingCallbacks = Thread.CurrentThread.ManagedThreadId;
                if (this.m_kernelEvent != null)
                {
                    this.m_kernelEvent.Set();
                }
                this.ExecuteCallbackHandlers(throwOnFirstException);
            }
        }

        internal void ThrowIfDisposed()
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("CancellationTokenSource_Disposed"));
            }
        }

        internal void WaitForCallbackToComplete(CancellationCallbackInfo callbackInfo)
        {
            SpinWait wait = new SpinWait();
            while (this.ExecutingCallback == callbackInfo)
            {
                wait.SpinOnce();
            }
        }

        internal bool CanBeCanceled
        {
            get
            {
                return (this.m_state != 0);
            }
        }

        internal CancellationCallbackInfo ExecutingCallback
        {
            get
            {
                return this.m_executingCallback;
            }
        }

        internal bool IsCancellationCompleted
        {
            get
            {
                return (this.m_state == 3);
            }
        }

        public bool IsCancellationRequested
        {
            get
            {
                return (this.m_state >= 2);
            }
        }

        internal bool IsDisposed
        {
            get
            {
                return this.m_disposed;
            }
        }

        internal int ThreadIDExecutingCallbacks
        {
            get
            {
                return this.m_threadIDExecutingCallbacks;
            }
            set
            {
                this.m_threadIDExecutingCallbacks = value;
            }
        }

        public CancellationToken Token
        {
            get
            {
                this.ThrowIfDisposed();
                return new CancellationToken(this);
            }
        }

        internal System.Threading.WaitHandle WaitHandle
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.m_kernelEvent == null)
                {
                    ManualResetEvent event2 = new ManualResetEvent(false);
                    if (Interlocked.CompareExchange<ManualResetEvent>(ref this.m_kernelEvent, event2, null) != null)
                    {
                        event2.Dispose();
                    }
                    if (this.IsCancellationRequested)
                    {
                        this.m_kernelEvent.Set();
                    }
                }
                return this.m_kernelEvent;
            }
        }
    }
}

