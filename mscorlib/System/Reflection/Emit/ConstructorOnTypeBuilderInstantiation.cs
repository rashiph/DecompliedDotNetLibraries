namespace System.Reflection.Emit
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class ConstructorOnTypeBuilderInstantiation : ConstructorInfo
    {
        internal ConstructorInfo m_ctor;
        private TypeBuilderInstantiation m_type;

        internal ConstructorOnTypeBuilderInstantiation(ConstructorInfo constructor, TypeBuilderInstantiation type)
        {
            this.m_ctor = constructor;
            this.m_type = type;
        }

        internal static ConstructorInfo GetConstructor(ConstructorInfo Constructor, TypeBuilderInstantiation type)
        {
            return new ConstructorOnTypeBuilderInstantiation(Constructor, type);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.m_ctor.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.m_ctor.GetCustomAttributes(attributeType, inherit);
        }

        public override Type[] GetGenericArguments()
        {
            return this.m_ctor.GetGenericArguments();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return this.m_ctor.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            return this.m_ctor.GetParameters();
        }

        internal override Type[] GetParameterTypes()
        {
            return this.m_ctor.GetParameterTypes();
        }

        internal override Type GetReturnType()
        {
            return this.DeclaringType;
        }

        public Type GetType()
        {
            return base.GetType();
        }

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.m_ctor.IsDefined(attributeType, inherit);
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return this.m_ctor.Attributes;
            }
        }

        public override CallingConventions CallingConvention
        {
            get
            {
                return this.m_ctor.CallingConvention;
            }
        }

        public override bool ContainsGenericParameters
        {
            get
            {
                return false;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_type;
            }
        }

        public override bool IsGenericMethod
        {
            get
            {
                return false;
            }
        }

        public override bool IsGenericMethodDefinition
        {
            get
            {
                return false;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return this.m_ctor.MemberType;
            }
        }

        internal int MetadataTokenInternal
        {
            get
            {
                ConstructorBuilder ctor = this.m_ctor as ConstructorBuilder;
                if (ctor != null)
                {
                    return ctor.MetadataTokenInternal;
                }
                return this.m_ctor.MetadataToken;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                return this.m_ctor.MethodHandle;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_ctor.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_ctor.Name;
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

