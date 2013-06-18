namespace System.Management
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Threading;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class ManagementOptions : ICloneable
    {
        internal ManagementNamedValueCollection context;
        internal int flags;
        public static readonly TimeSpan InfiniteTimeout = TimeSpan.MaxValue;
        internal TimeSpan timeout;

        internal event IdentifierChangedEventHandler IdentifierChanged;

        internal ManagementOptions() : this(null, InfiniteTimeout)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ManagementOptions(ManagementNamedValueCollection context, TimeSpan timeout) : this(context, timeout, 0)
        {
        }

        internal ManagementOptions(ManagementNamedValueCollection context, TimeSpan timeout, int flags)
        {
            this.flags = flags;
            if (context != null)
            {
                this.Context = context;
            }
            else
            {
                this.context = null;
            }
            this.Timeout = timeout;
        }

        public abstract object Clone();
        internal void FireIdentifierChanged()
        {
            if (this.IdentifierChanged != null)
            {
                this.IdentifierChanged(this, null);
            }
        }

        internal IWbemContext GetContext()
        {
            if (this.context != null)
            {
                return this.context.GetContext();
            }
            return null;
        }

        internal void HandleIdentifierChange(object sender, IdentifierChangedEventArgs args)
        {
            this.FireIdentifierChanged();
        }

        public ManagementNamedValueCollection Context
        {
            get
            {
                if (this.context == null)
                {
                    return (this.context = new ManagementNamedValueCollection());
                }
                return this.context;
            }
            set
            {
                ManagementNamedValueCollection context = this.context;
                if (value != null)
                {
                    this.context = value.Clone();
                }
                else
                {
                    this.context = new ManagementNamedValueCollection();
                }
                if (context != null)
                {
                    context.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                }
                this.context.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                this.HandleIdentifierChange(this, null);
            }
        }

        internal int Flags
        {
            get
            {
                return this.flags;
            }
            set
            {
                this.flags = value;
            }
        }

        internal bool SendStatus
        {
            get
            {
                if ((this.Flags & 0x80) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                this.Flags = !value ? (this.Flags & -129) : (this.Flags | 0x80);
            }
        }

        public TimeSpan Timeout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.timeout;
            }
            set
            {
                if (value.Ticks < 0L)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.timeout = value;
                this.FireIdentifierChanged();
            }
        }
    }
}

