namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSWrappedField : JSField, IWrappedMember
    {
        internal FieldInfo wrappedField;
        internal object wrappedObject;

        internal JSWrappedField(FieldInfo field, object obj)
        {
            if (field is JSFieldInfo)
            {
                field = ((JSFieldInfo) field).field;
            }
            this.wrappedField = field;
            this.wrappedObject = obj;
            if ((obj is JSObject) && !Typeob.JSObject.IsAssignableFrom(field.DeclaringType))
            {
                if (obj is BooleanObject)
                {
                    this.wrappedObject = ((BooleanObject) obj).value;
                }
                else if (obj is NumberObject)
                {
                    this.wrappedObject = ((NumberObject) obj).value;
                }
                else if (obj is StringObject)
                {
                    this.wrappedObject = ((StringObject) obj).value;
                }
            }
        }

        internal override string GetClassFullName()
        {
            if (this.wrappedField is JSField)
            {
                return ((JSField) this.wrappedField).GetClassFullName();
            }
            return this.wrappedField.DeclaringType.FullName;
        }

        internal override object GetMetaData()
        {
            if (this.wrappedField is JSField)
            {
                return ((JSField) this.wrappedField).GetMetaData();
            }
            return this.wrappedField;
        }

        internal override PackageScope GetPackage()
        {
            if (this.wrappedField is JSField)
            {
                return ((JSField) this.wrappedField).GetPackage();
            }
            return null;
        }

        public override object GetValue(object obj)
        {
            return this.wrappedField.GetValue(this.wrappedObject);
        }

        public object GetWrappedObject()
        {
            return this.wrappedObject;
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo locale)
        {
            this.wrappedField.SetValue(this.wrappedObject, value, invokeAttr, binder, locale);
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return this.wrappedField.Attributes;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.wrappedField.DeclaringType;
            }
        }

        public override Type FieldType
        {
            get
            {
                return this.wrappedField.FieldType;
            }
        }

        public override string Name
        {
            get
            {
                return this.wrappedField.Name;
            }
        }
    }
}

