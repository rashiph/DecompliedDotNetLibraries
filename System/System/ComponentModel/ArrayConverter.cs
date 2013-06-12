namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class ArrayConverter : CollectionConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(string)) && (value is Array))
            {
                return SR.GetString("ArrayConverterText", new object[] { value.GetType().Name });
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptor[] properties = null;
            if (value.GetType().IsArray)
            {
                int length = ((Array) value).GetLength(0);
                properties = new PropertyDescriptor[length];
                Type arrayType = value.GetType();
                Type elementType = arrayType.GetElementType();
                for (int i = 0; i < length; i++)
                {
                    properties[i] = new ArrayPropertyDescriptor(arrayType, elementType, i);
                }
            }
            return new PropertyDescriptorCollection(properties);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private class ArrayPropertyDescriptor : TypeConverter.SimplePropertyDescriptor
        {
            private int index;

            public ArrayPropertyDescriptor(Type arrayType, Type elementType, int index) : base(arrayType, "[" + index + "]", elementType, null)
            {
                this.index = index;
            }

            public override object GetValue(object instance)
            {
                if (instance is Array)
                {
                    Array array = (Array) instance;
                    if (array.GetLength(0) > this.index)
                    {
                        return array.GetValue(this.index);
                    }
                }
                return null;
            }

            public override void SetValue(object instance, object value)
            {
                if (instance is Array)
                {
                    Array array = (Array) instance;
                    if (array.GetLength(0) > this.index)
                    {
                        array.SetValue(value, this.index);
                    }
                    this.OnValueChanged(instance, EventArgs.Empty);
                }
            }
        }
    }
}

