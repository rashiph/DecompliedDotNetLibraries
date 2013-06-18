namespace Microsoft.JScript
{
    using System;

    internal sealed class MetadataEnumValue : EnumWrapper
    {
        private Type _type;
        private object _value;

        private MetadataEnumValue(Type type, object value)
        {
            this._type = type;
            this._value = value;
        }

        internal static object GetEnumValue(Type type, object value)
        {
            if (!type.Assembly.ReflectionOnly)
            {
                return Enum.ToObject(type, value);
            }
            return new MetadataEnumValue(type, value);
        }

        internal override string name
        {
            get
            {
                string name = Enum.GetName(this._type, this._value);
                if (name == null)
                {
                    name = this._value.ToString();
                }
                return name;
            }
        }

        internal override Type type
        {
            get
            {
                return this._type;
            }
        }

        internal override object value
        {
            get
            {
                return this._value;
            }
        }
    }
}

