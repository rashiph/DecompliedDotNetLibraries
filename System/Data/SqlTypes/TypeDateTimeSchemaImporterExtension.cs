namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeDateTimeSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeDateTimeSchemaImporterExtension() : base("datetime", "System.Data.SqlTypes.SqlDateTime")
        {
        }
    }
}

