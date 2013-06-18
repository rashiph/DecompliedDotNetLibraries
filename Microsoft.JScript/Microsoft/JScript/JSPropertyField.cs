namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSPropertyField : JSField
    {
        internal object wrappedObject;
        internal PropertyInfo wrappedProperty;

        internal JSPropertyField(PropertyInfo field, object obj)
        {
            this.wrappedProperty = field;
            this.wrappedObject = obj;
        }

        public override object GetValue(object obj)
        {
            return this.wrappedProperty.GetValue(this.wrappedObject, new object[0]);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo locale)
        {
            this.wrappedProperty.SetValue(this.wrappedObject, value, invokeAttr, binder, new object[0], locale);
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return FieldAttributes.Public;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.wrappedProperty.DeclaringType;
            }
        }

        public override Type FieldType
        {
            get
            {
                return this.wrappedProperty.PropertyType;
            }
        }

        public override string Name
        {
            get
            {
                return this.wrappedProperty.Name;
            }
        }
    }
}

