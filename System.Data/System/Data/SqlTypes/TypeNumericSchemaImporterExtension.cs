namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeNumericSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeNumericSchemaImporterExtension() : base("numeric", "System.Data.SqlTypes.SqlDecimal", false)
        {
        }
    }
}

