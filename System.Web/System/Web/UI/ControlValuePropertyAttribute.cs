namespace System.Web.UI
{
    using System;
    using System.ComponentModel;
    using System.Web.Util;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ControlValuePropertyAttribute : Attribute
    {
        private readonly object _defaultValue;
        private readonly string _name;

        public ControlValuePropertyAttribute(string name)
        {
            this._name = name;
        }

        public ControlValuePropertyAttribute(string name, object defaultValue)
        {
            this._name = name;
            this._defaultValue = defaultValue;
        }

        public ControlValuePropertyAttribute(string name, Type type, string defaultValue)
        {
            this._name = name;
            try
            {
                this._defaultValue = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(defaultValue);
            }
            catch
            {
            }
        }

        public override bool Equals(object obj)
        {
            ControlValuePropertyAttribute attribute = obj as ControlValuePropertyAttribute;
            if ((attribute == null) || !string.Equals(this._name, attribute.Name, StringComparison.Ordinal))
            {
                return false;
            }
            if (this._defaultValue != null)
            {
                return this._defaultValue.Equals(attribute.DefaultValue);
            }
            return (attribute.DefaultValue == null);
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes((this.Name != null) ? this.Name.GetHashCode() : 0, (this.DefaultValue != null) ? this.DefaultValue.GetHashCode() : 0);
        }

        public object DefaultValue
        {
            get
            {
                return this._defaultValue;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

