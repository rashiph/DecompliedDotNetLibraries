namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal sealed class ReliableInputConnection
    {
        private bool isLastKnown;
        private bool isSequenceClosed;
        private long last;
        private SequenceRangeCollection ranges = SequenceRangeCollection.Empty;
        private System.ServiceModel.ReliableMessagingVersion reliableMessagingVersion;
        private InterruptibleWaitObject shutdownWaitObject = new InterruptibleWaitObject(false);
        private bool terminated;
        private InterruptibleWaitObject terminateWaitObject = new InterruptibleWaitObject(false, false);

        public void Abort(ChannelBase channel)
        {
            this.shutdownWaitObject.Abort(channel);
            this.terminateWaitObject.Abort(channel);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] { new OperationWithTimeoutBeginCallback(this.shutdownWaitObject.BeginWait), new OperationWithTimeoutBeginCallback(this.terminateWaitObject.BeginWait) };
            OperationEndCallback[] endOperations = new OperationEndCallback[] { new OperationEndCallback(this.shutdownWaitObject.EndWait), new OperationEndCallback(this.terminateWaitObject.EndWait) };
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
        }

        public bool CanMerge(long sequenceNumber)
        {
            return CanMerge(sequenceNumber, this.ranges);
        }

        public static bool CanMerge(long sequenceNumber, SequenceRangeCollection ranges)
        {
            if (ranges.Count < ReliableMessagingConstants.MaxSequenceRanges)
            {
                return true;
            }
            ranges = ranges.MergeWith(sequenceNumber);
            return (ranges.Count <= ReliableMessagingConstants.MaxSequenceRanges);
        }

        public void Close(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.shutdownWaitObject.Wait(helper.RemainingTime());
            this.terminateWaitObject.Wait(helper.RemainingTime());
        }

        public void EndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        public void Fault(ChannelBase channel)
        {
            this.shutdownWaitObject.Fault(channel);
            this.terminateWaitObject.Fault(channel);
        }

        public bool IsValid(long sequenceNumber, bool isLast)
        {
            if (this.reliableMessagingVersion == System.ServiceModel.ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (isLast)
                {
                    if (this.last != 0L)
                    {
                        return (sequenceNumber == this.last);
                    }
                    if (this.ranges.Count > 0)
                    {
                        SequenceRange range = this.ranges[this.ranges.Count - 1];
                        return (sequenceNumber > range.Upper);
                    }
                    return true;
                }
                if (this.last > 0L)
                {
                    return (sequenceNumber < this.last);
                }
            }
            else if (this.isLastKnown)
            {
                return this.ranges.Contains(sequenceNumber);
            }
            return true;
        }

        public void Merge(long sequenceNumber, bool isLast)
        {
            this.ranges = this.ranges.MergeWith(sequenceNumber);
            if (isLast)
            {
                this.last = sequenceNumber;
            }
            if (this.AllAdded)
            {
                this.shutdownWaitObject.Set();
            }
        }

        public bool SetCloseSequenceLast(long last)
        {
            bool flag;
            WsrmUtilities.AssertWsrm11(this.reliableMessagingVersion);
            if ((last < 1L) || (this.ranges.Count == 0))
            {
                flag = true;
            }
            else
            {
                SequenceRange range = this.ranges[this.ranges.Count - 1];
                flag = last >= range.Upper;
            }
            if (flag)
            {
                this.isSequenceClosed = true;
                this.SetLast(last);
            }
            return flag;
        }

        private void SetLast(long last)
        {
            if (this.isLastKnown)
            {
                throw Fx.AssertAndThrow("Last can only be set once.");
            }
            this.last = last;
            this.isLastKnown = true;
            this.shutdownWaitObject.Set();
        }

        public bool SetTerminateSequenceLast(long last, out bool isLastLargeEnough)
        {
            WsrmUtilities.AssertWsrm11(this.reliableMessagingVersion);
            isLastLargeEnough = true;
            if (last < 1L)
            {
                return false;
            }
            int count = this.ranges.Count;
            long num2 = (count > 0) ? this.ranges[count - 1].Upper : 0L;
            if (last < num2)
            {
                isLastLargeEnough = false;
                return false;
            }
            if ((count > 1) || (last > num2))
            {
                return false;
            }
            this.SetLast(last);
            return true;
        }

        public bool Terminate()
        {
            if ((this.reliableMessagingVersion != System.ServiceModel.ReliableMessagingVersion.WSReliableMessagingFebruary2005) && !this.isSequenceClosed)
            {
                return this.isLastKnown;
            }
            if (!this.terminated && this.AllAdded)
            {
                this.terminateWaitObject.Set();
                this.terminated = true;
            }
            return this.terminated;
        }

        public bool AllAdded
        {
            get
            {
                if (this.ranges.Count == 1)
                {
                    SequenceRange range = this.ranges[0];
                    if (range.Lower == 1L)
                    {
                        SequenceRange range2 = this.ranges[0];
                        if (range2.Upper == this.last)
                        {
                            return true;
                        }
                    }
                }
                return this.isLastKnown;
            }
        }

        public bool IsLastKnown
        {
            get
            {
                if (this.last == 0L)
                {
                    return this.isLastKnown;
                }
                return true;
            }
        }

        public bool IsSequenceClosed
        {
            get
            {
                return this.isSequenceClosed;
            }
        }

        public long Last
        {
            get
            {
                return this.last;
            }
        }

        public SequenceRangeCollection Ranges
        {
            get
            {
                return this.ranges;
            }
        }

        public System.ServiceModel.ReliableMessagingVersion ReliableMessagingVersion
        {
            set
            {
                this.reliableMessagingVersion = value;
            }
        }
    }
}

