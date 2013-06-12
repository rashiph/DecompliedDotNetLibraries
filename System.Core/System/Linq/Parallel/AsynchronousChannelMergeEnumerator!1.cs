namespace System.Linq.Parallel
{
    using System;
    using System.Linq;
    using System.Threading;

    internal sealed class AsynchronousChannelMergeEnumerator<T> : MergeEnumerator<T>
    {
        private ManualResetEventSlim[] m_channelEvents;
        private int m_channelIndex;
        private AsynchronousChannel<T>[] m_channels;
        private T m_currentElement;
        private bool[] m_done;

        internal AsynchronousChannelMergeEnumerator(QueryTaskGroupState taskGroupState, AsynchronousChannel<T>[] channels) : base(taskGroupState)
        {
            this.m_channels = channels;
            this.m_channelIndex = -1;
            this.m_done = new bool[this.m_channels.Length];
        }

        public override bool MoveNext()
        {
            int channelIndex = this.m_channelIndex;
            if (channelIndex == -1)
            {
                this.m_channelIndex = channelIndex = 0;
            }
            if (channelIndex == this.m_channels.Length)
            {
                return false;
            }
            if (!this.m_done[channelIndex] && this.m_channels[channelIndex].TryDequeue(ref this.m_currentElement))
            {
                this.m_channelIndex = (channelIndex + 1) % this.m_channels.Length;
                return true;
            }
            return this.MoveNextSlowPath();
        }

        private bool MoveNextSlowPath()
        {
            int num3;
            int num = 0;
            int channelIndex = this.m_channelIndex;
            while ((num3 = this.m_channelIndex) != this.m_channels.Length)
            {
                AsynchronousChannel<T> channel = this.m_channels[num3];
                bool flag = this.m_done[num3];
                if (!flag && channel.TryDequeue(ref this.m_currentElement))
                {
                    this.m_channelIndex = (num3 + 1) % this.m_channels.Length;
                    return true;
                }
                if (!flag && channel.IsDone)
                {
                    if (!channel.IsChunkBufferEmpty)
                    {
                        channel.TryDequeue(ref this.m_currentElement);
                        return true;
                    }
                    this.m_done[num3] = true;
                    if (this.m_channelEvents != null)
                    {
                        this.m_channelEvents[num3] = null;
                    }
                    flag = true;
                    channel.Dispose();
                }
                if (flag && (++num == this.m_channels.Length))
                {
                    this.m_channelIndex = num3 = this.m_channels.Length;
                    break;
                }
                this.m_channelIndex = num3 = (num3 + 1) % this.m_channels.Length;
                if (num3 == channelIndex)
                {
                    try
                    {
                        if (this.m_channelEvents == null)
                        {
                            this.m_channelEvents = new ManualResetEventSlim[this.m_channels.Length];
                        }
                        num = 0;
                        for (int i = 0; i < this.m_channels.Length; i++)
                        {
                            if (!this.m_done[i] && this.m_channels[i].TryDequeue(ref this.m_currentElement, ref this.m_channelEvents[i]))
                            {
                                return true;
                            }
                            if (this.m_channelEvents[i] == null)
                            {
                                if (!this.m_done[i])
                                {
                                    this.m_done[i] = true;
                                    this.m_channels[i].Dispose();
                                }
                                if (++num == this.m_channels.Length)
                                {
                                    this.m_channelIndex = num3 = this.m_channels.Length;
                                    break;
                                }
                            }
                        }
                        if (num3 == this.m_channels.Length)
                        {
                            break;
                        }
                        channelIndex = this.m_channelIndex = AsynchronousChannelMergeEnumerator<T>.WaitAny(this.m_channelEvents);
                        num = 0;
                        continue;
                    }
                    finally
                    {
                        for (int j = 0; j < this.m_channelEvents.Length; j++)
                        {
                            if (this.m_channelEvents[j] != null)
                            {
                                this.m_channels[j].DoneWithDequeueWait();
                            }
                        }
                    }
                }
            }
            base.m_taskGroupState.QueryEnd(false);
            return false;
        }

        private static int WaitAny(ManualResetEventSlim[] events)
        {
            SpinWait wait = new SpinWait();
            for (int i = 0; i < 20; i++)
            {
                for (int k = 0; k < events.Length; k++)
                {
                    if ((events[k] != null) && events[k].IsSet)
                    {
                        return k;
                    }
                }
                wait.SpinOnce();
            }
            int num3 = 0;
            for (int j = 0; j < events.Length; j++)
            {
                if (events[j] == null)
                {
                    num3++;
                }
            }
            WaitHandle[] waitHandles = new WaitHandle[events.Length - num3];
            int index = 0;
            int num6 = 0;
            while (index < events.Length)
            {
                if (events[index] != null)
                {
                    waitHandles[num6] = events[index].WaitHandle;
                    num6++;
                }
                index++;
            }
            int num7 = WaitHandle.WaitAny(waitHandles);
            int num8 = 0;
            int num9 = -1;
            while (num8 < events.Length)
            {
                if (events[num8] != null)
                {
                    num9++;
                    if (num9 == num7)
                    {
                        return num8;
                    }
                }
                num8++;
            }
            return num7;
        }

        public override T Current
        {
            get
            {
                if ((this.m_channelIndex == -1) || (this.m_channelIndex == this.m_channels.Length))
                {
                    throw new InvalidOperationException(System.Linq.SR.GetString("PLINQ_CommonEnumerator_Current_NotStarted"));
                }
                return this.m_currentElement;
            }
        }
    }
}

