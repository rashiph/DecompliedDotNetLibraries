namespace System.Reflection.Emit
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class MethodBuilderInstantiation : MethodInfo
    {
        private Type[] m_inst;
        internal MethodInfo m_method;

        internal MethodBuilderInstantiation(MethodInfo method, Type[] inst)
        {
            this.m_method = method;
            this.m_inst = inst;
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
            return this.m_inst;
        }

        public override MethodInfo GetGenericMethodDefinition()
        {
            return this.m_method;
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return this.m_method.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            throw new NotSupportedException();
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

        public override MethodInfo MakeGenericMethod(params Type[] arguments)
        {
            throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericMethodDefinition"));
        }

        internal static MethodInfo MakeGenericMethod(MethodInfo method, Type[] inst)
        {
            if (!method.IsGenericMethodDefinition)
            {
                throw new InvalidOperationException();
            }
            return new MethodBuilderInstantiation(method, inst);
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
                for (int i = 0; i < this.m_inst.Length; i++)
                {
                    if (this.m_inst[i].ContainsGenericParameters)
                    {
                        return true;
                    }
                }
                return ((this.DeclaringType != null) && this.DeclaringType.ContainsGenericParameters);
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_method.DeclaringType;
            }
        }

        public override bool IsGenericMethod
        {
            get
            {
                return true;
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
                return this.m_method.MemberType;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
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
                return this.m_method.ReflectedType;
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

