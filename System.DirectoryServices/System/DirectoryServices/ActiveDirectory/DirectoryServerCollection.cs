namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class DirectoryServerCollection : CollectionBase
    {
        internal Hashtable changeList;
        internal DirectoryContext context;
        private ArrayList copyList;
        private DirectoryEntry crossRefEntry;
        internal bool initialized;
        private bool isADAM;
        private bool isForNC;
        internal string siteDN;
        internal string transportDN;

        internal DirectoryServerCollection(DirectoryContext context, string siteDN, string transportName)
        {
            this.copyList = new ArrayList();
            Hashtable table = new Hashtable();
            this.changeList = Hashtable.Synchronized(table);
            this.context = context;
            this.siteDN = siteDN;
            this.transportDN = transportName;
        }

        internal DirectoryServerCollection(DirectoryContext context, DirectoryEntry crossRefEntry, bool isADAM, ReadOnlyDirectoryServerCollection servers)
        {
            this.copyList = new ArrayList();
            this.context = context;
            this.crossRefEntry = crossRefEntry;
            this.isADAM = isADAM;
            this.isForNC = true;
            foreach (DirectoryServer server in servers)
            {
                base.InnerList.Add(server);
            }
        }

        public int Add(DirectoryServer server)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            if (this.isForNC)
            {
                if (!this.isADAM)
                {
                    if (!(server is DomainController))
                    {
                        throw new ArgumentException(Res.GetString("ServerShouldBeDC"), "server");
                    }
                    if (((DomainController) server).NumericOSVersion < 5.2)
                    {
                        throw new ArgumentException(Res.GetString("ServerShouldBeW2K3"), "server");
                    }
                }
                if (this.Contains(server))
                {
                    throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { server }), "server");
                }
                return base.List.Add(server);
            }
            string str = (server is DomainController) ? ((DomainController) server).SiteObjectName : ((AdamInstance) server).SiteObjectName;
            if (Utils.Compare(this.siteDN, str) != 0)
            {
                throw new ArgumentException(Res.GetString("NotWithinSite"));
            }
            if (this.Contains(server))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { server }), "server");
            }
            return base.List.Add(server);
        }

        public void AddRange(DirectoryServer[] servers)
        {
            if (servers == null)
            {
                throw new ArgumentNullException("servers");
            }
            DirectoryServer[] serverArray = servers;
            for (int i = 0; i < serverArray.Length; i++)
            {
                if (serverArray[i] == null)
                {
                    throw new ArgumentException("servers");
                }
            }
            for (int j = 0; j < servers.Length; j++)
            {
                this.Add(servers[j]);
            }
        }

        public bool Contains(DirectoryServer server)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                DirectoryServer server2 = (DirectoryServer) base.InnerList[i];
                if (Utils.Compare(server2.Name, server.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(DirectoryServer[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        internal string[] GetMultiValuedProperty()
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                DirectoryServer server = (DirectoryServer) base.InnerList[i];
                string str = (server is DomainController) ? ((DomainController) server).NtdsaObjectName : ((AdamInstance) server).NtdsaObjectName;
                list.Add(str);
            }
            return (string[]) list.ToArray(typeof(string));
        }

        public int IndexOf(DirectoryServer server)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                DirectoryServer server2 = (DirectoryServer) base.InnerList[i];
                if (Utils.Compare(server2.Name, server.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, DirectoryServer server)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            if (this.isForNC)
            {
                if (!this.isADAM)
                {
                    if (!(server is DomainController))
                    {
                        throw new ArgumentException(Res.GetString("ServerShouldBeDC"), "server");
                    }
                    if (((DomainController) server).NumericOSVersion < 5.2)
                    {
                        throw new ArgumentException(Res.GetString("ServerShouldBeW2K3"), "server");
                    }
                }
                if (this.Contains(server))
                {
                    throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { server }), "server");
                }
                base.List.Insert(index, server);
            }
            else
            {
                string str = (server is DomainController) ? ((DomainController) server).SiteObjectName : ((AdamInstance) server).SiteObjectName;
                if (Utils.Compare(this.siteDN, str) != 0)
                {
                    throw new ArgumentException(Res.GetString("NotWithinSite"), "server");
                }
                if (this.Contains(server))
                {
                    throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { server }));
                }
                base.List.Insert(index, server);
            }
        }

        protected override void OnClear()
        {
            if (this.initialized && !this.isForNC)
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
            if (this.isForNC)
            {
                if (this.crossRefEntry == null)
                {
                    return;
                }
                try
                {
                    if (this.crossRefEntry.Properties.Contains(PropertyManager.MsDSNCReplicaLocations))
                    {
                        this.crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].Clear();
                    }
                    return;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
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
            if (this.isForNC)
            {
                if (this.crossRefEntry == null)
                {
                    return;
                }
                try
                {
                    DirectoryServer server = (DirectoryServer) value;
                    string str = (server is DomainController) ? ((DomainController) server).NtdsaObjectName : ((AdamInstance) server).NtdsaObjectName;
                    this.crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].Add(str);
                    return;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
            if (this.initialized)
            {
                DirectoryServer server2 = (DirectoryServer) value;
                string name = server2.Name;
                string dn = (server2 is DomainController) ? ((DomainController) server2).ServerObjectName : ((AdamInstance) server2).ServerObjectName;
                try
                {
                    if (this.changeList.Contains(name))
                    {
                        ((DirectoryEntry) this.changeList[name]).Properties["bridgeheadTransportList"].Value = this.transportDN;
                    }
                    else
                    {
                        DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
                        directoryEntry.Properties["bridgeheadTransportList"].Value = this.transportDN;
                        this.changeList.Add(name, directoryEntry);
                    }
                }
                catch (COMException exception2)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception2);
                }
            }
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            if (this.isForNC)
            {
                try
                {
                    if (this.crossRefEntry != null)
                    {
                        string str = (value is DomainController) ? ((DomainController) value).NtdsaObjectName : ((AdamInstance) value).NtdsaObjectName;
                        this.crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].Remove(str);
                    }
                    return;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
            DirectoryServer server = (DirectoryServer) value;
            string name = server.Name;
            string dn = (server is DomainController) ? ((DomainController) server).ServerObjectName : ((AdamInstance) server).ServerObjectName;
            try
            {
                if (this.changeList.Contains(name))
                {
                    ((DirectoryEntry) this.changeList[name]).Properties["bridgeheadTransportList"].Clear();
                }
                else
                {
                    DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
                    directoryEntry.Properties["bridgeheadTransportList"].Clear();
                    this.changeList.Add(name, directoryEntry);
                }
            }
            catch (COMException exception2)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception2);
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
            if (this.isForNC)
            {
                if (this.isADAM)
                {
                    if (!(value is AdamInstance))
                    {
                        throw new ArgumentException(Res.GetString("ServerShouldBeAI"), "value");
                    }
                }
                else if (!(value is DomainController))
                {
                    throw new ArgumentException(Res.GetString("ServerShouldBeDC"), "value");
                }
            }
            else if (!(value is DirectoryServer))
            {
                throw new ArgumentException("value");
            }
        }

        public void Remove(DirectoryServer server)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                DirectoryServer server2 = (DirectoryServer) base.InnerList[i];
                if (Utils.Compare(server2.Name, server.Name) == 0)
                {
                    base.List.Remove(server2);
                    return;
                }
            }
            throw new ArgumentException(Res.GetString("NotFoundInCollection", new object[] { server }), "server");
        }

        public DirectoryServer this[int index]
        {
            get
            {
                return (DirectoryServer) base.InnerList[index];
            }
            set
            {
                DirectoryServer server = value;
                if (server == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this.Contains(server))
                {
                    throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { server }), "value");
                }
                base.List[index] = server;
            }
        }
    }
}

