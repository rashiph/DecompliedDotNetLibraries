namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class NestedContainer : Container, INestedContainer, IContainer, IDisposable
    {
        private IComponent _owner;

        public NestedContainer(IComponent owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this._owner = owner;
            this._owner.Disposed += new EventHandler(this.OnOwnerDisposed);
        }

        protected override ISite CreateSite(IComponent component, string name)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            return new Site(component, this, name);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._owner.Disposed -= new EventHandler(this.OnOwnerDisposed);
            }
            base.Dispose(disposing);
        }

        protected override object GetService(Type service)
        {
            if (service == typeof(INestedContainer))
            {
                return this;
            }
            return base.GetService(service);
        }

        private void OnOwnerDisposed(object sender, EventArgs e)
        {
            base.Dispose();
        }

        public IComponent Owner
        {
            get
            {
                return this._owner;
            }
        }

        protected virtual string OwnerName
        {
            get
            {
                string str = null;
                if ((this._owner == null) || (this._owner.Site == null))
                {
                    return str;
                }
                INestedSite site = this._owner.Site as INestedSite;
                if (site != null)
                {
                    return site.FullName;
                }
                return this._owner.Site.Name;
            }
        }

        private class Site : INestedSite, ISite, IServiceProvider
        {
            private IComponent component;
            private NestedContainer container;
            private string name;

            internal Site(IComponent component, NestedContainer container, string name)
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
                    IComponent owner = this.container.Owner;
                    return (((owner != null) && (owner.Site != null)) && owner.Site.DesignMode);
                }
            }

            public string FullName
            {
                get
                {
                    if (this.name == null)
                    {
                        return this.name;
                    }
                    string ownerName = this.container.OwnerName;
                    string name = this.name;
                    if (ownerName != null)
                    {
                        name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { ownerName, name });
                    }
                    return name;
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

