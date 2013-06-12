namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeUniqueIdentifierSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeUniqueIdentifierSchemaImporterExtension() : base("uniqueidentifier", "System.Data.SqlTypes.SqlGuid")
        {
        }
    }
}

