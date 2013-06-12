namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeVarImageSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeVarImageSchemaImporterExtension() : base("image", "System.Data.SqlTypes.SqlBinary", false)
        {
        }
    }
}

