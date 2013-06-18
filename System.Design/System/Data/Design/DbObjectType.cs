namespace System.Data.Design
{
    using System;

    internal enum DbObjectType
    {
        Unknown,
        Table,
        View,
        StoredProcedure,
        Function,
        Package,
        PackageBody
    }
}

