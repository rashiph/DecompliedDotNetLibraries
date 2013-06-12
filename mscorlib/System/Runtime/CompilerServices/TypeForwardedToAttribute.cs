namespace System.Runtime.CompilerServices
{
    using System;
    using System.Reflection;
    using System.Security;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class TypeForwardedToAttribute : Attribute
    {
        private Type _destination;

        public TypeForwardedToAttribute(Type destination)
        {
            this._destination = destination;
        }

        [SecurityCritical]
        internal static TypeForwardedToAttribute[] GetCustomAttribute(RuntimeAssembly assembly)
        {
            Type[] o = null;
            RuntimeAssembly.GetForwardedTypes(assembly.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<Type[]>(ref o));
            TypeForwardedToAttribute[] attributeArray = new TypeForwardedToAttribute[o.Length];
            for (int i = 0; i < o.Length; i++)
            {
                attributeArray[i] = new TypeForwardedToAttribute(o[i]);
            }
            return attributeArray;
        }

        public Type Destination
        {
            get
            {
                return this._destination;
            }
        }
    }
}

