namespace System.Xml.Serialization.Configuration
{
    using System;
    using System.Globalization;

    internal static class ConfigurationStrings
    {
        internal const string CheckDeserializeAdvances = "checkDeserializeAdvances";
        internal const string DateTimeSerializationSectionName = "dateTimeSerialization";
        internal const string Mode = "mode";
        internal const string Name = "name";
        internal const string SchemaImporterExtensionsSectionName = "schemaImporterExtensions";
        internal const string SectionGroupName = "system.xml.serialization";
        internal const string SqlTypesSchemaImporterBigInt = "SqlTypesSchemaImporterBigInt";
        internal const string SqlTypesSchemaImporterBinary = "SqlTypesSchemaImporterBinary";
        internal const string SqlTypesSchemaImporterBit = "SqlTypesSchemaImporterBit";
        internal const string SqlTypesSchemaImporterChar = "SqlTypesSchemaImporterChar";
        internal const string SqlTypesSchemaImporterDateTime = "SqlTypesSchemaImporterDateTime";
        internal const string SqlTypesSchemaImporterDecimal = "SqlTypesSchemaImporterDecimal";
        internal const string SqlTypesSchemaImporterFloat = "SqlTypesSchemaImporterFloat";
        internal const string SqlTypesSchemaImporterImage = "SqlTypesSchemaImporterImage";
        internal const string SqlTypesSchemaImporterInt = "SqlTypesSchemaImporterInt";
        internal const string SqlTypesSchemaImporterMoney = "SqlTypesSchemaImporterMoney";
        internal const string SqlTypesSchemaImporterNChar = "SqlTypesSchemaImporterNChar";
        internal const string SqlTypesSchemaImporterNText = "SqlTypesSchemaImporterNText";
        internal const string SqlTypesSchemaImporterNumeric = "SqlTypesSchemaImporterNumeric";
        internal const string SqlTypesSchemaImporterNVarChar = "SqlTypesSchemaImporterNVarChar";
        internal const string SqlTypesSchemaImporterReal = "SqlTypesSchemaImporterReal";
        internal const string SqlTypesSchemaImporterSmallDateTime = "SqlTypesSchemaImporterSmallDateTime";
        internal const string SqlTypesSchemaImporterSmallInt = "SqlTypesSchemaImporterSmallInt";
        internal const string SqlTypesSchemaImporterSmallMoney = "SqlTypesSchemaImporterSmallMoney";
        internal const string SqlTypesSchemaImporterText = "SqlTypesSchemaImporterText";
        internal const string SqlTypesSchemaImporterTinyInt = "SqlTypesSchemaImporterTinyInt";
        internal const string SqlTypesSchemaImporterUniqueIdentifier = "SqlTypesSchemaImporterUniqueIdentifier";
        internal const string SqlTypesSchemaImporterVarBinary = "SqlTypesSchemaImporterVarBinary";
        internal const string SqlTypesSchemaImporterVarChar = "SqlTypesSchemaImporterVarChar";
        internal const string TempFilesLocation = "tempFilesLocation";
        internal const string Type = "type";
        internal const string XmlSerializerSectionName = "xmlSerializer";

        private static string GetSectionPath(string sectionName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { "system.xml.serialization", sectionName });
        }

        internal static string DateTimeSerializationSectionPath
        {
            get
            {
                return GetSectionPath("dateTimeSerialization");
            }
        }

        internal static string SchemaImporterExtensionsSectionPath
        {
            get
            {
                return GetSectionPath("schemaImporterExtensions");
            }
        }

        internal static string XmlSerializerSectionPath
        {
            get
            {
                return GetSectionPath("xmlSerializer");
            }
        }
    }
}

