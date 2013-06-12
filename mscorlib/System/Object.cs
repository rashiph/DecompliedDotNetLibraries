namespace System
{
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDual), ComVisible(true)]
    public class Object
    {
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual bool Equals(object obj)
        {
            return RuntimeHelpers.Equals(this, obj);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool Equals(object objA, object objB)
        {
            return ((objA == objB) || (((objA != null) && (objB != null)) && objA.Equals(objB)));
        }

        private void FieldGetter(string typeName, string fieldName, ref object val)
        {
            val = this.GetFieldInfo(typeName, fieldName).GetValue(this);
        }

        [SecurityCritical]
        private void FieldSetter(string typeName, string fieldName, object val)
        {
            FieldInfo fieldInfo = this.GetFieldInfo(typeName, fieldName);
            if (fieldInfo.IsInitOnly)
            {
                throw new FieldAccessException(Environment.GetResourceString("FieldAccess_InitOnly"));
            }
            Message.CoerceArg(val, fieldInfo.FieldType);
            fieldInfo.SetValue(this, val);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        protected virtual void Finalize()
        {
        }

        private FieldInfo GetFieldInfo(string typeName, string fieldName)
        {
            Type baseType = this.GetType();
            while (null != baseType)
            {
                if (baseType.FullName.Equals(typeName))
                {
                    break;
                }
                baseType = baseType.BaseType;
            }
            if (null == baseType)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadType"), new object[] { typeName }));
            }
            FieldInfo field = baseType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (null == field)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadField"), new object[] { fieldName, typeName }));
            }
            return field;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern Type GetType();
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        protected extern object MemberwiseClone();
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool ReferenceEquals(object objA, object objB)
        {
            return (objA == objB);
        }

        public virtual string ToString()
        {
            return this.GetType().ToString();
        }
    }
}

