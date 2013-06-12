namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;

    [Serializable]
    internal struct CustomAttributeEncodedArgument
    {
        private CustomAttributeEncodedArgument[] m_arrayValue;
        private long m_primitiveValue;
        private string m_stringValue;
        private System.Reflection.CustomAttributeType m_type;

        [SecurityCritical]
        internal static void ParseAttributeArguments(ConstArray attributeBlob, ref CustomAttributeCtorParameter[] customAttributeCtorParameters, ref CustomAttributeNamedParameter[] customAttributeNamedParameters, RuntimeModule customAttributeModule)
        {
            if (customAttributeModule == null)
            {
                throw new ArgumentNullException("customAttributeModule");
            }
            if ((customAttributeCtorParameters.Length != 0) || (customAttributeNamedParameters.Length != 0))
            {
                ParseAttributeArguments(attributeBlob.Signature, attributeBlob.Length, ref customAttributeCtorParameters, ref customAttributeNamedParameters, (RuntimeAssembly) customAttributeModule.Assembly);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void ParseAttributeArguments(IntPtr pCa, int cCa, ref CustomAttributeCtorParameter[] CustomAttributeCtorParameters, ref CustomAttributeNamedParameter[] CustomAttributeTypedArgument, RuntimeAssembly assembly);

        public CustomAttributeEncodedArgument[] ArrayValue
        {
            get
            {
                return this.m_arrayValue;
            }
        }

        public System.Reflection.CustomAttributeType CustomAttributeType
        {
            get
            {
                return this.m_type;
            }
        }

        public long PrimitiveValue
        {
            get
            {
                return this.m_primitiveValue;
            }
        }

        public string StringValue
        {
            get
            {
                return this.m_stringValue;
            }
        }
    }
}

