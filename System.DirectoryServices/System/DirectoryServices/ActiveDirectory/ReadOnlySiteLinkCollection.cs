namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ReadOnlySiteLinkCollection : ReadOnlyCollectionBase
    {
        internal ReadOnlySiteLinkCollection()
        {
        }

        internal int Add(ActiveDirectorySiteLink link)
        {
            return base.InnerList.Add(link);
        }

        internal void Clear()
        {
            base.InnerList.Clear();
        }

        public bool Contains(ActiveDirectorySiteLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException("link");
            }
            string str = (string) PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySiteLink link2 = (ActiveDirectorySiteLink) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(link2.context, link2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ActiveDirectorySiteLink[] links, int index)
        {
            base.InnerList.CopyTo(links, index);
        }

        public int IndexOf(ActiveDirectorySiteLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException("link");
            }
            string str = (string) PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySiteLink link2 = (ActiveDirectorySiteLink) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(link2.context, link2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public ActiveDirectorySiteLink this[int index]
        {
            get
            {
                return (ActiveDirectorySiteLink) base.InnerList[index];
            }
        }
    }
}

