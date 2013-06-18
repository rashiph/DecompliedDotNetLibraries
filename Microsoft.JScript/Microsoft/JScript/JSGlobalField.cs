namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSGlobalField : JSVariableField
    {
        internal FieldInfo ILField;

        internal JSGlobalField(ScriptObject obj, string name, object value, FieldAttributes attributeFlags) : base(name, obj, attributeFlags)
        {
            base.value = value;
            this.ILField = null;
        }

        public override object GetValue(object obj)
        {
            if (this.ILField == null)
            {
                return base.value;
            }
            return this.ILField.GetValue(null);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            if (this.ILField != null)
            {
                this.ILField.SetValue(null, value, invokeAttr, binder, culture);
            }
            else if ((base.IsLiteral || base.IsInitOnly) && !(base.value is Microsoft.JScript.Missing))
            {
                if ((!(base.value is FunctionObject) || !(value is FunctionObject)) || !this.Name.Equals(((FunctionObject) value).name))
                {
                    throw new JScriptException(JSError.AssignmentToReadOnly);
                }
                base.value = value;
            }
            else if (base.type != null)
            {
                base.value = Microsoft.JScript.Convert.Coerce(value, base.type);
            }
            else
            {
                base.value = value;
            }
        }
    }
}

