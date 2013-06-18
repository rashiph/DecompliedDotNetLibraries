namespace System.Management
{
    using System;

    public class ObjectGetOptions : ManagementOptions
    {
        public ObjectGetOptions() : this(null, ManagementOptions.InfiniteTimeout, false)
        {
        }

        public ObjectGetOptions(ManagementNamedValueCollection context) : this(context, ManagementOptions.InfiniteTimeout, false)
        {
        }

        public ObjectGetOptions(ManagementNamedValueCollection context, TimeSpan timeout, bool useAmendedQualifiers) : base(context, timeout)
        {
            this.UseAmendedQualifiers = useAmendedQualifiers;
        }

        internal static ObjectGetOptions _Clone(ObjectGetOptions options)
        {
            return _Clone(options, null);
        }

        internal static ObjectGetOptions _Clone(ObjectGetOptions options, IdentifierChangedEventHandler handler)
        {
            ObjectGetOptions options2;
            if (options != null)
            {
                options2 = new ObjectGetOptions(options.context, options.timeout, options.UseAmendedQualifiers);
            }
            else
            {
                options2 = new ObjectGetOptions();
            }
            if (handler != null)
            {
                options2.IdentifierChanged += handler;
                return options2;
            }
            if (options != null)
            {
                options2.IdentifierChanged += new IdentifierChangedEventHandler(options.HandleIdentifierChange);
            }
            return options2;
        }

        public override object Clone()
        {
            ManagementNamedValueCollection context = null;
            if (base.Context != null)
            {
                context = base.Context.Clone();
            }
            return new ObjectGetOptions(context, base.Timeout, this.UseAmendedQualifiers);
        }

        public bool UseAmendedQualifiers
        {
            get
            {
                if ((base.Flags & 0x20000) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                base.Flags = value ? (base.Flags | 0x20000) : (base.Flags & -131073);
                base.FireIdentifierChanged();
            }
        }
    }
}

