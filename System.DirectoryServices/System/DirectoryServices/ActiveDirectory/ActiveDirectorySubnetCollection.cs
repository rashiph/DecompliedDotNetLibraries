namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    public class ActiveDirectorySubnetCollection : CollectionBase
    {
        internal Hashtable changeList;
        private DirectoryContext context;
        private ArrayList copyList = new ArrayList();
        internal bool initialized;
        private string siteDN;

        internal ActiveDirectorySubnetCollection(DirectoryContext context, string siteDN)
        {
            this.context = context;
            this.siteDN = siteDN;
            Hashtable table = new Hashtable();
            this.changeList = Hashtable.Synchronized(table);
        }

        public int Add(ActiveDirectorySubnet subnet)
        {
            if (subnet == null)
            {
                throw new ArgumentNullException("subnet");
            }
            if (!subnet.existing)
            {
                throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", new object[] { subnet.Name }));
            }
            if (this.Contains(subnet))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { subnet }), "subnet");
            }
            return base.List.Add(subnet);
        }

        public void AddRange(ActiveDirectorySubnet[] subnets)
        {
            if (subnets == null)
            {
                throw new ArgumentNullException("subnets");
            }
            ActiveDirectorySubnet[] subnetArray = subnets;
            for (int i = 0; i < subnetArray.Length; i++)
            {
                if (subnetArray[i] == null)
                {
                    throw new ArgumentException("subnets");
                }
            }
            for (int j = 0; j < subnets.Length; j++)
            {
                this.Add(subnets[j]);
            }
        }

        public void AddRange(ActiveDirectorySubnetCollection subnets)
        {
            if (subnets == null)
            {
                throw new ArgumentNullException("subnets");
            }
            int count = subnets.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(subnets[i]);
            }
        }

        public bool Contains(ActiveDirectorySubnet subnet)
        {
            if (subnet == null)
            {
                throw new ArgumentNullException("subnet");
            }
            if (!subnet.existing)
            {
                throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", new object[] { subnet.Name }));
            }
            string str = (string) PropertyManager.GetPropertyValue(subnet.context, subnet.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySubnet subnet2 = (ActiveDirectorySubnet) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(subnet2.context, subnet2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ActiveDirectorySubnet[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(ActiveDirectorySubnet subnet)
        {
            if (subnet == null)
            {
                throw new ArgumentNullException("subnet");
            }
            if (!subnet.existing)
            {
                throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", new object[] { subnet.Name }));
            }
            string str = (string) PropertyManager.GetPropertyValue(subnet.context, subnet.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySubnet subnet2 = (ActiveDirectorySubnet) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(subnet2.context, subnet2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, ActiveDirectorySubnet subnet)
        {
            if (subnet == null)
            {
                throw new ArgumentNullException("subnet");
            }
            if (!subnet.existing)
            {
                throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", new object[] { subnet.Name }));
            }
            if (this.Contains(subnet))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { subnet }), "subnet");
            }
            base.List.Insert(index, subnet);
        }

        private string MakePath(string subnetDN)
        {
            string rdnFromDN = Utils.GetRdnFromDN(subnetDN);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < rdnFromDN.Length; i++)
            {
                if (rdnFromDN[i] == '/')
                {
                    builder.Append('\\');
                }
                builder.Append(rdnFromDN[i]);
            }
            return (builder.ToString() + "," + subnetDN.Substring(rdnFromDN.Length + 1));
        }

        protected override void OnClear()
        {
            if (this.initialized)
            {
                this.copyList.Clear();
                foreach (object obj2 in base.List)
                {
                    this.copyList.Add(obj2);
                }
            }
        }

        protected override void OnClearComplete()
        {
            if (this.initialized)
            {
                for (int i = 0; i < this.copyList.Count; i++)
                {
                    this.OnRemoveComplete(i, this.copyList[i]);
                }
            }
        }

        protected override void OnInsertComplete(int index, object value)
        {
            if (this.initialized)
            {
                ActiveDirectorySubnet subnet = (ActiveDirectorySubnet) value;
                string key = (string) PropertyManager.GetPropertyValue(subnet.context, subnet.cachedEntry, PropertyManager.DistinguishedName);
                try
                {
                    if (this.changeList.Contains(key))
                    {
                        ((DirectoryEntry) this.changeList[key]).Properties["siteObject"].Value = this.siteDN;
                    }
                    else
                    {
                        DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.MakePath(key));
                        directoryEntry.Properties["siteObject"].Value = this.siteDN;
                        this.changeList.Add(key, directoryEntry);
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            ActiveDirectorySubnet subnet = (ActiveDirectorySubnet) value;
            string key = (string) PropertyManager.GetPropertyValue(subnet.context, subnet.cachedEntry, PropertyManager.DistinguishedName);
            try
            {
                if (this.changeList.Contains(key))
                {
                    ((DirectoryEntry) this.changeList[key]).Properties["siteObject"].Clear();
                }
                else
                {
                    DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.MakePath(key));
                    directoryEntry.Properties["siteObject"].Clear();
                    this.changeList.Add(key, directoryEntry);
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
        }

        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            this.OnRemoveComplete(index, oldValue);
            this.OnInsertComplete(index, newValue);
        }

        protected override void OnValidate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(value is ActiveDirectorySubnet))
            {
                throw new ArgumentException("value");
            }
            if (!((ActiveDirectorySubnet) value).existing)
            {
                throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", new object[] { ((ActiveDirectorySubnet) value).Name }));
            }
        }

        public void Remove(ActiveDirectorySubnet subnet)
        {
            if (subnet == null)
            {
                throw new ArgumentNullException("subnet");
            }
            if (!subnet.existing)
            {
                throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", new object[] { subnet.Name }));
            }
            string str = (string) PropertyManager.GetPropertyValue(subnet.context, subnet.cachedEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySubnet subnet2 = (ActiveDirectorySubnet) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(subnet2.context, subnet2.cachedEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    base.List.Remove(subnet2);
                    return;
                }
            }
            throw new ArgumentException(Res.GetString("NotFoundInCollection", new object[] { subnet }), "subnet");
        }

        public ActiveDirectorySubnet this[int index]
        {
            get
            {
                return (ActiveDirectorySubnet) base.InnerList[index];
            }
            set
            {
                ActiveDirectorySubnet subnet = value;
                if (subnet == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!subnet.existing)
                {
                    throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", new object[] { subnet.Name }));
                }
                if (this.Contains(subnet))
                {
                    throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { subnet }), "value");
                }
                base.List[index] = subnet;
            }
        }
    }
}

