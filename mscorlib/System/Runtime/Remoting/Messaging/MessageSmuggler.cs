namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Proxies;
    using System.Security;

    internal class MessageSmuggler
    {
        private static bool CanSmuggleObjectDirectly(object obj)
        {
            if ((!(obj is string) && !(obj.GetType() == typeof(void))) && !obj.GetType().IsPrimitive)
            {
                return false;
            }
            return true;
        }

        [SecurityCritical]
        protected static object FixupArg(object arg, ref ArrayList argsToSerialize)
        {
            int count;
            if (arg == null)
            {
                return null;
            }
            MarshalByRefObject proxy = arg as MarshalByRefObject;
            if (proxy != null)
            {
                if (!RemotingServices.IsTransparentProxy(proxy) || (RemotingServices.GetRealProxy(proxy) is RemotingProxy))
                {
                    ObjRef ref2 = RemotingServices.MarshalInternal(proxy, null, null);
                    if (ref2.CanSmuggle())
                    {
                        if (!RemotingServices.IsTransparentProxy(proxy))
                        {
                            ServerIdentity identity = (ServerIdentity) MarshalByRefObject.GetIdentity(proxy);
                            identity.SetHandle();
                            ref2.SetServerIdentity(identity.GetHandle());
                            ref2.SetDomainID(AppDomain.CurrentDomain.GetId());
                        }
                        ObjRef objRef = ref2.CreateSmuggleableCopy();
                        objRef.SetMarshaledObject();
                        return new SmuggledObjRef(objRef);
                    }
                }
                if (argsToSerialize == null)
                {
                    argsToSerialize = new ArrayList();
                }
                count = argsToSerialize.Count;
                argsToSerialize.Add(arg);
                return new SerializedArg(count);
            }
            if (CanSmuggleObjectDirectly(arg))
            {
                return arg;
            }
            Array array = arg as Array;
            if (array != null)
            {
                Type elementType = array.GetType().GetElementType();
                if (elementType.IsPrimitive || (elementType == typeof(string)))
                {
                    return array.Clone();
                }
            }
            if (argsToSerialize == null)
            {
                argsToSerialize = new ArrayList();
            }
            count = argsToSerialize.Count;
            argsToSerialize.Add(arg);
            return new SerializedArg(count);
        }

        [SecurityCritical]
        protected static object[] FixupArgs(object[] args, ref ArrayList argsToSerialize)
        {
            object[] objArray = new object[args.Length];
            int length = args.Length;
            for (int i = 0; i < length; i++)
            {
                objArray[i] = FixupArg(args[i], ref argsToSerialize);
            }
            return objArray;
        }

        [SecurityCritical]
        protected static int StoreUserPropertiesForMethodMessage(IMethodMessage msg, ref ArrayList argsToSerialize)
        {
            IDictionary properties = msg.Properties;
            MessageDictionary dictionary2 = properties as MessageDictionary;
            if (dictionary2 != null)
            {
                if (!dictionary2.HasUserData())
                {
                    return 0;
                }
                int num = 0;
                foreach (DictionaryEntry entry in dictionary2.InternalDictionary)
                {
                    if (argsToSerialize == null)
                    {
                        argsToSerialize = new ArrayList();
                    }
                    argsToSerialize.Add(entry);
                    num++;
                }
                return num;
            }
            int num2 = 0;
            foreach (DictionaryEntry entry2 in properties)
            {
                if (argsToSerialize == null)
                {
                    argsToSerialize = new ArrayList();
                }
                argsToSerialize.Add(entry2);
                num2++;
            }
            return num2;
        }

        [SecurityCritical]
        protected static object UndoFixupArg(object arg, ArrayList deserializedArgs)
        {
            SmuggledObjRef ref2 = arg as SmuggledObjRef;
            if (ref2 != null)
            {
                return ref2.ObjRef.GetRealObjectHelper();
            }
            SerializedArg arg2 = arg as SerializedArg;
            if (arg2 != null)
            {
                return deserializedArgs[arg2.Index];
            }
            return arg;
        }

        [SecurityCritical]
        protected static object[] UndoFixupArgs(object[] args, ArrayList deserializedArgs)
        {
            object[] objArray = new object[args.Length];
            int length = args.Length;
            for (int i = 0; i < length; i++)
            {
                objArray[i] = UndoFixupArg(args[i], deserializedArgs);
            }
            return objArray;
        }

        protected class SerializedArg
        {
            private int _index;

            public SerializedArg(int index)
            {
                this._index = index;
            }

            public int Index
            {
                get
                {
                    return this._index;
                }
            }
        }
    }
}

