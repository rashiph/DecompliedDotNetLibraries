namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeDecimalSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeDecimalSchemaImporterExtension() : base("decimal", "System.Data.SqlTypes.SqlDecimal", false)
        {
        }
    }
}

