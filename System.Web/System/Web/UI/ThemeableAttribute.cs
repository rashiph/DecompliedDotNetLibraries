namespace System.Web.UI
{
    using System;
    using System.Collections;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class ThemeableAttribute : Attribute
    {
        private bool _themeable;
        private static Hashtable _themeableTypes = Hashtable.Synchronized(new Hashtable());
        public static readonly ThemeableAttribute Default = Yes;
        public static readonly ThemeableAttribute No = new ThemeableAttribute(false);
        public static readonly ThemeableAttribute Yes = new ThemeableAttribute(true);

        public ThemeableAttribute(bool themeable)
        {
            this._themeable = themeable;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            ThemeableAttribute attribute = obj as ThemeableAttribute;
            return ((attribute != null) && (attribute.Themeable == this._themeable));
        }

        public override int GetHashCode()
        {
            return this._themeable.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public static bool IsObjectThemeable(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            return IsTypeThemeable(instance.GetType());
        }

        public static bool IsTypeThemeable(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            object obj2 = _themeableTypes[type];
            if (obj2 == null)
            {
                ThemeableAttribute customAttribute = Attribute.GetCustomAttribute(type, typeof(ThemeableAttribute)) as ThemeableAttribute;
                obj2 = (customAttribute == null) ? ((object) 0) : ((object) customAttribute.Themeable);
                _themeableTypes[type] = obj2;
            }
            return (bool) obj2;
        }

        public bool Themeable
        {
            get
            {
                return this._themeable;
            }
        }
    }
}

