namespace System.Linq.Parallel
{
    using System;
    using System.Linq;

    internal sealed class SynchronousChannelMergeEnumerator<T> : MergeEnumerator<T>
    {
        private int m_channelIndex;
        private SynchronousChannel<T>[] m_channels;
        private T m_currentElement;

        internal SynchronousChannelMergeEnumerator(QueryTaskGroupState taskGroupState, SynchronousChannel<T>[] channels) : base(taskGroupState)
        {
            this.m_channels = channels;
            this.m_channelIndex = -1;
        }

        public override bool MoveNext()
        {
            if (this.m_channelIndex == -1)
            {
                this.m_channelIndex = 0;
            }
            while (this.m_channelIndex != this.m_channels.Length)
            {
                SynchronousChannel<T> channel = this.m_channels[this.m_channelIndex];
                if (channel.Count == 0)
                {
                    this.m_channelIndex++;
                }
                else
                {
                    this.m_currentElement = channel.Dequeue();
                    return true;
                }
            }
            return false;
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

