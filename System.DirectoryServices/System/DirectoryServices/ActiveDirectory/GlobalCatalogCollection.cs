namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class GlobalCatalogCollection : ReadOnlyCollectionBase
    {
        internal GlobalCatalogCollection()
        {
        }

        internal GlobalCatalogCollection(ArrayList values)
        {
            if (values != null)
            {
                base.InnerList.AddRange(values);
            }
        }

        public bool Contains(GlobalCatalog globalCatalog)
        {
            if (globalCatalog == null)
            {
                throw new ArgumentNullException("globalCatalog");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                GlobalCatalog catalog = (GlobalCatalog) base.InnerList[i];
                if (Utils.Compare(catalog.Name, globalCatalog.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(GlobalCatalog[] globalCatalogs, int index)
        {
            base.InnerList.CopyTo(globalCatalogs, index);
        }

        public int IndexOf(GlobalCatalog globalCatalog)
        {
            if (globalCatalog == null)
            {
                throw new ArgumentNullException("globalCatalog");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                GlobalCatalog catalog = (GlobalCatalog) base.InnerList[i];
                if (Utils.Compare(catalog.Name, globalCatalog.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public GlobalCatalog this[int index]
        {
            get
            {
                return (GlobalCatalog) base.InnerList[index];
            }
        }
    }
}

