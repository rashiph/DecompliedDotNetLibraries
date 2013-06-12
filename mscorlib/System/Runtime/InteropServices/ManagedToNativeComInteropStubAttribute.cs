namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(false), AttributeUsage(AttributeTargets.Method, Inherited=false, AllowMultiple=false)]
    public sealed class ManagedToNativeComInteropStubAttribute : Attribute
    {
        internal Type _classType;
        internal string _methodName;

        public ManagedToNativeComInteropStubAttribute(Type classType, string methodName)
        {
            this._classType = classType;
            this._methodName = methodName;
        }

        public Type ClassType
        {
            get
            {
                return this._classType;
            }
        }

        public string MethodName
        {
            get
            {
                return this._methodName;
            }
        }
    }
}

