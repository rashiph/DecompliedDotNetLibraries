namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal sealed class SiteNestedContainer : NestedContainer
    {
        private string _containerName;
        private DesignerHost _host;
        private bool _safeToCallOwner;
        private IServiceContainer _services;

        internal SiteNestedContainer(IComponent owner, string containerName, DesignerHost host) : base(owner)
        {
            this._containerName = containerName;
            this._host = host;
            this._safeToCallOwner = true;
        }

        public override void Add(IComponent component, string name)
        {
            if (this._host.AddToContainerPreProcess(component, name, this))
            {
                base.Add(component, name);
                try
                {
                    this._host.AddToContainerPostProcess(component, name, this);
                }
                catch (Exception exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        this.Remove(component);
                    }
                    throw;
                }
            }
        }

        protected override ISite CreateSite(IComponent component, string name)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            return new NestedSite(component, this._host, name, this);
        }

        protected override object GetService(Type serviceType)
        {
            object service = base.GetService(serviceType);
            if (service != null)
            {
                return service;
            }
            if (serviceType == typeof(IServiceContainer))
            {
                if (this._services == null)
                {
                    this._services = new ServiceContainer(this._host);
                }
                return this._services;
            }
            if (this._services != null)
            {
                return this._services.GetService(serviceType);
            }
            if ((base.Owner.Site != null) && this._safeToCallOwner)
            {
                try
                {
                    this._safeToCallOwner = false;
                    return base.Owner.Site.GetService(serviceType);
                }
                finally
                {
                    this._safeToCallOwner = true;
                }
            }
            return null;
        }

        internal object GetServiceInternal(Type serviceType)
        {
            return this.GetService(serviceType);
        }

        public override void Remove(IComponent component)
        {
            if (this._host.RemoveFromContainerPreProcess(component, this))
            {
                ISite site = component.Site;
                base.RemoveWithoutUnsiting(component);
                this._host.RemoveFromContainerPostProcess(component, this);
            }
        }

        protected override string OwnerName
        {
            get
            {
                string ownerName = base.OwnerName;
                if ((this._containerName != null) && (this._containerName.Length > 0))
                {
                    ownerName = string.Format(CultureInfo.CurrentCulture, "{0}.{1}", new object[] { ownerName, this._containerName });
                }
                return ownerName;
            }
        }

        private sealed class NestedSite : DesignerHost.Site, INestedSite, ISite, IServiceProvider
        {
            private SiteNestedContainer _container;
            private string _name;

            internal NestedSite(IComponent component, DesignerHost host, string name, Container container) : base(component, host, name, container)
            {
                this._container = container as SiteNestedContainer;
                this._name = name;
            }

            public string FullName
            {
                get
                {
                    if (this._name == null)
                    {
                        return this._name;
                    }
                    string ownerName = this._container.OwnerName;
                    string name = this.Name;
                    if (ownerName != null)
                    {
                        name = string.Format(CultureInfo.CurrentCulture, "{0}.{1}", new object[] { ownerName, name });
                    }
                    return name;
                }
            }
        }
    }
}

