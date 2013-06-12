namespace System.Reflection.Emit
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class FieldOnTypeBuilderInstantiation : System.Reflection.FieldInfo
    {
        private System.Reflection.FieldInfo m_field;
        private TypeBuilderInstantiation m_type;

        internal FieldOnTypeBuilderInstantiation(System.Reflection.FieldInfo field, TypeBuilderInstantiation type)
        {
            this.m_field = field;
            this.m_type = type;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.m_field.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.m_field.GetCustomAttributes(attributeType, inherit);
        }

        internal static System.Reflection.FieldInfo GetField(System.Reflection.FieldInfo Field, TypeBuilderInstantiation type)
        {
            System.Reflection.FieldInfo info = null;
            if (type.m_hashtable.Contains(Field))
            {
                return (type.m_hashtable[Field] as System.Reflection.FieldInfo);
            }
            info = new FieldOnTypeBuilderInstantiation(Field, type);
            type.m_hashtable[Field] = info;
            return info;
        }

        public override Type[] GetOptionalCustomModifiers()
        {
            return this.m_field.GetOptionalCustomModifiers();
        }

        public override Type[] GetRequiredCustomModifiers()
        {
            return this.m_field.GetRequiredCustomModifiers();
        }

        public Type GetType()
        {
            return base.GetType();
        }

        public override object GetValue(object obj)
        {
            throw new InvalidOperationException();
        }

        public override object GetValueDirect(TypedReference obj)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.m_field.IsDefined(attributeType, inherit);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }

        public override void SetValueDirect(TypedReference obj, object value)
        {
            throw new NotImplementedException();
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return this.m_field.Attributes;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_type;
            }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal System.Reflection.FieldInfo FieldInfo
        {
            get
            {
                return this.m_field;
            }
        }

        public override Type FieldType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Field;
            }
        }

        internal int MetadataTokenInternal
        {
            get
            {
                FieldBuilder field = this.m_field as FieldBuilder;
                if (field != null)
                {
                    return field.MetadataTokenInternal;
                }
                return this.m_field.MetadataToken;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_field.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_field.Name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_type;
            }
        }
    }
}

