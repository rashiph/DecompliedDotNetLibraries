namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct InterfaceMapping
    {
        [ComVisible(true)]
        public Type TargetType;
        [ComVisible(true)]
        public Type InterfaceType;
        [ComVisible(true)]
        public MethodInfo[] TargetMethods;
        [ComVisible(true)]
        public MethodInfo[] InterfaceMethods;
    }
}

