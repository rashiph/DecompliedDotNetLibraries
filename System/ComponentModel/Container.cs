namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class Container : IContainer, IDisposable
    {
        private bool checkedFilter;
        private ComponentCollection components;
        private ContainerFilterService filter;
        private int siteCount;
        private ISite[] sites;
        private object syncObj = new object();

        public virtual void Add(IComponent component)
        {
            this.Add(component, null);
        }

        public virtual void Add(IComponent component, string name)
        {
            lock (this.syncObj)
            {
                if (component != null)
                {
                    ISite site = component.Site;
                    if ((site == null) || (site.Container != this))
                    {
                        if (this.sites == null)
                        {
                            this.sites = new ISite[4];
                        }
                        else
                        {
                            this.ValidateName(component, name);
                            if (this.sites.Length == this.siteCount)
                            {
                                ISite[] destinationArray = new ISite[this.siteCount * 2];
                                Array.Copy(this.sites, 0, destinationArray, 0, this.siteCount);
                                this.sites = destinationArray;
                            }
                        }
                        if (site != null)
                        {
                            site.Container.Remove(component);
                        }
                        ISite site2 = this.CreateSite(component, name);
                        this.sites[this.siteCount++] = site2;
                        component.Site = site2;
                        this.components = null;
                    }
                }
            }
        }

        protected virtual ISite CreateSite(IComponent component, string name)
        {
            return new Site(component, this, name);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.syncObj)
                {
                    while (this.siteCount > 0)
                    {
                        ISite site = this.sites[--this.siteCount];
                        site.Component.Site = null;
                        site.Component.Dispose();
                    }
                    this.sites = null;
                    this.components = null;
                }
            }
        }

        ~Container()
        {
            this.Dispose(false);
        }

        protected virtual object GetService(Type service)
        {
            if (!(service == typeof(IContainer)))
            {
                return null;
            }
            return this;
        }

        public virtual void Remove(IComponent component)
        {
            this.Remove(component, false);
        }

        private void Remove(IComponent component, bool preserveSite)
        {
            lock (this.syncObj)
            {
                if (component != null)
                {
                    ISite site = component.Site;
                    if ((site != null) && (site.Container == this))
                    {
                        if (!preserveSite)
                        {
                            component.Site = null;
                        }
                        for (int i = 0; i < this.siteCount; i++)
                        {
                            if (this.sites[i] == site)
                            {
                                this.siteCount--;
                                Array.Copy(this.sites, i + 1, this.sites, i, this.siteCount - i);
                                this.sites[this.siteCount] = null;
                                this.components = null;
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected void RemoveWithoutUnsiting(IComponent component)
        {
            this.Remove(component, true);
        }

        protected virtual void ValidateName(IComponent component, string name)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (name != null)
            {
                for (int i = 0; i < Math.Min(this.siteCount, this.sites.Length); i++)
                {
                    ISite site = this.sites[i];
                    if (((site != null) && (site.Name != null)) && (string.Equals(site.Name, name, StringComparison.OrdinalIgnoreCase) && (site.Component != component)))
                    {
                        InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(site.Component)[typeof(InheritanceAttribute)];
                        if (attribute.InheritanceLevel != InheritanceLevel.InheritedReadOnly)
                        {
                            throw new ArgumentException(SR.GetString("DuplicateComponentName", new object[] { name }));
                        }
                    }
                }
            }
        }

        public virtual ComponentCollection Components
        {
            get
            {
                lock (this.syncObj)
                {
                    if (this.components == null)
                    {
                        IComponent[] componentArray = new IComponent[this.siteCount];
                        for (int i = 0; i < this.siteCount; i++)
                        {
                            componentArray[i] = this.sites[i].Component;
                        }
                        this.components = new ComponentCollection(componentArray);
                        if ((this.filter == null) && this.checkedFilter)
                        {
                            this.checkedFilter = false;
                        }
                    }
                    if (!this.checkedFilter)
                    {
                        this.filter = this.GetService(typeof(ContainerFilterService)) as ContainerFilterService;
                        this.checkedFilter = true;
                    }
                    if (this.filter != null)
                    {
                        ComponentCollection components = this.filter.FilterComponents(this.components);
                        if (components != null)
                        {
                            this.components = components;
                        }
                    }
                    return this.components;
                }
            }
        }

        private class Site : ISite, IServiceProvider
        {
            private IComponent component;
            private System.ComponentModel.Container container;
            private string name;

            internal Site(IComponent component, System.ComponentModel.Container container, string name)
            {
                this.component = component;
                this.container = container;
                this.name = name;
            }

            public object GetService(Type service)
            {
                if (!(service == typeof(ISite)))
                {
                    return this.container.GetService(service);
                }
                return this;
            }

            public IComponent Component
            {
                get
                {
                    return this.component;
                }
            }

            public IContainer Container
            {
                get
                {
                    return this.container;
                }
            }

            public bool DesignMode
            {
                get
                {
                    return false;
                }
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
                set
                {
                    if (((value == null) || (this.name == null)) || !value.Equals(this.name))
                    {
                        this.container.ValidateName(this.component, value);
                        this.name = value;
                    }
                }
            }
        }
    }
}

