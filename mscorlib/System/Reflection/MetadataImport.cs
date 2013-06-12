namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MetadataImport
    {
        private IntPtr m_metadataImport2;
        private object m_keepalive;
        internal static readonly MetadataImport EmptyImport;
        internal static Guid IID_IMetaDataImport;
        internal static Guid IID_IMetaDataAssemblyImport;
        internal static Guid IID_IMetaDataTables;
        public override int GetHashCode()
        {
            return ValueType.GetHashCodeOfPtr(this.m_metadataImport2);
        }

        public override bool Equals(object obj)
        {
            return ((obj is MetadataImport) && this.Equals((MetadataImport) obj));
        }

        [SecuritySafeCritical]
        private bool Equals(MetadataImport import)
        {
            return (import.m_metadataImport2 == this.m_metadataImport2);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetMarshalAs(IntPtr pNativeType, int cNativeType, out int unmanagedType, out int safeArraySubType, out string safeArrayUserDefinedSubType, out int arraySubType, out int sizeParamIndex, out int sizeConst, out string marshalType, out string marshalCookie, out int iidParamIndex);
        [SecurityCritical]
        internal static void GetMarshalAs(ConstArray nativeType, out UnmanagedType unmanagedType, out VarEnum safeArraySubType, out string safeArrayUserDefinedSubType, out UnmanagedType arraySubType, out int sizeParamIndex, out int sizeConst, out string marshalType, out string marshalCookie, out int iidParamIndex)
        {
            int num;
            int num2;
            int num3;
            _GetMarshalAs(nativeType.Signature, nativeType.Length, out num, out num2, out safeArrayUserDefinedSubType, out num3, out sizeParamIndex, out sizeConst, out marshalType, out marshalCookie, out iidParamIndex);
            unmanagedType = (UnmanagedType) num;
            safeArraySubType = (VarEnum) num2;
            arraySubType = (UnmanagedType) num3;
        }

        internal static void ThrowError(int hResult)
        {
            throw new MetadataException(hResult);
        }

        internal MetadataImport(IntPtr metadataImport2, object keepalive)
        {
            this.m_metadataImport2 = metadataImport2;
            this.m_keepalive = keepalive;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void _Enum(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int type, int parent, int* result, int count);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _EnumCount(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int type, int parent, out int count);
        [SecurityCritical]
        public unsafe void Enum(int type, int parent, int* result, int count)
        {
            _Enum(this.m_metadataImport2, out MetadataArgs.Skip, type, parent, result, count);
        }

        [SecurityCritical]
        public int EnumCount(int type, int parent)
        {
            int count = 0;
            _EnumCount(this.m_metadataImport2, out MetadataArgs.Skip, type, parent, out count);
            return count;
        }

        [SecurityCritical]
        public unsafe void EnumNestedTypes(int mdTypeDef, int* result, int count)
        {
            this.Enum(0x2000000, mdTypeDef, result, count);
        }

        [SecurityCritical]
        public int EnumNestedTypesCount(int mdTypeDef)
        {
            return this.EnumCount(0x2000000, mdTypeDef);
        }

        [SecurityCritical]
        public unsafe void EnumCustomAttributes(int mdToken, int* result, int count)
        {
            this.Enum(0xc000000, mdToken, result, count);
        }

        [SecurityCritical]
        public int EnumCustomAttributesCount(int mdToken)
        {
            return this.EnumCount(0xc000000, mdToken);
        }

        [SecurityCritical]
        public unsafe void EnumParams(int mdMethodDef, int* result, int count)
        {
            this.Enum(0x8000000, mdMethodDef, result, count);
        }

        [SecurityCritical]
        public int EnumParamsCount(int mdMethodDef)
        {
            return this.EnumCount(0x8000000, mdMethodDef);
        }

        [SecurityCritical]
        public unsafe void GetAssociates(int mdPropEvent, AssociateRecord* result, int count)
        {
            int* numPtr = (int*) stackalloc byte[(((IntPtr) (count * 2)) * 4)];
            this.Enum(0x6000000, mdPropEvent, numPtr, count);
            for (int i = 0; i < count; i++)
            {
                result[i].MethodDefToken = numPtr[i * 2];
                result[i].Semantics = *((MethodSemanticsAttributes*) (numPtr + ((i * 2) + 1)));
            }
        }

        [SecurityCritical]
        public int GetAssociatesCount(int mdPropEvent)
        {
            return this.EnumCount(0x6000000, mdPropEvent);
        }

        [SecurityCritical]
        public unsafe void EnumFields(int mdTypeDef, int* result, int count)
        {
            this.Enum(0x4000000, mdTypeDef, result, count);
        }

        [SecurityCritical]
        public int EnumFieldsCount(int mdTypeDef)
        {
            return this.EnumCount(0x4000000, mdTypeDef);
        }

        [SecurityCritical]
        public unsafe void EnumProperties(int mdTypeDef, int* result, int count)
        {
            this.Enum(0x17000000, mdTypeDef, result, count);
        }

        [SecurityCritical]
        public int EnumPropertiesCount(int mdTypeDef)
        {
            return this.EnumCount(0x17000000, mdTypeDef);
        }

        [SecurityCritical]
        public unsafe void EnumEvents(int mdTypeDef, int* result, int count)
        {
            this.Enum(0x14000000, mdTypeDef, result, count);
        }

        [SecurityCritical]
        public int EnumEventsCount(int mdTypeDef)
        {
            return this.EnumCount(0x14000000, mdTypeDef);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern string _GetDefaultValue(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, out long value, out int length, out int corElementType);
        [SecurityCritical]
        public string GetDefaultValue(int mdToken, out long value, out int length, out CorElementType corElementType)
        {
            int num;
            string str = _GetDefaultValue(this.m_metadataImport2, out MetadataArgs.Skip, mdToken, out value, out length, out num);
            corElementType = (CorElementType) ((byte) num);
            return str;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void _GetUserString(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, void** name, out int length);
        [SecurityCritical]
        public unsafe string GetUserString(int mdToken)
        {
            void* voidPtr;
            int num;
            _GetUserString(this.m_metadataImport2, out MetadataArgs.Skip, mdToken, &voidPtr, out num);
            if (voidPtr == null)
            {
                return null;
            }
            char[] chArray = new char[num];
            for (int i = 0; i < num; i++)
            {
                chArray[i] = *((char*) (voidPtr + (i * 2)));
            }
            return new string(chArray);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void _GetName(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, void** name);
        [SecurityCritical]
        public unsafe Utf8String GetName(int mdToken)
        {
            void* voidPtr;
            _GetName(this.m_metadataImport2, out MetadataArgs.Skip, mdToken, &voidPtr);
            return new Utf8String(voidPtr);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void _GetNamespace(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, void** namesp);
        [SecurityCritical]
        public unsafe Utf8String GetNamespace(int mdToken)
        {
            void* voidPtr;
            _GetNamespace(this.m_metadataImport2, out MetadataArgs.Skip, mdToken, &voidPtr);
            return new Utf8String(voidPtr);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void _GetEventProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, void** name, out int eventAttributes);
        [SecurityCritical]
        public unsafe void GetEventProps(int mdToken, out void* name, out EventAttributes eventAttributes)
        {
            int num;
            void* voidPtr;
            _GetEventProps(this.m_metadataImport2, out MetadataArgs.Skip, mdToken, &voidPtr, out num);
            name = voidPtr;
            eventAttributes = (EventAttributes) num;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetFieldDefProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, out int fieldAttributes);
        [SecurityCritical]
        public void GetFieldDefProps(int mdToken, out FieldAttributes fieldAttributes)
        {
            int num;
            _GetFieldDefProps(this.m_metadataImport2, out MetadataArgs.Skip, mdToken, out num);
            fieldAttributes = (FieldAttributes) num;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void _GetPropertyProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, void** name, out int propertyAttributes, out ConstArray signature);
        [SecurityCritical]
        public unsafe void GetPropertyProps(int mdToken, out void* name, out PropertyAttributes propertyAttributes, out ConstArray signature)
        {
            int num;
            void* voidPtr;
            _GetPropertyProps(this.m_metadataImport2, out MetadataArgs.Skip, mdToken, &voidPtr, out num, out signature);
            name = voidPtr;
            propertyAttributes = (PropertyAttributes) num;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetParentToken(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, out int tkParent);
        [SecurityCritical]
        public int GetParentToken(int tkToken)
        {
            int num;
            _GetParentToken(this.m_metadataImport2, out MetadataArgs.Skip, tkToken, out num);
            return num;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetParamDefProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int parameterToken, out int sequence, out int attributes);
        [SecurityCritical]
        public void GetParamDefProps(int parameterToken, out int sequence, out ParameterAttributes attributes)
        {
            int num;
            _GetParamDefProps(this.m_metadataImport2, out MetadataArgs.Skip, parameterToken, out sequence, out num);
            attributes = (ParameterAttributes) num;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetGenericParamProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int genericParameter, out int flags);
        [SecurityCritical]
        public void GetGenericParamProps(int genericParameter, out GenericParameterAttributes attributes)
        {
            int num;
            _GetGenericParamProps(this.m_metadataImport2, out MetadataArgs.Skip, genericParameter, out num);
            attributes = (GenericParameterAttributes) num;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetScopeProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, out Guid mvid);
        [SecurityCritical]
        public void GetScopeProps(out Guid mvid)
        {
            _GetScopeProps(this.m_metadataImport2, out MetadataArgs.Skip, out mvid);
        }

        [SecurityCritical]
        public ConstArray GetMethodSignature(MetadataToken token)
        {
            if (token.IsMemberRef)
            {
                return this.GetMemberRefProps((int) token);
            }
            return this.GetSigOfMethodDef((int) token);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetSigOfMethodDef(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int methodToken, ref ConstArray signature);
        [SecurityCritical]
        public ConstArray GetSigOfMethodDef(int methodToken)
        {
            ConstArray signature = new ConstArray();
            _GetSigOfMethodDef(this.m_metadataImport2, out MetadataArgs.Skip, methodToken, ref signature);
            return signature;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetSignatureFromToken(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int methodToken, ref ConstArray signature);
        [SecurityCritical]
        public ConstArray GetSignatureFromToken(int token)
        {
            ConstArray signature = new ConstArray();
            _GetSignatureFromToken(this.m_metadataImport2, out MetadataArgs.Skip, token, ref signature);
            return signature;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetMemberRefProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int memberTokenRef, out ConstArray signature);
        [SecurityCritical]
        public ConstArray GetMemberRefProps(int memberTokenRef)
        {
            ConstArray signature = new ConstArray();
            _GetMemberRefProps(this.m_metadataImport2, out MetadataArgs.Skip, memberTokenRef, out signature);
            return signature;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetCustomAttributeProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int customAttributeToken, out int constructorToken, out ConstArray signature);
        [SecurityCritical]
        public void GetCustomAttributeProps(int customAttributeToken, out int constructorToken, out ConstArray signature)
        {
            _GetCustomAttributeProps(this.m_metadataImport2, out MetadataArgs.Skip, customAttributeToken, out constructorToken, out signature);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetClassLayout(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int typeTokenDef, out int packSize, out int classSize);
        [SecurityCritical]
        public void GetClassLayout(int typeTokenDef, out int packSize, out int classSize)
        {
            _GetClassLayout(this.m_metadataImport2, out MetadataArgs.Skip, typeTokenDef, out packSize, out classSize);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool _GetFieldOffset(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int typeTokenDef, int fieldTokenDef, out int offset);
        [SecurityCritical]
        public bool GetFieldOffset(int typeTokenDef, int fieldTokenDef, out int offset)
        {
            return _GetFieldOffset(this.m_metadataImport2, out MetadataArgs.Skip, typeTokenDef, fieldTokenDef, out offset);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetSigOfFieldDef(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int fieldToken, ref ConstArray fieldMarshal);
        [SecurityCritical]
        public ConstArray GetSigOfFieldDef(int fieldToken)
        {
            ConstArray fieldMarshal = new ConstArray();
            _GetSigOfFieldDef(this.m_metadataImport2, out MetadataArgs.Skip, fieldToken, ref fieldMarshal);
            return fieldMarshal;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _GetFieldMarshal(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int fieldToken, ref ConstArray fieldMarshal);
        [SecurityCritical]
        public ConstArray GetFieldMarshal(int fieldToken)
        {
            ConstArray fieldMarshal = new ConstArray();
            _GetFieldMarshal(this.m_metadataImport2, out MetadataArgs.Skip, fieldToken, ref fieldMarshal);
            return fieldMarshal;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void _GetPInvokeMap(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int token, out int attributes, void** importName, void** importDll);
        [SecurityCritical]
        public unsafe void GetPInvokeMap(int token, out PInvokeAttributes attributes, out string importName, out string importDll)
        {
            int num;
            void* voidPtr;
            void* voidPtr2;
            _GetPInvokeMap(this.m_metadataImport2, out MetadataArgs.Skip, token, out num, &voidPtr, &voidPtr2);
            importName = new Utf8String(voidPtr).ToString();
            importDll = new Utf8String(voidPtr2).ToString();
            attributes = (PInvokeAttributes) num;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool _IsValidToken(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int token);
        [SecurityCritical]
        public bool IsValidToken(int token)
        {
            return _IsValidToken(this.m_metadataImport2, out MetadataArgs.Skip, token);
        }

        static MetadataImport()
        {
            EmptyImport = new MetadataImport(IntPtr.Zero, null);
            IID_IMetaDataImport = new Guid(0xd26df2ea, 0x7f58, 0x4183, 0x86, 190, 0x30, 0xae, 0x29, 0xa7, 0x5d, 0x8d);
            IID_IMetaDataAssemblyImport = new Guid(0xee62470b, 0xe94b, 0x424e, 0x9b, 0x7c, 0x2f, 0, 0xc9, 0x24, 0x9f, 0x93);
            IID_IMetaDataTables = new Guid(0xd8f579ab, 0x402d, 0x4b8e, 130, 0xd9, 0x5d, 0x63, 0xb1, 6, 0x5c, 0x68);
        }
    }
}

