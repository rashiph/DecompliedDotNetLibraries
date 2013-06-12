namespace System.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    [ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_ParameterBuilder)), ComVisible(true)]
    public class ParameterBuilder : _ParameterBuilder
    {
        private ParameterAttributes m_attributes;
        private int m_iPosition;
        private MethodBuilder m_methodBuilder;
        private ParameterToken m_pdToken;
        private string m_strParamName;

        private ParameterBuilder()
        {
        }

        [SecurityCritical]
        internal ParameterBuilder(MethodBuilder methodBuilder, int sequence, ParameterAttributes attributes, string strParamName)
        {
            this.m_iPosition = sequence;
            this.m_strParamName = strParamName;
            this.m_methodBuilder = methodBuilder;
            this.m_strParamName = strParamName;
            this.m_attributes = attributes;
            this.m_pdToken = new ParameterToken(TypeBuilder.SetParamInfo(this.m_methodBuilder.GetModuleBuilder().GetNativeHandle(), this.m_methodBuilder.GetToken().Token, sequence, attributes, strParamName));
        }

        public virtual ParameterToken GetToken()
        {
            return this.m_pdToken;
        }

        [SecuritySafeCritical]
        public virtual void SetConstant(object defaultValue)
        {
            TypeBuilder.SetConstantValue(this.m_methodBuilder.GetModuleBuilder(), this.m_pdToken.Token, (this.m_iPosition == 0) ? this.m_methodBuilder.ReturnType : this.m_methodBuilder.m_parameterTypes[this.m_iPosition - 1], defaultValue);
        }

        [SecuritySafeCritical]
        public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            if (customBuilder == null)
            {
                throw new ArgumentNullException("customBuilder");
            }
            customBuilder.CreateCustomAttribute((ModuleBuilder) this.m_methodBuilder.GetModule(), this.m_pdToken.Token);
        }

        [SecuritySafeCritical, ComVisible(true)]
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
            TypeBuilder.DefineCustomAttribute(this.m_methodBuilder.GetModuleBuilder(), this.m_pdToken.Token, ((ModuleBuilder) this.m_methodBuilder.GetModule()).GetConstructorToken(con).Token, binaryAttribute, false, false);
        }

        [SecuritySafeCritical, Obsolete("An alternate API is available: Emit the MarshalAs custom attribute instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual void SetMarshal(UnmanagedMarshal unmanagedMarshal)
        {
            if (unmanagedMarshal == null)
            {
                throw new ArgumentNullException("unmanagedMarshal");
            }
            byte[] bytes = unmanagedMarshal.InternalGetBytes();
            TypeBuilder.SetFieldMarshal(this.m_methodBuilder.GetModuleBuilder().GetNativeHandle(), this.m_pdToken.Token, bytes, bytes.Length);
        }

        void _ParameterBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _ParameterBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _ParameterBuilder.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _ParameterBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public virtual int Attributes
        {
            get
            {
                return (int) this.m_attributes;
            }
        }

        public bool IsIn
        {
            get
            {
                return ((this.m_attributes & ParameterAttributes.In) != ParameterAttributes.None);
            }
        }

        public bool IsOptional
        {
            get
            {
                return ((this.m_attributes & ParameterAttributes.Optional) != ParameterAttributes.None);
            }
        }

        public bool IsOut
        {
            get
            {
                return ((this.m_attributes & ParameterAttributes.Out) != ParameterAttributes.None);
            }
        }

        internal int MetadataTokenInternal
        {
            get
            {
                return this.m_pdToken.Token;
            }
        }

        public virtual string Name
        {
            get
            {
                return this.m_strParamName;
            }
        }

        public virtual int Position
        {
            get
            {
                return this.m_iPosition;
            }
        }
    }
}

