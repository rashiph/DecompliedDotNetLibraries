namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class FilterableAttribute : Attribute
    {
        private bool _filterable;
        private static Hashtable _filterableTypes = Hashtable.Synchronized(new Hashtable());
        public static readonly FilterableAttribute Default = Yes;
        public static readonly FilterableAttribute No = new FilterableAttribute(false);
        public static readonly FilterableAttribute Yes = new FilterableAttribute(true);

        public FilterableAttribute(bool filterable)
        {
            this._filterable = filterable;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            FilterableAttribute attribute = obj as FilterableAttribute;
            return ((attribute != null) && (attribute.Filterable == this._filterable));
        }

        public override int GetHashCode()
        {
            return this._filterable.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public static bool IsObjectFilterable(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            return IsTypeFilterable(instance.GetType());
        }

        public static bool IsPropertyFilterable(PropertyDescriptor propertyDescriptor)
        {
            FilterableAttribute attribute = (FilterableAttribute) propertyDescriptor.Attributes[typeof(FilterableAttribute)];
            if (attribute != null)
            {
                return attribute.Filterable;
            }
            return true;
        }

        public static bool IsTypeFilterable(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            object obj2 = _filterableTypes[type];
            if (obj2 == null)
            {
                FilterableAttribute attribute = (FilterableAttribute) TypeDescriptor.GetAttributes(type)[typeof(FilterableAttribute)];
                obj2 = (attribute == null) ? ((object) 0) : ((object) attribute.Filterable);
                _filterableTypes[type] = obj2;
            }
            return (bool) obj2;
        }

        public bool Filterable
        {
            get
            {
                return this._filterable;
            }
        }
    }
}

