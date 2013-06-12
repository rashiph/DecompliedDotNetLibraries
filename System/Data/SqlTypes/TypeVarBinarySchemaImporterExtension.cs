namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeVarBinarySchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeVarBinarySchemaImporterExtension() : base("varbinary", "System.Data.SqlTypes.SqlBinary", false)
        {
        }
    }
}

