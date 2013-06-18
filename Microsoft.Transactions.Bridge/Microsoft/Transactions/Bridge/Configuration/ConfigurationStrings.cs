namespace Microsoft.Transactions.Bridge.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;

    internal static class ConfigurationStrings
    {
        internal const string AddressPrefix = "addressPrefix";
        internal const string Protocols = "protocols";
        internal const string SectionGroupName = "microsoft.transactions.bridge";
        internal const string TransactionBridgeSectionName = "transactionBridge";
        internal const string TransactionManagerType = "transactionManagerType";
        internal const string Type = "type";
        internal const string WSTransactionSectionName = "wsTransaction";

        internal static object GetSection(string sectionPath)
        {
            return ConfigurationManager.GetSection(sectionPath);
        }

        internal static string GetSectionPath(string sectionName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { "microsoft.transactions.bridge", sectionName });
        }
    }
}

