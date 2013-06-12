namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public sealed class DesignTimeVisibleAttribute : Attribute
    {
        public static readonly DesignTimeVisibleAttribute Default = Yes;
        public static readonly DesignTimeVisibleAttribute No = new DesignTimeVisibleAttribute(false);
        private bool visible;
        public static readonly DesignTimeVisibleAttribute Yes = new DesignTimeVisibleAttribute(true);

        public DesignTimeVisibleAttribute()
        {
        }

        public DesignTimeVisibleAttribute(bool visible)
        {
            this.visible = visible;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DesignTimeVisibleAttribute attribute = obj as DesignTimeVisibleAttribute;
            return ((attribute != null) && (attribute.Visible == this.visible));
        }

        public override int GetHashCode()
        {
            return (typeof(DesignTimeVisibleAttribute).GetHashCode() ^ (this.visible ? -1 : 0));
        }

        public override bool IsDefaultAttribute()
        {
            return (this.Visible == Default.Visible);
        }

        public bool Visible
        {
            get
            {
                return this.visible;
            }
        }
    }
}

