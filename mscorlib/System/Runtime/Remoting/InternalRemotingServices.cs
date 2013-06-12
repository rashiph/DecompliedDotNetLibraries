namespace System.Runtime.Remoting
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Cache;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Security;

    [SecurityCritical, ComVisible(true)]
    public class InternalRemotingServices
    {
        [Conditional("_LOGGING"), SecurityCritical]
        public static void DebugOutChnl(string s)
        {
            Message.OutToUnmanagedDebugger("CHNL:" + s + "\n");
        }

        [SecurityCritical]
        public static SoapAttribute GetCachedSoapAttribute(object reflectionObject)
        {
            MemberInfo mi = reflectionObject as MemberInfo;
            RuntimeParameterInfo info2 = reflectionObject as RuntimeParameterInfo;
            if (mi != null)
            {
                return GetReflectionCachedData(mi).GetSoapAttribute();
            }
            if (info2 != null)
            {
                return GetReflectionCachedData(info2).GetSoapAttribute();
            }
            return null;
        }

        internal static RemotingCachedData GetReflectionCachedData(MemberInfo mi)
        {
            RemotingCachedData data = null;
            MethodBase base2 = null;
            RuntimeType type = null;
            RuntimeFieldInfo ri = null;
            RuntimeEventInfo info2 = null;
            RuntimePropertyInfo info3 = null;
            SerializationFieldInfo info4 = null;
            base2 = mi as MethodBase;
            if (base2 != null)
            {
                return GetReflectionCachedData(base2);
            }
            type = mi as RuntimeType;
            if (type != null)
            {
                return GetReflectionCachedData(type);
            }
            ri = mi as RuntimeFieldInfo;
            if (ri != null)
            {
                data = (RemotingCachedData) ri.RemotingCache[CacheObjType.RemotingData];
                if (data == null)
                {
                    ri.RemotingCache[CacheObjType.RemotingData] = data = new RemotingCachedData(ri);
                }
                return data;
            }
            info2 = mi as RuntimeEventInfo;
            if (info2 != null)
            {
                data = (RemotingCachedData) info2.RemotingCache[CacheObjType.RemotingData];
                if (data == null)
                {
                    info2.RemotingCache[CacheObjType.RemotingData] = data = new RemotingCachedData(info2);
                }
                return data;
            }
            info3 = mi as RuntimePropertyInfo;
            if (info3 != null)
            {
                data = (RemotingCachedData) info3.RemotingCache[CacheObjType.RemotingData];
                if (data == null)
                {
                    info3.RemotingCache[CacheObjType.RemotingData] = data = new RemotingCachedData(info3);
                }
                return data;
            }
            info4 = mi as SerializationFieldInfo;
            if (info4 == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeReflectionObject"));
            }
            data = (RemotingCachedData) info4.RemotingCache[CacheObjType.RemotingData];
            if (data == null)
            {
                info4.RemotingCache[CacheObjType.RemotingData] = data = new RemotingCachedData(info4);
            }
            return data;
        }

        internal static RemotingMethodCachedData GetReflectionCachedData(MethodBase mi)
        {
            RemotingMethodCachedData data = null;
            RuntimeMethodInfo ri = null;
            RuntimeConstructorInfo info2 = null;
            ri = mi as RuntimeMethodInfo;
            if (ri != null)
            {
                data = (RemotingMethodCachedData) ri.RemotingCache[CacheObjType.RemotingData];
                if (data == null)
                {
                    ri.RemotingCache[CacheObjType.RemotingData] = data = new RemotingMethodCachedData(ri);
                }
                return data;
            }
            info2 = mi as RuntimeConstructorInfo;
            if (info2 == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeReflectionObject"));
            }
            data = (RemotingMethodCachedData) info2.RemotingCache[CacheObjType.RemotingData];
            if (data == null)
            {
                info2.RemotingCache[CacheObjType.RemotingData] = data = new RemotingMethodCachedData(info2);
            }
            return data;
        }

        internal static RemotingCachedData GetReflectionCachedData(RuntimeParameterInfo reflectionObject)
        {
            RemotingCachedData data = null;
            data = (RemotingCachedData) reflectionObject.RemotingCache[CacheObjType.RemotingData];
            if (data == null)
            {
                data = new RemotingCachedData(reflectionObject);
                reflectionObject.RemotingCache[CacheObjType.RemotingData] = data;
            }
            return data;
        }

        internal static RemotingTypeCachedData GetReflectionCachedData(RuntimeType mi)
        {
            RemotingTypeCachedData data = null;
            data = (RemotingTypeCachedData) mi.RemotingCache[CacheObjType.RemotingData];
            if (data == null)
            {
                data = new RemotingTypeCachedData(mi);
                mi.RemotingCache[CacheObjType.RemotingData] = data;
            }
            return data;
        }

        [Conditional("_DEBUG")]
        public static void RemotingAssert(bool condition, string message)
        {
        }

        [Conditional("_LOGGING")]
        public static void RemotingTrace(params object[] messages)
        {
        }

        [CLSCompliant(false), SecurityCritical]
        public static void SetServerIdentity(MethodCall m, object srvID)
        {
            IInternalMessage message = m;
            message.ServerIdentityObject = (ServerIdentity) srvID;
        }
    }
}

