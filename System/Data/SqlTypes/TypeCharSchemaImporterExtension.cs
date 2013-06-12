namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeCharSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeCharSchemaImporterExtension() : base("char", "System.Data.SqlTypes.SqlString", false)
        {
        }
    }
}

