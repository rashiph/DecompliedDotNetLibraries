namespace System.Reflection.Emit
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_PropertyBuilder)), ComVisible(true), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class PropertyBuilder : PropertyInfo, _PropertyBuilder
    {
        private PropertyAttributes m_attributes;
        private TypeBuilder m_containingType;
        private MethodInfo m_getMethod;
        private ModuleBuilder m_moduleBuilder;
        private string m_name;
        private System.Reflection.Emit.PropertyToken m_prToken;
        private Type m_returnType;
        private MethodInfo m_setMethod;
        private SignatureHelper m_signature;
        private int m_tkProperty;

        private PropertyBuilder()
        {
        }

        internal PropertyBuilder(ModuleBuilder mod, string name, SignatureHelper sig, PropertyAttributes attr, Type returnType, System.Reflection.Emit.PropertyToken prToken, TypeBuilder containingType)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if (name[0] == '\0')
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IllegalName"), "name");
            }
            this.m_name = name;
            this.m_moduleBuilder = mod;
            this.m_signature = sig;
            this.m_attributes = attr;
            this.m_returnType = returnType;
            this.m_prToken = prToken;
            this.m_tkProperty = prToken.Token;
            this.m_containingType = containingType;
        }

        [SecuritySafeCritical]
        public void AddOtherMethod(MethodBuilder mdBuilder)
        {
            this.SetMethodSemantics(mdBuilder, MethodSemanticsAttributes.Other);
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (nonPublic || (this.m_getMethod == null))
            {
                return this.m_getMethod;
            }
            if ((this.m_getMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)
            {
                return this.m_getMethod;
            }
            return null;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (nonPublic || (this.m_setMethod == null))
            {
                return this.m_setMethod;
            }
            if ((this.m_setMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)
            {
                return this.m_setMethod;
            }
            return null;
        }

        public override object GetValue(object obj, object[] index)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        [SecuritySafeCritical]
        public void SetConstant(object defaultValue)
        {
            this.m_containingType.ThrowIfCreated();
            TypeBuilder.SetConstantValue(this.m_moduleBuilder, this.m_prToken.Token, this.m_returnType, defaultValue);
        }

        [SecuritySafeCritical]
        public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            if (customBuilder == null)
            {
                throw new ArgumentNullException("customBuilder");
            }
            this.m_containingType.ThrowIfCreated();
            customBuilder.CreateCustomAttribute(this.m_moduleBuilder, this.m_prToken.Token);
        }

        [ComVisible(true), SecuritySafeCritical]
        public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
        {
            if (con == null)
            {
                throw new ArgumentNullException("con");
            }
            if (binaryAttribute == null)
            {
                throw new ArgumentNullException("binaryAttribute");
            }
            this.m_containingType.ThrowIfCreated();
            TypeBuilder.DefineCustomAttribute(this.m_moduleBuilder, this.m_prToken.Token, this.m_moduleBuilder.GetConstructorToken(con).Token, binaryAttribute, false, false);
        }

        [SecuritySafeCritical]
        public void SetGetMethod(MethodBuilder mdBuilder)
        {
            this.SetMethodSemantics(mdBuilder, MethodSemanticsAttributes.Getter);
            this.m_getMethod = mdBuilder;
        }

        [SecurityCritical]
        private void SetMethodSemantics(MethodBuilder mdBuilder, MethodSemanticsAttributes semantics)
        {
            if (mdBuilder == null)
            {
                throw new ArgumentNullException("mdBuilder");
            }
            this.m_containingType.ThrowIfCreated();
            TypeBuilder.DefineMethodSemantics(this.m_moduleBuilder.GetNativeHandle(), this.m_prToken.Token, semantics, mdBuilder.GetToken().Token);
        }

        [SecuritySafeCritical]
        public void SetSetMethod(MethodBuilder mdBuilder)
        {
            this.SetMethodSemantics(mdBuilder, MethodSemanticsAttributes.Setter);
            this.m_setMethod = mdBuilder;
        }

        public override void SetValue(object obj, object value, object[] index)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        void _PropertyBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _PropertyBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _PropertyBuilder.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _PropertyBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return this.m_attributes;
            }
        }

        public override bool CanRead
        {
            get
            {
                return (this.m_getMethod != null);
            }
        }

        public override bool CanWrite
        {
            get
            {
                return (this.m_setMethod != null);
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_containingType;
            }
        }

        internal int MetadataTokenInternal
        {
            get
            {
                return this.m_tkProperty;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_containingType.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_name;
            }
        }

        public System.Reflection.Emit.PropertyToken PropertyToken
        {
            get
            {
                return this.m_prToken;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.m_returnType;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_containingType;
            }
        }
    }
}

