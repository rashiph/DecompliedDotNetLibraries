namespace System.Reflection.Emit
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Security;

    internal sealed class SymbolMethod : MethodInfo
    {
        private CallingConventions m_callingConvention;
        private Type m_containingType;
        private MethodToken m_mdMethod;
        private ModuleBuilder m_module;
        private string m_name;
        private Type[] m_parameterTypes;
        private Type m_returnType;
        private SignatureHelper m_signature;

        [SecurityCritical]
        internal SymbolMethod(ModuleBuilder mod, MethodToken token, Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
        {
            this.m_mdMethod = token;
            this.m_returnType = returnType;
            if (parameterTypes != null)
            {
                this.m_parameterTypes = new Type[parameterTypes.Length];
                Array.Copy(parameterTypes, this.m_parameterTypes, parameterTypes.Length);
            }
            else
            {
                this.m_parameterTypes = new Type[0];
            }
            this.m_module = mod;
            this.m_containingType = arrayClass;
            this.m_name = methodName;
            this.m_callingConvention = callingConvention;
            this.m_signature = SignatureHelper.GetMethodSigHelper(mod, callingConvention, returnType, null, null, parameterTypes, null, null);
        }

        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SymbolMethod"));
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SymbolMethod"));
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SymbolMethod"));
        }

        public System.Reflection.Module GetModule()
        {
            return this.m_module;
        }

        public override ParameterInfo[] GetParameters()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SymbolMethod"));
        }

        internal override Type[] GetParameterTypes()
        {
            return this.m_parameterTypes;
        }

        public MethodToken GetToken()
        {
            return this.m_mdMethod;
        }

        internal MethodToken GetToken(ModuleBuilder mod)
        {
            return mod.GetArrayMethodToken(this.m_containingType, this.m_name, this.m_callingConvention, this.m_returnType, this.m_parameterTypes);
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SymbolMethod"));
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SymbolMethod"));
        }

        public override MethodAttributes Attributes
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SymbolMethod"));
            }
        }

        public override CallingConventions CallingConvention
        {
            get
            {
                return this.m_callingConvention;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_containingType;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SymbolMethod"));
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_module;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_containingType;
            }
        }

        public override Type ReturnType
        {
            get
            {
                return this.m_returnType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return null;
            }
        }
    }
}

