namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Proxies;
    using System.Security;

    internal sealed class ObjectCloneHelper
    {
        private static StreamingContext s_cloneContext = new StreamingContext(StreamingContextStates.CrossAppDomain);
        private static IFormatterConverter s_converter = new FormatterConverter();
        private static ISerializationSurrogate s_ObjRefRemotingSurrogate = new ObjRefSurrogate();
        private static ISerializationSurrogate s_RemotingSurrogate = new RemotingSurrogate();

        [SecurityCritical]
        internal static object GetObjectData(object serObj, out string typeName, out string assemName, out string[] fieldNames, out object[] fieldValues)
        {
            Type type = null;
            object obj2 = null;
            if (RemotingServices.IsTransparentProxy(serObj))
            {
                type = typeof(MarshalByRefObject);
            }
            else
            {
                type = serObj.GetType();
            }
            SerializationInfo info = new SerializationInfo(type, s_converter);
            if (serObj is ObjRef)
            {
                s_ObjRefRemotingSurrogate.GetObjectData(serObj, info, s_cloneContext);
            }
            else if (RemotingServices.IsTransparentProxy(serObj) || (serObj is MarshalByRefObject))
            {
                if (!RemotingServices.IsTransparentProxy(serObj) || (RemotingServices.GetRealProxy(serObj) is RemotingProxy))
                {
                    ObjRef ref2 = RemotingServices.MarshalInternal((MarshalByRefObject) serObj, null, null);
                    if (ref2.CanSmuggle())
                    {
                        if (RemotingServices.IsTransparentProxy(serObj))
                        {
                            RealProxy realProxy = RemotingServices.GetRealProxy(serObj);
                            ref2.SetServerIdentity(realProxy._srvIdentity);
                            ref2.SetDomainID(realProxy._domainID);
                        }
                        else
                        {
                            ServerIdentity identity = (ServerIdentity) MarshalByRefObject.GetIdentity((MarshalByRefObject) serObj);
                            identity.SetHandle();
                            ref2.SetServerIdentity(identity.GetHandle());
                            ref2.SetDomainID(AppDomain.CurrentDomain.GetId());
                        }
                        ref2.SetMarshaledObject();
                        obj2 = ref2;
                    }
                }
                if (obj2 == null)
                {
                    s_RemotingSurrogate.GetObjectData(serObj, info, s_cloneContext);
                }
            }
            else
            {
                if (!(serObj is ISerializable))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_SerializationException"));
                }
                ((ISerializable) serObj).GetObjectData(info, s_cloneContext);
            }
            if (obj2 == null)
            {
                typeName = info.FullTypeName;
                assemName = info.AssemblyName;
                fieldNames = info.MemberNames;
                fieldValues = info.MemberValues;
                return obj2;
            }
            typeName = null;
            assemName = null;
            fieldNames = null;
            fieldValues = null;
            return obj2;
        }

        [SecurityCritical]
        internal static SerializationInfo PrepareConstructorArgs(object serObj, string[] fieldNames, object[] fieldValues, out StreamingContext context)
        {
            SerializationInfo info = null;
            if (serObj is ISerializable)
            {
                info = new SerializationInfo(serObj.GetType(), s_converter);
                for (int i = 0; i < fieldNames.Length; i++)
                {
                    if (fieldNames[i] != null)
                    {
                        info.AddValue(fieldNames[i], fieldValues[i]);
                    }
                }
            }
            else
            {
                Hashtable hashtable = new Hashtable();
                int index = 0;
                int num3 = 0;
                while (index < fieldNames.Length)
                {
                    if (fieldNames[index] != null)
                    {
                        hashtable[fieldNames[index]] = fieldValues[index];
                        num3++;
                    }
                    index++;
                }
                MemberInfo[] serializableMembers = FormatterServices.GetSerializableMembers(serObj.GetType());
                for (int j = 0; j < serializableMembers.Length; j++)
                {
                    string name = serializableMembers[j].Name;
                    if (!hashtable.Contains(name))
                    {
                        object[] customAttributes = serializableMembers[j].GetCustomAttributes(typeof(OptionalFieldAttribute), false);
                        if ((customAttributes == null) || (customAttributes.Length == 0))
                        {
                            throw new SerializationException(Environment.GetResourceString("Serialization_MissingMember", new object[] { serializableMembers[j], serObj.GetType(), typeof(OptionalFieldAttribute).FullName }));
                        }
                    }
                    else
                    {
                        object obj2 = hashtable[name];
                        FormatterServices.SerializationSetValue(serializableMembers[j], serObj, obj2);
                    }
                }
            }
            context = s_cloneContext;
            return info;
        }
    }
}

