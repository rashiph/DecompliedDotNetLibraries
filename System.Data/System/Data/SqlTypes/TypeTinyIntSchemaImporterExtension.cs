namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeTinyIntSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeTinyIntSchemaImporterExtension() : base("tinyint", "System.Data.SqlTypes.SqlByte")
        {
        }
    }
}

