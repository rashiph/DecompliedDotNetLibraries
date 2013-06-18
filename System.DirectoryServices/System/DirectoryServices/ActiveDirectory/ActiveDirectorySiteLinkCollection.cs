namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ActiveDirectorySiteLinkCollection : CollectionBase
    {
        internal DirectoryContext context;
        internal DirectoryEntry de;
        internal bool initialized;

        internal ActiveDirectorySiteLinkCollection()
        {
        }

        public int Add(ActiveDirectorySiteLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException("link");
            }
            if (!link.existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", new object[] { link.Name }));
            }
            if (this.Contains(link))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { link }), "link");
            }
            return base.List.Add(link);
        }

        public void AddRange(ActiveDirectorySiteLink[] links)
        {
            if (links == null)
            {
                throw new ArgumentNullException("links");
            }
            for (int i = 0; i < links.Length; i++)
            {
                this.Add(links[i]);
            }
        }

        public void AddRange(ActiveDirectorySiteLinkCollection links)
        {
            if (links == null)
            {
                throw new ArgumentNullException("links");
            }
            int count = links.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(links[i]);
            }
        }

        public bool Contains(ActiveDirectorySiteLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException("link");
            }
            if (!link.existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", new object[] { link.Name }));
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

        public void CopyTo(ActiveDirectorySiteLink[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(ActiveDirectorySiteLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException("link");
            }
            if (!link.existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", new object[] { link.Name }));
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

        public void Insert(int index, ActiveDirectorySiteLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!link.existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", new object[] { link.Name }));
            }
            if (this.Contains(link))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { link }), "link");
            }
            base.List.Insert(index, link);
        }

        protected override void OnClearComplete()
        {
            if (this.initialized)
            {
                try
                {
                    if (this.de.Properties.Contains("siteLinkList"))
                    {
                        this.de.Properties["siteLinkList"].Clear();
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        protected override void OnInsertComplete(int index, object value)
        {
            if (this.initialized)
            {
                ActiveDirectorySiteLink link = (ActiveDirectorySiteLink) value;
                string str = (string) PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
                try
                {
                    this.de.Properties["siteLinkList"].Add(str);
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            ActiveDirectorySiteLink link = (ActiveDirectorySiteLink) value;
            string str = (string) PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
            try
            {
                this.de.Properties["siteLinkList"].Remove(str);
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
        }

        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            ActiveDirectorySiteLink link = (ActiveDirectorySiteLink) newValue;
            string str = (string) PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
            try
            {
                this.de.Properties["siteLinkList"][index] = str;
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
        }

        protected override void OnValidate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(value is ActiveDirectorySiteLink))
            {
                throw new ArgumentException("value");
            }
            if (!((ActiveDirectorySiteLink) value).existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", new object[] { ((ActiveDirectorySiteLink) value).Name }));
            }
        }

        public void Remove(ActiveDirectorySiteLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException("link");
            }
            if (!link.existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", new object[] { link.Name }));
            }
            string str = (string) PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySiteLink link2 = (ActiveDirectorySiteLink) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(link2.context, link2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    base.List.Remove(link2);
                    return;
                }
            }
            throw new ArgumentException(Res.GetString("NotFoundInCollection", new object[] { link }), "link");
        }

        public ActiveDirectorySiteLink this[int index]
        {
            get
            {
                return (ActiveDirectorySiteLink) base.InnerList[index];
            }
            set
            {
                ActiveDirectorySiteLink link = value;
                if (link == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!link.existing)
                {
                    throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", new object[] { link.Name }));
                }
                if (this.Contains(link))
                {
                    throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { link }), "value");
                }
                base.List[index] = link;
            }
        }
    }
}

