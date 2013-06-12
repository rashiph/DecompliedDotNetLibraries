namespace System.Data.SqlTypes
{
    using System;

    public sealed class TypeMoneySchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
    {
        public TypeMoneySchemaImporterExtension() : base("money", "System.Data.SqlTypes.SqlMoney")
        {
        }
    }
}

