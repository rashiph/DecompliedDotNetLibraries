namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class DesignSurface : IDisposable, IServiceProvider
    {
        private DesignerHost _host;
        private bool _loaded;
        private ICollection _loadErrors;
        private IServiceProvider _parentProvider;
        private System.ComponentModel.Design.ServiceContainer _serviceContainer;

        public event EventHandler Disposed;

        public event EventHandler Flushed;

        public event LoadedEventHandler Loaded;

        public event EventHandler Loading;

        public event EventHandler Unloaded;

        public event EventHandler Unloading;

        public event EventHandler ViewActivated;

        public DesignSurface() : this((IServiceProvider) null)
        {
        }

        public DesignSurface(IServiceProvider parentProvider)
        {
            this._parentProvider = parentProvider;
            this._serviceContainer = new DesignSurfaceServiceContainer(this._parentProvider);
            ServiceCreatorCallback callback = new ServiceCreatorCallback(this.OnCreateService);
            this.ServiceContainer.AddService(typeof(ISelectionService), callback);
            this.ServiceContainer.AddService(typeof(IExtenderProviderService), callback);
            this.ServiceContainer.AddService(typeof(IExtenderListService), callback);
            this.ServiceContainer.AddService(typeof(ITypeDescriptorFilterService), callback);
            this.ServiceContainer.AddService(typeof(IReferenceService), callback);
            this.ServiceContainer.AddService(typeof(DesignSurface), this);
            this._host = new DesignerHost(this);
        }

        public DesignSurface(Type rootComponentType) : this(null, rootComponentType)
        {
        }

        public DesignSurface(IServiceProvider parentProvider, Type rootComponentType) : this(parentProvider)
        {
            if (rootComponentType == null)
            {
                throw new ArgumentNullException("rootComponentType");
            }
            this.BeginLoad(rootComponentType);
        }

        public void BeginLoad(DesignerLoader loader)
        {
            if (loader == null)
            {
                throw new ArgumentNullException("loader");
            }
            if (this._host == null)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            this._loadErrors = null;
            this._host.BeginLoad(loader);
        }

        public void BeginLoad(Type rootComponentType)
        {
            if (rootComponentType == null)
            {
                throw new ArgumentNullException("rootComponentType");
            }
            if (this._host == null)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            this.BeginLoad(new DefaultDesignerLoader(rootComponentType));
        }

        [Obsolete("CreateComponent has been replaced by CreateInstance and will be removed after Beta2")]
        protected internal virtual IComponent CreateComponent(Type componentType)
        {
            return (this.CreateInstance(componentType) as IComponent);
        }

        protected internal virtual IDesigner CreateDesigner(IComponent component, bool rootDesigner)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (this._host == null)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (rootDesigner)
            {
                return (TypeDescriptor.CreateDesigner(component, typeof(IRootDesigner)) as IRootDesigner);
            }
            return TypeDescriptor.CreateDesigner(component, typeof(IDesigner));
        }

        protected internal virtual object CreateInstance(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            ConstructorInfo constructor = null;
            object obj2 = null;
            constructor = TypeDescriptor.GetReflectionType(type).GetConstructor(new Type[0]);
            if (constructor != null)
            {
                obj2 = TypeDescriptor.CreateInstance(this, type, new Type[0], new object[0]);
            }
            else
            {
                if (typeof(IComponent).IsAssignableFrom(type))
                {
                    constructor = TypeDescriptor.GetReflectionType(type).GetConstructor(BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(IContainer) }, null);
                }
                if (constructor != null)
                {
                    obj2 = TypeDescriptor.CreateInstance(this, type, new Type[] { typeof(IContainer) }, new object[] { this.ComponentContainer });
                }
            }
            if (obj2 == null)
            {
                obj2 = Activator.CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null);
            }
            return obj2;
        }

        public INestedContainer CreateNestedContainer(IComponent owningComponent)
        {
            return this.CreateNestedContainer(owningComponent, null);
        }

        public INestedContainer CreateNestedContainer(IComponent owningComponent, string containerName)
        {
            if (this._host == null)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (owningComponent == null)
            {
                throw new ArgumentNullException("owningComponent");
            }
            return new SiteNestedContainer(owningComponent, containerName, this._host);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Disposed != null)
                {
                    this.Disposed(this, EventArgs.Empty);
                }
                try
                {
                    try
                    {
                        if (this._host != null)
                        {
                            this._host.DisposeHost();
                        }
                    }
                    finally
                    {
                        if (this._serviceContainer != null)
                        {
                            this._serviceContainer.RemoveService(typeof(DesignSurface));
                            this._serviceContainer.Dispose();
                        }
                    }
                }
                finally
                {
                    this._host = null;
                    this._serviceContainer = null;
                }
            }
        }

        public void Flush()
        {
            if (this._host != null)
            {
                this._host.Flush();
            }
            if (this.Flushed != null)
            {
                this.Flushed(this, EventArgs.Empty);
            }
        }

        public object GetService(Type serviceType)
        {
            if (this._serviceContainer != null)
            {
                return this._serviceContainer.GetService(serviceType);
            }
            return null;
        }

        private object OnCreateService(IServiceContainer container, Type serviceType)
        {
            if (serviceType == typeof(ISelectionService))
            {
                return new SelectionService(container);
            }
            if (serviceType == typeof(IExtenderProviderService))
            {
                return new ExtenderProviderService();
            }
            if (serviceType == typeof(IExtenderListService))
            {
                return this.GetService(typeof(IExtenderProviderService));
            }
            if (serviceType == typeof(ITypeDescriptorFilterService))
            {
                return new TypeDescriptorFilterService();
            }
            if (serviceType == typeof(IReferenceService))
            {
                return new ReferenceService(container);
            }
            return null;
        }

        protected virtual void OnLoaded(LoadedEventArgs e)
        {
            if (this.Loaded != null)
            {
                this.Loaded(this, e);
            }
        }

        internal void OnLoaded(bool successful, ICollection errors)
        {
            this._loaded = successful;
            this._loadErrors = errors;
            if (successful && (((IDesignerHost) this._host).RootComponent == null))
            {
                ArrayList list = new ArrayList();
                Exception exception = new InvalidOperationException(System.Design.SR.GetString("DesignSurfaceNoRootComponent")) {
                    HelpLink = "DesignSurfaceNoRootComponent"
                };
                list.Add(exception);
                if (errors != null)
                {
                    list.AddRange(errors);
                }
                errors = list;
                successful = false;
            }
            this.OnLoaded(new LoadedEventArgs(successful, errors));
        }

        internal void OnLoading()
        {
            this.OnLoading(EventArgs.Empty);
        }

        protected virtual void OnLoading(EventArgs e)
        {
            if (this.Loading != null)
            {
                this.Loading(this, e);
            }
        }

        internal void OnUnloaded()
        {
            this.OnUnloaded(EventArgs.Empty);
        }

        protected virtual void OnUnloaded(EventArgs e)
        {
            if (this.Unloaded != null)
            {
                this.Unloaded(this, e);
            }
        }

        internal void OnUnloading()
        {
            this.OnUnloading(EventArgs.Empty);
            this._loaded = false;
        }

        protected virtual void OnUnloading(EventArgs e)
        {
            if (this.Unloading != null)
            {
                this.Unloading(this, e);
            }
        }

        internal void OnViewActivate()
        {
            this.OnViewActivate(EventArgs.Empty);
        }

        protected virtual void OnViewActivate(EventArgs e)
        {
            if (this.ViewActivated != null)
            {
                this.ViewActivated(this, e);
            }
        }

        public IContainer ComponentContainer
        {
            get
            {
                if (this._host == null)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                return ((IDesignerHost) this._host).Container;
            }
        }

        public bool DtelLoading { get; set; }

        public bool IsLoaded
        {
            get
            {
                return this._loaded;
            }
        }

        public ICollection LoadErrors
        {
            get
            {
                if (this._loadErrors != null)
                {
                    return this._loadErrors;
                }
                return new object[0];
            }
        }

        protected System.ComponentModel.Design.ServiceContainer ServiceContainer
        {
            get
            {
                if (this._serviceContainer == null)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                return this._serviceContainer;
            }
        }

        public object View
        {
            get
            {
                Exception exception;
                if (this._host == null)
                {
                    throw new ObjectDisposedException(this.ToString());
                }
                IComponent rootComponent = ((IDesignerHost) this._host).RootComponent;
                if (rootComponent == null)
                {
                    if (this._loadErrors != null)
                    {
                        foreach (object obj2 in this._loadErrors)
                        {
                            exception = obj2 as Exception;
                            if (exception != null)
                            {
                                throw new InvalidOperationException(exception.Message, exception);
                            }
                            throw new InvalidOperationException(obj2.ToString());
                        }
                    }
                    exception = new InvalidOperationException(System.Design.SR.GetString("DesignSurfaceNoRootComponent")) {
                        HelpLink = "DesignSurfaceNoRootComponent"
                    };
                    throw exception;
                }
                IRootDesigner designer = ((IDesignerHost) this._host).GetDesigner(rootComponent) as IRootDesigner;
                if (designer == null)
                {
                    exception = new InvalidOperationException(System.Design.SR.GetString("DesignSurfaceDesignerNotLoaded")) {
                        HelpLink = "DesignSurfaceDesignerNotLoaded"
                    };
                    throw exception;
                }
                ViewTechnology[] supportedTechnologies = designer.SupportedTechnologies;
                int index = 0;
                while (index < supportedTechnologies.Length)
                {
                    ViewTechnology technology = supportedTechnologies[index];
                    return designer.GetView(technology);
                }
                exception = new NotSupportedException(System.Design.SR.GetString("DesignSurfaceNoSupportedTechnology")) {
                    HelpLink = "DesignSurfaceNoSupportedTechnology"
                };
                throw exception;
            }
        }

        private class DefaultDesignerLoader : DesignerLoader
        {
            private ICollection _components;
            private Type _type;

            public DefaultDesignerLoader(ICollection components)
            {
                this._components = components;
            }

            public DefaultDesignerLoader(Type type)
            {
                this._type = type;
            }

            public override void BeginLoad(IDesignerLoaderHost loaderHost)
            {
                string baseClassName = null;
                if (this._type != null)
                {
                    loaderHost.CreateComponent(this._type);
                    baseClassName = this._type.FullName;
                }
                else
                {
                    foreach (IComponent component in this._components)
                    {
                        loaderHost.Container.Add(component);
                    }
                }
                loaderHost.EndLoad(baseClassName, true, null);
            }

            public override void Dispose()
            {
            }
        }
    }
}

