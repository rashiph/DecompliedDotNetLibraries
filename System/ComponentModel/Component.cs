namespace System.ComponentModel
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DesignerCategory("Component")]
    public class Component : MarshalByRefObject, IComponent, IDisposable
    {
        private static readonly object EventDisposed = new object();
        private EventHandlerList events;
        private ISite site;

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public event EventHandler Disposed
        {
            add
            {
                this.Events.AddHandler(EventDisposed, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventDisposed, value);
            }
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
                lock (this)
                {
                    if ((this.site != null) && (this.site.Container != null))
                    {
                        this.site.Container.Remove(this);
                    }
                    if (this.events != null)
                    {
                        EventHandler handler = (EventHandler) this.events[EventDisposed];
                        if (handler != null)
                        {
                            handler(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        ~Component()
        {
            this.Dispose(false);
        }

        protected virtual object GetService(Type service)
        {
            ISite site = this.site;
            if (site != null)
            {
                return site.GetService(service);
            }
            return null;
        }

        public override string ToString()
        {
            ISite site = this.site;
            if (site != null)
            {
                return (site.Name + " [" + base.GetType().FullName + "]");
            }
            return base.GetType().FullName;
        }

        protected virtual bool CanRaiseEvents
        {
            get
            {
                return true;
            }
        }

        internal bool CanRaiseEventsInternal
        {
            get
            {
                return this.CanRaiseEvents;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IContainer Container
        {
            get
            {
                ISite site = this.site;
                if (site != null)
                {
                    return site.Container;
                }
                return null;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        protected bool DesignMode
        {
            get
            {
                ISite site = this.site;
                return ((site != null) && site.DesignMode);
            }
        }

        protected EventHandlerList Events
        {
            get
            {
                if (this.events == null)
                {
                    this.events = new EventHandlerList(this);
                }
                return this.events;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ISite Site
        {
            get
            {
                return this.site;
            }
            set
            {
                this.site = value;
            }
        }
    }
}

