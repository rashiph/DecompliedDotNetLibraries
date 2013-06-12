namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Cache;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal abstract class RuntimeFieldInfo : FieldInfo, ISerializable
    {
        private System.Reflection.BindingFlags m_bindingFlags;
        private InternalCache m_cachedData;
        protected RuntimeType m_declaringType;
        protected RuntimeType.RuntimeTypeCache m_reflectedTypeCache;

        protected RuntimeFieldInfo()
        {
        }

        protected RuntimeFieldInfo(RuntimeType.RuntimeTypeCache reflectedTypeCache, RuntimeType declaringType, System.Reflection.BindingFlags bindingFlags)
        {
            this.m_bindingFlags = bindingFlags;
            this.m_declaringType = declaringType;
            this.m_reflectedTypeCache = reflectedTypeCache;
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
            MemberInfoSerializationHolder.GetSerializationInfo(info, this.Name, this.ReflectedType, this.ToString(), MemberTypes.Field);
        }

        internal abstract RuntimeModule GetRuntimeModule();
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
            return (this.FieldType.SigToString() + " " + this.Name);
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
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (!this.m_reflectedTypeCache.IsGlobal)
                {
                    return this.m_declaringType;
                }
                return null;
            }
        }

        internal RuntimeTypeHandle DeclaringTypeHandle
        {
            get
            {
                Type declaringType = this.DeclaringType;
                if (declaringType == null)
                {
                    return new RuntimeTypeHandle(this.GetRuntimeModule().RuntimeType);
                }
                return declaringType.GetTypeHandleInternal();
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Field;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.GetRuntimeModule();
            }
        }

        public override Type ReflectedType
        {
            get
            {
                if (!this.m_reflectedTypeCache.IsGlobal)
                {
                    return this.m_reflectedTypeCache.RuntimeType;
                }
                return null;
            }
        }

        private RuntimeType ReflectedTypeInternal
        {
            get
            {
                return this.m_reflectedTypeCache.GetRuntimeType();
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

