namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;
    using System.Xml;

    internal sealed class TransmissionStrategy
    {
        private bool aborted;
        private bool closed;
        private int congestionControlModeAcks;
        private UniqueId id;
        private long last;
        private int lossWindowSize;
        private int maxWindowSize;
        private long meanRtt;
        private ComponentExceptionHandler onException;
        private int quotaRemaining;
        private ReliableMessagingVersion reliableMessagingVersion;
        private bool requestAcks;
        private List<long> retransmissionWindow = new List<long>();
        private RetryHandler retryTimeoutElapsedHandler;
        private IOThreadTimer retryTimer;
        private long serrRtt;
        private int slowStartThreshold;
        private bool startup = true;
        private object thisLock = new object();
        private long timeout;
        private Queue<IQueueAdder> waitQueue = new Queue<IQueueAdder>();
        private SlidingWindow window;
        private int windowSize = 1;
        private long windowStart = 1L;

        public TransmissionStrategy(ReliableMessagingVersion reliableMessagingVersion, TimeSpan initRtt, int maxWindowSize, bool requestAcks, UniqueId id)
        {
            if (initRtt < TimeSpan.Zero)
            {
                throw Fx.AssertAndThrow("Argument initRtt cannot be negative.");
            }
            if (maxWindowSize <= 0)
            {
                throw Fx.AssertAndThrow("Argument maxWindow size must be positive.");
            }
            this.id = id;
            this.maxWindowSize = this.lossWindowSize = maxWindowSize;
            this.meanRtt = Math.Min((long) initRtt.TotalMilliseconds, 0x55555555555555L) << 7;
            this.serrRtt = this.meanRtt >> 1;
            this.window = new SlidingWindow(maxWindowSize);
            this.slowStartThreshold = maxWindowSize;
            this.timeout = Math.Max((long) (0xc800L + this.meanRtt), (long) (this.meanRtt + (this.serrRtt << 2)));
            this.quotaRemaining = 0x7fffffff;
            this.retryTimer = new IOThreadTimer(new Action<object>(this.OnRetryElapsed), null, true);
            this.requestAcks = requestAcks;
            this.reliableMessagingVersion = reliableMessagingVersion;
        }

        public void Abort(ChannelBase channel)
        {
            lock (this.ThisLock)
            {
                this.aborted = true;
                if (!this.closed)
                {
                    this.closed = true;
                    this.retryTimer.Cancel();
                    while (this.waitQueue.Count > 0)
                    {
                        this.waitQueue.Dequeue().Abort(channel);
                    }
                    this.window.Close();
                }
            }
        }

        public bool Add(Message message, TimeSpan timeout, object state, out MessageAttemptInfo attemptInfo)
        {
            return this.InternalAdd(message, false, timeout, state, out attemptInfo);
        }

        public MessageAttemptInfo AddLast(Message message, TimeSpan timeout, object state)
        {
            if (this.reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                throw Fx.AssertAndThrow("Last message supported only in February 2005.");
            }
            MessageAttemptInfo attemptInfo = new MessageAttemptInfo();
            this.InternalAdd(message, true, timeout, state, out attemptInfo);
            return attemptInfo;
        }

        private MessageAttemptInfo AddToWindow(Message message, bool isLast, object state)
        {
            MessageAttemptInfo info = new MessageAttemptInfo();
            long sequenceNumber = this.windowStart + this.window.Count;
            WsrmUtilities.AddSequenceHeader(this.reliableMessagingVersion, message, this.id, sequenceNumber, isLast);
            if (this.requestAcks && ((this.window.Count == (this.windowSize - 1)) || (this.quotaRemaining == 1)))
            {
                message.Properties.AllowOutputBatching = false;
                WsrmUtilities.AddAckRequestedHeader(this.reliableMessagingVersion, message, this.id);
            }
            if (this.window.Count == 0)
            {
                this.retryTimer.Set(this.Timeout);
            }
            this.window.Add(message, Now, state);
            this.quotaRemaining--;
            if (isLast)
            {
                this.last = sequenceNumber;
            }
            int index = (int) (sequenceNumber - this.windowStart);
            return new MessageAttemptInfo(this.window.GetMessage(index), sequenceNumber, 0, state);
        }

        public IAsyncResult BeginAdd(Message message, TimeSpan timeout, object state, AsyncCallback callback, object asyncState)
        {
            return this.InternalBeginAdd(message, false, timeout, state, callback, asyncState);
        }

        public IAsyncResult BeginAddLast(Message message, TimeSpan timeout, object state, AsyncCallback callback, object asyncState)
        {
            if (this.reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                throw Fx.AssertAndThrow("Last message supported only in February 2005.");
            }
            return this.InternalBeginAdd(message, true, timeout, state, callback, asyncState);
        }

        private bool CanAdd()
        {
            return (((this.window.Count < this.windowSize) && (this.quotaRemaining > 0)) && (this.waitQueue.Count == 0));
        }

        public void Close()
        {
            lock (this.ThisLock)
            {
                if (!this.closed)
                {
                    this.closed = true;
                    this.retryTimer.Cancel();
                    if (this.waitQueue.Count != 0)
                    {
                        throw Fx.AssertAndThrow("The reliable channel must throw prior to the call to Close() if there are outstanding send or request operations.");
                    }
                    this.window.Close();
                }
            }
        }

        public void DequeuePending()
        {
            Queue<IQueueAdder> queue = null;
            lock (this.ThisLock)
            {
                if (this.closed || (this.waitQueue.Count == 0))
                {
                    return;
                }
                int num = Math.Min(this.windowSize, this.quotaRemaining) - this.window.Count;
                if (num <= 0)
                {
                    return;
                }
                num = Math.Min(num, this.waitQueue.Count);
                queue = new Queue<IQueueAdder>(num);
                while (num-- > 0)
                {
                    IQueueAdder item = this.waitQueue.Dequeue();
                    item.Complete0();
                    queue.Enqueue(item);
                }
                goto Label_00A6;
            }
        Label_009B:
            queue.Dequeue().Complete1();
        Label_00A6:
            if (queue.Count > 0)
            {
                goto Label_009B;
            }
        }

        public bool EndAdd(IAsyncResult result, out MessageAttemptInfo attemptInfo)
        {
            return this.InternalEndAdd(result, out attemptInfo);
        }

        public MessageAttemptInfo EndAddLast(IAsyncResult result)
        {
            MessageAttemptInfo attemptInfo = new MessageAttemptInfo();
            this.InternalEndAdd(result, out attemptInfo);
            return attemptInfo;
        }

        public void Fault(ChannelBase channel)
        {
            lock (this.ThisLock)
            {
                if (!this.closed)
                {
                    this.closed = true;
                    this.retryTimer.Cancel();
                    while (this.waitQueue.Count > 0)
                    {
                        this.waitQueue.Dequeue().Fault(channel);
                    }
                    this.window.Close();
                }
            }
        }

        public MessageAttemptInfo GetMessageInfoForRetry(bool remove)
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return new MessageAttemptInfo();
                }
                if (remove)
                {
                    if (this.retransmissionWindow.Count == 0)
                    {
                        throw Fx.AssertAndThrow("The caller is not allowed to remove a message attempt when there are no message attempts.");
                    }
                    this.retransmissionWindow.RemoveAt(0);
                }
                while (this.retransmissionWindow.Count > 0)
                {
                    long sequenceNumber = this.retransmissionWindow[0];
                    if (sequenceNumber < this.windowStart)
                    {
                        this.retransmissionWindow.RemoveAt(0);
                    }
                    else
                    {
                        int index = (int) (sequenceNumber - this.windowStart);
                        if (this.window.GetTransferred(index))
                        {
                            this.retransmissionWindow.RemoveAt(0);
                            continue;
                        }
                        return new MessageAttemptInfo(this.window.GetMessage(index), sequenceNumber, this.window.GetRetryCount(index), this.window.GetState(index));
                    }
                }
                return new MessageAttemptInfo();
            }
        }

        private bool InternalAdd(Message message, bool isLast, TimeSpan timeout, object state, out MessageAttemptInfo attemptInfo)
        {
            WaitQueueAdder adder;
            attemptInfo = new MessageAttemptInfo();
            lock (this.ThisLock)
            {
                if (isLast && (this.last != 0L))
                {
                    throw Fx.AssertAndThrow("Can't add more than one last message.");
                }
                if (!this.IsAddValid())
                {
                    return false;
                }
                this.ThrowIfRollover();
                if (this.CanAdd())
                {
                    attemptInfo = this.AddToWindow(message, isLast, state);
                    return true;
                }
                adder = new WaitQueueAdder(this, message, isLast, state);
                this.waitQueue.Enqueue(adder);
            }
            attemptInfo = adder.Wait(timeout);
            return true;
        }

        private IAsyncResult InternalBeginAdd(Message message, bool isLast, TimeSpan timeout, object state, AsyncCallback callback, object asyncState)
        {
            bool flag;
            MessageAttemptInfo parameter = new MessageAttemptInfo();
            lock (this.ThisLock)
            {
                if (isLast && (this.last != 0L))
                {
                    throw Fx.AssertAndThrow("Can't add more than one last message.");
                }
                flag = this.IsAddValid();
                if (flag)
                {
                    this.ThrowIfRollover();
                    if (!this.CanAdd())
                    {
                        AsyncQueueAdder item = new AsyncQueueAdder(message, isLast, timeout, state, this, callback, asyncState);
                        this.waitQueue.Enqueue(item);
                        return item;
                    }
                    parameter = this.AddToWindow(message, isLast, state);
                }
            }
            return new CompletedAsyncResult<bool, MessageAttemptInfo>(flag, parameter, callback, asyncState);
        }

        private bool InternalEndAdd(IAsyncResult result, out MessageAttemptInfo attemptInfo)
        {
            if (result is CompletedAsyncResult<bool, MessageAttemptInfo>)
            {
                return CompletedAsyncResult<bool, MessageAttemptInfo>.End(result, out attemptInfo);
            }
            attemptInfo = AsyncQueueAdder.End((AsyncQueueAdder) result);
            return true;
        }

        private bool IsAddValid()
        {
            return (!this.aborted && !this.closed);
        }

        public bool IsFinalAckConsistent(SequenceRangeCollection ranges)
        {
            lock (this.ThisLock)
            {
                SequenceRange range2;
                if (this.closed)
                {
                    return true;
                }
                if ((this.windowStart == 1L) && (this.window.Count == 0))
                {
                    return (ranges.Count == 0);
                }
                if (ranges.Count != 0)
                {
                    SequenceRange range = ranges[0];
                    if (range.Lower == 1L)
                    {
                        goto Label_005F;
                    }
                }
                return false;
            Label_005F:
                range2 = ranges[0];
                return (range2.Upper >= (this.windowStart - 1L));
            }
        }

        public void OnRetryElapsed(object state)
        {
            try
            {
                MessageAttemptInfo attemptInfo = new MessageAttemptInfo();
                lock (this.ThisLock)
                {
                    if (this.closed || (this.window.Count == 0))
                    {
                        return;
                    }
                    this.window.RecordRetry(0, Now);
                    this.congestionControlModeAcks = 0;
                    this.slowStartThreshold = Math.Max(1, this.windowSize >> 1);
                    this.lossWindowSize = this.windowSize;
                    this.windowSize = 1;
                    this.timeout = this.timeout << 1;
                    this.startup = false;
                    attemptInfo = new MessageAttemptInfo(this.window.GetMessage(0), this.windowStart, this.window.GetRetryCount(0), this.window.GetState(0));
                }
                this.retryTimeoutElapsedHandler(attemptInfo);
                lock (this.ThisLock)
                {
                    if (!this.closed && (this.window.Count > 0))
                    {
                        this.retryTimer.Set(this.Timeout);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.onException(exception);
            }
        }

        public void ProcessAcknowledgement(SequenceRangeCollection ranges, out bool invalidAck, out bool inconsistentAck)
        {
            invalidAck = false;
            inconsistentAck = false;
            bool flag = false;
            bool flag2 = false;
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return;
                }
                long num = (this.windowStart + this.window.Count) - 1L;
                long num2 = this.windowStart - 1L;
                int transferredCount = this.window.TransferredCount;
                for (int i = 0; i < ranges.Count; i++)
                {
                    SequenceRange range = ranges[i];
                    if (range.Upper > num)
                    {
                        invalidAck = true;
                        return;
                    }
                    if (((range.Lower > 1L) && (range.Lower <= num2)) || (range.Upper < num2))
                    {
                        flag2 = true;
                    }
                    if (range.Upper >= this.windowStart)
                    {
                        if (range.Lower <= this.windowStart)
                        {
                            flag = true;
                        }
                        if (!flag)
                        {
                            int beginIndex = (int) (range.Lower - this.windowStart);
                            int endIndex = (range.Upper > num) ? (this.window.Count - 1) : ((int) (range.Upper - this.windowStart));
                            flag = this.window.GetTransferredInRangeCount(beginIndex, endIndex) < ((endIndex - beginIndex) + 1);
                        }
                        if ((transferredCount > 0) && !flag2)
                        {
                            int num7 = (range.Lower < this.windowStart) ? ((int) 0L) : ((int) (range.Lower - this.windowStart));
                            int num8 = (range.Upper > num) ? (this.window.Count - 1) : ((int) (range.Upper - this.windowStart));
                            transferredCount -= this.window.GetTransferredInRangeCount(num7, num8);
                        }
                    }
                }
                if (transferredCount > 0)
                {
                    flag2 = true;
                }
            }
            inconsistentAck = flag2 && flag;
        }

        public bool ProcessTransferred(long transferred, int quotaRemaining)
        {
            if (transferred <= 0L)
            {
                throw Fx.AssertAndThrow("Argument transferred must be a valid sequence number.");
            }
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return false;
                }
                return this.ProcessTransferred(new SequenceRange(transferred), quotaRemaining);
            }
        }

        private bool ProcessTransferred(SequenceRange range, int quotaRemaining)
        {
            if (range.Upper < this.windowStart)
            {
                if (((range.Upper == (this.windowStart - 1L)) && (quotaRemaining != -1)) && (quotaRemaining > this.quotaRemaining))
                {
                    this.quotaRemaining = quotaRemaining - Math.Min(this.windowSize, this.window.Count);
                }
                return false;
            }
            if (range.Lower > this.windowStart)
            {
                for (long j = range.Lower; j <= range.Upper; j += 1L)
                {
                    this.window.SetTransferred((int) (j - this.windowStart));
                }
                return false;
            }
            bool flag = false;
            this.retryTimer.Cancel();
            long num = (range.Upper - this.windowStart) + 1L;
            if (num == 1L)
            {
                for (int k = 1; k < this.window.Count; k++)
                {
                    if (!this.window.GetTransferred(k))
                    {
                        break;
                    }
                    num += 1L;
                }
            }
            long now = Now;
            long num4 = this.windowStart + this.windowSize;
            for (int i = 0; i < ((int) num); i++)
            {
                this.UpdateStats(now, this.window.GetLastAttemptTime(i));
            }
            if (quotaRemaining != -1)
            {
                int num6 = Math.Min(this.windowSize, this.window.Count) - ((int) num);
                this.quotaRemaining = quotaRemaining - Math.Max(0, num6);
            }
            this.window.Remove((int) num);
            this.windowStart += num;
            int num7 = 0;
            if (this.windowSize <= this.slowStartThreshold)
            {
                this.windowSize = Math.Min(this.maxWindowSize, Math.Min((int) (this.slowStartThreshold + 1), (int) (this.windowSize + ((int) num))));
                if (!this.startup)
                {
                    num7 = 0;
                }
                else
                {
                    num7 = Math.Max(0, ((int) num4) - ((int) this.windowStart));
                }
            }
            else
            {
                this.congestionControlModeAcks += (int) num;
                int num8 = Math.Max(1, (this.lossWindowSize - this.slowStartThreshold) / 8);
                int num9 = ((this.windowSize - this.slowStartThreshold) * this.windowSize) / num8;
                if (this.congestionControlModeAcks > num9)
                {
                    this.congestionControlModeAcks = 0;
                    this.windowSize = Math.Min(this.maxWindowSize, this.windowSize + 1);
                }
                num7 = Math.Max(0, ((int) num4) - ((int) this.windowStart));
            }
            int num10 = Math.Min(this.windowSize, this.window.Count);
            if (num7 < num10)
            {
                flag = this.retransmissionWindow.Count == 0;
                for (int m = num7; (m < this.windowSize) && (m < this.window.Count); m++)
                {
                    long item = this.windowStart + m;
                    if (!this.window.GetTransferred(m) && !this.retransmissionWindow.Contains(item))
                    {
                        this.window.RecordRetry(m, Now);
                        this.retransmissionWindow.Add(item);
                    }
                }
            }
            if (this.window.Count > 0)
            {
                this.retryTimer.Set(this.Timeout);
            }
            return flag;
        }

        public bool ProcessTransferred(SequenceRangeCollection ranges, int quotaRemaining)
        {
            if (ranges.Count == 0)
            {
                return false;
            }
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return false;
                }
                bool flag = false;
                for (int i = 0; i < ranges.Count; i++)
                {
                    if (this.ProcessTransferred(ranges[i], quotaRemaining))
                    {
                        flag = true;
                    }
                }
                return flag;
            }
        }

        private bool RemoveAdder(IQueueAdder adder)
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return false;
                }
                bool flag = false;
                for (int i = 0; i < this.waitQueue.Count; i++)
                {
                    IQueueAdder objB = this.waitQueue.Dequeue();
                    if (object.ReferenceEquals(adder, objB))
                    {
                        flag = true;
                    }
                    else
                    {
                        this.waitQueue.Enqueue(objB);
                    }
                }
                return flag;
            }
        }

        public bool SetLast()
        {
            if (this.reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("SetLast supported only in 1.1.");
            }
            lock (this.ThisLock)
            {
                if (this.last != 0L)
                {
                    throw Fx.AssertAndThrow("Cannot set last more than once.");
                }
                this.last = (this.windowStart + this.window.Count) - 1L;
                return ((this.last == 0L) || this.DoneTransmitting);
            }
        }

        private void ThrowIfRollover()
        {
            if (((this.windowStart + this.window.Count) + this.waitQueue.Count) == 0x7fffffffffffffffL)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageNumberRolloverFault(this.id).CreateException());
            }
        }

        private void UpdateStats(long now, long lastAttemptTime)
        {
            now = Math.Max(now, lastAttemptTime);
            long num = now - lastAttemptTime;
            long num2 = num - this.meanRtt;
            this.serrRtt = Math.Min((long) (this.serrRtt + ((Math.Abs(num2) - this.serrRtt) >> 3)), (long) 0x1555555555555555L);
            this.meanRtt = Math.Min((long) (this.meanRtt + (num2 >> 3)), (long) 0x2aaaaaaaaaaaaaaaL);
            this.timeout = Math.Max((long) (0xc800L + this.meanRtt), (long) (this.meanRtt + (this.serrRtt << 2)));
        }

        public bool DoneTransmitting
        {
            get
            {
                return ((this.last != 0L) && (this.windowStart == (this.last + 1L)));
            }
        }

        public bool HasPending
        {
            get
            {
                if (this.window.Count <= 0)
                {
                    return (this.waitQueue.Count > 0);
                }
                return true;
            }
        }

        public long Last
        {
            get
            {
                return this.last;
            }
        }

        private static long Now
        {
            get
            {
                return ((Ticks.Now / 0x2710L) << 7);
            }
        }

        public ComponentExceptionHandler OnException
        {
            set
            {
                this.onException = value;
            }
        }

        public int QuotaRemaining
        {
            get
            {
                return this.quotaRemaining;
            }
        }

        public RetryHandler RetryTimeoutElapsed
        {
            set
            {
                this.retryTimeoutElapsedHandler = value;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public int Timeout
        {
            get
            {
                return (int) (this.timeout >> 7);
            }
        }

        private class AsyncQueueAdder : WaitAsyncResult, TransmissionStrategy.IQueueAdder
        {
            private MessageAttemptInfo attemptInfo;
            private bool isLast;
            private TransmissionStrategy strategy;

            public AsyncQueueAdder(Message message, bool isLast, TimeSpan timeout, object state, TransmissionStrategy strategy, AsyncCallback callback, object asyncState) : base(timeout, true, callback, asyncState)
            {
                this.attemptInfo = new MessageAttemptInfo();
                this.attemptInfo = new MessageAttemptInfo(message, 0L, 0, state);
                this.isLast = isLast;
                this.strategy = strategy;
                base.Begin();
            }

            public void Abort(CommunicationObject communicationObject)
            {
                this.attemptInfo.Message.Close();
                base.OnAborted(communicationObject);
            }

            public void Complete0()
            {
                this.attemptInfo = this.strategy.AddToWindow(this.attemptInfo.Message, this.isLast, this.attemptInfo.State);
            }

            public void Complete1()
            {
                base.OnSignaled();
            }

            public static MessageAttemptInfo End(TransmissionStrategy.AsyncQueueAdder result)
            {
                AsyncResult.End<TransmissionStrategy.AsyncQueueAdder>(result);
                return result.attemptInfo;
            }

            public void Fault(CommunicationObject communicationObject)
            {
                this.attemptInfo.Message.Close();
                base.OnFaulted(communicationObject);
            }

            protected override string GetTimeoutString(TimeSpan timeout)
            {
                return System.ServiceModel.SR.GetString("TimeoutOnAddToWindow", new object[] { timeout });
            }

            protected override void OnTimerElapsed(object state)
            {
                if (this.strategy.RemoveAdder(this))
                {
                    base.OnTimerElapsed(state);
                }
            }
        }

        private static class Constants
        {
            public const int ChebychevFactor = 2;
            public const int Gain = 3;
            public const long MaxMeanRtt = 0x2aaaaaaaaaaaaaaaL;
            public const long MaxSerrRtt = 0x1555555555555555L;
            public const int TimeMultiplier = 7;
        }

        private interface IQueueAdder
        {
            void Abort(CommunicationObject communicationObject);
            void Complete0();
            void Complete1();
            void Fault(CommunicationObject communicationObject);
        }

        private class SlidingWindow
        {
            private TransmissionInfo[] buffer;
            private int head;
            private int maxSize;
            private int tail;

            public SlidingWindow(int maxSize)
            {
                this.maxSize = maxSize + 1;
                this.buffer = new TransmissionInfo[this.maxSize];
            }

            public void Add(Message message, long addTime, object state)
            {
                if (this.Count >= (this.maxSize - 1))
                {
                    throw Fx.AssertAndThrow("The caller is not allowed to add messages beyond the sliding window's maximum size.");
                }
                this.buffer[this.tail] = new TransmissionInfo(message, addTime, state);
                this.tail = (this.tail + 1) % this.maxSize;
            }

            private void AssertIndex(int index)
            {
                if (index >= this.Count)
                {
                    throw Fx.AssertAndThrow("Argument index must be less than Count.");
                }
                if (index < 0)
                {
                    throw Fx.AssertAndThrow("Argument index must be positive.");
                }
            }

            public void Close()
            {
                this.Remove(this.Count);
            }

            public long GetLastAttemptTime(int index)
            {
                this.AssertIndex(index);
                return this.buffer[(this.head + index) % this.maxSize].LastAttemptTime;
            }

            public Message GetMessage(int index)
            {
                this.AssertIndex(index);
                if (!this.buffer[(this.head + index) % this.maxSize].Transferred)
                {
                    return this.buffer[(this.head + index) % this.maxSize].Buffer.CreateMessage();
                }
                return null;
            }

            public int GetRetryCount(int index)
            {
                this.AssertIndex(index);
                return this.buffer[(this.head + index) % this.maxSize].RetryCount;
            }

            public object GetState(int index)
            {
                this.AssertIndex(index);
                return this.buffer[(this.head + index) % this.maxSize].State;
            }

            public bool GetTransferred(int index)
            {
                this.AssertIndex(index);
                return this.buffer[(this.head + index) % this.maxSize].Transferred;
            }

            public int GetTransferredInRangeCount(int beginIndex, int endIndex)
            {
                if (beginIndex < 0)
                {
                    throw Fx.AssertAndThrow("Argument beginIndex cannot be negative.");
                }
                if (endIndex >= this.Count)
                {
                    throw Fx.AssertAndThrow("Argument endIndex cannot be greater than Count.");
                }
                if (endIndex < beginIndex)
                {
                    throw Fx.AssertAndThrow("Argument endIndex cannot be less than argument beginIndex.");
                }
                int num = 0;
                for (int i = beginIndex; i <= endIndex; i++)
                {
                    if (this.buffer[(this.head + i) % this.maxSize].Transferred)
                    {
                        num++;
                    }
                }
                return num;
            }

            public int RecordRetry(int index, long retryTime)
            {
                this.AssertIndex(index);
                this.buffer[(this.head + index) % this.maxSize].LastAttemptTime = retryTime;
                return ++this.buffer[(this.head + index) % this.maxSize].RetryCount;
            }

            public void Remove(int count)
            {
                if (count > this.Count)
                {
                }
                while (count-- > 0)
                {
                    this.buffer[this.head].Buffer.Close();
                    this.buffer[this.head].Buffer = null;
                    this.head = (this.head + 1) % this.maxSize;
                }
            }

            public void SetTransferred(int index)
            {
                this.AssertIndex(index);
                this.buffer[(this.head + index) % this.maxSize].Transferred = true;
            }

            public int Count
            {
                get
                {
                    if (this.tail >= this.head)
                    {
                        return (this.tail - this.head);
                    }
                    return ((this.tail - this.head) + this.maxSize);
                }
            }

            public int TransferredCount
            {
                get
                {
                    if (this.Count == 0)
                    {
                        return 0;
                    }
                    return this.GetTransferredInRangeCount(0, this.Count - 1);
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct TransmissionInfo
            {
                internal MessageBuffer Buffer;
                internal long LastAttemptTime;
                internal int RetryCount;
                internal object State;
                internal bool Transferred;
                public TransmissionInfo(Message message, long lastAttemptTime, object state)
                {
                    this.Buffer = message.CreateBufferedCopy(0x7fffffff);
                    this.LastAttemptTime = lastAttemptTime;
                    this.RetryCount = 0;
                    this.State = state;
                    this.Transferred = false;
                }
            }
        }

        private class WaitQueueAdder : TransmissionStrategy.IQueueAdder
        {
            private MessageAttemptInfo attemptInfo = new MessageAttemptInfo();
            private ManualResetEvent completeEvent = new ManualResetEvent(false);
            private Exception exception;
            private bool isLast;
            private TransmissionStrategy strategy;

            public WaitQueueAdder(TransmissionStrategy strategy, Message message, bool isLast, object state)
            {
                this.strategy = strategy;
                this.isLast = isLast;
                this.attemptInfo = new MessageAttemptInfo(message, 0L, 0, state);
            }

            public void Abort(CommunicationObject communicationObject)
            {
                this.exception = communicationObject.CreateClosedException();
                this.completeEvent.Set();
            }

            public void Complete0()
            {
                this.attemptInfo = this.strategy.AddToWindow(this.attemptInfo.Message, this.isLast, this.attemptInfo.State);
                this.completeEvent.Set();
            }

            public void Complete1()
            {
            }

            public void Fault(CommunicationObject communicationObject)
            {
                this.exception = communicationObject.GetTerminalException();
                this.completeEvent.Set();
            }

            public MessageAttemptInfo Wait(TimeSpan timeout)
            {
                if ((!TimeoutHelper.WaitOne(this.completeEvent, timeout) && this.strategy.RemoveAdder(this)) && (this.exception == null))
                {
                    this.exception = new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnAddToWindow", new object[] { timeout }));
                }
                if (this.exception != null)
                {
                    this.attemptInfo.Message.Close();
                    this.completeEvent.Close();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.exception);
                }
                this.completeEvent.Close();
                return this.attemptInfo;
            }
        }
    }
}

