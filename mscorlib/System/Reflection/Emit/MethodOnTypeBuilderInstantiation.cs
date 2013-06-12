namespace System.Reflection.Emit
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class MethodOnTypeBuilderInstantiation : MethodInfo
    {
        internal MethodInfo m_method;
        private TypeBuilderInstantiation m_type;

        internal MethodOnTypeBuilderInstantiation(MethodInfo method, TypeBuilderInstantiation type)
        {
            this.m_method = method;
            this.m_type = type;
        }

        public override MethodInfo GetBaseDefinition()
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.m_method.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.m_method.GetCustomAttributes(attributeType, inherit);
        }

        public override Type[] GetGenericArguments()
        {
            return this.m_method.GetGenericArguments();
        }

        public override MethodInfo GetGenericMethodDefinition()
        {
            return this.m_method;
        }

        internal static MethodInfo GetMethod(MethodInfo method, TypeBuilderInstantiation type)
        {
            return new MethodOnTypeBuilderInstantiation(method, type);
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return this.m_method.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            return this.m_method.GetParameters();
        }

        internal override Type[] GetParameterTypes()
        {
            return this.m_method.GetParameterTypes();
        }

        public Type GetType()
        {
            return base.GetType();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.m_method.IsDefined(attributeType, inherit);
        }

        public override MethodInfo MakeGenericMethod(params Type[] typeArgs)
        {
            if (!this.IsGenericMethodDefinition)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericMethodDefinition"));
            }
            return MethodBuilderInstantiation.MakeGenericMethod(this, typeArgs);
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return this.m_method.Attributes;
            }
        }

        public override CallingConventions CallingConvention
        {
            get
            {
                return this.m_method.CallingConvention;
            }
        }

        public override bool ContainsGenericParameters
        {
            get
            {
                return this.m_method.ContainsGenericParameters;
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
                return this.m_method.IsGenericMethod;
            }
        }

        public override bool IsGenericMethodDefinition
        {
            get
            {
                return this.m_method.IsGenericMethodDefinition;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return this.m_method.MemberType;
            }
        }

        internal int MetadataTokenInternal
        {
            get
            {
                MethodBuilder method = this.m_method as MethodBuilder;
                if (method != null)
                {
                    return method.MetadataTokenInternal;
                }
                return this.m_method.MetadataToken;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                return this.m_method.MethodHandle;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_method.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_method.Name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_type;
            }
        }

        public override ParameterInfo ReturnParameter
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override Type ReturnType
        {
            get
            {
                return this.m_method.ReturnType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}

