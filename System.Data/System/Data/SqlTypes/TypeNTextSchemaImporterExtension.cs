namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeNTextSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeNTextSchemaImporterExtension() : base("ntext", "System.Data.SqlTypes.SqlString", false)
        {
        }
    }
}

