namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ReadOnlySiteCollection : ReadOnlyCollectionBase
    {
        internal ReadOnlySiteCollection()
        {
        }

        internal ReadOnlySiteCollection(ArrayList sites)
        {
            for (int i = 0; i < sites.Count; i++)
            {
                this.Add((ActiveDirectorySite) sites[i]);
            }
        }

        internal int Add(ActiveDirectorySite site)
        {
            return base.InnerList.Add(site);
        }

        internal void Clear()
        {
            base.InnerList.Clear();
        }

        public bool Contains(ActiveDirectorySite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            string str = (string) PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySite site2 = (ActiveDirectorySite) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(site2.context, site2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ActiveDirectorySite[] sites, int index)
        {
            base.InnerList.CopyTo(sites, index);
        }

        public int IndexOf(ActiveDirectorySite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            string str = (string) PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySite site2 = (ActiveDirectorySite) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(site2.context, site2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public ActiveDirectorySite this[int index]
        {
            get
            {
                return (ActiveDirectorySite) base.InnerList[index];
            }
        }
    }
}

