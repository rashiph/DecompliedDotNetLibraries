namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class HtmlControlPersistableAttribute : Attribute
    {
        internal static readonly HtmlControlPersistableAttribute Default = Yes;
        internal static readonly HtmlControlPersistableAttribute No = new HtmlControlPersistableAttribute(false);
        private bool persistable = true;
        internal static readonly HtmlControlPersistableAttribute Yes = new HtmlControlPersistableAttribute(true);

        internal HtmlControlPersistableAttribute(bool persistable)
        {
            this.persistable = persistable;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            HtmlControlPersistableAttribute attribute = obj as HtmlControlPersistableAttribute;
            return ((attribute != null) && (attribute.HtmlControlPersistable == this.persistable));
        }

        public override int GetHashCode()
        {
            return this.persistable.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        internal bool HtmlControlPersistable
        {
            get
            {
                return this.persistable;
            }
        }
    }
}

