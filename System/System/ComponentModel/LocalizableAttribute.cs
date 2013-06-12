namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class LocalizableAttribute : Attribute
    {
        public static readonly LocalizableAttribute Default = No;
        private bool isLocalizable;
        public static readonly LocalizableAttribute No = new LocalizableAttribute(false);
        public static readonly LocalizableAttribute Yes = new LocalizableAttribute(true);

        public LocalizableAttribute(bool isLocalizable)
        {
            this.isLocalizable = isLocalizable;
        }

        public override bool Equals(object obj)
        {
            LocalizableAttribute attribute = obj as LocalizableAttribute;
            return ((attribute != null) && (attribute.IsLocalizable == this.isLocalizable));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return (this.IsLocalizable == Default.IsLocalizable);
        }

        public bool IsLocalizable
        {
            get
            {
                return this.isLocalizable;
            }
        }
    }
}

