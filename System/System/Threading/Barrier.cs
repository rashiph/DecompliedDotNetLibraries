namespace System.Threading
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DebuggerDisplay("Participant Count={ParticipantCount},Participants Remaining={ParticipantsRemaining}"), ComVisible(false), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class Barrier : IDisposable
    {
        private const int CURRENT_MASK = 0x7fff0000;
        private int m_actionCallerID;
        private long m_currentPhase;
        private volatile int m_currentTotalCount;
        private bool m_disposed;
        private ManualResetEventSlim m_evenEvent;
        private Exception m_exception;
        private ManualResetEventSlim m_oddEvent;
        private ExecutionContext m_ownerThreadContext;
        private Action<Barrier> m_postPhaseAction;
        private const int MAX_PARTICIPANTS = 0x7fff;
        private const int SENSE_MASK = -2147483648;
        private const int TOTAL_MASK = 0x7fff;

        public Barrier(int participantCount) : this(participantCount, null)
        {
        }

        public Barrier(int participantCount, Action<Barrier> postPhaseAction)
        {
            if ((participantCount < 0) || (participantCount > 0x7fff))
            {
                throw new ArgumentOutOfRangeException("participantCount", participantCount, SR.GetString("Barrier_ctor_ArgumentOutOfRange"));
            }
            this.m_currentTotalCount = participantCount;
            this.m_postPhaseAction = postPhaseAction;
            this.m_oddEvent = new ManualResetEventSlim(true);
            this.m_evenEvent = new ManualResetEventSlim(false);
            if ((postPhaseAction != null) && !ExecutionContext.IsFlowSuppressed())
            {
                this.m_ownerThreadContext = ExecutionContext.Capture();
            }
            this.m_actionCallerID = 0;
        }

        public long AddParticipant()
        {
            long num;
            try
            {
                num = this.AddParticipants(1);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new InvalidOperationException(SR.GetString("Barrier_AddParticipants_Overflow_ArgumentOutOfRange"));
            }
            return num;
        }

        public long AddParticipants(int participantCount)
        {
            this.ThrowIfDisposed();
            if (participantCount < 1)
            {
                throw new ArgumentOutOfRangeException("participantCount", participantCount, SR.GetString("Barrier_AddParticipants_NonPositive_ArgumentOutOfRange"));
            }
            if (participantCount > 0x7fff)
            {
                throw new ArgumentOutOfRangeException("participantCount", SR.GetString("Barrier_AddParticipants_Overflow_ArgumentOutOfRange"));
            }
            if ((this.m_actionCallerID != 0) && (Thread.CurrentThread.ManagedThreadId == this.m_actionCallerID))
            {
                throw new InvalidOperationException(SR.GetString("Barrier_InvalidOperation_CalledFromPHA"));
            }
            SpinWait wait = new SpinWait();
            long num = 0L;
            while (true)
            {
                int num3;
                int num4;
                bool flag;
                int currentTotalCount = this.m_currentTotalCount;
                this.GetCurrentTotal(currentTotalCount, out num4, out num3, out flag);
                if ((participantCount + num3) > 0x7fff)
                {
                    throw new ArgumentOutOfRangeException("participantCount", SR.GetString("Barrier_AddParticipants_Overflow_ArgumentOutOfRange"));
                }
                if (this.SetCurrentTotal(currentTotalCount, num4, num3 + participantCount, flag))
                {
                    long currentPhase = this.m_currentPhase;
                    num = (flag != ((currentPhase % 2L) == 0L)) ? (currentPhase + 1L) : currentPhase;
                    if (num != currentPhase)
                    {
                        if (flag)
                        {
                            this.m_oddEvent.Wait();
                            return num;
                        }
                        this.m_evenEvent.Wait();
                        return num;
                    }
                    if (flag && this.m_evenEvent.IsSet)
                    {
                        this.m_evenEvent.Reset();
                        return num;
                    }
                    if (!flag && this.m_oddEvent.IsSet)
                    {
                        this.m_oddEvent.Reset();
                    }
                    return num;
                }
                wait.SpinOnce();
            }
        }

        public void Dispose()
        {
            if ((this.m_actionCallerID != 0) && (Thread.CurrentThread.ManagedThreadId == this.m_actionCallerID))
            {
                throw new InvalidOperationException(SR.GetString("Barrier_InvalidOperation_CalledFromPHA"));
            }
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.m_disposed)
            {
                if (disposing)
                {
                    this.m_oddEvent.Dispose();
                    this.m_evenEvent.Dispose();
                }
                this.m_disposed = true;
            }
        }

        private void FinishPhase(bool observedSense)
        {
            ContextCallback callback = null;
            if (this.m_postPhaseAction != null)
            {
                try
                {
                    try
                    {
                        this.m_actionCallerID = Thread.CurrentThread.ManagedThreadId;
                        if (this.m_ownerThreadContext != null)
                        {
                            if (callback == null)
                            {
                                callback = i => this.m_postPhaseAction(this);
                            }
                            ExecutionContext.Run(this.m_ownerThreadContext.CreateCopy(), callback, null);
                        }
                        else
                        {
                            this.m_postPhaseAction(this);
                        }
                        this.m_exception = null;
                    }
                    catch (Exception exception)
                    {
                        this.m_exception = exception;
                    }
                    return;
                }
                finally
                {
                    this.m_actionCallerID = 0;
                    this.SetResetEvents(observedSense);
                    if (this.m_exception != null)
                    {
                        throw new BarrierPostPhaseException(this.m_exception);
                    }
                }
            }
            this.SetResetEvents(observedSense);
        }

        private void GetCurrentTotal(int currentTotal, out int current, out int total, out bool sense)
        {
            total = currentTotal & 0x7fff;
            current = (currentTotal & 0x7fff0000) >> 0x10;
            sense = (currentTotal & -2147483648) == 0;
        }

        public void RemoveParticipant()
        {
            this.RemoveParticipants(1);
        }

        public void RemoveParticipants(int participantCount)
        {
            this.ThrowIfDisposed();
            if (participantCount < 1)
            {
                throw new ArgumentOutOfRangeException("participantCount", participantCount, SR.GetString("Barrier_RemoveParticipants_NonPositive_ArgumentOutOfRange"));
            }
            if ((this.m_actionCallerID != 0) && (Thread.CurrentThread.ManagedThreadId == this.m_actionCallerID))
            {
                throw new InvalidOperationException(SR.GetString("Barrier_InvalidOperation_CalledFromPHA"));
            }
            SpinWait wait = new SpinWait();
            while (true)
            {
                int num2;
                int num3;
                bool flag;
                int currentTotalCount = this.m_currentTotalCount;
                this.GetCurrentTotal(currentTotalCount, out num3, out num2, out flag);
                if (num2 < participantCount)
                {
                    throw new ArgumentOutOfRangeException("participantCount", SR.GetString("Barrier_RemoveParticipants_ArgumentOutOfRange"));
                }
                if ((num2 - participantCount) < num3)
                {
                    throw new InvalidOperationException(SR.GetString("Barrier_RemoveParticipants_InvalidOperation"));
                }
                int num4 = num2 - participantCount;
                if ((num4 > 0) && (num3 == num4))
                {
                    if (this.SetCurrentTotal(currentTotalCount, 0, num2 - participantCount, !flag))
                    {
                        this.FinishPhase(flag);
                        return;
                    }
                }
                else if (this.SetCurrentTotal(currentTotalCount, num3, num2 - participantCount, flag))
                {
                    return;
                }
                wait.SpinOnce();
            }
        }

        private bool SetCurrentTotal(int currentTotal, int current, int total, bool sense)
        {
            int num = (current << 0x10) | total;
            if (!sense)
            {
                num |= -2147483648;
            }
            return (Interlocked.CompareExchange(ref this.m_currentTotalCount, num, currentTotal) == currentTotal);
        }

        private void SetResetEvents(bool observedSense)
        {
            this.m_currentPhase += 1L;
            if (observedSense)
            {
                this.m_oddEvent.Reset();
                this.m_evenEvent.Set();
            }
            else
            {
                this.m_evenEvent.Reset();
                this.m_oddEvent.Set();
            }
        }

        public void SignalAndWait()
        {
            this.SignalAndWait(new CancellationToken());
        }

        public bool SignalAndWait(int millisecondsTimeout)
        {
            return this.SignalAndWait(millisecondsTimeout, new CancellationToken());
        }

        public void SignalAndWait(CancellationToken cancellationToken)
        {
            this.SignalAndWait(-1, cancellationToken);
        }

        public bool SignalAndWait(TimeSpan timeout)
        {
            return this.SignalAndWait(timeout, new CancellationToken());
        }

        public bool SignalAndWait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            bool flag;
            int num;
            int num2;
            int currentTotalCount;
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, SR.GetString("Barrier_SignalAndWait_ArgumentOutOfRange"));
            }
            if ((this.m_actionCallerID != 0) && (Thread.CurrentThread.ManagedThreadId == this.m_actionCallerID))
            {
                throw new InvalidOperationException(SR.GetString("Barrier_InvalidOperation_CalledFromPHA"));
            }
            SpinWait wait = new SpinWait();
            while (true)
            {
                currentTotalCount = this.m_currentTotalCount;
                this.GetCurrentTotal(currentTotalCount, out num2, out num, out flag);
                if (num == 0)
                {
                    throw new InvalidOperationException(SR.GetString("Barrier_SignalAndWait_InvalidOperation_ZeroTotal"));
                }
                if ((num2 == 0) && (flag != ((this.m_currentPhase % 2L) == 0L)))
                {
                    throw new InvalidOperationException(SR.GetString("Barrier_SignalAndWait_InvalidOperation_ThreadsExceeded"));
                }
                if ((num2 + 1) == num)
                {
                    if (this.SetCurrentTotal(currentTotalCount, 0, num, !flag))
                    {
                        if (CdsSyncEtwBCLProvider.Log.IsEnabled())
                        {
                            CdsSyncEtwBCLProvider.Log.Barrier_PhaseFinished(flag, this.m_currentPhase);
                        }
                        this.FinishPhase(flag);
                        return true;
                    }
                }
                else if (this.SetCurrentTotal(currentTotalCount, num2 + 1, num, flag))
                {
                    break;
                }
                wait.SpinOnce();
            }
            long currentPhase = this.m_currentPhase;
            ManualResetEventSlim slim = flag ? this.m_evenEvent : this.m_oddEvent;
            bool flag2 = false;
            bool flag3 = false;
            try
            {
                flag3 = slim.Wait(millisecondsTimeout, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                flag2 = true;
            }
            if (!flag3)
            {
                wait.Reset();
                while (true)
                {
                    bool flag4;
                    currentTotalCount = this.m_currentTotalCount;
                    this.GetCurrentTotal(currentTotalCount, out num2, out num, out flag4);
                    if ((currentPhase != this.m_currentPhase) || (flag != flag4))
                    {
                        slim.Wait();
                        break;
                    }
                    if (this.SetCurrentTotal(currentTotalCount, num2 - 1, num, flag))
                    {
                        if (flag2)
                        {
                            throw new OperationCanceledException(SR.GetString("Common_OperationCanceled"), cancellationToken);
                        }
                        return false;
                    }
                    wait.SpinOnce();
                }
            }
            if (this.m_exception != null)
            {
                throw new BarrierPostPhaseException(this.m_exception);
            }
            return true;
        }

        public bool SignalAndWait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout", timeout, SR.GetString("Barrier_SignalAndWait_ArgumentOutOfRange"));
            }
            return this.SignalAndWait((int) timeout.TotalMilliseconds, cancellationToken);
        }

        private void ThrowIfDisposed()
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException("Barrier", SR.GetString("Barrier_Dispose"));
            }
        }

        public long CurrentPhaseNumber
        {
            get
            {
                return this.m_currentPhase;
            }
        }

        public int ParticipantCount
        {
            get
            {
                return (this.m_currentTotalCount & 0x7fff);
            }
        }

        public int ParticipantsRemaining
        {
            get
            {
                int currentTotalCount = this.m_currentTotalCount;
                int num2 = currentTotalCount & 0x7fff;
                int num3 = (currentTotalCount & 0x7fff0000) >> 0x10;
                return (num2 - num3);
            }
        }
    }
}

