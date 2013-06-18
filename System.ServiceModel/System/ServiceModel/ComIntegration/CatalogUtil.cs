namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal static class CatalogUtil
    {
        internal static ComCatalogObject FindApplication(Guid applicationId)
        {
            ICatalog2 catalog = (ICatalog2) new xCatalog();
            ICatalogObject catalogObject = null;
            ICatalogCollection collection = null;
            try
            {
                collection = (ICatalogCollection) catalog.GetCollection("Partitions");
                collection.Populate();
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != HR.COMADMIN_E_PARTITIONS_DISABLED)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }
            }
            if (collection != null)
            {
                for (int i = 0; i < collection.Count(); i++)
                {
                    ICatalogObject obj3 = (ICatalogObject) collection.Item(i);
                    ICatalogCollection appCollection = (ICatalogCollection) collection.GetCollection("Applications", obj3.Key());
                    appCollection.Populate();
                    catalogObject = FindApplication(appCollection, applicationId);
                    if (catalogObject != null)
                    {
                        return new ComCatalogObject(catalogObject, appCollection);
                    }
                }
            }
            else
            {
                ICatalogCollection catalogs3 = (ICatalogCollection) catalog.GetCollection("Applications");
                catalogs3.Populate();
                catalogObject = FindApplication(catalogs3, applicationId);
                if (catalogObject != null)
                {
                    return new ComCatalogObject(catalogObject, catalogs3);
                }
            }
            return null;
        }

        private static ICatalogObject FindApplication(ICatalogCollection appCollection, Guid applicationId)
        {
            ICatalogObject obj2 = null;
            for (int i = 0; i < appCollection.Count(); i++)
            {
                obj2 = (ICatalogObject) appCollection.Item(i);
                if (Fx.CreateGuid((string) obj2.GetValue("ID")) == applicationId)
                {
                    return obj2;
                }
            }
            return null;
        }

        internal static string[] GetRoleMembers(ComCatalogObject application, ComCatalogCollection rolesCollection)
        {
            ComCatalogCollection collection = application.GetCollection("Roles");
            List<string> list = new List<string>();
            ComCatalogCollection.Enumerator enumerator = rolesCollection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ComCatalogObject current = enumerator.Current;
                string str = (string) current.GetValue("Name");
                ComCatalogCollection.Enumerator enumerator2 = collection.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    ComCatalogObject obj3 = enumerator2.Current;
                    string str2 = (string) obj3.GetValue("Name");
                    if (str == str2)
                    {
                        ComCatalogCollection.Enumerator enumerator3 = obj3.GetCollection("UsersInRole").GetEnumerator();
                        while (enumerator3.MoveNext())
                        {
                            ComCatalogObject obj4 = enumerator3.Current;
                            string item = (string) obj4.GetValue("User");
                            list.Add(item);
                        }
                        continue;
                    }
                }
            }
            return list.ToArray();
        }
    }
}

