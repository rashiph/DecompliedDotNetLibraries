namespace System
{
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    internal sealed class DelegateSerializationHolder : IObjectReference, ISerializable
    {
        private DelegateEntry m_delegateEntry;
        private MethodInfo[] m_methods;

        [SecurityCritical]
        private DelegateSerializationHolder(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            bool flag = true;
            try
            {
                this.m_delegateEntry = (DelegateEntry) info.GetValue("Delegate", typeof(DelegateEntry));
            }
            catch
            {
                this.m_delegateEntry = this.OldDelegateWireFormat(info, context);
                flag = false;
            }
            if (flag)
            {
                DelegateEntry delegateEntry = this.m_delegateEntry;
                int num = 0;
                while (delegateEntry != null)
                {
                    if (delegateEntry.target != null)
                    {
                        string target = delegateEntry.target as string;
                        if (target != null)
                        {
                            delegateEntry.target = info.GetValue(target, typeof(object));
                        }
                    }
                    num++;
                    delegateEntry = delegateEntry.delegateEntry;
                }
                MethodInfo[] infoArray = new MethodInfo[num];
                int index = 0;
                while (index < num)
                {
                    string name = "method" + index;
                    infoArray[index] = (MethodInfo) info.GetValueNoThrow(name, typeof(MethodInfo));
                    if (infoArray[index] == null)
                    {
                        break;
                    }
                    index++;
                }
                if (index == num)
                {
                    this.m_methods = infoArray;
                }
            }
        }

        [SecuritySafeCritical]
        private Delegate GetDelegate(DelegateEntry de, int index)
        {
            Delegate delegate2;
            try
            {
                if ((de.methodName == null) || (de.methodName.Length == 0))
                {
                    this.ThrowInsufficientState("MethodName");
                }
                if ((de.assembly == null) || (de.assembly.Length == 0))
                {
                    this.ThrowInsufficientState("DelegateAssembly");
                }
                if ((de.targetTypeName == null) || (de.targetTypeName.Length == 0))
                {
                    this.ThrowInsufficientState("TargetTypeName");
                }
                RuntimeType type = (RuntimeType) Assembly.Load(de.assembly).GetType(de.type, true, false);
                RuntimeType type2 = (RuntimeType) Assembly.Load(de.targetTypeAssembly).GetType(de.targetTypeName, true, false);
                if (this.m_methods != null)
                {
                    object firstArgument = (de.target != null) ? RemotingServices.CheckCast(de.target, type2) : null;
                    delegate2 = Delegate.InternalCreateDelegate(type, firstArgument, this.m_methods[index]);
                }
                else if (de.target != null)
                {
                    delegate2 = Delegate.CreateDelegate(type, RemotingServices.CheckCast(de.target, type2), de.methodName);
                }
                else
                {
                    delegate2 = Delegate.CreateDelegate(type, (Type) type2, de.methodName);
                }
                if (((delegate2.Method == null) || delegate2.Method.IsPublic) && ((delegate2.Method.DeclaringType == null) || delegate2.Method.DeclaringType.IsVisible))
                {
                    return delegate2;
                }
                new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
            }
            catch (Exception exception)
            {
                if (exception is SerializationException)
                {
                    throw exception;
                }
                throw new SerializationException(exception.Message, exception);
            }
            return delegate2;
        }

        [SecurityCritical]
        internal static DelegateEntry GetDelegateSerializationInfo(SerializationInfo info, Type delegateType, object target, MethodInfo method, int targetIndex)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!method.IsPublic || ((method.DeclaringType != null) && !method.DeclaringType.IsVisible))
            {
                new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
            }
            Type baseType = delegateType.BaseType;
            if ((baseType == null) || ((baseType != typeof(Delegate)) && (baseType != typeof(MulticastDelegate))))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
            }
            if (method.DeclaringType == null)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_GlobalMethodSerialization"));
            }
            DelegateEntry entry = new DelegateEntry(delegateType.FullName, delegateType.Module.Assembly.FullName, target, method.ReflectedType.Module.Assembly.FullName, method.ReflectedType.FullName, method.Name);
            if (info.MemberCount == 0)
            {
                info.SetType(typeof(DelegateSerializationHolder));
                info.AddValue("Delegate", entry, typeof(DelegateEntry));
            }
            if (target != null)
            {
                string str = "target" + targetIndex;
                info.AddValue(str, entry.target);
                entry.target = str;
            }
            string name = "method" + targetIndex;
            info.AddValue(name, method);
            return entry;
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DelegateSerHolderSerial"));
        }

        [SecurityCritical]
        public object GetRealObject(StreamingContext context)
        {
            int index = 0;
            for (DelegateEntry entry = this.m_delegateEntry; entry != null; entry = entry.Entry)
            {
                index++;
            }
            int num2 = index - 1;
            if (index == 1)
            {
                return this.GetDelegate(this.m_delegateEntry, 0);
            }
            object[] invocationList = new object[index];
            for (DelegateEntry entry2 = this.m_delegateEntry; entry2 != null; entry2 = entry2.Entry)
            {
                index--;
                invocationList[index] = this.GetDelegate(entry2, num2 - index);
            }
            return ((MulticastDelegate) invocationList[0]).NewMulticastDelegate(invocationList, invocationList.Length);
        }

        private DelegateEntry OldDelegateWireFormat(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            string type = info.GetString("DelegateType");
            string assembly = info.GetString("DelegateAssembly");
            object target = info.GetValue("Target", typeof(object));
            string targetTypeAssembly = info.GetString("TargetTypeAssembly");
            string targetTypeName = info.GetString("TargetTypeName");
            return new DelegateEntry(type, assembly, target, targetTypeAssembly, targetTypeName, info.GetString("MethodName"));
        }

        private void ThrowInsufficientState(string field)
        {
            throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientDeserializationState", new object[] { field }));
        }

        [Serializable]
        internal class DelegateEntry
        {
            internal string assembly;
            internal DelegateSerializationHolder.DelegateEntry delegateEntry;
            internal string methodName;
            internal object target;
            internal string targetTypeAssembly;
            internal string targetTypeName;
            internal string type;

            internal DelegateEntry(string type, string assembly, object target, string targetTypeAssembly, string targetTypeName, string methodName)
            {
                this.type = type;
                this.assembly = assembly;
                this.target = target;
                this.targetTypeAssembly = targetTypeAssembly;
                this.targetTypeName = targetTypeName;
                this.methodName = methodName;
            }

            internal DelegateSerializationHolder.DelegateEntry Entry
            {
                get
                {
                    return this.delegateEntry;
                }
                set
                {
                    this.delegateEntry = value;
                }
            }
        }
    }
}

