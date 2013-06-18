namespace System.Management
{
    using System;
    using System.Runtime;

    public class EventWatcherOptions : ManagementOptions
    {
        private int blockSize;

        public EventWatcherOptions() : this(null, ManagementOptions.InfiniteTimeout, 1)
        {
        }

        public EventWatcherOptions(ManagementNamedValueCollection context, TimeSpan timeout, int blockSize) : base(context, timeout)
        {
            this.blockSize = 1;
            base.Flags = 0x30;
            this.BlockSize = blockSize;
        }

        public override object Clone()
        {
            ManagementNamedValueCollection context = null;
            if (base.Context != null)
            {
                context = base.Context.Clone();
            }
            return new EventWatcherOptions(context, base.Timeout, this.blockSize);
        }

        public int BlockSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.blockSize;
            }
            set
            {
                this.blockSize = value;
                base.FireIdentifierChanged();
            }
        }
    }
}

