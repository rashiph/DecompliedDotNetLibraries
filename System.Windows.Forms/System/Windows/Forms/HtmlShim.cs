namespace System.Windows.Forms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    internal abstract class HtmlShim : IDisposable
    {
        private Dictionary<EventHandler, HtmlToClrEventProxy> attachedEventList;
        private int eventCount;
        private EventHandlerList events;

        protected HtmlShim()
        {
        }

        protected HtmlToClrEventProxy AddEventProxy(string eventName, EventHandler eventHandler)
        {
            if (this.attachedEventList == null)
            {
                this.attachedEventList = new Dictionary<EventHandler, HtmlToClrEventProxy>();
            }
            HtmlToClrEventProxy proxy = new HtmlToClrEventProxy(this, eventName, eventHandler);
            this.attachedEventList[eventHandler] = proxy;
            return proxy;
        }

        public void AddHandler(object key, Delegate value)
        {
            this.eventCount++;
            this.Events.AddHandler(key, value);
            this.OnEventHandlerAdded();
        }

        public abstract void AttachEventHandler(string eventName, EventHandler eventHandler);
        public abstract void ConnectToEvents();
        public abstract void DetachEventHandler(string eventName, EventHandler eventHandler);
        public virtual void DisconnectFromEvents()
        {
            if (this.attachedEventList != null)
            {
                EventHandler[] array = new EventHandler[this.attachedEventList.Count];
                this.attachedEventList.Keys.CopyTo(array, 0);
                foreach (EventHandler handler in array)
                {
                    HtmlToClrEventProxy proxy = this.attachedEventList[handler];
                    this.DetachEventHandler(proxy.EventName, handler);
                }
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
                this.DisconnectFromEvents();
                if (this.events != null)
                {
                    this.events.Dispose();
                    this.events = null;
                }
            }
        }

        ~HtmlShim()
        {
            this.Dispose(false);
        }

        public void FireEvent(object key, EventArgs e)
        {
            Delegate delegate2 = this.Events[key];
            if (delegate2 != null)
            {
                try
                {
                    delegate2.DynamicInvoke(new object[] { this.GetEventSender(), e });
                }
                catch (Exception exception)
                {
                    if (NativeWindow.WndProcShouldBeDebuggable)
                    {
                        throw;
                    }
                    Application.OnThreadException(exception);
                }
            }
        }

        protected abstract object GetEventSender();
        protected virtual void OnEventHandlerAdded()
        {
            this.ConnectToEvents();
        }

        protected virtual void OnEventHandlerRemoved()
        {
            if (this.eventCount <= 0)
            {
                this.DisconnectFromEvents();
                this.eventCount = 0;
            }
        }

        protected HtmlToClrEventProxy RemoveEventProxy(EventHandler eventHandler)
        {
            if ((this.attachedEventList != null) && this.attachedEventList.ContainsKey(eventHandler))
            {
                HtmlToClrEventProxy proxy = this.attachedEventList[eventHandler];
                this.attachedEventList.Remove(eventHandler);
                return proxy;
            }
            return null;
        }

        public void RemoveHandler(object key, Delegate value)
        {
            this.eventCount--;
            this.Events.RemoveHandler(key, value);
            this.OnEventHandlerRemoved();
        }

        public abstract UnsafeNativeMethods.IHTMLWindow2 AssociatedWindow { get; }

        private EventHandlerList Events
        {
            get
            {
                if (this.events == null)
                {
                    this.events = new EventHandlerList();
                }
                return this.events;
            }
        }
    }
}

