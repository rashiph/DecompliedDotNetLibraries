namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class DesignOnlyAttribute : Attribute
    {
        public static readonly DesignOnlyAttribute Default = No;
        private bool isDesignOnly;
        public static readonly DesignOnlyAttribute No = new DesignOnlyAttribute(false);
        public static readonly DesignOnlyAttribute Yes = new DesignOnlyAttribute(true);

        public DesignOnlyAttribute(bool isDesignOnly)
        {
            this.isDesignOnly = isDesignOnly;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DesignOnlyAttribute attribute = obj as DesignOnlyAttribute;
            return ((attribute != null) && (attribute.isDesignOnly == this.isDesignOnly));
        }

        public override int GetHashCode()
        {
            return this.isDesignOnly.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return (this.IsDesignOnly == Default.IsDesignOnly);
        }

        public bool IsDesignOnly
        {
            get
            {
                return this.isDesignOnly;
            }
        }
    }
}

