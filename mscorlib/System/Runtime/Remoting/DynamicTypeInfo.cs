namespace System.Runtime.Remoting
{
    using System;
    using System.Security;

    [Serializable]
    internal class DynamicTypeInfo : TypeInfo
    {
        [SecurityCritical]
        internal DynamicTypeInfo(RuntimeType typeOfObj) : base(typeOfObj)
        {
        }

        [SecurityCritical]
        public override bool CanCastTo(Type castType, object o)
        {
            return ((MarshalByRefObject) o).IsInstanceOfType(castType);
        }
    }
}

