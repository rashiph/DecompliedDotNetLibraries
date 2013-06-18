namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;

    internal interface IConfigCallback
    {
        bool AfterSaveChanges(object a, object key);
        bool Configure(object a, object key);
        void ConfigureDefaults(object a, object key);
        void ConfigureSubCollections(ICatalogCollection coll);
        object FindObject(ICatalogCollection coll, object key);
        IEnumerator GetEnumerator();
    }
}

