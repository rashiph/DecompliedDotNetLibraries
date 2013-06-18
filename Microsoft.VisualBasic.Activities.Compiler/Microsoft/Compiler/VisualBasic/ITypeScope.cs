namespace Microsoft.Compiler.VisualBasic
{
    using System;
    using System.Runtime.InteropServices;

    internal interface ITypeScope
    {
        Type[] FindTypes(string typeName, string nsPrefix);
        [return: MarshalAs(UnmanagedType.U1)]
        bool NamespaceExists(string ns);
    }
}

