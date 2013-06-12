namespace System.Timers
{
    using System;

    public class ElapsedEventArgs : EventArgs
    {
        private DateTime signalTime;

        internal ElapsedEventArgs(int low, int high)
        {
            long fileTime = (high << 0x20) | (low & ((long) 0xffffffffL));
            this.signalTime = DateTime.FromFileTime(fileTime);
        }

        public DateTime SignalTime
        {
            get
            {
                return this.signalTime;
            }
        }
    }
}

