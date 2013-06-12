namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SchemaSupport
    {
        internal Guid _schemaRowset;
        internal int _restrictions;
    }
}

