namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection.Cache;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;
    using System.Threading;

    [Serializable]
    internal sealed class RuntimePropertyInfo : PropertyInfo, ISerializable
    {
        private System.Reflection.BindingFlags m_bindingFlags;
        private InternalCache m_cachedData;
        private RuntimeType m_declaringType;
        private PropertyAttributes m_flags;
        private RuntimeMethodInfo m_getterMethod;
        private string m_name;
        private MethodInfo[] m_otherMethod;
        private ParameterInfo[] m_parameters;
        private RuntimeType.RuntimeTypeCache m_reflectedTypeCache;
        private RuntimeMethodInfo m_setterMethod;
        private System.Signature m_signature;
        private int m_token;
        private unsafe void* m_utf8name;

        [SecurityCritical]
        internal unsafe RuntimePropertyInfo(int tkProperty, RuntimeType declaredType, RuntimeType.RuntimeTypeCache reflectedTypeCache, out bool isPrivate)
        {
            RuntimeMethodInfo info;
            MetadataImport metadataImport = declaredType.GetRuntimeModule().MetadataImport;
            this.m_token = tkProperty;
            this.m_reflectedTypeCache = reflectedTypeCache;
            this.m_declaringType = declaredType;
            metadataImport.GetPropertyProps(tkProperty, out this.m_utf8name, out this.m_flags, out MetadataArgs.Skip.ConstArray);
            int associatesCount = metadataImport.GetAssociatesCount(tkProperty);
            AssociateRecord* result = (AssociateRecord*) stackalloc byte[(((IntPtr) associatesCount) * sizeof(AssociateRecord))];
            metadataImport.GetAssociates(tkProperty, result, associatesCount);
            Associates.AssignAssociates(result, associatesCount, declaredType, reflectedTypeCache.RuntimeType, out info, out info, out info, out this.m_getterMethod, out this.m_setterMethod, out this.m_otherMethod, out isPrivate, out this.m_bindingFlags);
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal override bool CacheEquals(object o)
        {
            RuntimePropertyInfo info = o as RuntimePropertyInfo;
            if (info == null)
            {
                return false;
            }
            return ((info.m_token == this.m_token) && RuntimeTypeHandle.GetModule(this.m_declaringType).Equals(RuntimeTypeHandle.GetModule(info.m_declaringType)));
        }

        internal bool EqualsSig(RuntimePropertyInfo target)
        {
            return System.Signature.DiffSigs(this.Signature, target.Signature);
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            List<MethodInfo> list = new List<MethodInfo>();
            if (Associates.IncludeAccessor(this.m_getterMethod, nonPublic))
            {
                list.Add(this.m_getterMethod);
            }
            if (Associates.IncludeAccessor(this.m_setterMethod, nonPublic))
            {
                list.Add(this.m_setterMethod);
            }
            if (this.m_otherMethod != null)
            {
                for (int i = 0; i < this.m_otherMethod.Length; i++)
                {
                    if (Associates.IncludeAccessor(this.m_otherMethod[i], nonPublic))
                    {
                        list.Add(this.m_otherMethod[i]);
                    }
                }
            }
            return list.ToArray();
        }

        public override object GetConstantValue()
        {
            return this.GetConstantValue(false);
        }

        [SecuritySafeCritical]
        internal object GetConstantValue(bool raw)
        {
            object obj2 = MdConstant.GetValue(this.GetRuntimeModule().MetadataImport, this.m_token, this.PropertyType.GetTypeHandleInternal(), raw);
            if (obj2 == DBNull.Value)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_EnumLitValueNotFound"));
            }
            return obj2;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
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

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (!Associates.IncludeAccessor(this.m_getterMethod, nonPublic))
            {
                return null;
            }
            return this.m_getterMethod;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            ParameterInfo[] indexParametersNoCopy = this.GetIndexParametersNoCopy();
            int length = indexParametersNoCopy.Length;
            if (length == 0)
            {
                return indexParametersNoCopy;
            }
            ParameterInfo[] destinationArray = new ParameterInfo[length];
            Array.Copy(indexParametersNoCopy, destinationArray, length);
            return destinationArray;
        }

        internal ParameterInfo[] GetIndexParametersNoCopy()
        {
            if (this.m_parameters == null)
            {
                int length = 0;
                ParameterInfo[] parametersNoCopy = null;
                MethodInfo getMethod = this.GetGetMethod(true);
                if (getMethod != null)
                {
                    parametersNoCopy = getMethod.GetParametersNoCopy();
                    length = parametersNoCopy.Length;
                }
                else
                {
                    getMethod = this.GetSetMethod(true);
                    if (getMethod != null)
                    {
                        parametersNoCopy = getMethod.GetParametersNoCopy();
                        length = parametersNoCopy.Length - 1;
                    }
                }
                ParameterInfo[] infoArray2 = new ParameterInfo[length];
                for (int i = 0; i < length; i++)
                {
                    infoArray2[i] = new RuntimeParameterInfo((RuntimeParameterInfo) parametersNoCopy[i], this);
                }
                this.m_parameters = infoArray2;
            }
            return this.m_parameters;
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            MemberInfoSerializationHolder.GetSerializationInfo(info, this.Name, this.ReflectedType, this.ToString(), MemberTypes.Property);
        }

        public override Type[] GetOptionalCustomModifiers()
        {
            return this.Signature.GetCustomModifiers(0, false);
        }

        public override object GetRawConstantValue()
        {
            return this.GetConstantValue(true);
        }

        public override Type[] GetRequiredCustomModifiers()
        {
            return this.Signature.GetCustomModifiers(0, true);
        }

        internal RuntimeModule GetRuntimeModule()
        {
            return this.m_declaringType.GetRuntimeModule();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (!Associates.IncludeAccessor(this.m_setterMethod, nonPublic))
            {
                return null;
            }
            return this.m_setterMethod;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), DebuggerStepThrough, DebuggerHidden]
        public override object GetValue(object obj, object[] index)
        {
            return this.GetValue(obj, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance, null, index, null);
        }

        [DebuggerHidden, DebuggerStepThrough]
        public override object GetValue(object obj, System.Reflection.BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            MethodInfo getMethod = this.GetGetMethod(true);
            if (getMethod == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_GetMethNotFnd"));
            }
            return getMethod.Invoke(obj, invokeAttr, binder, index, null);
        }

        [SecuritySafeCritical]
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
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

        [DebuggerHidden, DebuggerStepThrough, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override void SetValue(object obj, object value, object[] index)
        {
            this.SetValue(obj, value, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance, null, index, null);
        }

        [DebuggerStepThrough, DebuggerHidden]
        public override void SetValue(object obj, object value, System.Reflection.BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            MethodInfo setMethod = this.GetSetMethod(true);
            if (setMethod == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_SetMethNotFnd"));
            }
            object[] parameters = null;
            if (index != null)
            {
                parameters = new object[index.Length + 1];
                for (int i = 0; i < index.Length; i++)
                {
                    parameters[i] = index[i];
                }
                parameters[index.Length] = value;
            }
            else
            {
                parameters = new object[] { value };
            }
            setMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(this.PropertyType.SigToString());
            builder.Append(" ");
            builder.Append(this.Name);
            RuntimeType[] arguments = this.Signature.Arguments;
            if (arguments.Length > 0)
            {
                builder.Append(" [");
                builder.Append(MethodBase.ConstructParameters(arguments, this.Signature.CallingConvention));
                builder.Append("]");
            }
            return builder.ToString();
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return this.m_flags;
            }
        }

        internal System.Reflection.BindingFlags BindingFlags
        {
            get
            {
                return this.m_bindingFlags;
            }
        }

        public override bool CanRead
        {
            get
            {
                return (this.m_getterMethod != null);
            }
        }

        public override bool CanWrite
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.m_setterMethod != null);
            }
        }

        public override Type DeclaringType
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.m_declaringType;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Property;
            }
        }

        public override int MetadataToken
        {
            get
            {
                return this.m_token;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.GetRuntimeModule();
            }
        }

        public override string Name
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_name == null)
                {
                    this.m_name = new Utf8String(this.m_utf8name).ToString();
                }
                return this.m_name;
            }
        }

        public override Type PropertyType
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.Signature.ReturnType;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_reflectedTypeCache.RuntimeType;
            }
        }

        internal InternalCache RemotingCache
        {
            get
            {
                InternalCache cachedData = this.m_cachedData;
                if (cachedData == null)
                {
                    cachedData = new InternalCache("MemberInfo");
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

        internal System.Signature Signature
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_signature == null)
                {
                    ConstArray array;
                    void* voidPtr;
                    this.GetRuntimeModule().MetadataImport.GetPropertyProps(this.m_token, out voidPtr, out MetadataArgs.Skip.PropertyAttributes, out array);
                    void* pCorSig = array.Signature.ToPointer();
                    this.m_signature = new System.Signature(pCorSig, array.Length, this.m_declaringType);
                }
                return this.m_signature;
            }
        }
    }
}

