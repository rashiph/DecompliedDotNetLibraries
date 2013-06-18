namespace System.ServiceModel.ComIntegration
{
    using System;

    internal class ComCatalogObject
    {
        private ICatalogCollection catalogCollection;
        private ICatalogObject catalogObject;

        public ComCatalogObject(ICatalogObject catalogObject, ICatalogCollection catalogCollection)
        {
            this.catalogObject = catalogObject;
            this.catalogCollection = catalogCollection;
        }

        public ComCatalogCollection GetCollection(string collectionName)
        {
            ICatalogCollection collection = (ICatalogCollection) this.catalogCollection.GetCollection(collectionName, this.catalogObject.Key());
            collection.Populate();
            return new ComCatalogCollection(collection);
        }

        public object GetValue(string key)
        {
            return this.catalogObject.GetValue(key);
        }

        public string Name
        {
            get
            {
                return (string) this.catalogObject.Name();
            }
        }
    }
}

