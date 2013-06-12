namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Cache;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal sealed class RuntimeParameterInfo : ParameterInfo, ISerializable
    {
        private InternalCache m_cachedData;
        [NonSerialized]
        private volatile bool m_nameIsCached;
        [NonSerialized]
        private readonly bool m_noDefaultValue;
        [NonSerialized]
        private MethodBase m_originalMember;
        [NonSerialized]
        private MetadataImport m_scope;
        [NonSerialized]
        private Signature m_signature;
        [NonSerialized]
        private int m_tkParamDef;
        private static readonly Type s_CustomConstantAttributeType = typeof(CustomConstantAttribute);
        private static readonly Type s_DecimalConstantAttributeType = typeof(DecimalConstantAttribute);

        private RuntimeParameterInfo()
        {
            this.m_nameIsCached = true;
            this.m_noDefaultValue = true;
        }

        private RuntimeParameterInfo(RuntimeParameterInfo accessor, MemberInfo member)
        {
            base.MemberImpl = member;
            this.m_originalMember = accessor.MemberImpl as MethodBase;
            base.NameImpl = accessor.Name;
            this.m_nameIsCached = true;
            base.ClassImpl = accessor.ParameterType;
            base.PositionImpl = accessor.Position;
            base.AttrsImpl = accessor.Attributes;
            this.m_tkParamDef = System.Reflection.MetadataToken.IsNullToken(accessor.MetadataToken) ? 0x8000000 : accessor.MetadataToken;
            this.m_scope = accessor.m_scope;
        }

        internal RuntimeParameterInfo(RuntimeParameterInfo accessor, RuntimePropertyInfo property) : this(accessor, (MemberInfo) property)
        {
            this.m_signature = property.Signature;
        }

        internal RuntimeParameterInfo(MethodInfo owner, string name, Type parameterType, int position)
        {
            base.MemberImpl = owner;
            base.NameImpl = name;
            this.m_nameIsCached = true;
            this.m_noDefaultValue = true;
            base.ClassImpl = parameterType;
            base.PositionImpl = position;
            base.AttrsImpl = ParameterAttributes.None;
            this.m_tkParamDef = 0x8000000;
            this.m_scope = MetadataImport.EmptyImport;
        }

        private RuntimeParameterInfo(Signature signature, MetadataImport scope, int tkParamDef, int position, ParameterAttributes attributes, MemberInfo member)
        {
            base.PositionImpl = position;
            base.MemberImpl = member;
            this.m_signature = signature;
            this.m_tkParamDef = System.Reflection.MetadataToken.IsNullToken(tkParamDef) ? 0x8000000 : tkParamDef;
            this.m_scope = scope;
            base.AttrsImpl = attributes;
            base.ClassImpl = null;
            base.NameImpl = null;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (System.Reflection.MetadataToken.IsNullToken(this.m_tkParamDef))
            {
                return new object[0];
            }
            return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (System.Reflection.MetadataToken.IsNullToken(this.m_tkParamDef))
            {
                return new object[0];
            }
            RuntimeType underlyingSystemType = attributeType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
            }
            return CustomAttribute.GetCustomAttributes(this, underlyingSystemType);
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return CustomAttributeData.GetCustomAttributesInternal(this);
        }

        [SecuritySafeCritical]
        internal object GetDefaultValue(bool raw)
        {
            object missing = null;
            if (!this.m_noDefaultValue)
            {
                if (this.ParameterType == typeof(DateTime))
                {
                    if (raw)
                    {
                        CustomAttributeTypedArgument argument = CustomAttributeData.Filter(CustomAttributeData.GetCustomAttributes(this), typeof(DateTimeConstantAttribute), 0);
                        if (argument.ArgumentType != null)
                        {
                            return new DateTime((long) argument.Value);
                        }
                    }
                    else
                    {
                        object[] customAttributes = this.GetCustomAttributes(typeof(DateTimeConstantAttribute), false);
                        if ((customAttributes != null) && (customAttributes.Length != 0))
                        {
                            return ((DateTimeConstantAttribute) customAttributes[0]).Value;
                        }
                    }
                }
                if (!System.Reflection.MetadataToken.IsNullToken(this.m_tkParamDef))
                {
                    missing = MdConstant.GetValue(this.m_scope, this.m_tkParamDef, this.ParameterType.GetTypeHandleInternal(), raw);
                }
                if (missing == DBNull.Value)
                {
                    if (raw)
                    {
                        IList<CustomAttributeData> attrs = CustomAttributeData.GetCustomAttributes(this);
                        CustomAttributeTypedArgument argument2 = CustomAttributeData.Filter(attrs, s_CustomConstantAttributeType, "Value");
                        if (argument2.ArgumentType == null)
                        {
                            argument2 = CustomAttributeData.Filter(attrs, s_DecimalConstantAttributeType, "Value");
                            if (argument2.ArgumentType == null)
                            {
                                for (int i = 0; i < attrs.Count; i++)
                                {
                                    if (attrs[i].Constructor.DeclaringType == s_DecimalConstantAttributeType)
                                    {
                                        ParameterInfo[] parameters = attrs[i].Constructor.GetParameters();
                                        if (parameters.Length != 0)
                                        {
                                            if (parameters[2].ParameterType == typeof(uint))
                                            {
                                                IList<CustomAttributeTypedArgument> constructorArguments = attrs[i].ConstructorArguments;
                                                CustomAttributeTypedArgument argument3 = constructorArguments[4];
                                                int lo = (int) ((uint) argument3.Value);
                                                CustomAttributeTypedArgument argument4 = constructorArguments[3];
                                                int mid = (int) ((uint) argument4.Value);
                                                CustomAttributeTypedArgument argument5 = constructorArguments[2];
                                                int hi = (int) ((uint) argument5.Value);
                                                CustomAttributeTypedArgument argument6 = constructorArguments[1];
                                                byte num5 = (byte) argument6.Value;
                                                CustomAttributeTypedArgument argument7 = constructorArguments[0];
                                                byte scale = (byte) argument7.Value;
                                                argument2 = new CustomAttributeTypedArgument(new decimal(lo, mid, hi, num5 != 0, scale));
                                            }
                                            else
                                            {
                                                IList<CustomAttributeTypedArgument> list3 = attrs[i].ConstructorArguments;
                                                CustomAttributeTypedArgument argument8 = list3[4];
                                                int num7 = (int) argument8.Value;
                                                CustomAttributeTypedArgument argument9 = list3[3];
                                                int num8 = (int) argument9.Value;
                                                CustomAttributeTypedArgument argument10 = list3[2];
                                                int num9 = (int) argument10.Value;
                                                CustomAttributeTypedArgument argument11 = list3[1];
                                                byte num10 = (byte) argument11.Value;
                                                CustomAttributeTypedArgument argument12 = list3[0];
                                                byte num11 = (byte) argument12.Value;
                                                argument2 = new CustomAttributeTypedArgument(new decimal(num7, num8, num9, num10 != 0, num11));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (argument2.ArgumentType != null)
                        {
                            missing = argument2.Value;
                        }
                    }
                    else
                    {
                        object[] objArray2 = this.GetCustomAttributes(s_CustomConstantAttributeType, false);
                        if (objArray2.Length != 0)
                        {
                            missing = ((CustomConstantAttribute) objArray2[0]).Value;
                        }
                        else
                        {
                            objArray2 = this.GetCustomAttributes(s_DecimalConstantAttributeType, false);
                            if (objArray2.Length != 0)
                            {
                                missing = ((DecimalConstantAttribute) objArray2[0]).Value;
                            }
                        }
                    }
                }
                if ((missing == DBNull.Value) && base.IsOptional)
                {
                    missing = Type.Missing;
                }
            }
            return missing;
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.SetType(typeof(ParameterInfo));
            info.AddValue("AttrsImpl", this.Attributes);
            info.AddValue("ClassImpl", this.ParameterType);
            info.AddValue("DefaultValueImpl", this.DefaultValue);
            info.AddValue("MemberImpl", this.Member);
            info.AddValue("NameImpl", this.Name);
            info.AddValue("PositionImpl", this.Position);
            info.AddValue("_token", this.m_tkParamDef);
        }

        public override Type[] GetOptionalCustomModifiers()
        {
            return this.m_signature.GetCustomModifiers(base.PositionImpl + 1, false);
        }

        [SecurityCritical]
        internal static ParameterInfo[] GetParameters(IRuntimeMethodInfo method, MemberInfo member, Signature sig)
        {
            ParameterInfo info;
            return GetParameters(method, member, sig, out info, false);
        }

        [SecurityCritical]
        internal static unsafe ParameterInfo[] GetParameters(IRuntimeMethodInfo methodHandle, MemberInfo member, Signature sig, out ParameterInfo returnParameter, bool fetchReturnParameter)
        {
            returnParameter = null;
            int length = sig.Arguments.Length;
            ParameterInfo[] infoArray = fetchReturnParameter ? null : new ParameterInfo[length];
            int methodDef = RuntimeMethodHandle.GetMethodDef(methodHandle);
            int count = 0;
            if (!System.Reflection.MetadataToken.IsNullToken(methodDef))
            {
                MetadataImport metadataImport = RuntimeTypeHandle.GetMetadataImport(RuntimeMethodHandle.GetDeclaringType(methodHandle));
                count = metadataImport.EnumParamsCount(methodDef);
                int* result = (int*) stackalloc byte[(((IntPtr) count) * 4)];
                metadataImport.EnumParams(methodDef, result, count);
                if (count > (length + 1))
                {
                    throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ParameterSignatureMismatch"));
                }
                for (uint i = 0; i < count; i++)
                {
                    ParameterAttributes attributes;
                    int num5;
                    int parameterToken = result[(int) ((int*) i)];
                    metadataImport.GetParamDefProps(parameterToken, out num5, out attributes);
                    num5--;
                    if (fetchReturnParameter && (num5 == -1))
                    {
                        if (returnParameter != null)
                        {
                            throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ParameterSignatureMismatch"));
                        }
                        returnParameter = new RuntimeParameterInfo(sig, metadataImport, parameterToken, num5, attributes, member);
                    }
                    else if (!fetchReturnParameter && (num5 >= 0))
                    {
                        if (num5 >= length)
                        {
                            throw new BadImageFormatException(Environment.GetResourceString("BadImageFormat_ParameterSignatureMismatch"));
                        }
                        infoArray[num5] = new RuntimeParameterInfo(sig, metadataImport, parameterToken, num5, attributes, member);
                    }
                }
            }
            if (fetchReturnParameter)
            {
                if (returnParameter == null)
                {
                    returnParameter = new RuntimeParameterInfo(sig, MetadataImport.EmptyImport, 0, -1, ParameterAttributes.None, member);
                }
                return infoArray;
            }
            if (count < (infoArray.Length + 1))
            {
                for (int j = 0; j < infoArray.Length; j++)
                {
                    if (infoArray[j] == null)
                    {
                        infoArray[j] = new RuntimeParameterInfo(sig, MetadataImport.EmptyImport, 0, j, ParameterAttributes.None, member);
                    }
                }
            }
            return infoArray;
        }

        public override Type[] GetRequiredCustomModifiers()
        {
            return this.m_signature.GetCustomModifiers(base.PositionImpl + 1, true);
        }

        [SecurityCritical]
        internal static ParameterInfo GetReturnParameter(IRuntimeMethodInfo method, MemberInfo member, Signature sig)
        {
            ParameterInfo info;
            GetParameters(method, member, sig, out info, true);
            return info;
        }

        internal RuntimeModule GetRuntimeModule()
        {
            RuntimeMethodInfo member = this.Member as RuntimeMethodInfo;
            RuntimeConstructorInfo info2 = this.Member as RuntimeConstructorInfo;
            RuntimePropertyInfo info3 = this.Member as RuntimePropertyInfo;
            if (member != null)
            {
                return member.GetRuntimeModule();
            }
            if (info2 != null)
            {
                return info2.GetRuntimeModule();
            }
            if (info3 != null)
            {
                return info3.GetRuntimeModule();
            }
            return null;
        }

        [SecuritySafeCritical]
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (System.Reflection.MetadataToken.IsNullToken(this.m_tkParamDef))
            {
                return false;
            }
            RuntimeType underlyingSystemType = attributeType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
            }
            return CustomAttribute.IsDefined(this, underlyingSystemType);
        }

        internal void OnCacheClear(object sender, ClearCacheEventArgs cacheEventArgs)
        {
            this.m_cachedData = null;
        }

        public override object DefaultValue
        {
            get
            {
                return this.GetDefaultValue(false);
            }
        }

        internal MethodBase DefiningMethod
        {
            get
            {
                return ((this.m_originalMember != null) ? this.m_originalMember : (base.MemberImpl as MethodBase));
            }
        }

        public override int MetadataToken
        {
            get
            {
                return this.m_tkParamDef;
            }
        }

        public override string Name
        {
            [SecuritySafeCritical]
            get
            {
                if (!this.m_nameIsCached)
                {
                    if (!System.Reflection.MetadataToken.IsNullToken(this.m_tkParamDef))
                    {
                        string str = this.m_scope.GetName(this.m_tkParamDef).ToString();
                        base.NameImpl = str;
                    }
                    this.m_nameIsCached = true;
                }
                return base.NameImpl;
            }
        }

        public override Type ParameterType
        {
            get
            {
                if (base.ClassImpl == null)
                {
                    RuntimeType returnType;
                    if (base.PositionImpl == -1)
                    {
                        returnType = this.m_signature.ReturnType;
                    }
                    else
                    {
                        returnType = this.m_signature.Arguments[base.PositionImpl];
                    }
                    base.ClassImpl = returnType;
                }
                return base.ClassImpl;
            }
        }

        public override object RawDefaultValue
        {
            get
            {
                return this.GetDefaultValue(true);
            }
        }

        internal InternalCache RemotingCache
        {
            get
            {
                InternalCache cachedData = this.m_cachedData;
                if (cachedData == null)
                {
                    cachedData = new InternalCache("ParameterInfo");
                    InternalCache cache2 = Interlocked.CompareExchange<InternalCache>(ref this.m_cachedData, cachedData, null);
                    if (cache2 != null)
                    {
                        cachedData = cache2;
                    }
                    GC.ClearCache += new ClearCacheHandler(this.OnCacheClear);
                }
                return cachedData;
            }
        }
    }
}

