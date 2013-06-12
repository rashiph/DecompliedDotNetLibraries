namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class BrowsableAttribute : Attribute
    {
        private bool browsable = true;
        public static readonly BrowsableAttribute Default = Yes;
        public static readonly BrowsableAttribute No = new BrowsableAttribute(false);
        public static readonly BrowsableAttribute Yes = new BrowsableAttribute(true);

        public BrowsableAttribute(bool browsable)
        {
            this.browsable = browsable;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            BrowsableAttribute attribute = obj as BrowsableAttribute;
            return ((attribute != null) && (attribute.Browsable == this.browsable));
        }

        public override int GetHashCode()
        {
            return this.browsable.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public bool Browsable
        {
            get
            {
                return this.browsable;
            }
        }
    }
}

