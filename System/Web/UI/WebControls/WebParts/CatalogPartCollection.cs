namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web;

    public sealed class CatalogPartCollection : ReadOnlyCollectionBase
    {
        public static readonly CatalogPartCollection Empty = new CatalogPartCollection();

        public CatalogPartCollection()
        {
        }

        public CatalogPartCollection(ICollection catalogParts)
        {
            this.Initialize(null, catalogParts);
        }

        public CatalogPartCollection(CatalogPartCollection existingCatalogParts, ICollection catalogParts)
        {
            this.Initialize(existingCatalogParts, catalogParts);
        }

        internal int Add(CatalogPart value)
        {
            return base.InnerList.Add(value);
        }

        public bool Contains(CatalogPart catalogPart)
        {
            return base.InnerList.Contains(catalogPart);
        }

        public void CopyTo(CatalogPart[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(CatalogPart catalogPart)
        {
            return base.InnerList.IndexOf(catalogPart);
        }

        private void Initialize(CatalogPartCollection existingCatalogParts, ICollection catalogParts)
        {
            if (existingCatalogParts != null)
            {
                foreach (CatalogPart part in existingCatalogParts)
                {
                    base.InnerList.Add(part);
                }
            }
            if (catalogParts != null)
            {
                foreach (object obj2 in catalogParts)
                {
                    if (obj2 == null)
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Collection_CantAddNull"), "catalogParts");
                    }
                    if (!(obj2 is CatalogPart))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "CatalogPart" }), "catalogParts");
                    }
                    base.InnerList.Add(obj2);
                }
            }
        }

        public CatalogPart this[int index]
        {
            get
            {
                return (CatalogPart) base.InnerList[index];
            }
        }

        public CatalogPart this[string id]
        {
            get
            {
                foreach (CatalogPart part in base.InnerList)
                {
                    if (string.Equals(part.ID, id, StringComparison.OrdinalIgnoreCase))
                    {
                        return part;
                    }
                }
                return null;
            }
        }
    }
}

