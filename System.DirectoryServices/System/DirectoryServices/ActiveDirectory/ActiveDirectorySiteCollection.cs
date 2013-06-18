namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ActiveDirectorySiteCollection : CollectionBase
    {
        internal DirectoryContext context;
        internal DirectoryEntry de;
        internal bool initialized;

        internal ActiveDirectorySiteCollection()
        {
        }

        internal ActiveDirectorySiteCollection(ArrayList sites)
        {
            for (int i = 0; i < sites.Count; i++)
            {
                this.Add((ActiveDirectorySite) sites[i]);
            }
        }

        public int Add(ActiveDirectorySite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            if (!site.existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteNotCommitted", new object[] { site.Name }));
            }
            if (this.Contains(site))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { site }), "site");
            }
            return base.List.Add(site);
        }

        public void AddRange(ActiveDirectorySite[] sites)
        {
            if (sites == null)
            {
                throw new ArgumentNullException("sites");
            }
            for (int i = 0; i < sites.Length; i++)
            {
                this.Add(sites[i]);
            }
        }

        public void AddRange(ActiveDirectorySiteCollection sites)
        {
            if (sites == null)
            {
                throw new ArgumentNullException("sites");
            }
            int count = sites.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(sites[i]);
            }
        }

        public bool Contains(ActiveDirectorySite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            if (!site.existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteNotCommitted", new object[] { site.Name }));
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

        public void CopyTo(ActiveDirectorySite[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(ActiveDirectorySite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            if (!site.existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteNotCommitted", new object[] { site.Name }));
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

        public void Insert(int index, ActiveDirectorySite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            if (!site.existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteNotCommitted", new object[] { site.Name }));
            }
            if (this.Contains(site))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { site }), "site");
            }
            base.List.Insert(index, site);
        }

        protected override void OnClearComplete()
        {
            if (this.initialized)
            {
                try
                {
                    if (this.de.Properties.Contains("siteList"))
                    {
                        this.de.Properties["siteList"].Clear();
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
                ActiveDirectorySite site = (ActiveDirectorySite) value;
                string str = (string) PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
                try
                {
                    this.de.Properties["siteList"].Add(str);
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            ActiveDirectorySite site = (ActiveDirectorySite) value;
            string str = (string) PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
            try
            {
                this.de.Properties["siteList"].Remove(str);
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
        }

        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            ActiveDirectorySite site = (ActiveDirectorySite) newValue;
            string str = (string) PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
            try
            {
                this.de.Properties["siteList"][index] = str;
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
            if (!(value is ActiveDirectorySite))
            {
                throw new ArgumentException("value");
            }
            if (!((ActiveDirectorySite) value).existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteNotCommitted", new object[] { ((ActiveDirectorySite) value).Name }));
            }
        }

        public void Remove(ActiveDirectorySite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            if (!site.existing)
            {
                throw new InvalidOperationException(Res.GetString("SiteNotCommitted", new object[] { site.Name }));
            }
            string str = (string) PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySite site2 = (ActiveDirectorySite) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(site2.context, site2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    base.List.Remove(site2);
                    return;
                }
            }
            throw new ArgumentException(Res.GetString("NotFoundInCollection", new object[] { site }), "site");
        }

        public ActiveDirectorySite this[int index]
        {
            get
            {
                return (ActiveDirectorySite) base.InnerList[index];
            }
            set
            {
                ActiveDirectorySite site = value;
                if (site == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!site.existing)
                {
                    throw new InvalidOperationException(Res.GetString("SiteNotCommitted", new object[] { site.Name }));
                }
                if (this.Contains(site))
                {
                    throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { site }), "value");
                }
                base.List[index] = site;
            }
        }
    }
}

