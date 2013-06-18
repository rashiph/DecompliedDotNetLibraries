namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSPrototypeField : JSField
    {
        internal FieldInfo prototypeField;
        private object prototypeObject;
        internal object value;

        internal JSPrototypeField(object prototypeObject, FieldInfo prototypeField)
        {
            this.prototypeObject = prototypeObject;
            this.prototypeField = prototypeField;
            this.value = Microsoft.JScript.Missing.Value;
        }

        public override object GetValue(object obj)
        {
            if (this.value is Microsoft.JScript.Missing)
            {
                return this.prototypeField.GetValue(this.prototypeObject);
            }
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
                return this.prototypeField.Name;
            }
        }
    }
}

