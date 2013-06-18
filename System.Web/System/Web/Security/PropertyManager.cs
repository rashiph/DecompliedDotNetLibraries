namespace System.Web.Security
{
    using System;
    using System.Configuration.Provider;
    using System.DirectoryServices;
    using System.Web;

    internal static class PropertyManager
    {
        public static object GetPropertyValue(DirectoryEntry directoryEntry, string propertyName)
        {
            if (directoryEntry.Properties[propertyName].Count != 0)
            {
                return directoryEntry.Properties[propertyName].Value;
            }
            if (directoryEntry.Properties["distinguishedName"].Count != 0)
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_Property_not_found_on_object", new object[] { propertyName, (string) directoryEntry.Properties["distinguishedName"].Value }));
            }
            throw new ProviderException(System.Web.SR.GetString("ADMembership_Property_not_found", new object[] { propertyName }));
        }

        public static object GetSearchResultPropertyValue(SearchResult res, string propertyName)
        {
            ResultPropertyValueCollection values = null;
            values = res.Properties[propertyName];
            if ((values == null) || (values.Count < 1))
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_Property_not_found", new object[] { propertyName }));
            }
            return values[0];
        }
    }
}

