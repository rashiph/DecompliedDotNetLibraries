namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeTextSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeTextSchemaImporterExtension() : base("text", "System.Data.SqlTypes.SqlString", false)
        {
        }
    }
}

