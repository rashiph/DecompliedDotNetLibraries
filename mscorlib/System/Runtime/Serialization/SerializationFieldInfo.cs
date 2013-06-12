namespace System.Runtime.Serialization
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Cache;
    using System.Security;
    using System.Threading;

    internal sealed class SerializationFieldInfo : System.Reflection.FieldInfo
    {
        internal const string FakeNameSeparatorString = "+";
        private InternalCache m_cachedData;
        private RuntimeFieldInfo m_field;
        private string m_serializationName;

        internal SerializationFieldInfo(RuntimeFieldInfo field, string namePrefix)
        {
            this.m_field = field;
            this.m_serializationName = namePrefix + "+" + this.m_field.Name;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.m_field.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.m_field.GetCustomAttributes(attributeType, inherit);
        }

        public override object GetValue(object obj)
        {
            return this.m_field.GetValue(obj);
        }

        internal object InternalGetValue(object obj, bool requiresAccessCheck)
        {
            RtFieldInfo field = this.m_field as RtFieldInfo;
            if (field != null)
            {
                return field.InternalGetValue(obj, requiresAccessCheck);
            }
            return this.m_field.GetValue(obj);
        }

        [SecurityCritical]
        internal void InternalSetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture, bool requiresAccessCheck, bool isBinderDefault)
        {
            RtFieldInfo field = this.m_field as RtFieldInfo;
            if (field != null)
            {
                field.InternalSetValue(obj, value, invokeAttr, binder, culture, false);
            }
            else
            {
                this.m_field.SetValue(obj, value, invokeAttr, binder, culture);
            }
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.m_field.IsDefined(attributeType, inherit);
        }

        internal void OnCacheClear(object sender, ClearCacheEventArgs cacheEventArgs)
        {
            this.m_cachedData = null;
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            this.m_field.SetValue(obj, value, invokeAttr, binder, culture);
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return this.m_field.Attributes;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_field.DeclaringType;
            }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get
            {
                return this.m_field.FieldHandle;
            }
        }

        internal RuntimeFieldInfo FieldInfo
        {
            get
            {
                return this.m_field;
            }
        }

        public override Type FieldType
        {
            get
            {
                return this.m_field.FieldType;
            }
        }

        public override int MetadataToken
        {
            get
            {
                return this.m_field.MetadataToken;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_field.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_serializationName;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_field.ReflectedType;
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

