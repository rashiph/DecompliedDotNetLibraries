namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public abstract class BasicDesignerLoader : DesignerLoader, IDesignerLoaderService
    {
        private string _baseComponentClassName;
        private IDesignerLoaderHost _host;
        private bool _hostInitialized;
        private int _loadDependencyCount;
        private bool _loading;
        private DesignerSerializationManager _serializationManager;
        private IDisposable _serializationSession;
        private BitVector32 _state = new BitVector32();
        private static readonly int StateActiveDocument = BitVector32.CreateMask(StateReloadSupported);
        private static readonly int StateDeferredReload = BitVector32.CreateMask(StateActiveDocument);
        private static readonly int StateEnableComponentEvents = BitVector32.CreateMask(StateModifyIfErrors);
        private static readonly int StateFlushInProgress = BitVector32.CreateMask(StateLoadFailed);
        private static readonly int StateFlushReload = BitVector32.CreateMask(StateForceReload);
        private static readonly int StateForceReload = BitVector32.CreateMask(StateReloadAtIdle);
        private static readonly int StateLoaded = BitVector32.CreateMask();
        private static readonly int StateLoadFailed = BitVector32.CreateMask(StateLoaded);
        private static readonly int StateModified = BitVector32.CreateMask(StateFlushInProgress);
        private static readonly int StateModifyIfErrors = BitVector32.CreateMask(StateFlushReload);
        private static readonly int StateReloadAtIdle = BitVector32.CreateMask(StateDeferredReload);
        private static readonly int StateReloadSupported = BitVector32.CreateMask(StateModified);

        protected BasicDesignerLoader()
        {
            this._state[StateFlushInProgress] = false;
            this._state[StateReloadSupported] = true;
            this._state[StateEnableComponentEvents] = false;
            this._hostInitialized = false;
            this._loading = false;
        }

        public override void BeginLoad(IDesignerLoaderHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (this._state[StateLoaded])
            {
                Exception exception = new InvalidOperationException(System.Design.SR.GetString("BasicDesignerLoaderAlreadyLoaded")) {
                    HelpLink = "BasicDesignerLoaderAlreadyLoaded"
                };
                throw exception;
            }
            if ((this._host != null) && (this._host != host))
            {
                Exception exception2 = new InvalidOperationException(System.Design.SR.GetString("BasicDesignerLoaderDifferentHost")) {
                    HelpLink = "BasicDesignerLoaderDifferentHost"
                };
                throw exception2;
            }
            this._state[StateLoaded | StateLoadFailed] = false;
            this._loadDependencyCount = 0;
            if (this._host == null)
            {
                this._host = host;
                this._hostInitialized = true;
                this._serializationManager = new DesignerSerializationManager(this._host);
                DesignSurfaceServiceContainer container = this.GetService(typeof(DesignSurfaceServiceContainer)) as DesignSurfaceServiceContainer;
                if (container != null)
                {
                    container.AddFixedService(typeof(IDesignerSerializationManager), this._serializationManager);
                }
                else
                {
                    IServiceContainer container2 = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
                    if (container2 == null)
                    {
                        this.ThrowMissingService(typeof(IServiceContainer));
                    }
                    container2.AddService(typeof(IDesignerSerializationManager), this._serializationManager);
                }
                this.Initialize();
                host.Activated += new EventHandler(this.OnDesignerActivate);
                host.Deactivated += new EventHandler(this.OnDesignerDeactivate);
            }
            bool successful = true;
            ArrayList errorCollection = null;
            IDesignerLoaderService service = this.GetService(typeof(IDesignerLoaderService)) as IDesignerLoaderService;
            try
            {
                if (service != null)
                {
                    service.AddLoadDependency();
                }
                else
                {
                    this._loading = true;
                    this.OnBeginLoad();
                }
                this.PerformLoad(this._serializationManager);
            }
            catch (Exception innerException)
            {
                while (innerException is TargetInvocationException)
                {
                    innerException = innerException.InnerException;
                }
                errorCollection = new ArrayList();
                errorCollection.Add(innerException);
                successful = false;
            }
            if (service != null)
            {
                service.DependentLoadComplete(successful, errorCollection);
            }
            else
            {
                this.OnEndLoad(successful, errorCollection);
                this._loading = false;
            }
        }

        public override void Dispose()
        {
            if (this._state[StateReloadAtIdle])
            {
                Application.Idle -= new EventHandler(this.OnIdle);
            }
            this.UnloadDocument();
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                service.ComponentAdding -= new ComponentEventHandler(this.OnComponentAdding);
                service.ComponentRemoving -= new ComponentEventHandler(this.OnComponentRemoving);
                service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                service.ComponentChanging -= new ComponentChangingEventHandler(this.OnComponentChanging);
                service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
            }
            if (this._host != null)
            {
                this._host.RemoveService(typeof(IDesignerLoaderService));
                this._host.Activated -= new EventHandler(this.OnDesignerActivate);
                this._host.Deactivated -= new EventHandler(this.OnDesignerDeactivate);
                this._host = null;
            }
        }

        protected virtual bool EnableComponentNotification(bool enable)
        {
            bool flag = this._state[StateEnableComponentEvents];
            if (!flag && enable)
            {
                this._state[StateEnableComponentEvents] = true;
                return flag;
            }
            if (flag && !enable)
            {
                this._state[StateEnableComponentEvents] = false;
            }
            return flag;
        }

        public override void Flush()
        {
            if ((!this._state[StateFlushInProgress] && this._state[StateLoaded]) && this.Modified)
            {
                this._state[StateFlushInProgress] = true;
                Cursor current = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    IDesignerLoaderHost host = this._host;
                    bool flag = true;
                    if ((host != null) && (host.RootComponent != null))
                    {
                        using (this._serializationManager.CreateSession())
                        {
                            try
                            {
                                this.PerformFlush(this._serializationManager);
                            }
                            catch (CheckoutException)
                            {
                                flag = false;
                                throw;
                            }
                            catch (Exception exception)
                            {
                                this._serializationManager.Errors.Add(exception);
                            }
                            ICollection errors = this._serializationManager.Errors;
                            if ((errors != null) && (errors.Count > 0))
                            {
                                this.ReportFlushErrors(errors);
                            }
                        }
                    }
                    if (flag)
                    {
                        this.Modified = false;
                    }
                }
                finally
                {
                    this._state[StateFlushInProgress] = false;
                    Cursor.Current = current;
                }
            }
        }

        protected object GetService(System.Type serviceType)
        {
            object service = null;
            if (this._host != null)
            {
                service = this._host.GetService(serviceType);
            }
            return service;
        }

        protected virtual void Initialize()
        {
            this.LoaderHost.AddService(typeof(IDesignerLoaderService), this);
        }

        protected virtual bool IsReloadNeeded()
        {
            return true;
        }

        protected virtual void OnBeginLoad()
        {
            this._serializationSession = this._serializationManager.CreateSession();
            this._state[StateLoaded] = false;
            this.EnableComponentNotification(false);
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                service.ComponentAdding -= new ComponentEventHandler(this.OnComponentAdding);
                service.ComponentRemoving -= new ComponentEventHandler(this.OnComponentRemoving);
                service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                service.ComponentChanging -= new ComponentChangingEventHandler(this.OnComponentChanging);
                service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
            }
        }

        protected virtual void OnBeginUnload()
        {
        }

        private void OnComponentAdded(object sender, ComponentEventArgs e)
        {
            if (this._state[StateEnableComponentEvents] && !this.LoaderHost.Loading)
            {
                this.Modified = true;
            }
        }

        private void OnComponentAdding(object sender, ComponentEventArgs e)
        {
            if (this._state[StateEnableComponentEvents] && !this.LoaderHost.Loading)
            {
                this.OnModifying();
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (this._state[StateEnableComponentEvents] && !this.LoaderHost.Loading)
            {
                this.Modified = true;
            }
        }

        private void OnComponentChanging(object sender, ComponentChangingEventArgs e)
        {
            if (this._state[StateEnableComponentEvents] && !this.LoaderHost.Loading)
            {
                this.OnModifying();
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            if (this._state[StateEnableComponentEvents] && !this.LoaderHost.Loading)
            {
                this.Modified = true;
            }
        }

        private void OnComponentRemoving(object sender, ComponentEventArgs e)
        {
            if (this._state[StateEnableComponentEvents] && !this.LoaderHost.Loading)
            {
                this.OnModifying();
            }
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs e)
        {
            if (this._state[StateEnableComponentEvents] && !this.LoaderHost.Loading)
            {
                this.OnModifying();
                this.Modified = true;
            }
        }

        private void OnDesignerActivate(object sender, EventArgs e)
        {
            this._state[StateActiveDocument] = true;
            if (this._state[StateDeferredReload] && (this._host != null))
            {
                this._state[StateDeferredReload] = false;
                ReloadOptions flags = ReloadOptions.Default;
                if (this._state[StateForceReload])
                {
                    flags |= ReloadOptions.Force;
                }
                if (!this._state[StateFlushReload])
                {
                    flags |= ReloadOptions.NoFlush;
                }
                if (this._state[StateModifyIfErrors])
                {
                    flags |= ReloadOptions.ModifyOnError;
                }
                this.Reload(flags);
            }
        }

        private void OnDesignerDeactivate(object sender, EventArgs e)
        {
            this._state[StateActiveDocument] = false;
        }

        protected virtual void OnEndLoad(bool successful, ICollection errors)
        {
            successful = (successful && ((errors == null) || (errors.Count == 0))) && ((this._serializationManager.Errors == null) || (this._serializationManager.Errors.Count == 0));
            try
            {
                this._state[StateLoaded] = true;
                IDesignerLoaderHost2 host = this.GetService(typeof(IDesignerLoaderHost2)) as IDesignerLoaderHost2;
                if (!successful && ((host == null) || !host.IgnoreErrorsDuringReload))
                {
                    if (host != null)
                    {
                        host.CanReloadWithErrors = this.LoaderHost.RootComponent != null;
                    }
                    this.UnloadDocument();
                }
                else
                {
                    successful = true;
                }
                if (errors != null)
                {
                    foreach (object obj2 in errors)
                    {
                        this._serializationManager.Errors.Add(obj2);
                    }
                }
                errors = this._serializationManager.Errors;
            }
            finally
            {
                this._serializationSession.Dispose();
                this._serializationSession = null;
            }
            if (successful)
            {
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                    service.ComponentAdding += new ComponentEventHandler(this.OnComponentAdding);
                    service.ComponentRemoving += new ComponentEventHandler(this.OnComponentRemoving);
                    service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                    service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                    service.ComponentChanging += new ComponentChangingEventHandler(this.OnComponentChanging);
                    service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
                }
                this.EnableComponentNotification(true);
            }
            this.LoaderHost.EndLoad(this._baseComponentClassName, successful, errors);
            if ((this._state[StateModifyIfErrors] && (errors != null)) && (errors.Count > 0))
            {
                try
                {
                    this.OnModifying();
                    this.Modified = true;
                }
                catch (CheckoutException exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        throw;
                    }
                }
            }
        }

        private void OnIdle(object sender, EventArgs e)
        {
            Application.Idle -= new EventHandler(this.OnIdle);
            if (this._state[StateReloadAtIdle])
            {
                this._state[StateReloadAtIdle] = false;
                DesignSurfaceManager service = (DesignSurfaceManager) this.GetService(typeof(DesignSurfaceManager));
                DesignSurface objB = (DesignSurface) this.GetService(typeof(DesignSurface));
                if (((service != null) && (objB != null)) && !object.ReferenceEquals(service.ActiveDesignSurface, objB))
                {
                    this._state[StateActiveDocument] = false;
                    this._state[StateDeferredReload] = true;
                }
                else
                {
                    IDesignerLoaderHost loaderHost = this.LoaderHost;
                    if ((loaderHost != null) && (this._state[StateForceReload] || this.IsReloadNeeded()))
                    {
                        try
                        {
                            if (this._state[StateFlushReload])
                            {
                                this.Flush();
                            }
                            this.UnloadDocument();
                            loaderHost.Reload();
                        }
                        finally
                        {
                            this._state[(StateForceReload | StateModifyIfErrors) | StateFlushReload] = false;
                        }
                    }
                }
            }
        }

        protected virtual void OnModifying()
        {
        }

        protected abstract void PerformFlush(IDesignerSerializationManager serializationManager);
        protected abstract void PerformLoad(IDesignerSerializationManager serializationManager);
        protected void Reload(ReloadOptions flags)
        {
            this._state[StateForceReload] = (flags & ReloadOptions.Force) != ReloadOptions.Default;
            this._state[StateFlushReload] = (flags & ReloadOptions.NoFlush) == ReloadOptions.Default;
            this._state[StateModifyIfErrors] = (flags & ReloadOptions.ModifyOnError) != ReloadOptions.Default;
            if (!this._state[StateFlushInProgress])
            {
                if (this._state[StateActiveDocument])
                {
                    if (!this._state[StateReloadAtIdle])
                    {
                        Application.Idle += new EventHandler(this.OnIdle);
                        this._state[StateReloadAtIdle] = true;
                    }
                }
                else
                {
                    this._state[StateDeferredReload] = true;
                }
            }
        }

        protected virtual void ReportFlushErrors(ICollection errors)
        {
            object obj2 = null;
            foreach (object obj3 in errors)
            {
                obj2 = obj3;
            }
            if (obj2 != null)
            {
                Exception exception = obj2 as Exception;
                if (exception == null)
                {
                    exception = new InvalidOperationException(obj2.ToString());
                }
                throw exception;
            }
        }

        protected void SetBaseComponentClassName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this._baseComponentClassName = name;
        }

        void IDesignerLoaderService.AddLoadDependency()
        {
            if (this._serializationManager == null)
            {
                throw new InvalidOperationException();
            }
            if (this._loadDependencyCount++ == 0)
            {
                this.OnBeginLoad();
            }
        }

        void IDesignerLoaderService.DependentLoadComplete(bool successful, ICollection errorCollection)
        {
            if (this._loadDependencyCount == 0)
            {
                throw new InvalidOperationException();
            }
            if (!successful)
            {
                this._state[StateLoadFailed] = true;
            }
            if (--this._loadDependencyCount == 0)
            {
                this.OnEndLoad(!this._state[StateLoadFailed], errorCollection);
            }
            else if (errorCollection != null)
            {
                foreach (object obj2 in errorCollection)
                {
                    this._serializationManager.Errors.Add(obj2);
                }
            }
        }

        bool IDesignerLoaderService.Reload()
        {
            if (this._state[StateReloadSupported] && (this._loadDependencyCount == 0))
            {
                this.Reload(ReloadOptions.Force);
                return true;
            }
            return false;
        }

        private void ThrowMissingService(System.Type serviceType)
        {
            Exception exception = new InvalidOperationException(System.Design.SR.GetString("BasicDesignerLoaderMissingService", new object[] { serviceType.Name })) {
                HelpLink = "BasicDesignerLoaderMissingService"
            };
            throw exception;
        }

        private void UnloadDocument()
        {
            this.OnBeginUnload();
            this._state[StateLoaded] = false;
            this._baseComponentClassName = null;
        }

        protected IDesignerLoaderHost LoaderHost
        {
            get
            {
                if (this._host != null)
                {
                    return this._host;
                }
                if (this._hostInitialized)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                throw new InvalidOperationException(System.Design.SR.GetString("BasicDesignerLoaderNotInitialized"));
            }
        }

        public override bool Loading
        {
            get
            {
                if (this._loadDependencyCount <= 0)
                {
                    return this._loading;
                }
                return true;
            }
        }

        protected virtual bool Modified
        {
            get
            {
                return this._state[StateModified];
            }
            set
            {
                this._state[StateModified] = value;
            }
        }

        protected object PropertyProvider
        {
            get
            {
                if (this._serializationManager == null)
                {
                    throw new InvalidOperationException(System.Design.SR.GetString("BasicDesignerLoaderNotInitialized"));
                }
                return this._serializationManager.PropertyProvider;
            }
            set
            {
                if (this._serializationManager == null)
                {
                    throw new InvalidOperationException(System.Design.SR.GetString("BasicDesignerLoaderNotInitialized"));
                }
                this._serializationManager.PropertyProvider = value;
            }
        }

        protected bool ReloadPending
        {
            get
            {
                return this._state[StateReloadAtIdle];
            }
        }

        [Flags]
        protected enum ReloadOptions
        {
            Default = 0,
            Force = 2,
            ModifyOnError = 1,
            NoFlush = 4
        }
    }
}

