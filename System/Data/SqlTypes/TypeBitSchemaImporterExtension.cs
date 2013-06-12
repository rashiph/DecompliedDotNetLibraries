namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeBitSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeBitSchemaImporterExtension() : base("bit", "System.Data.SqlTypes.SqlBoolean")
        {
        }
    }
}

