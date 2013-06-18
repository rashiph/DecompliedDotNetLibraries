namespace System.Data.Common
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SchemaInfo
    {
        public string name;
        public string typeName;
        public Type type;
    }
}

