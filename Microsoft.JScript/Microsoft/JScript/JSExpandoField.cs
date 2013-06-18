namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSExpandoField : JSField
    {
        private string name;
        private object value;

        internal JSExpandoField(string name) : this(name, null)
        {
        }

        internal JSExpandoField(string name, object value)
        {
            this.value = value;
            this.name = name;
        }

        public override object GetValue(object obj)
        {
            return this.value;
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo locale)
        {
            this.value = value;
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return (FieldAttributes.Static | FieldAttributes.Public);
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

