namespace System.ComponentModel.Design
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class DesignSurfaceManager : IServiceProvider, IDisposable
    {
        private ActiveDesignSurfaceChangedEventHandler _activeDesignSurfaceChanged;
        private DesignSurfaceEventHandler _designSurfaceCreated;
        private DesignSurfaceEventHandler _designSurfaceDisposed;
        private IServiceProvider _parentProvider;
        private EventHandler _selectionChanged;
        private System.ComponentModel.Design.ServiceContainer _serviceContainer;

        public event ActiveDesignSurfaceChangedEventHandler ActiveDesignSurfaceChanged
        {
            add
            {
                if (this._activeDesignSurfaceChanged == null)
                {
                    IDesignerEventService eventService = this.EventService;
                    if (eventService != null)
                    {
                        eventService.ActiveDesignerChanged += new ActiveDesignerEventHandler(this.OnActiveDesignerChanged);
                    }
                }
                this._activeDesignSurfaceChanged = (ActiveDesignSurfaceChangedEventHandler) Delegate.Combine(this._activeDesignSurfaceChanged, value);
            }
            remove
            {
                this._activeDesignSurfaceChanged = (ActiveDesignSurfaceChangedEventHandler) Delegate.Remove(this._activeDesignSurfaceChanged, value);
                if (this._activeDesignSurfaceChanged == null)
                {
                    IDesignerEventService eventService = this.EventService;
                    if (eventService != null)
                    {
                        eventService.ActiveDesignerChanged -= new ActiveDesignerEventHandler(this.OnActiveDesignerChanged);
                    }
                }
            }
        }

        public event DesignSurfaceEventHandler DesignSurfaceCreated
        {
            add
            {
                if (this._designSurfaceCreated == null)
                {
                    IDesignerEventService eventService = this.EventService;
                    if (eventService != null)
                    {
                        eventService.DesignerCreated += new DesignerEventHandler(this.OnDesignerCreated);
                    }
                }
                this._designSurfaceCreated = (DesignSurfaceEventHandler) Delegate.Combine(this._designSurfaceCreated, value);
            }
            remove
            {
                this._designSurfaceCreated = (DesignSurfaceEventHandler) Delegate.Remove(this._designSurfaceCreated, value);
                if (this._designSurfaceCreated == null)
                {
                    IDesignerEventService eventService = this.EventService;
                    if (eventService != null)
                    {
                        eventService.DesignerCreated -= new DesignerEventHandler(this.OnDesignerCreated);
                    }
                }
            }
        }

        public event DesignSurfaceEventHandler DesignSurfaceDisposed
        {
            add
            {
                if (this._designSurfaceDisposed == null)
                {
                    IDesignerEventService eventService = this.EventService;
                    if (eventService != null)
                    {
                        eventService.DesignerDisposed += new DesignerEventHandler(this.OnDesignerDisposed);
                    }
                }
                this._designSurfaceDisposed = (DesignSurfaceEventHandler) Delegate.Combine(this._designSurfaceDisposed, value);
            }
            remove
            {
                this._designSurfaceDisposed = (DesignSurfaceEventHandler) Delegate.Remove(this._designSurfaceDisposed, value);
                if (this._designSurfaceDisposed == null)
                {
                    IDesignerEventService eventService = this.EventService;
                    if (eventService != null)
                    {
                        eventService.DesignerDisposed -= new DesignerEventHandler(this.OnDesignerDisposed);
                    }
                }
            }
        }

        public event EventHandler SelectionChanged
        {
            add
            {
                if (this._selectionChanged == null)
                {
                    IDesignerEventService eventService = this.EventService;
                    if (eventService != null)
                    {
                        eventService.SelectionChanged += new EventHandler(this.OnSelectionChanged);
                    }
                }
                this._selectionChanged = (EventHandler) Delegate.Combine(this._selectionChanged, value);
            }
            remove
            {
                this._selectionChanged = (EventHandler) Delegate.Remove(this._selectionChanged, value);
                if (this._selectionChanged == null)
                {
                    IDesignerEventService eventService = this.EventService;
                    if (eventService != null)
                    {
                        eventService.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                    }
                }
            }
        }

        public DesignSurfaceManager() : this(null)
        {
        }

        public DesignSurfaceManager(IServiceProvider parentProvider)
        {
            this._parentProvider = parentProvider;
            ServiceCreatorCallback callback = new ServiceCreatorCallback(this.OnCreateService);
            this.ServiceContainer.AddService(typeof(IDesignerEventService), callback);
        }

        public DesignSurface CreateDesignSurface()
        {
            DesignSurface surface = this.CreateDesignSurfaceCore(this);
            DesignerEventService service = this.GetService(typeof(IDesignerEventService)) as DesignerEventService;
            if (service != null)
            {
                service.OnCreateDesigner(surface);
            }
            return surface;
        }

        public DesignSurface CreateDesignSurface(IServiceProvider parentProvider)
        {
            if (parentProvider == null)
            {
                throw new ArgumentNullException("parentProvider");
            }
            IServiceProvider provider = new MergedServiceProvider(parentProvider, this);
            DesignSurface surface = this.CreateDesignSurfaceCore(provider);
            DesignerEventService service = this.GetService(typeof(IDesignerEventService)) as DesignerEventService;
            if (service != null)
            {
                service.OnCreateDesigner(surface);
            }
            return surface;
        }

        protected virtual DesignSurface CreateDesignSurfaceCore(IServiceProvider parentProvider)
        {
            return new DesignSurface(parentProvider);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this._serviceContainer != null))
            {
                this._serviceContainer.Dispose();
                this._serviceContainer = null;
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

        private void OnActiveDesignerChanged(object sender, ActiveDesignerEventArgs e)
        {
            if (this._activeDesignSurfaceChanged != null)
            {
                DesignSurface newSurface = null;
                DesignSurface oldSurface = null;
                if (e.OldDesigner != null)
                {
                    oldSurface = e.OldDesigner.GetService(typeof(DesignSurface)) as DesignSurface;
                }
                if (e.NewDesigner != null)
                {
                    newSurface = e.NewDesigner.GetService(typeof(DesignSurface)) as DesignSurface;
                }
                this._activeDesignSurfaceChanged(this, new ActiveDesignSurfaceChangedEventArgs(oldSurface, newSurface));
            }
        }

        private object OnCreateService(IServiceContainer container, Type serviceType)
        {
            if (serviceType == typeof(IDesignerEventService))
            {
                return new DesignerEventService();
            }
            return null;
        }

        private void OnDesignerCreated(object sender, DesignerEventArgs e)
        {
            if (this._designSurfaceCreated != null)
            {
                DesignSurface service = e.Designer.GetService(typeof(DesignSurface)) as DesignSurface;
                if (service != null)
                {
                    this._designSurfaceCreated(this, new DesignSurfaceEventArgs(service));
                }
            }
        }

        private void OnDesignerDisposed(object sender, DesignerEventArgs e)
        {
            if (this._designSurfaceDisposed != null)
            {
                DesignSurface service = e.Designer.GetService(typeof(DesignSurface)) as DesignSurface;
                if (service != null)
                {
                    this._designSurfaceDisposed(this, new DesignSurfaceEventArgs(service));
                }
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (this._selectionChanged != null)
            {
                this._selectionChanged(this, e);
            }
        }

        public virtual DesignSurface ActiveDesignSurface
        {
            get
            {
                IDesignerEventService eventService = this.EventService;
                if (eventService != null)
                {
                    IDesignerHost activeDesigner = eventService.ActiveDesigner;
                    if (activeDesigner != null)
                    {
                        return (activeDesigner.GetService(typeof(DesignSurface)) as DesignSurface);
                    }
                }
                return null;
            }
            set
            {
                DesignerEventService eventService = this.EventService as DesignerEventService;
                if (eventService != null)
                {
                    eventService.OnActivateDesigner(value);
                }
            }
        }

        public DesignSurfaceCollection DesignSurfaces
        {
            get
            {
                IDesignerEventService eventService = this.EventService;
                if (eventService != null)
                {
                    return new DesignSurfaceCollection(eventService.Designers);
                }
                return new DesignSurfaceCollection(null);
            }
        }

        private IDesignerEventService EventService
        {
            get
            {
                return (this.GetService(typeof(IDesignerEventService)) as IDesignerEventService);
            }
        }

        protected System.ComponentModel.Design.ServiceContainer ServiceContainer
        {
            get
            {
                if (this._serviceContainer == null)
                {
                    this._serviceContainer = new System.ComponentModel.Design.ServiceContainer(this._parentProvider);
                }
                return this._serviceContainer;
            }
        }

        private sealed class MergedServiceProvider : IServiceProvider
        {
            private IServiceProvider _primaryProvider;
            private IServiceProvider _secondaryProvider;

            internal MergedServiceProvider(IServiceProvider primaryProvider, IServiceProvider secondaryProvider)
            {
                this._primaryProvider = primaryProvider;
                this._secondaryProvider = secondaryProvider;
            }

            object IServiceProvider.GetService(Type serviceType)
            {
                if (serviceType == null)
                {
                    throw new ArgumentNullException("serviceType");
                }
                object service = this._primaryProvider.GetService(serviceType);
                if (service == null)
                {
                    service = this._secondaryProvider.GetService(serviceType);
                }
                return service;
            }
        }
    }
}

