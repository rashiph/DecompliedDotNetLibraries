namespace System.Web.Management
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;

    internal sealed class WebEventBuffer
    {
        private Queue _buffer;
        private long _burstWaitTimeMs = 0x7d0L;
        private int _discardedSinceLastFlush;
        private WebEventBufferFlushCallback _flushCallback;
        private DateTime _lastAdd = DateTime.MinValue;
        private DateTime _lastFlushTime = DateTime.MinValue;
        private DateTime _lastScheduledFlushTime = DateTime.MinValue;
        private int _maxBufferSize;
        private int _maxBufferThreads;
        private int _maxFlushSize;
        private int _notificationSequence;
        private BufferedWebEventProvider _provider;
        private long _regularFlushIntervalMs;
        private bool _regularTimeoutUsed;
        private DateTime _startTime = DateTime.MinValue;
        private int _threadsInFlush;
        private Timer _timer;
        private long _urgentFlushIntervalMs;
        private bool _urgentFlushScheduled;
        private int _urgentFlushThreshold;
        private static long Infinite = 0x7fffffffffffffffL;

        internal WebEventBuffer(BufferedWebEventProvider provider, string bufferMode, WebEventBufferFlushCallback callback)
        {
            this._provider = provider;
            BufferModeSettings settings = RuntimeConfig.GetAppLKGConfig().HealthMonitoring.BufferModes[bufferMode];
            if (settings == null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Health_mon_buffer_mode_not_found", new object[] { bufferMode }));
            }
            if (settings.RegularFlushInterval == TimeSpan.MaxValue)
            {
                this._regularFlushIntervalMs = Infinite;
            }
            else
            {
                try
                {
                    this._regularFlushIntervalMs = (long) settings.RegularFlushInterval.TotalMilliseconds;
                }
                catch (OverflowException)
                {
                    this._regularFlushIntervalMs = Infinite;
                }
            }
            if (settings.UrgentFlushInterval == TimeSpan.MaxValue)
            {
                this._urgentFlushIntervalMs = Infinite;
            }
            else
            {
                try
                {
                    this._urgentFlushIntervalMs = (long) settings.UrgentFlushInterval.TotalMilliseconds;
                }
                catch (OverflowException)
                {
                    this._urgentFlushIntervalMs = Infinite;
                }
            }
            this._urgentFlushThreshold = settings.UrgentFlushThreshold;
            this._maxBufferSize = settings.MaxBufferSize;
            this._maxFlushSize = settings.MaxFlushSize;
            this._maxBufferThreads = settings.MaxBufferThreads;
            this._burstWaitTimeMs = Math.Min(this._burstWaitTimeMs, this._urgentFlushIntervalMs);
            this._flushCallback = callback;
            this._buffer = new Queue();
            if (this._regularFlushIntervalMs != Infinite)
            {
                this._startTime = DateTime.UtcNow;
                this._regularTimeoutUsed = true;
                this._urgentFlushScheduled = false;
                this.SetTimer(this.GetNextRegularFlushDueTimeInMs());
            }
        }

        internal void AddEvent(WebBaseEvent webEvent)
        {
            lock (this._buffer)
            {
                if (this._buffer.Count == this._maxBufferSize)
                {
                    this._buffer.Dequeue();
                    this._discardedSinceLastFlush++;
                }
                this._buffer.Enqueue(webEvent);
                if (this._buffer.Count >= this._urgentFlushThreshold)
                {
                    this.Flush(this._maxFlushSize, FlushCallReason.UrgentFlushThresholdExceeded);
                }
                this._lastAdd = DateTime.UtcNow;
            }
        }

        private bool AnticipateBurst(DateTime now)
        {
            if ((this._urgentFlushThreshold == 1) && (this._buffer.Count == 1))
            {
                TimeSpan span = (TimeSpan) (now - this._lastAdd);
                return (span.TotalMilliseconds >= this._urgentFlushIntervalMs);
            }
            return false;
        }

        internal void Flush(int max, FlushCallReason reason)
        {
            WebBaseEvent[] events = null;
            DateTime utcNow = DateTime.UtcNow;
            long waitTimeMs = 0L;
            DateTime maxValue = DateTime.MaxValue;
            int eventsDiscardedSinceLastNotification = -1;
            int eventsInBuffer = -1;
            int num4 = 0;
            EventNotificationType regular = EventNotificationType.Regular;
            bool flag = true;
            bool flag2 = false;
            bool flag3 = false;
            lock (this._buffer)
            {
                if (this._buffer.Count == 0)
                {
                    flag = false;
                }
                switch (reason)
                {
                    case FlushCallReason.UrgentFlushThresholdExceeded:
                        if (!this._urgentFlushScheduled)
                        {
                            break;
                        }
                        return;

                    case FlushCallReason.Timer:
                        if (this._regularFlushIntervalMs != Infinite)
                        {
                            flag2 = true;
                            waitTimeMs = this.GetNextRegularFlushDueTimeInMs();
                        }
                        goto Label_00D3;

                    default:
                        goto Label_00D3;
                }
                flag = false;
                flag2 = true;
                flag3 = true;
                if (this.AnticipateBurst(utcNow))
                {
                    waitTimeMs = this._burstWaitTimeMs;
                }
                else
                {
                    waitTimeMs = 0L;
                }
                TimeSpan span = (TimeSpan) (utcNow - this._lastScheduledFlushTime);
                long totalMilliseconds = (long) span.TotalMilliseconds;
                if ((totalMilliseconds + waitTimeMs) < this._urgentFlushIntervalMs)
                {
                    waitTimeMs = this._urgentFlushIntervalMs - totalMilliseconds;
                }
            Label_00D3:
                if (flag)
                {
                    if (this._threadsInFlush >= this._maxBufferThreads)
                    {
                        num4 = 0;
                    }
                    else
                    {
                        num4 = Math.Min(this._buffer.Count, max);
                    }
                }
                if (flag)
                {
                    if (num4 > 0)
                    {
                        events = new WebBaseEvent[num4];
                        for (int i = 0; i < num4; i++)
                        {
                            events[i] = (WebBaseEvent) this._buffer.Dequeue();
                        }
                        maxValue = this._lastFlushTime;
                        this._lastFlushTime = utcNow;
                        if (reason == FlushCallReason.Timer)
                        {
                            this._lastScheduledFlushTime = utcNow;
                        }
                        eventsDiscardedSinceLastNotification = this._discardedSinceLastFlush;
                        this._discardedSinceLastFlush = 0;
                        if (reason == FlushCallReason.StaticFlush)
                        {
                            regular = EventNotificationType.Flush;
                        }
                        else
                        {
                            regular = this._regularTimeoutUsed ? EventNotificationType.Regular : EventNotificationType.Urgent;
                        }
                    }
                    eventsInBuffer = this._buffer.Count;
                    if (eventsInBuffer >= this._urgentFlushThreshold)
                    {
                        flag2 = true;
                        flag3 = true;
                        waitTimeMs = this._urgentFlushIntervalMs;
                    }
                }
                this._urgentFlushScheduled = false;
                if (flag2)
                {
                    if (flag3)
                    {
                        long nextRegularFlushDueTimeInMs = this.GetNextRegularFlushDueTimeInMs();
                        if (nextRegularFlushDueTimeInMs < waitTimeMs)
                        {
                            waitTimeMs = nextRegularFlushDueTimeInMs;
                            this._regularTimeoutUsed = true;
                        }
                        else
                        {
                            this._regularTimeoutUsed = false;
                        }
                    }
                    else
                    {
                        this._regularTimeoutUsed = true;
                    }
                    this.SetTimer(waitTimeMs);
                    this._urgentFlushScheduled = flag3;
                }
                if ((reason == FlushCallReason.Timer) && !flag2)
                {
                    this._timer.Dispose();
                    this._timer = null;
                    this._urgentFlushScheduled = false;
                }
                if (events != null)
                {
                    Interlocked.Increment(ref this._threadsInFlush);
                }
            }
            if (events != null)
            {
                ApplicationImpersonationContext context = new ApplicationImpersonationContext();
                try
                {
                    WebEventBufferFlushInfo flushInfo = new WebEventBufferFlushInfo(new WebBaseEventCollection(events), regular, Interlocked.Increment(ref this._notificationSequence), maxValue, eventsDiscardedSinceLastNotification, eventsInBuffer);
                    this._flushCallback(flushInfo);
                }
                catch (Exception exception)
                {
                    try
                    {
                        this._provider.LogException(exception);
                    }
                    catch
                    {
                    }
                }
                catch
                {
                    try
                    {
                        this._provider.LogException(new Exception(System.Web.SR.GetString("Provider_Error")));
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    if (context != null)
                    {
                        ((IDisposable) context).Dispose();
                    }
                }
                Interlocked.Decrement(ref this._threadsInFlush);
            }
        }

        private void FlushTimerCallback(object state)
        {
            this.Flush(this._maxFlushSize, FlushCallReason.Timer);
        }

        private long GetNextRegularFlushDueTimeInMs()
        {
            long num3 = this._regularFlushIntervalMs;
            if (this._regularFlushIntervalMs == Infinite)
            {
                return Infinite;
            }
            TimeSpan span = (TimeSpan) (DateTime.UtcNow - this._startTime);
            long totalMilliseconds = (long) span.TotalMilliseconds;
            long num = (((totalMilliseconds + num3) + 0x1f3L) / num3) * num3;
            return (num - totalMilliseconds);
        }

        private string PrintTime(DateTime t)
        {
            return (t.ToString("T", DateTimeFormatInfo.InvariantInfo) + "." + t.Millisecond.ToString("d03", CultureInfo.InvariantCulture));
        }

        private void SetTimer(long waitTimeMs)
        {
            if (this._timer == null)
            {
                this._timer = new Timer(new TimerCallback(this.FlushTimerCallback), null, waitTimeMs, -1L);
            }
            else
            {
                this._timer.Change(waitTimeMs, -1L);
            }
        }

        internal void Shutdown()
        {
            if (this._timer != null)
            {
                this._timer.Dispose();
                this._timer = null;
            }
        }
    }
}

