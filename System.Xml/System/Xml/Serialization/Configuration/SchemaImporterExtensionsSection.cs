namespace System.Xml.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.Xml.Serialization.Advanced;

    public sealed class SchemaImporterExtensionsSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty schemaImporterExtensions = new ConfigurationProperty(null, typeof(SchemaImporterExtensionElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        public SchemaImporterExtensionsSection()
        {
            this.properties.Add(this.schemaImporterExtensions);
        }

        private static string GetSqlTypeSchemaImporter(string typeName)
        {
            return ("System.Data.SqlTypes." + typeName + ", System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        }

        protected override void InitializeDefault()
        {
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterChar", GetSqlTypeSchemaImporter("TypeCharSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterNChar", GetSqlTypeSchemaImporter("TypeNCharSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterVarChar", GetSqlTypeSchemaImporter("TypeVarCharSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterNVarChar", GetSqlTypeSchemaImporter("TypeNVarCharSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterText", GetSqlTypeSchemaImporter("TypeTextSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterNText", GetSqlTypeSchemaImporter("TypeNTextSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterVarBinary", GetSqlTypeSchemaImporter("TypeVarBinarySchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterBinary", GetSqlTypeSchemaImporter("TypeBinarySchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterImage", GetSqlTypeSchemaImporter("TypeVarImageSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterDecimal", GetSqlTypeSchemaImporter("TypeDecimalSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterNumeric", GetSqlTypeSchemaImporter("TypeNumericSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterBigInt", GetSqlTypeSchemaImporter("TypeBigIntSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterInt", GetSqlTypeSchemaImporter("TypeIntSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterSmallInt", GetSqlTypeSchemaImporter("TypeSmallIntSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterTinyInt", GetSqlTypeSchemaImporter("TypeTinyIntSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterBit", GetSqlTypeSchemaImporter("TypeBitSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterFloat", GetSqlTypeSchemaImporter("TypeFloatSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterReal", GetSqlTypeSchemaImporter("TypeRealSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterDateTime", GetSqlTypeSchemaImporter("TypeDateTimeSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterSmallDateTime", GetSqlTypeSchemaImporter("TypeSmallDateTimeSchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterMoney", GetSqlTypeSchemaImporter("TypeMoneySchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterSmallMoney", GetSqlTypeSchemaImporter("TypeSmallMoneySchemaImporterExtension")));
            this.SchemaImporterExtensions.Add(new SchemaImporterExtensionElement("SqlTypesSchemaImporterUniqueIdentifier", GetSqlTypeSchemaImporter("TypeUniqueIdentifierSchemaImporterExtension")));
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public SchemaImporterExtensionElementCollection SchemaImporterExtensions
        {
            get
            {
                return (SchemaImporterExtensionElementCollection) base[this.schemaImporterExtensions];
            }
        }

        internal SchemaImporterExtensionCollection SchemaImporterExtensionsInternal
        {
            get
            {
                SchemaImporterExtensionCollection extensions = new SchemaImporterExtensionCollection();
                foreach (SchemaImporterExtensionElement element in this.SchemaImporterExtensions)
                {
                    extensions.Add(element.Name, element.Type);
                }
                return extensions;
            }
        }
    }
}

