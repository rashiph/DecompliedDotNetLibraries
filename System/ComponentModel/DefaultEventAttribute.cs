namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DefaultEventAttribute : Attribute
    {
        public static readonly DefaultEventAttribute Default = new DefaultEventAttribute(null);
        private readonly string name;

        public DefaultEventAttribute(string name)
        {
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            DefaultEventAttribute attribute = obj as DefaultEventAttribute;
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

