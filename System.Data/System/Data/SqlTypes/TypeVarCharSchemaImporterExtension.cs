namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeVarCharSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeVarCharSchemaImporterExtension() : base("varchar", "System.Data.SqlTypes.SqlString", false)
        {
        }
    }
}

