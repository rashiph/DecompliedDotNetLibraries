namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Cache;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal sealed class RuntimeEventInfo : EventInfo, ISerializable
    {
        private RuntimeMethodInfo m_addMethod;
        private System.Reflection.BindingFlags m_bindingFlags;
        private InternalCache m_cachedData;
        private RuntimeType m_declaringType;
        private EventAttributes m_flags;
        private string m_name;
        private MethodInfo[] m_otherMethod;
        private RuntimeMethodInfo m_raiseMethod;
        private RuntimeType.RuntimeTypeCache m_reflectedTypeCache;
        private RuntimeMethodInfo m_removeMethod;
        private int m_token;
        private unsafe void* m_utf8name;

        internal RuntimeEventInfo()
        {
        }

        [SecurityCritical]
        internal unsafe RuntimeEventInfo(int tkEvent, RuntimeType declaredType, RuntimeType.RuntimeTypeCache reflectedTypeCache, out bool isPrivate)
        {
            RuntimeMethodInfo info;
            MetadataImport metadataImport = declaredType.GetRuntimeModule().MetadataImport;
            this.m_token = tkEvent;
            this.m_reflectedTypeCache = reflectedTypeCache;
            this.m_declaringType = declaredType;
            RuntimeType runtimeType = reflectedTypeCache.RuntimeType;
            metadataImport.GetEventProps(tkEvent, out this.m_utf8name, out this.m_flags);
            int associatesCount = metadataImport.GetAssociatesCount(tkEvent);
            AssociateRecord* result = (AssociateRecord*) stackalloc byte[(((IntPtr) associatesCount) * sizeof(AssociateRecord))];
            metadataImport.GetAssociates(tkEvent, result, associatesCount);
            Associates.AssignAssociates(result, associatesCount, declaredType, runtimeType, out this.m_addMethod, out this.m_removeMethod, out this.m_raiseMethod, out info, out info, out this.m_otherMethod, out isPrivate, out this.m_bindingFlags);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        internal override bool CacheEquals(object o)
        {
            RuntimeEventInfo info = o as RuntimeEventInfo;
            if (info == null)
            {
                return false;
            }
            return ((info.m_token == this.m_token) && RuntimeTypeHandle.GetModule(this.m_declaringType).Equals(RuntimeTypeHandle.GetModule(info.m_declaringType)));
        }

        public override MethodInfo GetAddMethod(bool nonPublic)
        {
            if (!Associates.IncludeAccessor(this.m_addMethod, nonPublic))
            {
                return null;
            }
            return this.m_addMethod;
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

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            MemberInfoSerializationHolder.GetSerializationInfo(info, this.Name, this.ReflectedType, null, MemberTypes.Event);
        }

        public override MethodInfo[] GetOtherMethods(bool nonPublic)
        {
            List<MethodInfo> list = new List<MethodInfo>();
            if (this.m_otherMethod == null)
            {
                return new MethodInfo[0];
            }
            for (int i = 0; i < this.m_otherMethod.Length; i++)
            {
                if (Associates.IncludeAccessor(this.m_otherMethod[i], nonPublic))
                {
                    list.Add(this.m_otherMethod[i]);
                }
            }
            return list.ToArray();
        }

        public override MethodInfo GetRaiseMethod(bool nonPublic)
        {
            if (!Associates.IncludeAccessor(this.m_raiseMethod, nonPublic))
            {
                return null;
            }
            return this.m_raiseMethod;
        }

        public override MethodInfo GetRemoveMethod(bool nonPublic)
        {
            if (!Associates.IncludeAccessor(this.m_removeMethod, nonPublic))
            {
                return null;
            }
            return this.m_removeMethod;
        }

        internal RuntimeModule GetRuntimeModule()
        {
            return this.m_declaringType.GetRuntimeModule();
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

        public override string ToString()
        {
            if ((this.m_addMethod == null) || (this.m_addMethod.GetParametersNoCopy().Length == 0))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NoPublicAddMethod"));
            }
            return (this.m_addMethod.GetParametersNoCopy()[0].ParameterType.SigToString() + " " + this.Name);
        }

        public override EventAttributes Attributes
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

        public override Type DeclaringType
        {
            get
            {
                return this.m_declaringType;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Event;
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
    }
}

