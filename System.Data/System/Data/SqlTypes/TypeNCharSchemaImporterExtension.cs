namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeNCharSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeNCharSchemaImporterExtension() : base("nchar", "System.Data.SqlTypes.SqlString", false)
        {
        }
    }
}

