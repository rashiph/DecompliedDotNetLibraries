namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class AmbientValueAttribute : Attribute
    {
        private readonly object value;

        public AmbientValueAttribute(bool value)
        {
            this.value = value;
        }

        public AmbientValueAttribute(byte value)
        {
            this.value = value;
        }

        public AmbientValueAttribute(char value)
        {
            this.value = value;
        }

        public AmbientValueAttribute(double value)
        {
            this.value = value;
        }

        public AmbientValueAttribute(short value)
        {
            this.value = value;
        }

        public AmbientValueAttribute(int value)
        {
            this.value = value;
        }

        public AmbientValueAttribute(long value)
        {
            this.value = value;
        }

        public AmbientValueAttribute(object value)
        {
            this.value = value;
        }

        public AmbientValueAttribute(float value)
        {
            this.value = value;
        }

        public AmbientValueAttribute(string value)
        {
            this.value = value;
        }

        public AmbientValueAttribute(Type type, string value)
        {
            try
            {
                this.value = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
            }
            catch
            {
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            AmbientValueAttribute attribute = obj as AmbientValueAttribute;
            if (attribute == null)
            {
                return false;
            }
            if (this.value != null)
            {
                return this.value.Equals(attribute.Value);
            }
            return (attribute.Value == null);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public object Value
        {
            get
            {
                return this.value;
            }
        }
    }
}

