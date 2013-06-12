namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeBinarySchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeBinarySchemaImporterExtension() : base("binary", "System.Data.SqlTypes.SqlBinary", false)
        {
        }
    }
}

