namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Globalization;
    using System.Reflection;

    internal sealed class DesignerHost : Container, IDesignerLoaderHost2, IDesignerLoaderHost, IDesignerHost, IServiceContainer, IServiceProvider, IDesignerHostTransactionState, IComponentChangeService, IReflect
    {
        private bool _canReloadWithErrors;
        private IDesignerEventService _designerEventService;
        private Hashtable _designers;
        private EventHandlerList _events;
        private bool _ignoreErrorsDuringReload;
        private HostDesigntimeLicenseContext _licenseCtx;
        private DesignerLoader _loader;
        private string _newComponentName;
        private IComponent _rootComponent;
        private string _rootComponentClassName;
        private ICollection _savedSelection;
        private static readonly object _selfLock = new object();
        private BitVector32 _state;
        private DesignSurface _surface;
        private Stack _transactions;
        private TypeDescriptionProviderService _typeService;
        private bool _typeServiceChecked;
        private static Type[] DefaultServices = new Type[] { typeof(IDesignerHost), typeof(IContainer), typeof(IComponentChangeService), typeof(IDesignerLoaderHost2) };
        private static readonly object EventActivated = new object();
        private static readonly object EventComponentAdded = new object();
        private static readonly object EventComponentAdding = new object();
        private static readonly object EventComponentChanged = new object();
        private static readonly object EventComponentChanging = new object();
        private static readonly object EventComponentRemoved = new object();
        private static readonly object EventComponentRemoving = new object();
        private static readonly object EventComponentRename = new object();
        private static readonly object EventDeactivated = new object();
        private static readonly object EventLoadComplete = new object();
        private static readonly object EventTransactionClosed = new object();
        private static readonly object EventTransactionClosing = new object();
        private static readonly object EventTransactionOpened = new object();
        private static readonly object EventTransactionOpening = new object();
        private static readonly int StateIsClosingTransaction = BitVector32.CreateMask(StateUnloading);
        private static readonly int StateLoading = BitVector32.CreateMask();
        private static readonly int StateUnloading = BitVector32.CreateMask(StateLoading);

        event ComponentEventHandler IComponentChangeService.ComponentAdded
        {
            add
            {
                this._events.AddHandler(EventComponentAdded, value);
            }
            remove
            {
                this._events.RemoveHandler(EventComponentAdded, value);
            }
        }

        event ComponentEventHandler IComponentChangeService.ComponentAdding
        {
            add
            {
                this._events.AddHandler(EventComponentAdding, value);
            }
            remove
            {
                this._events.RemoveHandler(EventComponentAdding, value);
            }
        }

        event ComponentChangedEventHandler IComponentChangeService.ComponentChanged
        {
            add
            {
                this._events.AddHandler(EventComponentChanged, value);
            }
            remove
            {
                this._events.RemoveHandler(EventComponentChanged, value);
            }
        }

        event ComponentChangingEventHandler IComponentChangeService.ComponentChanging
        {
            add
            {
                this._events.AddHandler(EventComponentChanging, value);
            }
            remove
            {
                this._events.RemoveHandler(EventComponentChanging, value);
            }
        }

        event ComponentEventHandler IComponentChangeService.ComponentRemoved
        {
            add
            {
                this._events.AddHandler(EventComponentRemoved, value);
            }
            remove
            {
                this._events.RemoveHandler(EventComponentRemoved, value);
            }
        }

        event ComponentEventHandler IComponentChangeService.ComponentRemoving
        {
            add
            {
                this._events.AddHandler(EventComponentRemoving, value);
            }
            remove
            {
                this._events.RemoveHandler(EventComponentRemoving, value);
            }
        }

        event ComponentRenameEventHandler IComponentChangeService.ComponentRename
        {
            add
            {
                this._events.AddHandler(EventComponentRename, value);
            }
            remove
            {
                this._events.RemoveHandler(EventComponentRename, value);
            }
        }

        event EventHandler IDesignerHost.Activated
        {
            add
            {
                this._events.AddHandler(EventActivated, value);
            }
            remove
            {
                this._events.RemoveHandler(EventActivated, value);
            }
        }

        event EventHandler IDesignerHost.Deactivated
        {
            add
            {
                this._events.AddHandler(EventDeactivated, value);
            }
            remove
            {
                this._events.RemoveHandler(EventDeactivated, value);
            }
        }

        event EventHandler IDesignerHost.LoadComplete
        {
            add
            {
                this._events.AddHandler(EventLoadComplete, value);
            }
            remove
            {
                this._events.RemoveHandler(EventLoadComplete, value);
            }
        }

        event DesignerTransactionCloseEventHandler IDesignerHost.TransactionClosed
        {
            add
            {
                this._events.AddHandler(EventTransactionClosed, value);
            }
            remove
            {
                this._events.RemoveHandler(EventTransactionClosed, value);
            }
        }

        event DesignerTransactionCloseEventHandler IDesignerHost.TransactionClosing
        {
            add
            {
                this._events.AddHandler(EventTransactionClosing, value);
            }
            remove
            {
                this._events.RemoveHandler(EventTransactionClosing, value);
            }
        }

        event EventHandler IDesignerHost.TransactionOpened
        {
            add
            {
                this._events.AddHandler(EventTransactionOpened, value);
            }
            remove
            {
                this._events.RemoveHandler(EventTransactionOpened, value);
            }
        }

        event EventHandler IDesignerHost.TransactionOpening
        {
            add
            {
                this._events.AddHandler(EventTransactionOpening, value);
            }
            remove
            {
                this._events.RemoveHandler(EventTransactionOpening, value);
            }
        }

        public DesignerHost(DesignSurface surface)
        {
            this._surface = surface;
            this._state = new BitVector32();
            this._designers = new Hashtable();
            this._events = new EventHandlerList();
            DesignSurfaceServiceContainer service = this.GetService(typeof(DesignSurfaceServiceContainer)) as DesignSurfaceServiceContainer;
            if (service != null)
            {
                foreach (Type type in DefaultServices)
                {
                    service.AddFixedService(type, this);
                }
            }
            else
            {
                IServiceContainer container2 = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
                if (container2 != null)
                {
                    foreach (Type type2 in DefaultServices)
                    {
                        container2.AddService(type2, this);
                    }
                }
            }
        }

        public override void Add(IComponent component, string name)
        {
            if (!this._typeServiceChecked)
            {
                this._typeService = this.GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
                this._typeServiceChecked = true;
            }
            if ((this._typeService != null) && !TypeDescriptor.GetProvider(component).GetReflectionType(typeof(object)).IsDefined(typeof(ProjectTargetFrameworkAttribute), false))
            {
                TypeDescriptionProvider provider = this._typeService.GetProvider(component);
                if (provider != null)
                {
                    TypeDescriptor.AddProvider(provider, component);
                }
            }
            this.PerformAdd(component, name);
        }

        internal void AddToContainerPostProcess(IComponent component, string name, IContainer containerToAddTo)
        {
            if ((component is IExtenderProvider) && !TypeDescriptor.GetAttributes(component).Contains(InheritanceAttribute.InheritedReadOnly))
            {
                IExtenderProviderService service = this.GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
                if (service != null)
                {
                    service.AddExtenderProvider((IExtenderProvider) component);
                }
            }
            IDesigner designer = null;
            if (this._rootComponent == null)
            {
                designer = this._surface.CreateDesigner(component, true) as IRootDesigner;
                if (designer == null)
                {
                    Exception exception = new Exception(System.Design.SR.GetString("DesignerHostNoTopLevelDesigner", new object[] { component.GetType().FullName })) {
                        HelpLink = "DesignerHostNoTopLevelDesigner"
                    };
                    throw exception;
                }
                this._rootComponent = component;
                if (this._rootComponentClassName == null)
                {
                    this._rootComponentClassName = component.Site.Name;
                }
            }
            else
            {
                designer = this._surface.CreateDesigner(component, false);
            }
            if (designer != null)
            {
                this._designers[component] = designer;
                try
                {
                    designer.Initialize(component);
                    if (designer.Component == null)
                    {
                        throw new InvalidOperationException(System.Design.SR.GetString("DesignerHostDesignerNeedsComponent"));
                    }
                }
                catch
                {
                    this._designers.Remove(component);
                    throw;
                }
                if (designer is IExtenderProvider)
                {
                    IExtenderProviderService service2 = this.GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
                    if (service2 != null)
                    {
                        service2.AddExtenderProvider((IExtenderProvider) designer);
                    }
                }
            }
            ComponentEventArgs e = new ComponentEventArgs(component);
            ComponentEventHandler handler = this._events[EventComponentAdded] as ComponentEventHandler;
            if (handler != null)
            {
                handler(containerToAddTo, e);
            }
        }

        internal bool AddToContainerPreProcess(IComponent component, string name, IContainer containerToAddTo)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (this._state[StateUnloading])
            {
                Exception exception = new Exception(System.Design.SR.GetString("DesignerHostUnloading")) {
                    HelpLink = "DesignerHostUnloading"
                };
                throw exception;
            }
            if ((this._rootComponent != null) && string.Equals(component.GetType().FullName, this._rootComponentClassName, StringComparison.OrdinalIgnoreCase))
            {
                Exception exception2 = new Exception(System.Design.SR.GetString("DesignerHostCyclicAdd", new object[] { component.GetType().FullName, this._rootComponentClassName })) {
                    HelpLink = "DesignerHostCyclicAdd"
                };
                throw exception2;
            }
            ISite site = component.Site;
            if ((site != null) && (site.Container == this))
            {
                if (name != null)
                {
                    site.Name = name;
                }
                return false;
            }
            ComponentEventArgs e = new ComponentEventArgs(component);
            ComponentEventHandler handler = this._events[EventComponentAdding] as ComponentEventHandler;
            if (handler != null)
            {
                handler(containerToAddTo, e);
            }
            return true;
        }

        internal void BeginLoad(DesignerLoader loader)
        {
            if ((this._loader != null) && (this._loader != loader))
            {
                Exception exception = new InvalidOperationException(System.Design.SR.GetString("DesignerHostLoaderSpecified")) {
                    HelpLink = "DesignerHostLoaderSpecified"
                };
                throw exception;
            }
            IDesignerEventService service = null;
            bool flag = this._loader != null;
            this._loader = loader;
            if (!flag)
            {
                if (loader is IExtenderProvider)
                {
                    IExtenderProviderService service2 = this.GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
                    if (service2 != null)
                    {
                        service2.AddExtenderProvider((IExtenderProvider) loader);
                    }
                }
                service = this.GetService(typeof(IDesignerEventService)) as IDesignerEventService;
                if (service != null)
                {
                    service.ActiveDesignerChanged += new ActiveDesignerEventHandler(this.OnActiveDesignerChanged);
                    this._designerEventService = service;
                }
            }
            this._state[StateLoading] = true;
            this._surface.OnLoading();
            try
            {
                this._loader.BeginLoad(this);
            }
            catch (Exception innerException)
            {
                if (innerException is TargetInvocationException)
                {
                    innerException = innerException.InnerException;
                }
                string message = innerException.Message;
                if ((message == null) || (message.Length == 0))
                {
                    innerException = new Exception(System.Design.SR.GetString("DesignSurfaceFatalError", new object[] { innerException.ToString() }), innerException);
                }
                ((IDesignerLoaderHost) this).EndLoad(null, false, new object[] { innerException });
            }
            if (this._designerEventService == null)
            {
                this.OnActiveDesignerChanged(null, new ActiveDesignerEventArgs(null, this));
            }
        }

        protected override ISite CreateSite(IComponent component, string name)
        {
            if (this._newComponentName != null)
            {
                name = this._newComponentName;
                this._newComponentName = null;
            }
            INameCreationService service = this.GetService(typeof(INameCreationService)) as INameCreationService;
            if (name == null)
            {
                if (service != null)
                {
                    Type reflectionType = TypeDescriptor.GetReflectionType(component);
                    if (reflectionType.FullName.Equals(component.GetType().FullName))
                    {
                        reflectionType = component.GetType();
                    }
                    name = service.CreateName(this, reflectionType);
                }
                else
                {
                    name = string.Empty;
                }
            }
            else if (service != null)
            {
                service.ValidateName(name);
            }
            return new Site(component, this, name, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("DesignSurfaceContainerDispose"));
            }
            base.Dispose(disposing);
        }

        internal void DisposeHost()
        {
            try
            {
                if (this._loader != null)
                {
                    this._loader.Dispose();
                    this.Unload();
                }
                if (this._surface != null)
                {
                    if (this._designerEventService != null)
                    {
                        this._designerEventService.ActiveDesignerChanged -= new ActiveDesignerEventHandler(this.OnActiveDesignerChanged);
                    }
                    DesignSurfaceServiceContainer service = this.GetService(typeof(DesignSurfaceServiceContainer)) as DesignSurfaceServiceContainer;
                    if (service != null)
                    {
                        foreach (Type type in DefaultServices)
                        {
                            service.RemoveFixedService(type);
                        }
                    }
                    else
                    {
                        IServiceContainer container2 = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
                        if (container2 != null)
                        {
                            foreach (Type type2 in DefaultServices)
                            {
                                container2.RemoveService(type2);
                            }
                        }
                    }
                }
            }
            finally
            {
                this._loader = null;
                this._surface = null;
                this._events.Dispose();
            }
            base.Dispose(true);
        }

        internal void Flush()
        {
            if (this._loader != null)
            {
                this._loader.Flush();
            }
        }

        protected override object GetService(Type service)
        {
            object obj2 = null;
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }
            if (service == typeof(IMultitargetHelperService))
            {
                IServiceProvider provider = this._loader as IServiceProvider;
                if (provider != null)
                {
                    obj2 = provider.GetService(typeof(IMultitargetHelperService));
                }
                return obj2;
            }
            obj2 = base.GetService(service);
            if ((obj2 == null) && (this._surface != null))
            {
                obj2 = this._surface.GetService(service);
            }
            return obj2;
        }

        private void OnActiveDesignerChanged(object sender, ActiveDesignerEventArgs e)
        {
            object eventDeactivated = null;
            if (e.OldDesigner == this)
            {
                eventDeactivated = EventDeactivated;
            }
            else if (e.NewDesigner == this)
            {
                eventDeactivated = EventActivated;
            }
            if (eventDeactivated != null)
            {
                if ((e.OldDesigner == this) && (this._surface != null))
                {
                    this._surface.Flush();
                }
                EventHandler handler = this._events[eventDeactivated] as EventHandler;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        private void OnComponentRename(IComponent component, string oldName, string newName)
        {
            if (component == this._rootComponent)
            {
                string str = this._rootComponentClassName;
                int length = str.LastIndexOf(oldName);
                if ((((length + oldName.Length) == str.Length) && ((length - 1) >= 0)) && (str[length - 1] == '.'))
                {
                    this._rootComponentClassName = str.Substring(0, length) + newName;
                }
                else
                {
                    this._rootComponentClassName = newName;
                }
            }
            ComponentRenameEventHandler handler = this._events[EventComponentRename] as ComponentRenameEventHandler;
            if (handler != null)
            {
                handler(this, new ComponentRenameEventArgs(component, oldName, newName));
            }
        }

        private void OnLoadComplete(EventArgs e)
        {
            EventHandler handler = this._events[EventLoadComplete] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTransactionClosed(DesignerTransactionCloseEventArgs e)
        {
            DesignerTransactionCloseEventHandler handler = this._events[EventTransactionClosed] as DesignerTransactionCloseEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTransactionClosing(DesignerTransactionCloseEventArgs e)
        {
            DesignerTransactionCloseEventHandler handler = this._events[EventTransactionClosing] as DesignerTransactionCloseEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTransactionOpened(EventArgs e)
        {
            EventHandler handler = this._events[EventTransactionOpened] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTransactionOpening(EventArgs e)
        {
            EventHandler handler = this._events[EventTransactionOpening] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void PerformAdd(IComponent component, string name)
        {
            if (this.AddToContainerPreProcess(component, name, this))
            {
                base.Add(component, name);
                try
                {
                    this.AddToContainerPostProcess(component, name, this);
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

        public override void Remove(IComponent component)
        {
            if (this.RemoveFromContainerPreProcess(component, this))
            {
                Site site = component.Site as Site;
                base.RemoveWithoutUnsiting(component);
                site.Disposed = true;
                this.RemoveFromContainerPostProcess(component, this);
            }
        }

        internal void RemoveFromContainerPostProcess(IComponent component, IContainer container)
        {
            try
            {
                ComponentEventHandler handler = this._events[EventComponentRemoved] as ComponentEventHandler;
                ComponentEventArgs e = new ComponentEventArgs(component);
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            finally
            {
                component.Site = null;
            }
        }

        internal bool RemoveFromContainerPreProcess(IComponent component, IContainer container)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            ISite site = component.Site;
            if ((site == null) || (site.Container != container))
            {
                return false;
            }
            ComponentEventArgs e = new ComponentEventArgs(component);
            ComponentEventHandler handler = this._events[EventComponentRemoving] as ComponentEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
            if (component is IExtenderProvider)
            {
                IExtenderProviderService service = this.GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
                if (service != null)
                {
                    service.RemoveExtenderProvider((IExtenderProvider) component);
                }
            }
            IDesigner designer = this._designers[component] as IDesigner;
            if (designer is IExtenderProvider)
            {
                IExtenderProviderService service2 = this.GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
                if (service2 != null)
                {
                    service2.RemoveExtenderProvider((IExtenderProvider) designer);
                }
            }
            if (designer != null)
            {
                designer.Dispose();
                this._designers.Remove(component);
            }
            if (component == this._rootComponent)
            {
                this._rootComponent = null;
                this._rootComponentClassName = null;
            }
            return true;
        }

        void IComponentChangeService.OnComponentChanged(object component, MemberDescriptor member, object oldValue, object newValue)
        {
            if (!((IDesignerHost) this).Loading)
            {
                ComponentChangedEventHandler handler = this._events[EventComponentChanged] as ComponentChangedEventHandler;
                if (handler != null)
                {
                    handler(this, new ComponentChangedEventArgs(component, member, oldValue, newValue));
                }
            }
        }

        void IComponentChangeService.OnComponentChanging(object component, MemberDescriptor member)
        {
            if (!((IDesignerHost) this).Loading)
            {
                ComponentChangingEventHandler handler = this._events[EventComponentChanging] as ComponentChangingEventHandler;
                if (handler != null)
                {
                    handler(this, new ComponentChangingEventArgs(component, member));
                }
            }
        }

        void IDesignerHost.Activate()
        {
            this._surface.OnViewActivate();
        }

        IComponent IDesignerHost.CreateComponent(Type componentType)
        {
            return ((IDesignerHost) this).CreateComponent(componentType, null);
        }

        IComponent IDesignerHost.CreateComponent(Type componentType, string name)
        {
            IComponent component;
            if (componentType == null)
            {
                throw new ArgumentNullException("componentType");
            }
            System.ComponentModel.LicenseContext currentContext = LicenseManager.CurrentContext;
            bool flag = false;
            if (currentContext != this.LicenseContext)
            {
                LicenseManager.CurrentContext = this.LicenseContext;
                LicenseManager.LockContext(_selfLock);
                flag = true;
            }
            try
            {
                try
                {
                    this._newComponentName = name;
                    component = this._surface.CreateInstance(componentType) as IComponent;
                }
                finally
                {
                    this._newComponentName = null;
                }
                if (component == null)
                {
                    InvalidOperationException exception = new InvalidOperationException(System.Design.SR.GetString("DesignerHostFailedComponentCreate", new object[] { componentType.Name })) {
                        HelpLink = "DesignerHostFailedComponentCreate"
                    };
                    throw exception;
                }
                if ((component.Site != null) && (component.Site.Container == this))
                {
                    return component;
                }
                this.PerformAdd(component, name);
            }
            finally
            {
                if (flag)
                {
                    LicenseManager.UnlockContext(_selfLock);
                    LicenseManager.CurrentContext = currentContext;
                }
            }
            return component;
        }

        DesignerTransaction IDesignerHost.CreateTransaction()
        {
            return ((IDesignerHost) this).CreateTransaction(null);
        }

        DesignerTransaction IDesignerHost.CreateTransaction(string description)
        {
            if (description == null)
            {
                description = System.Design.SR.GetString("DesignerHostGenericTransactionName");
            }
            return new DesignerHostTransaction(this, description);
        }

        void IDesignerHost.DestroyComponent(IComponent component)
        {
            string name;
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if ((component.Site != null) && (component.Site.Name != null))
            {
                name = component.Site.Name;
            }
            else
            {
                name = component.GetType().Name;
            }
            InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)];
            if ((attribute != null) && (attribute.InheritanceLevel != InheritanceLevel.NotInherited))
            {
                Exception exception = new InvalidOperationException(System.Design.SR.GetString("DesignerHostCantDestroyInheritedComponent", new object[] { name })) {
                    HelpLink = "DesignerHostCantDestroyInheritedComponent"
                };
                throw exception;
            }
            if (((IDesignerHost) this).InTransaction)
            {
                this.Remove(component);
                component.Dispose();
            }
            else
            {
                using (DesignerTransaction transaction = ((IDesignerHost) this).CreateTransaction(System.Design.SR.GetString("DesignerHostDestroyComponentTransaction", new object[] { name })))
                {
                    this.Remove(component);
                    component.Dispose();
                    transaction.Commit();
                }
            }
        }

        IDesigner IDesignerHost.GetDesigner(IComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            return (this._designers[component] as IDesigner);
        }

        Type IDesignerHost.GetType(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            ITypeResolutionService service = this.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;
            if (service != null)
            {
                return service.GetType(typeName);
            }
            return Type.GetType(typeName);
        }

        void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            IServiceContainer service = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (service == null)
            {
                throw new ObjectDisposedException("IServiceContainer");
            }
            service.AddService(serviceType, callback);
        }

        void IServiceContainer.AddService(Type serviceType, object serviceInstance)
        {
            IServiceContainer service = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (service == null)
            {
                throw new ObjectDisposedException("IServiceContainer");
            }
            service.AddService(serviceType, serviceInstance);
        }

        void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            IServiceContainer service = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (service == null)
            {
                throw new ObjectDisposedException("IServiceContainer");
            }
            service.AddService(serviceType, callback, promote);
        }

        void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
        {
            IServiceContainer service = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (service == null)
            {
                throw new ObjectDisposedException("IServiceContainer");
            }
            service.AddService(serviceType, serviceInstance, promote);
        }

        void IServiceContainer.RemoveService(Type serviceType)
        {
            IServiceContainer service = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (service == null)
            {
                throw new ObjectDisposedException("IServiceContainer");
            }
            service.RemoveService(serviceType);
        }

        void IServiceContainer.RemoveService(Type serviceType, bool promote)
        {
            IServiceContainer service = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (service == null)
            {
                throw new ObjectDisposedException("IServiceContainer");
            }
            service.RemoveService(serviceType, promote);
        }

        void IDesignerLoaderHost.EndLoad(string rootClassName, bool successful, ICollection errorCollection)
        {
            bool flag = this._state[StateLoading];
            this._state[StateLoading] = false;
            if (rootClassName != null)
            {
                this._rootComponentClassName = rootClassName;
            }
            else if ((this._rootComponent != null) && (this._rootComponent.Site != null))
            {
                this._rootComponentClassName = this._rootComponent.Site.Name;
            }
            if (successful && (this._rootComponent == null))
            {
                ArrayList list = new ArrayList();
                InvalidOperationException exception = new InvalidOperationException(System.Design.SR.GetString("DesignerHostNoBaseClass")) {
                    HelpLink = "DesignerHostNoBaseClass"
                };
                list.Add(exception);
                errorCollection = list;
                successful = false;
            }
            if (!successful)
            {
                this.Unload();
            }
            if (flag && (this._surface != null))
            {
                this._surface.OnLoaded(successful, errorCollection);
            }
            if (successful && flag)
            {
                IRootDesigner designer = ((IDesignerHost) this).GetDesigner(this._rootComponent) as IRootDesigner;
                IHelpService service = this.GetService(typeof(IHelpService)) as IHelpService;
                if (service != null)
                {
                    service.AddContextAttribute("Keyword", "Designer_" + designer.GetType().FullName, HelpKeywordType.F1Keyword);
                }
                try
                {
                    this.OnLoadComplete(EventArgs.Empty);
                }
                catch (Exception exception2)
                {
                    this._state[StateLoading] = true;
                    this.Unload();
                    ArrayList list2 = new ArrayList();
                    list2.Add(exception2);
                    if (errorCollection != null)
                    {
                        list2.AddRange(errorCollection);
                    }
                    errorCollection = list2;
                    successful = false;
                    if (this._surface != null)
                    {
                        this._surface.OnLoaded(successful, errorCollection);
                    }
                    throw;
                }
                if (successful && (this._savedSelection != null))
                {
                    ISelectionService service2 = this.GetService(typeof(ISelectionService)) as ISelectionService;
                    if (service2 != null)
                    {
                        ArrayList components = new ArrayList(this._savedSelection.Count);
                        foreach (string str in this._savedSelection)
                        {
                            IComponent component = this.Components[str];
                            if (component != null)
                            {
                                components.Add(component);
                            }
                        }
                        this._savedSelection = null;
                        service2.SetSelectedComponents(components, SelectionTypes.Replace);
                    }
                }
            }
        }

        void IDesignerLoaderHost.Reload()
        {
            if (this._loader != null)
            {
                this._surface.Flush();
                ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    ArrayList list = new ArrayList(service.SelectionCount);
                    foreach (object obj2 in service.GetSelectedComponents())
                    {
                        IComponent component = obj2 as IComponent;
                        if (((component != null) && (component.Site != null)) && (component.Site.Name != null))
                        {
                            list.Add(component.Site.Name);
                        }
                    }
                    this._savedSelection = list;
                }
                this.Unload();
                this.BeginLoad(this._loader);
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return this.GetService(serviceType);
        }

        FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
        {
            return typeof(IDesignerHost).GetField(name, bindingAttr);
        }

        FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
        {
            return typeof(IDesignerHost).GetFields(bindingAttr);
        }

        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
        {
            return typeof(IDesignerHost).GetMember(name, bindingAttr);
        }

        MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
        {
            return typeof(IDesignerHost).GetMembers(bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
        {
            return typeof(IDesignerHost).GetMethod(name, bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            return typeof(IDesignerHost).GetMethod(name, bindingAttr, binder, types, modifiers);
        }

        MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
        {
            return typeof(IDesignerHost).GetMethods(bindingAttr);
        }

        PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
        {
            return typeof(IDesignerHost).GetProperties(bindingAttr);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
        {
            return typeof(IDesignerHost).GetProperty(name, bindingAttr);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return typeof(IDesignerHost).GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        object IReflect.InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            return typeof(IDesignerHost).InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        private void Unload()
        {
            this._surface.OnUnloading();
            IHelpService service = this.GetService(typeof(IHelpService)) as IHelpService;
            if (((service != null) && (this._rootComponent != null)) && (this._designers[this._rootComponent] != null))
            {
                service.RemoveContextAttribute("Keyword", "Designer_" + this._designers[this._rootComponent].GetType().FullName);
            }
            ISelectionService service2 = (ISelectionService) this.GetService(typeof(ISelectionService));
            if (service2 != null)
            {
                service2.SetSelectedComponents(null, SelectionTypes.Replace);
            }
            this._state[StateUnloading] = true;
            DesignerTransaction transaction = ((IDesignerHost) this).CreateTransaction();
            ArrayList exceptions = new ArrayList();
            try
            {
                IComponent[] array = new IComponent[this.Components.Count];
                this.Components.CopyTo(array, 0);
                foreach (IComponent component in array)
                {
                    if (!object.ReferenceEquals(component, this._rootComponent))
                    {
                        IDesigner designer = this._designers[component] as IDesigner;
                        if (designer != null)
                        {
                            this._designers.Remove(component);
                            try
                            {
                                designer.Dispose();
                            }
                            catch (Exception exception)
                            {
                                if (designer == null)
                                {
                                }
                                else
                                {
                                    string name = designer.GetType().Name;
                                }
                                exceptions.Add(exception);
                            }
                        }
                        try
                        {
                            component.Dispose();
                        }
                        catch (Exception exception2)
                        {
                            if (component == null)
                            {
                            }
                            else
                            {
                                string text4 = component.GetType().Name;
                            }
                            exceptions.Add(exception2);
                        }
                    }
                }
                if (this._rootComponent != null)
                {
                    IDesigner designer2 = this._designers[this._rootComponent] as IDesigner;
                    if (designer2 != null)
                    {
                        this._designers.Remove(this._rootComponent);
                        try
                        {
                            designer2.Dispose();
                        }
                        catch (Exception exception3)
                        {
                            if (designer2 == null)
                            {
                            }
                            else
                            {
                                string text6 = designer2.GetType().Name;
                            }
                            exceptions.Add(exception3);
                        }
                    }
                    try
                    {
                        this._rootComponent.Dispose();
                    }
                    catch (Exception exception4)
                    {
                        if (this._rootComponent == null)
                        {
                        }
                        else
                        {
                            string text8 = this._rootComponent.GetType().Name;
                        }
                        exceptions.Add(exception4);
                    }
                }
                this._designers.Clear();
                while (this.Components.Count > 0)
                {
                    this.Remove(this.Components[0]);
                }
            }
            finally
            {
                transaction.Commit();
                this._state[StateUnloading] = false;
            }
            if ((this._transactions != null) && (this._transactions.Count > 0))
            {
                while (this._transactions.Count > 0)
                {
                    ((DesignerTransaction) this._transactions.Peek()).Commit();
                }
            }
            this._surface.OnUnloaded();
            if (exceptions.Count > 0)
            {
                throw new ExceptionCollection(exceptions);
            }
        }

        internal bool IsClosingTransaction
        {
            get
            {
                return this._state[StateIsClosingTransaction];
            }
            set
            {
                this._state[StateIsClosingTransaction] = value;
            }
        }

        internal HostDesigntimeLicenseContext LicenseContext
        {
            get
            {
                if (this._licenseCtx == null)
                {
                    this._licenseCtx = new HostDesigntimeLicenseContext(this);
                }
                return this._licenseCtx;
            }
        }

        IContainer IDesignerHost.Container
        {
            get
            {
                return this;
            }
        }

        bool IDesignerHost.InTransaction
        {
            get
            {
                return (((this._transactions != null) && (this._transactions.Count > 0)) || this.IsClosingTransaction);
            }
        }

        bool IDesignerHost.Loading
        {
            get
            {
                return ((this._state[StateLoading] || this._state[StateUnloading]) || ((this._loader != null) && this._loader.Loading));
            }
        }

        IComponent IDesignerHost.RootComponent
        {
            get
            {
                return this._rootComponent;
            }
        }

        string IDesignerHost.RootComponentClassName
        {
            get
            {
                return this._rootComponentClassName;
            }
        }

        string IDesignerHost.TransactionDescription
        {
            get
            {
                if ((this._transactions != null) && (this._transactions.Count > 0))
                {
                    return ((DesignerTransaction) this._transactions.Peek()).Description;
                }
                return null;
            }
        }

        bool IDesignerHostTransactionState.IsClosingTransaction
        {
            get
            {
                return this.IsClosingTransaction;
            }
        }

        bool IDesignerLoaderHost2.CanReloadWithErrors
        {
            get
            {
                return this._canReloadWithErrors;
            }
            set
            {
                this._canReloadWithErrors = value;
            }
        }

        bool IDesignerLoaderHost2.IgnoreErrorsDuringReload
        {
            get
            {
                return this._ignoreErrorsDuringReload;
            }
            set
            {
                if (!value || ((IDesignerLoaderHost2) this).CanReloadWithErrors)
                {
                    this._ignoreErrorsDuringReload = value;
                }
            }
        }

        Type IReflect.UnderlyingSystemType
        {
            get
            {
                return typeof(IDesignerHost).UnderlyingSystemType;
            }
        }

        private sealed class DesignerHostTransaction : DesignerTransaction
        {
            private DesignerHost _host;

            public DesignerHostTransaction(DesignerHost host, string description) : base(description)
            {
                this._host = host;
                if (this._host._transactions == null)
                {
                    this._host._transactions = new Stack();
                }
                this._host._transactions.Push(this);
                this._host.OnTransactionOpening(EventArgs.Empty);
                this._host.OnTransactionOpened(EventArgs.Empty);
            }

            protected override void OnCancel()
            {
                if (this._host != null)
                {
                    if (this._host._transactions.Peek() != this)
                    {
                        string description = ((DesignerTransaction) this._host._transactions.Peek()).Description;
                        throw new InvalidOperationException(System.Design.SR.GetString("DesignerHostNestedTransaction", new object[] { base.Description, description }));
                    }
                    this._host.IsClosingTransaction = true;
                    try
                    {
                        this._host._transactions.Pop();
                        DesignerTransactionCloseEventArgs e = new DesignerTransactionCloseEventArgs(false, this._host._transactions.Count == 0);
                        this._host.OnTransactionClosing(e);
                        this._host.OnTransactionClosed(e);
                    }
                    finally
                    {
                        this._host.IsClosingTransaction = false;
                        this._host = null;
                    }
                }
            }

            protected override void OnCommit()
            {
                if (this._host != null)
                {
                    if (this._host._transactions.Peek() != this)
                    {
                        string description = ((DesignerTransaction) this._host._transactions.Peek()).Description;
                        throw new InvalidOperationException(System.Design.SR.GetString("DesignerHostNestedTransaction", new object[] { base.Description, description }));
                    }
                    this._host.IsClosingTransaction = true;
                    try
                    {
                        this._host._transactions.Pop();
                        DesignerTransactionCloseEventArgs e = new DesignerTransactionCloseEventArgs(true, this._host._transactions.Count == 0);
                        this._host.OnTransactionClosing(e);
                        this._host.OnTransactionClosed(e);
                    }
                    finally
                    {
                        this._host.IsClosingTransaction = false;
                        this._host = null;
                    }
                }
            }
        }

        internal class Site : ISite, IServiceContainer, IServiceProvider, IDictionaryService
        {
            private IComponent _component;
            private Container _container;
            private Hashtable _dictionary;
            private bool _disposed;
            private DesignerHost _host;
            private string _name;
            private SiteNestedContainer _nestedContainer;

            internal Site(IComponent component, DesignerHost host, string name, Container container)
            {
                this._component = component;
                this._host = host;
                this._name = name;
                this._container = container;
            }

            object IDictionaryService.GetKey(object value)
            {
                if (this._dictionary != null)
                {
                    foreach (DictionaryEntry entry in this._dictionary)
                    {
                        object obj2 = entry.Value;
                        if ((value != null) && value.Equals(obj2))
                        {
                            return entry.Key;
                        }
                    }
                }
                return null;
            }

            object IDictionaryService.GetValue(object key)
            {
                if (this._dictionary != null)
                {
                    return this._dictionary[key];
                }
                return null;
            }

            void IDictionaryService.SetValue(object key, object value)
            {
                if (this._dictionary == null)
                {
                    this._dictionary = new Hashtable();
                }
                if (value == null)
                {
                    this._dictionary.Remove(key);
                }
                else
                {
                    this._dictionary[key] = value;
                }
            }

            void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback)
            {
                this.SiteServiceContainer.AddService(serviceType, callback);
            }

            void IServiceContainer.AddService(Type serviceType, object serviceInstance)
            {
                this.SiteServiceContainer.AddService(serviceType, serviceInstance);
            }

            void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
            {
                this.SiteServiceContainer.AddService(serviceType, callback, promote);
            }

            void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
            {
                this.SiteServiceContainer.AddService(serviceType, serviceInstance, promote);
            }

            void IServiceContainer.RemoveService(Type serviceType)
            {
                this.SiteServiceContainer.RemoveService(serviceType);
            }

            void IServiceContainer.RemoveService(Type serviceType, bool promote)
            {
                this.SiteServiceContainer.RemoveService(serviceType, promote);
            }

            object IServiceProvider.GetService(Type service)
            {
                if (service == null)
                {
                    throw new ArgumentNullException("service");
                }
                if (service == typeof(IDictionaryService))
                {
                    return this;
                }
                if (service == typeof(INestedContainer))
                {
                    if (this._nestedContainer == null)
                    {
                        this._nestedContainer = new SiteNestedContainer(this._component, null, this._host);
                    }
                    return this._nestedContainer;
                }
                if (((service != typeof(IServiceContainer)) && (service != typeof(IContainer))) && (this._nestedContainer != null))
                {
                    return this._nestedContainer.GetServiceInternal(service);
                }
                return this._host.GetService(service);
            }

            internal bool Disposed
            {
                get
                {
                    return this._disposed;
                }
                set
                {
                    this._disposed = value;
                }
            }

            private IServiceContainer SiteServiceContainer
            {
                get
                {
                    SiteNestedContainer service = ((IServiceProvider) this).GetService(typeof(INestedContainer)) as SiteNestedContainer;
                    return (service.GetServiceInternal(typeof(IServiceContainer)) as IServiceContainer);
                }
            }

            IComponent ISite.Component
            {
                get
                {
                    return this._component;
                }
            }

            IContainer ISite.Container
            {
                get
                {
                    return this._container;
                }
            }

            bool ISite.DesignMode
            {
                get
                {
                    return true;
                }
            }

            string ISite.Name
            {
                get
                {
                    return this._name;
                }
                set
                {
                    if (value == null)
                    {
                        value = string.Empty;
                    }
                    if (this._name != value)
                    {
                        bool flag = true;
                        if (value.Length > 0)
                        {
                            IComponent component = this._container.Components[value];
                            flag = this._component != component;
                            if ((component != null) && flag)
                            {
                                Exception exception = new Exception(System.Design.SR.GetString("DesignerHostDuplicateName", new object[] { value })) {
                                    HelpLink = "DesignerHostDuplicateName"
                                };
                                throw exception;
                            }
                        }
                        if (flag)
                        {
                            INameCreationService service = (INameCreationService) ((IServiceProvider) this).GetService(typeof(INameCreationService));
                            if (service != null)
                            {
                                service.ValidateName(value);
                            }
                        }
                        string oldName = this._name;
                        this._name = value;
                        this._host.OnComponentRename(this._component, oldName, this._name);
                    }
                }
            }
        }
    }
}

