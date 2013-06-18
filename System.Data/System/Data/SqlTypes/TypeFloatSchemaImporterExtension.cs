namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeFloatSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeFloatSchemaImporterExtension() : base("float", "System.Data.SqlTypes.SqlDouble")
        {
        }
    }
}

