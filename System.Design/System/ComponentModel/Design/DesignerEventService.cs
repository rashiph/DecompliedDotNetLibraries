namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal sealed class DesignerEventService : IDesignerEventService
    {
        private IDesignerHost _activeDesigner;
        private bool _deferredSelChange;
        private DesignerCollection _designerCollection;
        private ArrayList _designerList;
        private EventHandlerList _events;
        private bool _inTransaction;
        private static readonly object EventActiveDesignerChanged = new object();
        private static readonly object EventDesignerCreated = new object();
        private static readonly object EventDesignerDisposed = new object();
        private static readonly object EventSelectionChanged = new object();

        event ActiveDesignerEventHandler IDesignerEventService.ActiveDesignerChanged
        {
            add
            {
                if (this._events == null)
                {
                    this._events = new EventHandlerList();
                }
                this._events[EventActiveDesignerChanged] = Delegate.Combine(this._events[EventActiveDesignerChanged], value);
            }
            remove
            {
                if (this._events != null)
                {
                    this._events[EventActiveDesignerChanged] = Delegate.Remove(this._events[EventActiveDesignerChanged], value);
                }
            }
        }

        event DesignerEventHandler IDesignerEventService.DesignerCreated
        {
            add
            {
                if (this._events == null)
                {
                    this._events = new EventHandlerList();
                }
                this._events[EventDesignerCreated] = Delegate.Combine(this._events[EventDesignerCreated], value);
            }
            remove
            {
                if (this._events != null)
                {
                    this._events[EventDesignerCreated] = Delegate.Remove(this._events[EventDesignerCreated], value);
                }
            }
        }

        event DesignerEventHandler IDesignerEventService.DesignerDisposed
        {
            add
            {
                if (this._events == null)
                {
                    this._events = new EventHandlerList();
                }
                this._events[EventDesignerDisposed] = Delegate.Combine(this._events[EventDesignerDisposed], value);
            }
            remove
            {
                if (this._events != null)
                {
                    this._events[EventDesignerDisposed] = Delegate.Remove(this._events[EventDesignerDisposed], value);
                }
            }
        }

        event EventHandler IDesignerEventService.SelectionChanged
        {
            add
            {
                if (this._events == null)
                {
                    this._events = new EventHandlerList();
                }
                this._events[EventSelectionChanged] = Delegate.Combine(this._events[EventSelectionChanged], value);
            }
            remove
            {
                if (this._events != null)
                {
                    this._events[EventSelectionChanged] = Delegate.Remove(this._events[EventSelectionChanged], value);
                }
            }
        }

        internal DesignerEventService()
        {
        }

        internal void OnActivateDesigner(DesignSurface surface)
        {
            IDesignerHost item = null;
            if (surface != null)
            {
                item = surface.GetService(typeof(IDesignerHost)) as IDesignerHost;
            }
            if ((item != null) && ((this._designerList == null) || !this._designerList.Contains(item)))
            {
                this.OnCreateDesigner(surface);
            }
            if (this._activeDesigner != item)
            {
                IDesignerHost provider = this._activeDesigner;
                this._activeDesigner = item;
                if (provider != null)
                {
                    this.SinkChangeEvents(provider, false);
                }
                if (this._activeDesigner != null)
                {
                    this.SinkChangeEvents(this._activeDesigner, true);
                }
                if (this._events != null)
                {
                    ActiveDesignerEventHandler handler = this._events[EventActiveDesignerChanged] as ActiveDesignerEventHandler;
                    if (handler != null)
                    {
                        handler(this, new ActiveDesignerEventArgs(provider, item));
                    }
                }
                this.OnSelectionChanged(this, EventArgs.Empty);
            }
        }

        private void OnComponentAddedRemoved(object sender, ComponentEventArgs ce)
        {
            IComponent component = ce.Component;
            if (component != null)
            {
                ISite site = component.Site;
                if (site != null)
                {
                    IDesignerHost container = site.Container as IDesignerHost;
                    if ((container != null) && container.Loading)
                    {
                        this._deferredSelChange = true;
                        return;
                    }
                }
            }
            this.OnSelectionChanged(this, EventArgs.Empty);
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
        {
            IComponent component = ce.Component as IComponent;
            if (component != null)
            {
                ISite site = component.Site;
                if (site != null)
                {
                    ISelectionService service = site.GetService(typeof(ISelectionService)) as ISelectionService;
                    if ((service != null) && service.GetComponentSelected(component))
                    {
                        this.OnSelectionChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        internal void OnCreateDesigner(DesignSurface surface)
        {
            IDesignerHost service = surface.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (this._designerList == null)
            {
                this._designerList = new ArrayList();
            }
            this._designerList.Add(service);
            surface.Disposed += new EventHandler(this.OnDesignerDisposed);
            if (this._events != null)
            {
                DesignerEventHandler handler = this._events[EventDesignerCreated] as DesignerEventHandler;
                if (handler != null)
                {
                    handler(this, new DesignerEventArgs(service));
                }
            }
        }

        private void OnDesignerDisposed(object sender, EventArgs e)
        {
            DesignSurface provider = (DesignSurface) sender;
            provider.Disposed -= new EventHandler(this.OnDesignerDisposed);
            this.SinkChangeEvents(provider, false);
            IDesignerHost service = provider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service != null)
            {
                if (this._events != null)
                {
                    DesignerEventHandler handler = this._events[EventDesignerDisposed] as DesignerEventHandler;
                    if (handler != null)
                    {
                        handler(this, new DesignerEventArgs(service));
                    }
                }
                if (this._designerList != null)
                {
                    this._designerList.Remove(service);
                }
            }
        }

        private void OnLoadComplete(object sender, EventArgs e)
        {
            if (this._deferredSelChange)
            {
                this._deferredSelChange = false;
                this.OnSelectionChanged(this, EventArgs.Empty);
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (this._inTransaction)
            {
                this._deferredSelChange = true;
            }
            else if (this._events != null)
            {
                EventHandler handler = this._events[EventSelectionChanged] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        private void OnTransactionClosed(object sender, DesignerTransactionCloseEventArgs e)
        {
            if (e.LastTransaction)
            {
                this._inTransaction = false;
                if (this._deferredSelChange)
                {
                    this._deferredSelChange = false;
                    this.OnSelectionChanged(this, EventArgs.Empty);
                }
            }
        }

        private void OnTransactionOpened(object sender, EventArgs e)
        {
            this._inTransaction = true;
        }

        private void SinkChangeEvents(IServiceProvider provider, bool sink)
        {
            ISelectionService service = provider.GetService(typeof(ISelectionService)) as ISelectionService;
            IComponentChangeService service2 = provider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            IDesignerHost sender = provider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (sink)
            {
                if (service != null)
                {
                    service.SelectionChanged += new EventHandler(this.OnSelectionChanged);
                }
                if (service2 != null)
                {
                    ComponentEventHandler handler = new ComponentEventHandler(this.OnComponentAddedRemoved);
                    service2.ComponentAdded += handler;
                    service2.ComponentRemoved += handler;
                    service2.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                }
                if (sender != null)
                {
                    sender.TransactionOpened += new EventHandler(this.OnTransactionOpened);
                    sender.TransactionClosed += new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                    sender.LoadComplete += new EventHandler(this.OnLoadComplete);
                    if (sender.InTransaction)
                    {
                        this.OnTransactionOpened(sender, EventArgs.Empty);
                    }
                }
            }
            else
            {
                if (service != null)
                {
                    service.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                }
                if (service2 != null)
                {
                    ComponentEventHandler handler2 = new ComponentEventHandler(this.OnComponentAddedRemoved);
                    service2.ComponentAdded -= handler2;
                    service2.ComponentRemoved -= handler2;
                    service2.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                }
                if (sender != null)
                {
                    sender.TransactionOpened -= new EventHandler(this.OnTransactionOpened);
                    sender.TransactionClosed -= new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                    sender.LoadComplete -= new EventHandler(this.OnLoadComplete);
                    if (sender.InTransaction)
                    {
                        this.OnTransactionClosed(sender, new DesignerTransactionCloseEventArgs(false, true));
                    }
                }
            }
        }

        IDesignerHost IDesignerEventService.ActiveDesigner
        {
            get
            {
                return this._activeDesigner;
            }
        }

        DesignerCollection IDesignerEventService.Designers
        {
            get
            {
                if (this._designerList == null)
                {
                    this._designerList = new ArrayList();
                }
                if (this._designerCollection == null)
                {
                    this._designerCollection = new DesignerCollection(this._designerList);
                }
                return this._designerCollection;
            }
        }
    }
}

