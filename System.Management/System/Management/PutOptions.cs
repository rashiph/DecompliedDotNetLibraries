namespace System.Management
{
    using System;

    public class PutOptions : ManagementOptions
    {
        public PutOptions() : this(null, ManagementOptions.InfiniteTimeout, false, PutType.UpdateOrCreate)
        {
        }

        public PutOptions(ManagementNamedValueCollection context) : this(context, ManagementOptions.InfiniteTimeout, false, PutType.UpdateOrCreate)
        {
        }

        public PutOptions(ManagementNamedValueCollection context, TimeSpan timeout, bool useAmendedQualifiers, PutType putType) : base(context, timeout)
        {
            this.UseAmendedQualifiers = useAmendedQualifiers;
            this.Type = putType;
        }

        public override object Clone()
        {
            ManagementNamedValueCollection context = null;
            if (base.Context != null)
            {
                context = base.Context.Clone();
            }
            return new PutOptions(context, base.Timeout, this.UseAmendedQualifiers, this.Type);
        }

        public PutType Type
        {
            get
            {
                if ((base.Flags & 1) != 0)
                {
                    return PutType.UpdateOnly;
                }
                if ((base.Flags & 2) == 0)
                {
                    return PutType.UpdateOrCreate;
                }
                return PutType.CreateOnly;
            }
            set
            {
                switch (value)
                {
                    case PutType.UpdateOnly:
                        base.Flags |= 1;
                        return;

                    case PutType.CreateOnly:
                        base.Flags |= 2;
                        return;

                    case PutType.UpdateOrCreate:
                        base.Flags = base.Flags;
                        return;
                }
                throw new ArgumentException(null, "Type");
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

