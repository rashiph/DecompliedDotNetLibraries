namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DefaultPropertyAttribute : Attribute
    {
        public static readonly DefaultPropertyAttribute Default = new DefaultPropertyAttribute(null);
        private readonly string name;

        public DefaultPropertyAttribute(string name)
        {
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            DefaultPropertyAttribute attribute = obj as DefaultPropertyAttribute;
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

