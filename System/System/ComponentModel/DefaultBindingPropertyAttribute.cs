namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DefaultBindingPropertyAttribute : Attribute
    {
        public static readonly DefaultBindingPropertyAttribute Default = new DefaultBindingPropertyAttribute();
        private readonly string name;

        public DefaultBindingPropertyAttribute()
        {
            this.name = null;
        }

        public DefaultBindingPropertyAttribute(string name)
        {
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            DefaultBindingPropertyAttribute attribute = obj as DefaultBindingPropertyAttribute;
            return ((attribute != null) && (attribute.Name == this.name));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

