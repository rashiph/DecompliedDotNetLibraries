namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotifyParentPropertyAttribute : Attribute
    {
        public static readonly NotifyParentPropertyAttribute Default = No;
        public static readonly NotifyParentPropertyAttribute No = new NotifyParentPropertyAttribute(false);
        private bool notifyParent;
        public static readonly NotifyParentPropertyAttribute Yes = new NotifyParentPropertyAttribute(true);

        public NotifyParentPropertyAttribute(bool notifyParent)
        {
            this.notifyParent = notifyParent;
        }

        public override bool Equals(object obj)
        {
            return ((obj == this) || (((obj != null) && (obj is NotifyParentPropertyAttribute)) && (((NotifyParentPropertyAttribute) obj).NotifyParent == this.notifyParent)));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public bool NotifyParent
        {
            get
            {
                return this.notifyParent;
            }
        }
    }
}

