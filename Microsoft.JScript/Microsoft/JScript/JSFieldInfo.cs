namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    public sealed class JSFieldInfo : FieldInfo
    {
        private FieldAttributes attributes;
        private Type declaringType;
        internal FieldInfo field;
        private FieldAccessor fieldAccessor;
        private Type fieldType;

        internal JSFieldInfo(FieldInfo field)
        {
            this.field = field;
            this.attributes = field.Attributes;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new FieldInfo[0];
        }

        public override object[] GetCustomAttributes(Type t, bool inherit)
        {
            return new FieldInfo[0];
        }

        public override object GetValue(object obj)
        {
            FieldAccessor fieldAccessor = this.fieldAccessor;
            if (fieldAccessor == null)
            {
                this.fieldAccessor = fieldAccessor = FieldAccessor.GetAccessorFor(TypeReferences.ToExecutionContext(this.field));
            }
            return fieldAccessor.GetValue(obj);
        }

        public override bool IsDefined(Type type, bool inherit)
        {
            return false;
        }

        public void SetValue(object obj, object value)
        {
            if ((this.attributes & FieldAttributes.InitOnly) != FieldAttributes.PrivateScope)
            {
                throw new JScriptException(JSError.AssignmentToReadOnly);
            }
            this.SetValue(obj, value, BindingFlags.SetField, null, null);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            FieldAccessor fieldAccessor = this.fieldAccessor;
            if (fieldAccessor == null)
            {
                this.fieldAccessor = fieldAccessor = FieldAccessor.GetAccessorFor(this.field);
            }
            fieldAccessor.SetValue(obj, value);
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                Type declaringType = this.declaringType;
                if (declaringType == null)
                {
                    this.declaringType = declaringType = this.field.DeclaringType;
                }
                return declaringType;
            }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get
            {
                return this.field.FieldHandle;
            }
        }

        public override Type FieldType
        {
            get
            {
                Type fieldType = this.fieldType;
                if (fieldType == null)
                {
                    this.fieldType = fieldType = this.field.FieldType;
                }
                return fieldType;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Field;
            }
        }

        public override string Name
        {
            get
            {
                return this.field.Name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.field.ReflectedType;
            }
        }
    }
}

