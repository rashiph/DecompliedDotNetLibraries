namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeNVarCharSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeNVarCharSchemaImporterExtension() : base("nvarchar", "System.Data.SqlTypes.SqlString", false)
        {
        }
    }
}

