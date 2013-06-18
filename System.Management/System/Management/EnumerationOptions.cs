namespace System.Management
{
    using System;
    using System.Runtime;

    public class EnumerationOptions : ManagementOptions
    {
        private int blockSize;

        public EnumerationOptions() : this(null, ManagementOptions.InfiniteTimeout, 1, true, true, false, false, false, false, false)
        {
        }

        public EnumerationOptions(ManagementNamedValueCollection context, TimeSpan timeout, int blockSize, bool rewindable, bool returnImmediatley, bool useAmendedQualifiers, bool ensureLocatable, bool prototypeOnly, bool directRead, bool enumerateDeep) : base(context, timeout)
        {
            this.BlockSize = blockSize;
            this.Rewindable = rewindable;
            this.ReturnImmediately = returnImmediatley;
            this.UseAmendedQualifiers = useAmendedQualifiers;
            this.EnsureLocatable = ensureLocatable;
            this.PrototypeOnly = prototypeOnly;
            this.DirectRead = directRead;
            this.EnumerateDeep = enumerateDeep;
        }

        public override object Clone()
        {
            ManagementNamedValueCollection context = null;
            if (base.Context != null)
            {
                context = base.Context.Clone();
            }
            return new EnumerationOptions(context, base.Timeout, this.blockSize, this.Rewindable, this.ReturnImmediately, this.UseAmendedQualifiers, this.EnsureLocatable, this.PrototypeOnly, this.DirectRead, this.EnumerateDeep);
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
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.blockSize = value;
            }
        }

        public bool DirectRead
        {
            get
            {
                if ((base.Flags & 0x200) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                base.Flags = value ? (base.Flags | 0x200) : (base.Flags & -513);
            }
        }

        public bool EnsureLocatable
        {
            get
            {
                if ((base.Flags & 0x100) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                base.Flags = value ? (base.Flags | 0x100) : (base.Flags & -257);
            }
        }

        public bool EnumerateDeep
        {
            get
            {
                return ((base.Flags & 1) == 0);
            }
            set
            {
                base.Flags = !value ? (base.Flags | 1) : (base.Flags & -2);
            }
        }

        public bool PrototypeOnly
        {
            get
            {
                if ((base.Flags & 2) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                base.Flags = value ? (base.Flags | 2) : (base.Flags & -3);
            }
        }

        public bool ReturnImmediately
        {
            get
            {
                if ((base.Flags & 0x10) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                base.Flags = !value ? (base.Flags & -17) : (base.Flags | 0x10);
            }
        }

        public bool Rewindable
        {
            get
            {
                return ((base.Flags & 0x20) == 0);
            }
            set
            {
                base.Flags = value ? (base.Flags & -33) : (base.Flags | 0x20);
            }
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
            }
        }
    }
}

